using Content.Server._Eclipse.PoI;
using Content.Shared.EntityList;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._Eclipse.Maps.Celestial;

[RegisterComponent]
public sealed partial class CelestialMapComponent : Component
{
    [DataField("objects")]
    public List<CelestialMapGenerator> Objects = new();
}

/// <summary>
/// Spawns random entities from the list in a ring around the center
/// </summary>
[DataDefinition]
public sealed partial class CelestialMapComponentOverwrite : CelestialMapGenerator
{
    [DataField("components")]
    public ComponentRegistry Components = new();

    public override void Spawn(EntityUid origin, EntityManager entityManager)
    {
        entityManager.AddComponents(origin, Components, true);
    }
}

/// <summary>
/// Spawns random entities from the list in a ring around the center
/// </summary>
[DataDefinition]
public sealed partial class CelestialMapRadius : CelestialMapGenerator
{
    [DataField("avalible")]
    public ProtoId<EntityListPrototype> AvailableEntities;

    [DataField("startDistance")]
    public float StartDistance = 500;
    [DataField("endDistance")]
    public float EndDistance = 6000;

    [DataField("minCount")]
    public int MinCount = 2;
    [DataField("maxCount")]
    public int MaxCount = 4;

    [DataField("rotationOffset")]
    public Angle RotationOffset = Angle.FromDegrees(60f);

    public override void Spawn(EntityUid origin, EntityManager entityManager)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var prototype = IoCManager.Resolve<IPrototypeManager>();

        var list = prototype.Index(AvailableEntities);
        var count = random.Next(MinCount, MaxCount + 1);

        var offset = (EndDistance - StartDistance) / (count + 1);
        for (int i = 1; i < count + 1; i++)
        {
            var distance = StartDistance + offset * i;

            var angle = random.NextAngle();
            Vector2 position = angle.RotateVec(new Vector2(distance, 0f));
            var rotation = Angle.FromWorldVec(position.Normalized()) + RotationOffset;

            entityManager.SpawnAttachedTo(random.Pick(list.Entities),
                                          new EntityCoordinates(origin, position), rotation: rotation);
        }
    }
}

/// <summary>
/// Spawns entity at a certain distance from the center
/// </summary>
[DataDefinition]
public sealed partial class CelestialMapEntity : CelestialMapGenerator
{
    [DataField("entity")]
    public ProtoId<EntityPrototype>? Entity = null;

    [DataField("minDistance")]
    public float MinDistance = 1000;
    [DataField("maxDistance")]
    public float MaxDistance = 2000;

    public override void Spawn(EntityUid origin, EntityManager entityManager)
    {
        var random = IoCManager.Resolve<IRobustRandom>();

        var angle = random.NextAngle();
        Vector2 position = angle.RotateVec(new Vector2(random.NextFloat(MinDistance, MaxDistance), 0f));

        entityManager.SpawnAttachedTo(Entity, new EntityCoordinates(origin, position));
    }
}

/// <summary>
/// Spawns PoI near preferred bodies
/// </summary>
[DataDefinition]
public sealed partial class CelestialMapPoINearBody : CelestialMapNearBodyGenerator
{
    // String keys for better readability
    [DataField("groups")]
    public Dictionary<string, List<ProtoId<PoIPrototype>>> RandomSpawnGroups = new();

    public override void Spawn(EntityUid origin, EntityManager entityManager)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var poi = entityManager.System<PoISystem>();
        var transform = entityManager.System<TransformSystem>();

        if (!TryGetPosition(origin, entityManager, out var position))
            return;

        foreach (var group in RandomSpawnGroups)
        {
            var offset = random.NextAngle().RotateVec(new Vector2(random.NextFloat(0f, MaxDistance), 0f));

            var spawnPrototype = random.Pick(group.Value);
            poi.SpawnPoI(spawnPrototype, new MapCoordinates(position + offset, transform.GetMapId(origin)));
        }
    }
}

[DataDefinition]
public abstract partial class CelestialMapNearBodyGenerator : CelestialMapGenerator
{
    [DataField("prefer")]
    public ProtoId<EntityListPrototype> PreferList;

    [DataField("onlyPreferred")]
    public bool OnlyPreferred = false;

    [DataField("maxDistance")]
    public float MaxDistance = 300;

    public bool TryGetPosition(EntityUid origin, EntityManager entityManager, out Vector2 position)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var prototype = IoCManager.Resolve<IPrototypeManager>();
        var transform = entityManager.System<TransformSystem>();
        position = Vector2.Zero;

        var body = TryFindBody(origin, entityManager, random, prototype);
        if (body == null)
            return false;

        position = transform.GetWorldPosition(body.Value);

        return true;
    }

    public EntityUid? TryFindBody(EntityUid origin, EntityManager entityManager,
                                   IRobustRandom random, IPrototypeManager prototype)
    {
        var list = prototype.Index(PreferList);
        if (list == null || !entityManager.TryGetComponent<TransformComponent>(origin, out var transform))
            return null;

        // Enumerate all spawned entities
        List<EntityUid> preferedEntities = new();
        List<EntityUid> allEntities = new();

        var childrens = transform.ChildEnumerator;
        while (childrens.MoveNext(out var child))
        {
            allEntities.Add(child);

            if (!entityManager.TryGetComponent<MetaDataComponent>(child, out var meta) || meta.EntityPrototype == null)
                continue;
            if (list.Entities.Contains(meta.EntityPrototype))
                preferedEntities.Add(child);
        }

        if (preferedEntities.Count != 0)
            return random.Pick(preferedEntities);
        if (!OnlyPreferred)
            return random.Pick(allEntities);

        return null;
    }
}

public abstract partial class CelestialMapGenerator
{
    public abstract void Spawn(EntityUid origin, EntityManager entityManager);
}
