using Robust.Shared.Audio;

namespace Content.Server.Linking.Components;

[RegisterComponent]
public sealed partial class EntityLinkConfiguratorComponent : Component
{
    /// <summary>
    /// Determines whether the configurator is going to create symmetrical or directional links
    /// </summary>
    [DataField]
    public bool LinkModeSymmetrical = true;

    /// <summary>
    /// The first entity that will start the linking
    /// </summary>
    public EntityUid? ActiveDeviceLink;

    [DataField]
    public SoundSpecifier SoundSwitchMode = new SoundPathSpecifier("/Audio/Machines/quickbeep.ogg");
}
