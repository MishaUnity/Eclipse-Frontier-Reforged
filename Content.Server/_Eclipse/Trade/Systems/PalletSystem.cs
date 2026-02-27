using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Trade.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Stacks;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Trade;
using Content.Shared.Trade.BUI;
using Content.Shared.Trade.Events;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;

namespace Content.Server.Trade.Systems;

public sealed class PalletSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ILogManager _log = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly PricingSystem _pricing = default!;

    private ISawmill _sawmill = default!;

    private static readonly EntProtoId CashProto = "SpaceCash";

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _log.GetSawmill("trade");

        SubscribeLocalEvent<TradePalletConsoleComponent, TradePalletSellMessage>(OnPalletSale);
        SubscribeLocalEvent<TradePalletConsoleComponent, TradePalletAppraiseMessage>(OnPalletAppraise);
        SubscribeLocalEvent<TradePalletConsoleComponent, BoundUIOpenedEvent>(OnPalletUIOpen);
    }

    private readonly List<EntityUid> _listEnts = new();

    private void OnPalletSale(EntityUid uid, TradePalletConsoleComponent component, TradePalletSellMessage args)
    {
        if (!TryComp<TransformComponent>(uid, out var xform))
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        if (xform.GridUid is not { } gridUid)
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        GetPalletGoods(uid, gridUid, out var goods);

        if (goods.Count == 0)
        {
            _audio.PlayPvs(component.DenySound, uid);
            return;
        }

        var sellTotal = 0.0;

        _listEnts.Clear();
        foreach (var ent in goods)
        {
            var price = _pricing.GetPrice(ent);
            if (price <= 0)
            {
                _listEnts.Add(ent);
                continue;
            }

            sellTotal += price;
        }

        foreach (var ent in _listEnts)
        {
            goods.Remove(ent);
        }

        foreach (var ent in goods)
        {
            Del(ent);
        }

        var cash = Spawn(CashProto, Transform(uid).Coordinates);
        _stack.SetCount((cash, null), (int)Math.Floor(sellTotal));

        _audio.PlayPvs(component.ApproveSound, uid);
        UpdatePalletConsoleInterface(uid);
    }

    private void OnPalletAppraise(EntityUid uid, TradePalletConsoleComponent component, TradePalletAppraiseMessage args)
    {
        UpdatePalletConsoleInterface(uid);
    }

    private void OnPalletUIOpen(EntityUid uid, TradePalletConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdatePalletConsoleInterface(uid);
    }

    private void UpdatePalletConsoleInterface(Entity<TransformComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp, false))
        {
            _uiSystem.SetUiState(ent.Owner,
                TradePalletConsoleUiKey.Key,
                new TradePalletConsoleInterfaceState(0, 0, false));
            return;
        }

        if (ent.Comp.GridUid is not { } gridUid)
        {
            _uiSystem.SetUiState(ent.Owner,
                TradePalletConsoleUiKey.Key,
                new TradePalletConsoleInterfaceState(0, 0, false));
            return;
        }

        GetPalletGoods(ent, gridUid, out var goods);
        var sellTotal = 0.0;
        foreach (var uid in goods)
        {
            var price = _pricing.GetPrice(uid);
            if (price <= 0)
                continue;

            sellTotal += price;
        }

        _uiSystem.SetUiState(ent.Owner,
            TradePalletConsoleUiKey.Key,
            new TradePalletConsoleInterfaceState((int)Math.Floor(sellTotal), goods.Count, true));
    }

    private void GetPallets(EntityUid consoleUid, EntityUid gridUid, out HashSet<EntityUid> pallets)
    {
        pallets = new();

        var maybePallets = _link.GetLinkedEntities(consoleUid);
        if (maybePallets != null)
        {
            foreach (var pallet in maybePallets)
            {
                if (!TryComp<TransformComponent>(pallet, out var xform))
                {
                    _sawmill.Warning($"Found a linked entity {pallet} to a trade console that didn't have a TransformComponent");
                    _link.TryUnlinkOneWay(consoleUid, pallet);
                    continue;
                }

                if (xform.GridUid is not { } palletGridUid || palletGridUid != gridUid)
                {
                    // These are not anchorable by players, unlink
                    _link.TryUnlinkOneWay(consoleUid, pallet);
                    continue;
                }

                if (!HasComp<TradePalletComponent>(pallet))
                {
                    // The entity is not a trade pallet, log and unlink
                    _sawmill.Warning($"Found a linked entity {pallet} to a trade console that wasn't a trade pallet");
                    _link.TryUnlinkOneWay(consoleUid, pallet);
                    continue;
                }

                pallets.Add(pallet);
            }
        }
    }

    private readonly HashSet<EntityUid> _setEnts = new();

    private void GetPalletGoods(EntityUid consoleUid, EntityUid gridUid, out HashSet<EntityUid> goods)
    {
        goods = new HashSet<EntityUid>();
        _setEnts.Clear();

        GetPallets(consoleUid, gridUid, out var pallets);
        foreach (var uid in pallets)
        {
            _lookup.GetEntitiesIntersecting(uid, _setEnts, LookupFlags.Dynamic | LookupFlags.Sundries);
            foreach (var ent in _setEnts)
            {
                if (goods.Contains(ent) || !CanSell(ent))
                    continue;

                goods.Add(ent);
            }
        }
    }

    private bool CanSell(EntityUid uid)
    {
        if (HasComp<MobStateComponent>(uid) || HasComp<CargoSellBlacklistComponent>(uid))
            return false;

        if (!TryComp<TransformComponent>(uid, out var xform))
            return true;

        if (xform.Anchored)
            return false;

        // Recursively check for mobs at any point.
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            if (!CanSell(child))
                return false;
        }

        return true;
    }
}
