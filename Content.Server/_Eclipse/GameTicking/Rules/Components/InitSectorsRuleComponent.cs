using Content.Server._Eclipse.Maps.Sectors;
using Content.Server._Eclipse.PoI;
using Content.Shared._Eclipse.Maps.Sectors;
using Robust.Shared.Prototypes;

namespace Content.Server._Eclipse.GameTicking.Rules.Components;

[RegisterComponent, Access(typeof(InitSectorsRuleSystem))]
public sealed partial class InitSectorsRuleComponent : Component
{
    [DataField("position")]
    public SectorPositionGenerator? PositionGenerator;

    [DataField("content")]
    public List<SectorContentGenerator> Content = new();
}
