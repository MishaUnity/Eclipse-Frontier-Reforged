using Content.Server.Shuttles.Components;
using Content.Shared._Eclipse.Map.Celestial;
using Content.Shared._Eclipse.Maps.Celestial;
using Content.Shared.Abilities.Mime;
using Content.Shared.Fax.Components;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._Eclipse.Maps.Celestial;

public sealed partial class CelestialBodySystem : SharedCelestialBodySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PvsOverrideSystem _pvs = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CelestialBodyComponent, ComponentInit>(OnBodyInit);

        SubscribeLocalEvent<ShuttleComponent, MoveEvent>(OnShuttleMove);
    }

    private void OnBodyInit(EntityUid uid, CelestialBodyComponent component, ComponentInit args)
    {
        // Planets should be loaded even outside loading zone
        _pvs.AddGlobalOverride(uid);

        if (component.RandomParallaxes.Count != 0)
            component.ParallaxPrototype = _random.Pick(component.RandomParallaxes);

        var namesDataset = _prototype.Index(component.NamesDataset);
        if (namesDataset != null)
            _metaData.SetEntityName(uid, Loc.GetString(_random.Pick(namesDataset.Values)));

        Dirty(uid, component);
    }

    public EntityUid? GetClosestBody(EntityUid? gridUid)
    {
        if (gridUid == null)
            return null;

        if (!TryComp<AboveCelestialBodyComponent>(gridUid, out var above))
            return null;

        float closestDistance = float.MaxValue;
        EntityUid? closestBody = null;

        foreach (var body in above.Bodies)
        {
            float distance = Vector2.Distance(_transform.GetWorldPosition(gridUid.Value), _transform.GetWorldPosition(body));
            if (distance < closestDistance)
            {
                closestBody = body;
                closestDistance = distance;
            }
        }

        return closestBody;
    }

    private void ProcessShuttleEnter(EntityUid uid, EntityUid body)
    {
        var above = EnsureComp<AboveCelestialBodyComponent>(uid);

        if (above.Bodies.Contains(body))
            return;

        above.Bodies.Add(body);
        RaiseLocalEvent(uid, new EnterCelestialBodyRadiusEvent(body));
    }

    private void ProcessShuttleExit(EntityUid uid, EntityUid body)
    {
        if (!TryComp<AboveCelestialBodyComponent>(uid, out var above))
            return;

        if (!above.Bodies.Contains(body))
            return;

        above.Bodies.Remove(body);
        if (above.Bodies.Count == 0)
            RemComp<AboveCelestialBodyComponent>(uid);

        RaiseLocalEvent(uid, new ExitCelestialBodyRadiusEvent(body));
    }

    private void RefreshShuttle(EntityUid uid)
    {
        HashSet<Entity<CelestialBodyComponent>> bodies = new();
        _lookup.GetEntitiesOnMap(_transform.GetMapId(uid), bodies);

        if (TryComp<AboveCelestialBodyComponent>(uid, out var above))
        {
            foreach (var body in above.Bodies.ToArray())
                ProcessShuttleExit(uid, body);
        }

        foreach (var body in bodies)
        {
            float distance = Vector2.Distance(_transform.GetWorldPosition(uid), _transform.GetWorldPosition(body));
            if (distance <= body.Comp.Radius)
                ProcessShuttleEnter(uid, body);
        }
    }

    private void OnShuttleMove(EntityUid uid, ShuttleComponent component, MoveEvent args)
    {
        if (args.OldPosition.EntityId != args.NewPosition.EntityId)
        {
            RefreshShuttle(uid);
            return;
        }

        HashSet<Entity<CelestialBodyComponent>> bodies = new();
        _lookup.GetEntitiesOnMap(_transform.GetMapId(uid), bodies);

        foreach (var body in bodies)
        {
            float newDistance = Vector2.Distance(_transform.ToWorldPosition(args.NewPosition), _transform.GetWorldPosition(body));
            float oldDistance = Vector2.Distance(_transform.ToWorldPosition(args.OldPosition), _transform.GetWorldPosition(body));

            if (newDistance <= body.Comp.Radius && oldDistance >= body.Comp.Radius)
                ProcessShuttleEnter(uid, body);
            else if (newDistance >= body.Comp.Radius && oldDistance <= body.Comp.Radius)
                ProcessShuttleExit(uid, body);
        }
    }
}

/// <summary>
/// Rises on grid entity, when it enters celestial body radius.
/// </summary>
public record struct EnterCelestialBodyRadiusEvent
{
    public EntityUid Body;

    public EnterCelestialBodyRadiusEvent(EntityUid body)
    {
        Body = body;
    }
}

/// <summary>
/// Rises on grid entity, when it enters celestial body radius.
/// </summary>
public record struct ExitCelestialBodyRadiusEvent
{
    public EntityUid Body;

    public ExitCelestialBodyRadiusEvent(EntityUid body)
    {
        Body = body;
    }
}
