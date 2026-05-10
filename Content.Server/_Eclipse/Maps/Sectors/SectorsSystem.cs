using Content.Server._Eclipse.Maps.Celestial;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._Eclipse.Maps.Celestial;
using Content.Shared._Eclipse.Maps.Sectors;
using Content.Shared._Eclipse.Stargate;
using Content.Shared.Dataset;
using Content.Shared.Shuttles.Components;
using Content.Shared.Shuttles.Systems;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Content.Server._Eclipse.Maps.Sectors;

public sealed partial class SectorsSystem : SharedSectorsSystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly UserInterfaceSystem _interface = default!;
    [Dependency] private readonly MetaDataSystem _meta = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly CelestialBodySystem _celestialBody = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;

    private string[] _suffixCodes = { "Theta", "Alpha", "Beta", "Delta", "Omega", "KER", "TG", "PR", "LV", "SYS", "MG" };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShuttleComponent, EnterCelestialBodyRadiusEvent>(OnEnterBodyRadius);
        SubscribeLocalEvent<ShuttleComponent, ExitCelestialBodyRadiusEvent>(OnExitBodyRadius);

        Subs.BuiEvents<SectorsMapConsoleComponent>(SectorsMapConsoleUiKey.Key, subs =>
        {
            subs.Event<BoundUIOpenedEvent>(OnConsoleUiOpened);
            subs.Event<RequestStargateJumpMessage>(OnRequestStargateJump);
        });
    }

    private void OnEnterBodyRadius(EntityUid uid, ShuttleComponent component, EnterCelestialBodyRadiusEvent args)
    {
        UpdateConsolesOnGrid(uid);
    }

    private void OnExitBodyRadius(EntityUid uid, ShuttleComponent component, ExitCelestialBodyRadiusEvent args)
    {
        UpdateConsolesOnGrid(uid);
    }

    private void UpdateConsolesOnGrid(EntityUid gridUid)
    {
        HashSet<Entity<SectorsMapConsoleComponent>> consoles = new();
        _lookup.GetGridEntities(gridUid, consoles);

        foreach (var console in consoles)
            UpdateUiState(console);
    }

    private void OnConsoleUiOpened(Entity<SectorsMapConsoleComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUiState(ent);
    }

    private void OnRequestStargateJump(Entity<SectorsMapConsoleComponent> ent, ref RequestStargateJumpMessage args)
    {
        var gridUid = _transform.GetGrid(ent.Owner);
        var gateUid = _celestialBody.GetClosestBody(gridUid);

        if (gridUid == null || gateUid == null ||
            !TryComp<ShuttleComponent>(gridUid, out var shuttle) ||
            !TryComp<StargateComponent>(gateUid, out var gate))
            return;

        _shuttle.FTLToCoordinates(gridUid.Value, shuttle, _transform.ToCoordinates(gate.ExitPosition),
                                  gate.ExitRotation, 20f, 60f);
    }

    private void UpdateUiState(Entity<SectorsMapConsoleComponent> ent)
    {
        var sectorsQuery = EntityQueryEnumerator<MapSectorComponent>();
        var closestBody = _celestialBody.GetClosestBody(_transform.GetGrid(ent.Owner));

        var state = new SectorsMapBoundUserInterfaceState(sectorsQuery, closestBody, EntityManager);

        _interface.SetUiState(ent.Owner, SectorsMapConsoleUiKey.Key, state);
    }

    public Entity<MapSectorComponent> CreateEmptySector(Vector2 position)
    {
        var mapEntity = _map.CreateMap();
        var comp = EnsureComp<MapSectorComponent>(mapEntity);

        comp.Position = position;

        var prefix = _random.Pick(_suffixCodes);
        var formatedPosition = new Vector2i((int)Math.Round(position.X), (int)Math.Round(position.Y));
        var positionString = string.Format("{0}{1}", formatedPosition.X, formatedPosition.Y).Replace("-", "N");
        _meta.SetEntityName(mapEntity, string.Format("{0}-{1}", prefix, positionString));

        return new(mapEntity, comp);
    }
}
