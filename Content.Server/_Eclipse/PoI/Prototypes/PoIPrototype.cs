using Content.Shared.Station;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._Eclipse.PoI;

[Prototype("poi")]
public sealed partial class PoIPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// Name of the map to use in generic messages, like the map vote.
    /// </summary>
    [DataField(required: true)]
    public string MapName { get; private set; } = default!;

    /// <summary>
    /// Relative directory path to the given grid, i.e. `/Maps/saltern.yml`
    /// </summary>
    [DataField(required: true)]
    public ResPath MapPath { get; private set; } = default!;

    /// <summary>
    /// The station config for that grid.
    /// </summary>
    [DataField("station")]
    public StationConfig? Station = null;
}
