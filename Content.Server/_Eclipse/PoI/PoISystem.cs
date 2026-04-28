using Content.Server.Shuttles.Systems;
using Content.Server.Station.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

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
    public void SpawnPoI(ProtoId<PoIPrototype> prototype, MapCoordinates position)
    {
        var poiPrototype = _prototypeManager.Index(prototype);

        _map.CreateMap(out var mapId);

        if (_mapLoader.TryLoadGrid(mapId, poiPrototype.MapPath, out var grid))
        {
            var targetEnt = Spawn(null, position);
            _shuttle.TryFTLProximity(grid.Value, targetEnt);
            Del(targetEnt);

            _metaData.SetEntityName(grid.Value, poiPrototype.MapName);

            if (poiPrototype.Station != null)
                _station.InitializeNewStation(poiPrototype.Station, new List<EntityUid> { grid.Value.Owner }, null);
        }

        _map.DeleteMap(mapId);
    }
}
