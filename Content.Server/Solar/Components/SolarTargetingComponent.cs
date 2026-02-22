// Eclipse

using Content.Server.Solar.EntitySystems;
using Content.Shared.Guidebook;

namespace Content.Server.Solar.Components;

[RegisterComponent]
[Access(typeof(PowerSolarSystem), typeof(PowerSolarControlConsoleSystem))]
public sealed partial class SolarTargetingComponent : Component
{
    /// <summary>
    /// The current target panel rotation.
    /// </summary>
    [DataField]
    public Angle TargetPanelRotation = Angle.Zero;

    /// <summary>
    /// The current target panel velocity.
    /// </summary>
    [DataField]
    public Angle TargetPanelVelocity = Angle.Zero;

    /// <summary>
    // Last update of total panel power.
    /// </summary>
    [DataField]
    public float TotalPanelPower = 0;

    /// <summary>
    // Last update of total panels found for this grid.
    /// </summary>
    [DataField]
    public float NumPanels = 0;
}
