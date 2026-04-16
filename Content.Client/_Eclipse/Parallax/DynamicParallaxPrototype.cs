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

// Layer, that use RSI for render
[DataDefinition]
public sealed partial class DynamicParallaxLayer
{
    [DataField("visual")]
    public SpriteSpecifier Visual = default!;

    [DataField("config")]
    public DynamicParallaxLayerConfig Config = default!;
}

// Config for layer position
[DataDefinition]
public sealed partial class DynamicParallaxLayerConfig
{
    [DataField("scale")]
    public Vector2 Scale = Vector2.One;

    [DataField("tiled")]
    public bool Tiled = false;

    [DataField("worldPosition")]
    public Vector2 WorldHomePosition;

    [DataField("slowness")]
    public float Slowness = 0.5f;

    [DataField("shader")]
    public string? Shader = "unshaded";
}

// We dont know what types of layers we will have in future
public struct RenderedDynamicParallaxLayer
{
    public Texture Texture { get; set; }

    public DynamicParallaxLayerConfig Config { get; set; }
}
