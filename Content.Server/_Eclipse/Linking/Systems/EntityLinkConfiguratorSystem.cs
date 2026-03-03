using Content.Server.Linking.Components;
using Content.Shared.Database;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Linking.Systems;
using Content.Shared.Maps;
using Content.Shared.Popups;
using Content.Shared.Teleportation.Systems;
using Content.Shared.Verbs;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Utility;

namespace Content.Server.Linking.Systems;

public sealed class EntityLinkConfiguratorSystem : SharedEntityLinkConfiguratorSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LinkedEntitySystem _link = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<EntityLinkConfiguratorComponent, PostMapInitEvent>(OnPostMapInit);

        SubscribeLocalEvent<EntityLinkConfiguratorComponent, GetVerbsEvent<AlternativeVerb>>(OnAddSwitchModeVerb);

        SubscribeLocalEvent<EntityLinkConfiguratorComponent, AfterInteractEvent>(AfterInteract);
        SubscribeLocalEvent<EntityLinkConfiguratorComponent, ExaminedEvent>(DoExamine);
    }

    private void OnPostMapInit(EntityUid uid, EntityLinkConfiguratorComponent component, PostMapInitEvent args)
    {
        component.ActiveDeviceLink = null;
    }

    private void OnAddSwitchModeVerb(EntityUid uid, EntityLinkConfiguratorComponent configurator, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.Using.HasValue || !HasComp<EntityLinkConfiguratorComponent>(args.Target))
            return;

        AlternativeVerb verb = new()
        {
            Text = Loc.GetString("link-configurator-switch-mode"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/settings.svg.192dpi.png")),
            Act = () => SwitchMode((args.Target, configurator), args.User),
            Impact = LogImpact.Low
        };
        args.Verbs.Add(verb);
    }

    private void AfterInteract(EntityUid uid, EntityLinkConfiguratorComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach || !args.Target.HasValue)
            return;

        if (args.Target is not { Valid: true } target)
            return;

        if (component.ActiveDeviceLink.HasValue)
        {
            if (component.ActiveDeviceLink == target)
            {
                _popup.PopupEntity(Loc.GetString("link-configurator-link-stopped"), target, args.User);
                component.ActiveDeviceLink = null;
                return;
            }

            // Remove links if already linked
            if (_link.IsLinkedOneWay(component.ActiveDeviceLink.Value, target))
            {
                if (component.LinkModeSymmetrical && _link.IsLinkedOneWay(target, component.ActiveDeviceLink.Value))
                {
                    _link.TryUnlink(component.ActiveDeviceLink.Value, target);
                    _popup.PopupEntity(Loc.GetString("link-configurator-link-removed-symmetrical", ("first", Name(component.ActiveDeviceLink.Value)), ("second", Name(target))), target, args.User);
                }
                else
                {
                    _link.TryUnlinkOneWay(component.ActiveDeviceLink.Value, target);
                    _popup.PopupEntity(Loc.GetString("link-configurator-link-removed", ("first", Name(component.ActiveDeviceLink.Value)), ("second", Name(target))), target, args.User);
                }
                component.ActiveDeviceLink = null;
                return;
            }

            if (component.LinkModeSymmetrical)
            {
                // Can't fail due to the check above
                _link.TryLink(component.ActiveDeviceLink.Value, target, false);
                _popup.PopupEntity(Loc.GetString("link-configurator-link-created-symmetrical", ("first", Name(component.ActiveDeviceLink.Value)), ("second", Name(target))), target, args.User);
            }
            else
            {
                // Can't fail due to the check above
                _link.OneWayLink(component.ActiveDeviceLink.Value, target, false);
                _popup.PopupEntity(Loc.GetString("link-configurator-link-created", ("first", Name(component.ActiveDeviceLink.Value)), ("second", Name(target))), target, args.User);
            }
            component.ActiveDeviceLink = null;
        }
        else
        {
            _popup.PopupEntity(Loc.GetString("link-configurator-link-started", ("device", Name(target))), target, args.User);
            component.ActiveDeviceLink = target;
        }
    }

    private void DoExamine(EntityUid uid, EntityLinkConfiguratorComponent component, ExaminedEvent args)
    {
        var mode = component.LinkModeSymmetrical ? "link-configurator-examine-mode-symmetrical" : "link-configurator-examine-mode-oneway";
        args.PushMarkup(Loc.GetString("link-configurator-examine-current-mode", ("mode", Loc.GetString(mode))));
    }

    private void SwitchMode(Entity<EntityLinkConfiguratorComponent> ent, EntityUid? userUid)
    {
        ent.Comp.LinkModeSymmetrical = !ent.Comp.LinkModeSymmetrical;

        if (!userUid.HasValue)
            return;

        var mode = ent.Comp.LinkModeSymmetrical ? "link-configurator-mode-symmetrical" : "link-configurator-mode-oneway";
        _popup.PopupEntity(Loc.GetString("link-configurator-switched-mode", ("mode", Loc.GetString(mode))), ent, userUid.Value);

        _appearance.SetData(ent, EntityLinkConfiguratorVisuals.Mode, ent.Comp.LinkModeSymmetrical);

        var pitch = ent.Comp.LinkModeSymmetrical ? 1 : 0.8f;
        _audio.PlayPvs(ent.Comp.SoundSwitchMode, userUid.Value, AudioParams.Default.WithVolume(1.5f).WithPitchScale(pitch));
    }
}
