using Robust.Client.Graphics;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using System.Numerics;

[Prototype]
public sealed partial class DynamicParallaxPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("layers")]
    public List<DynamicParallaxLayer> Layers { get; private set; } = new();
}

/// <summary>
/// Layer, that use RSI sprite for render
/// <summary>
[DataDefinition]
public sealed partial class DynamicParallaxLayer
{
    [DataField("visual")]
    public SpriteSpecifier Visual = default!;

    [DataField("config")]
    public DynamicParallaxLayerConfig Config = default!;
}

/// <summary>
/// Layer position config, most parameters are identical to the parallax layer
/// </summary>
[DataDefinition]
public partial record struct DynamicParallaxLayerConfig
{
    [DataField("scale")]
    public Vector2 Scale = Vector2.One;

    [DataField("rotation")]
    public Angle Rotation = Angle.Zero;

    [DataField("tiled")]
    public bool Tiled = false;

    [DataField("worldPosition")]
    public Vector2 WorldHomePosition;

    [DataField("slowness")]
    public float Slowness = 0.5f;

    [DataField("shader")]
    public string? Shader = "unshaded";

    public DynamicParallaxLayerConfig() { }
}

/// <summary>
/// Rendered texture with attached config
/// </summary>
public struct RenderedDynamicParallaxLayer
{
    public Texture Texture { get; set; }

    public DynamicParallaxLayerConfig Config { get; set; }
}
