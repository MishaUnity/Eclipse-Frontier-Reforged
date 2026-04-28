using Content.Shared._Eclipse.Maps.Celestial;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Map;
using System.Numerics;

namespace Content.Server._Eclipse.Maps.Celestial;

public sealed partial class CelestialMapSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CelestialMapComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, CelestialMapComponent component, ComponentStartup args)
    {
        foreach (var element in component.Objects)
        {
            element.Spawn(uid, EntityManager);
        }
    }
}
