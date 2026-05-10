using Content.Shared._Eclipse.Maps.Sectors;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Server._Eclipse.Maps.Sectors.Generators;

[DataDefinition]
public sealed partial class DefaultSectorPositionGenerator : SectorPositionGenerator
{
    private List<Entity<MapSectorComponent>> _spawned = new();
    private SectorsSystem _sectors = default!;

    public override List<Entity<MapSectorComponent>> Generate(int count, EntityManager entityManager)
    {
        _sectors = entityManager.System<SectorsSystem>();

        Vector2 startPosition = Vector2.Zero;
        var centerSector = _sectors.CreateEmptySector(startPosition);

        _spawned.Add(centerSector);

        GenerateTree((int)Math.Ceiling(count / 4f), centerSector, new Vector2(0f, 1f), Angle.FromDegrees(90f));
        GenerateTree((int)Math.Ceiling(count / 4f), centerSector, new Vector2(0f, -1f), Angle.FromDegrees(90f));
        GenerateTree((int)Math.Floor(count / 4f), centerSector, new Vector2(1f, 0f), Angle.FromDegrees(90f));
        GenerateTree((int)Math.Floor(count / 4f), centerSector, new Vector2(-1f, 0f), Angle.FromDegrees(90f));

        foreach (var sector in _spawned)
        {
            if (sector.Comp.Connections.Count >= 4)
                continue;

            foreach (var potentialConnection in _spawned)
            {
                if (sector == potentialConnection || potentialConnection.Comp.Connections.Count >= 4)
                    continue;

                if (!CanCreateConnection(_spawned, Angle.FromDegrees(45f), 4f, sector, potentialConnection))
                    continue;

                ConnectSectors(sector, potentialConnection);
            }
        }

        return _spawned;
    }

    private void GenerateTree(int count, Entity<MapSectorComponent> source, Vector2 direction, Angle accuracy)
    {
        var random = IoCManager.Resolve<IRobustRandom>();

        var treeElements = new List<Entity<MapSectorComponent>>();

        Entity<MapSectorComponent> branchSource = source;
        while (count > 0)
        {
            Angle angle = random.NextAngle(-accuracy, accuracy);
            var distance = random.NextFloat(2f, 3f);
            var offset = angle.RotateVec(direction.Normalized()) * distance;
            var position = branchSource.Comp.Position + offset;

            if (GetClosestSector(_spawned, position, out _) < 1.5f)
                continue;

            var sector = _sectors.CreateEmptySector(position);

            ConnectSectors(branchSource, sector);

            treeElements.Add(sector);
            _spawned.Add(sector);
            branchSource = random.Pick(treeElements);

            count--;
        }
    }
}
