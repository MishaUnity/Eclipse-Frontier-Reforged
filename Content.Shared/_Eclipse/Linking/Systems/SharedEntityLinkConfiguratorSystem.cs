using Robust.Shared.Serialization;

namespace Content.Shared.Linking.Systems;

public abstract class SharedEntityLinkConfiguratorSystem : EntitySystem
{

}

[Serializable, NetSerializable]
public enum EntityLinkConfiguratorVisuals
{
    Mode
}

[Serializable, NetSerializable]
public enum EntityLinkConfiguratorLayers
{
    ModeLight
}
