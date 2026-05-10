using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Serialization;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Content.Shared._Eclipse.Maps.Sectors;

[RegisterComponent]
public sealed partial class SectorsMapConsoleComponent : Component
{

}

[Serializable, NetSerializable]
public enum SectorsMapConsoleUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public partial struct MapSectorData
{
    public List<Vector2> Connections;
    public string Name = "Unknown";
    public MapId MapId = MapId.Nullspace;

    public MapSectorData()
    {
        Connections = new();
    }
}

[Serializable, NetSerializable]
public sealed class SectorsMapBoundUserInterfaceState : BoundUserInterfaceState
{
    public Dictionary<Vector2, MapSectorData> Sectors = new();
    public NetEntity? ClosestBody;

    public SectorsMapBoundUserInterfaceState(EntityQueryEnumerator<MapSectorComponent> sectors, EntityUid? closestBody, EntityManager entityManager)
    {
        entityManager.TryGetNetEntity(closestBody, out ClosestBody);

        while (sectors.MoveNext(out var uid, out var comp))
        {
            var sectorData = new MapSectorData();

            if (entityManager.TryGetComponent<MetaDataComponent>(uid, out var meta))
                sectorData.Name = meta.EntityName;

            if (entityManager.TryGetComponent<MapComponent>(uid, out var map))
                sectorData.MapId = map.MapId;

            foreach (var connection in comp.Connections)
                sectorData.Connections.Add(connection.Comp.Position);

            Sectors.Add(comp.Position, sectorData);
        }
    }
}

[Serializable, NetSerializable]
public sealed class RequestStargateJumpMessage : BoundUserInterfaceMessage
{

}
