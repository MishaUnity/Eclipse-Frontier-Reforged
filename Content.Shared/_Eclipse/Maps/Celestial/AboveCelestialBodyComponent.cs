namespace Content.Shared._Eclipse.Maps.Celestial;

[RegisterComponent]
public sealed partial class AboveCelestialBodyComponent : Component
{
    [DataField]
    public List<EntityUid> Bodies = new();
}


