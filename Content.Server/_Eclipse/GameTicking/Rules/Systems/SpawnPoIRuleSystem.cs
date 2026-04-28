using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Content.Server._Eclipse.PoI;
using Content.Server.GameTicking.Rules;
using Content.Server._Eclipse.GameTicking.Rules.Components;

namespace Content.Server._Eclipse.GameTicking.Rules;

public sealed class SpawnPoIRuleSystem : GameRuleSystem<SpawnPoIRuleComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PoISystem _poi = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Started(EntityUid uid, SpawnPoIRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        foreach (var (key, group) in component.RandomSpawnGroups)
        {
            var spawnData = _random.Pick(group);
            var coords = _random.NextVector2(spawnData.MinOriginDistance, spawnData.MaxOriginDistance);

            _poi.SpawnPoI(spawnData.MapId, new MapCoordinates(coords, GameTicker.DefaultMap));
        }
    }
}
