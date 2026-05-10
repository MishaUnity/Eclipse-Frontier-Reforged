using Content.Server._Eclipse.PoI;
using Content.Shared._Eclipse.Maps.Sectors;
using Content.Shared._Eclipse.Stargate;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Eclipse.Maps.Sectors.Generators;


[DataDefinition]
public sealed partial class StargateSectorGenerator : SectorContentGenerator
{
    [DataField("group")]
    public List<ProtoId<PoIPrototype>> SpawnGroup = new();

    [DataField("distance")]
    public float Distance = 8000;

    private List<Entity<MapSectorComponent>> _spawned = new();

    private PoISystem _poi = default!;
    private EntityLookupSystem _lookup = default!;
    private MetaDataSystem _meta = default!;
    private TransformSystem _transform = default!;

    public override List<Entity<MapSectorComponent>> Generate(List<Entity<MapSectorComponent>> sectors, EntityManager entityManager)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var proto = IoCManager.Resolve<IPrototypeManager>();
        _poi = entityManager.System<PoISystem>();
        _transform = entityManager.System<TransformSystem>();
        _meta = entityManager.System<MetaDataSystem>();
        _lookup = entityManager.System<EntityLookupSystem>();

        foreach (var sector in sectors)
        {
            foreach (var connection in sector.Comp.Connections)
            {
                var direction = (connection.Comp.Position - sector.Comp.Position).Normalized();
                var position = direction * Distance;
                var exitPosition = -direction * Distance;

                var gateGrid = _poi.SpawnPoI(random.Pick(SpawnGroup), new MapCoordinates(position, _transform.GetMapId(sector.Owner)));
                if (gateGrid == null)
                    continue;

                HashSet<Entity<StargateComponent>> gateEntities = new();
                _lookup.GetGridEntities(gateGrid.Value, gateEntities);

                foreach (var entity in gateEntities)
                {
                    var gateComp = entityManager.EnsureComponent<StargateComponent>(entity);
                    gateComp.ExitPosition = new MapCoordinates(exitPosition, _transform.GetMapId(connection.Owner));
                    gateComp.ExitRotation = Angle.FromWorldVec(-direction);

                    if (entityManager.TryGetComponent<MetaDataComponent>(connection.Owner, out var meta))
                        _meta.SetEntityName(entity, meta.EntityName);
                }
            }
        }

        return _spawned;
    }
}
