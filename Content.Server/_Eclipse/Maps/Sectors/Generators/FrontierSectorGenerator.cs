using Content.Shared._Eclipse.Maps.Sectors;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace Content.Server._Eclipse.Maps.Sectors.Generators;

[DataDefinition]
public sealed partial class FrontierSectorGenerator : SectorContentGenerator
{
    [DataField("map")]
    public EntProtoId MapPrototype;

    private List<Entity<MapSectorComponent>> _spawned = new();

    public override List<Entity<MapSectorComponent>> Generate(List<Entity<MapSectorComponent>> sectors, EntityManager entityManager)
    {
        Dictionary<Entity<MapSectorComponent>, float> rating = new();
        foreach (var sector in sectors)
            rating.Add(sector, Vector2.Distance(Vector2.Zero, sector.Comp.Position));

        var sorted = rating.OrderBy(rate => rate.Value);
        var selected = sorted.ElementAt(0);

        SpawnEntity(selected.Key, Vector2.Zero, MapPrototype, entityManager);

        return _spawned;
    }
}
