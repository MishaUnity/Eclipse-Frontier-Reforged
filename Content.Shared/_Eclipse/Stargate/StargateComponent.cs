using Robust.Shared.Map;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared._Eclipse.Stargate;

[RegisterComponent]
public sealed partial class StargateComponent : Component
{
    [ViewVariables]
    public MapCoordinates ExitPosition;
    [ViewVariables]
    public Angle ExitRotation;
}
