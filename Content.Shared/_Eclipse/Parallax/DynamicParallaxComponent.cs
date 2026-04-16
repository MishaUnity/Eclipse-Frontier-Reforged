using Robust.Shared.GameStates;

namespace Content.Shared._Eclipse.Parallax;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DynamicParallaxComponent : Component
{
    [DataField("parallax"), AutoNetworkedField]
    public string Parallax = "default";
}
