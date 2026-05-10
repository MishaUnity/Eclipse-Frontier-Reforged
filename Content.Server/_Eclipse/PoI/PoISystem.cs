using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Eclipse.PoI;

public sealed partial class PoISystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    /// <summary>
    /// Tries to spawn PoI as close as possible to the specified coordinates, preventing overlap
    /// </summary>
    public EntityUid? SpawnPoI(ProtoId<PoIPrototype> prototype, MapCoordinates position)
    {
        var poiPrototype = _prototypeManager.Index(prototype);

        var grid = PlaceGrid(poiPrototype.MapPath, position);

        if (grid != null)
        {
            _metaData.SetEntityName(grid.Value, poiPrototype.MapName);

            if (poiPrototype.Station != null)
                _station.InitializeNewStation(poiPrototype.Station, new List<EntityUid> { grid.Value }, null);
        }

        return grid;
    }

    /// <summary>
    /// Load grid as close as possible to the specified coordinates, preventing overlap
    /// </summary>
    public EntityUid? PlaceGrid(ResPath path, MapCoordinates position)
    {
        _map.CreateMap(out var mapId);

        if (_mapLoader.TryLoadGrid(mapId, path, out var grid))
        {
            var targetEnt = Spawn(null, position);
            _shuttle.TryFTLProximity(grid.Value, targetEnt);
            Del(targetEnt);
        }

        _map.DeleteMap(mapId);

        return grid;
    }
}
