using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Prototypes;
using System.Numerics;

namespace Content.Shared._Eclipse.Maps.Sectors;

[RegisterComponent]
public sealed partial class MapSectorComponent : Component
{
    [ViewVariables]
    public List<Entity<MapSectorComponent>> Connections = new();

    [ViewVariables]
    public Vector2 Position;

    public List<SectorContentGenerator> UsedGenerators = new();
}

public abstract partial class SectorContentGenerator
{
    public abstract List<Entity<MapSectorComponent>> Generate(List<Entity<MapSectorComponent>> sectors, EntityManager entityManager);

    public void SpawnEntity(Entity<MapSectorComponent> sector, Vector2 position,
                            EntProtoId entity, EntityManager entityManager)
    {
        if (!entityManager.TryGetComponent<MapComponent>(sector, out var map))
            return;

        var coordinates = new MapCoordinates(position, map.MapId);
        entityManager.SpawnEntity(entity, coordinates);
    }
}

public abstract partial class SectorPositionGenerator
{
    public abstract List<Entity<MapSectorComponent>> Generate(int count, EntityManager entityManager);

    public void ConnectSectors(Entity<MapSectorComponent> a, Entity<MapSectorComponent> b)
    {
        a.Comp.Connections.Add(b);
        b.Comp.Connections.Add(a);
    }

    public float GetClosestSector(List<Entity<MapSectorComponent>> sectors, Vector2 position, out Entity<MapSectorComponent>? closest)
    {
        closest = null;
        float closestDistance = float.PositiveInfinity;

        foreach (var sector in sectors)
        {
            if (closest == null)
                closest = sector;

            var distance = Vector2.Distance(position, sector.Comp.Position);

            if (distance < closestDistance)
            {
                closest = sector;
                closestDistance = Vector2.Distance(position, closest.Value.Comp.Position);
            }
        }

        return closestDistance;
    }

    // Prevent overlap
    public bool CanCreateConnection(List<Entity<MapSectorComponent>> sectors, Angle minAngle, float maxDistance,
                                    MapSectorComponent source, MapSectorComponent target)
    {
        Vector2 targetVec = source.Position - target.Position;
        float targetDistance = Vector2.Distance(source.Position, target.Position);

        if (AngleToClosestConnection(source, target.Position) < minAngle || targetDistance > maxDistance)
            return false;

        foreach (var sector in sectors)
        {
            var angle = Angle.ShortestDistance(Angle.FromWorldVec(source.Position - sector.Comp.Position),
                                               Angle.FromWorldVec(targetVec));
            var distance = Vector2.Distance(source.Position, sector.Comp.Position);

            if (distance < targetDistance && Math.Abs(angle) < minAngle)
                return false;
        }

        return true;
    }

    public Angle AngleToClosestConnection(MapSectorComponent component, Vector2 target)
    {
        Angle min = Math.PI * 2f;
        Vector2 targetVec = component.Position - target;

        foreach (var connection in component.Connections)
        {
            var angle = Math.Abs(Angle.ShortestDistance(
                                 Angle.FromWorldVec(component.Position - connection.Comp.Position),
                                 Angle.FromWorldVec(targetVec)));
            if (angle < min)
                min = angle;
        }

        return min;
    }
}
