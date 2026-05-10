using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Content.Server._Eclipse.PoI;
using Content.Server.GameTicking.Rules;
using Content.Server._Eclipse.GameTicking.Rules.Components;
using Content.Server._Eclipse.Maps.Sectors;

namespace Content.Server._Eclipse.GameTicking.Rules;

public sealed class InitSectorsRuleSystem : GameRuleSystem<InitSectorsRuleComponent>
{
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Started(EntityUid uid, InitSectorsRuleComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (component.PositionGenerator == null)
            return;

        var spawned = component.PositionGenerator.Generate(16, EntityManager);

        foreach (var content in component.Content)
        {
            var used = content.Generate(spawned, EntityManager);
            foreach (var sector in used)
                sector.Comp.UsedGenerators.Add(content);
        }
    }
}
