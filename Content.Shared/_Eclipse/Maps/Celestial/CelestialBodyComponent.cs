using Content.Shared.Dataset;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared._Eclipse.Map.Celestial;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CelestialBodyComponent : Component
{
    [DataField("names")]
    public ProtoId<LocalizedDatasetPrototype>? NamesDataset = null;

    [DataField("randomParallaxes")]
    public List<string> RandomParallaxes = new();
    [DataField("parallax"), AutoNetworkedField]
    public string? ParallaxPrototype;

    [DataField("radius"), AutoNetworkedField]
    public float Radius = 100f;

    [DataField("radarVisible"), AutoNetworkedField]
    public bool RadarVisible = true;
    [DataField("radarIcon"), AutoNetworkedField]
    public SpriteSpecifier RadarIcon = new SpriteSpecifier.Rsi(new("/Textures/_Eclipse/Markers/space.rsi"), "unknown");
    [DataField("radarColor"), AutoNetworkedField]
    public Color RadarColor = new Color(135, 150, 175);
}
