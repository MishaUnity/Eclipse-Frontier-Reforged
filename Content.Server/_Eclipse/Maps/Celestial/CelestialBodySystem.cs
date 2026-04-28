using Content.Server.Shuttles.Components;
using Content.Shared._Eclipse.Map.Celestial;
using Content.Shared._Eclipse.Maps.Celestial;
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
        SubscribeLocalEvent<ShuttleComponent, EnterCelestialBodyRadiusEvent>(OnShuttleEnter);
        SubscribeLocalEvent<ShuttleComponent, ExitCelestialBodyRadiusEvent>(OnShuttleExit);
    }

    private void OnBodyInit(EntityUid uid, CelestialBodyComponent component, ComponentInit args)
    {
        // Planets should be loaded even outside loading zone
        _pvs.AddGlobalOverride(uid);

        var namesDataset = _prototype.Index(component.NamesDataset);
        if (namesDataset != null)
            _metaData.SetEntityName(uid, _random.Pick(namesDataset.Values));
    }

    public EntityUid? GetNearestBody(EntityUid gridUid)
    {
        if (!TryComp<AboveCelestialBodyComponent>(gridUid, out var above))
            return null;

        float nearestDistance = float.MaxValue;
        EntityUid? nearestBody = null;

        foreach (var body in above.Bodies)
        {
            float distance = Vector2.Distance(_transform.GetWorldPosition(gridUid), _transform.GetWorldPosition(body));
            if (distance < nearestDistance)
            {
                nearestBody = body;
                nearestDistance = distance;
            }
        }

        return nearestBody;
    }

    private void OnShuttleEnter(EntityUid uid, ShuttleComponent component, EnterCelestialBodyRadiusEvent args)
    {
        var above = EnsureComp<AboveCelestialBodyComponent>(uid);
        above.Bodies.Add(args.Body);
    }

    private void OnShuttleExit(EntityUid uid, ShuttleComponent component, ExitCelestialBodyRadiusEvent args)
    {
        if (!TryComp<AboveCelestialBodyComponent>(uid, out var above))
            return;

        above.Bodies.Remove(args.Body);
        if (above.Bodies.Count == 0)
            RemComp<AboveCelestialBodyComponent>(uid);
    }

    private void OnShuttleMove(EntityUid uid, ShuttleComponent component, MoveEvent args)
    {
        HashSet<Entity<CelestialBodyComponent>> bodies = new();
        _lookup.GetEntitiesOnMap(_transform.GetMapId(uid), bodies);

        foreach (var body in bodies)
        {
            float newDistance = Vector2.Distance(_transform.ToWorldPosition(args.NewPosition), _transform.GetWorldPosition(body));
            float oldDistance = Vector2.Distance(_transform.ToWorldPosition(args.OldPosition), _transform.GetWorldPosition(body));

            if (newDistance <= body.Comp.Radius && oldDistance >= body.Comp.Radius)
                RaiseLocalEvent(uid, new EnterCelestialBodyRadiusEvent(body));
            else if (newDistance >= body.Comp.Radius && oldDistance <= body.Comp.Radius)
                RaiseLocalEvent(uid, new ExitCelestialBodyRadiusEvent(body));
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
