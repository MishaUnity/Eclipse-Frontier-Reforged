using Content.Shared.Dataset;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Shared._Eclipse.Map.Celestial;

[RegisterComponent]
public sealed partial class CelestialBodyComponent : Component
{
    [DataField("names")]
    public ProtoId<LocalizedDatasetPrototype>? NamesDataset = null;

    [DataField("parallax")]
    public string? ParallaxPrototype;

    [DataField("radius")]
    public float Radius = 100f;

    [DataField("radarVisible")]
    public bool RadarVisible = true;
    [DataField("radarIcon")]
    public SpriteSpecifier RadarIcon = new SpriteSpecifier.Rsi(new("/Textures/_Eclipse/Markers/space.rsi"), "unknown");
    [DataField("radarColor")]
    public Color RadarColor = new Color(135, 150, 175);
}
