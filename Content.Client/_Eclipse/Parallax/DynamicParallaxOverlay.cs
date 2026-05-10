using Content.Client.Parallax;
using Content.Client.Parallax.Managers;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Client._Eclipse.Parallax;

public sealed class DynamicParallaxOverlay : Overlay
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IParallaxManager _parallax = default!;

    private readonly DynamicParallaxSystem _dynamic;
    private readonly MapSystem _map;

    public override OverlaySpace Space => OverlaySpace.WorldSpaceBelowWorld;

    public DynamicParallaxOverlay(DynamicParallaxSystem dynamic, MapSystem map)
    {
        ZIndex = ParallaxSystem.ParallaxZIndex + 1;
        IoCManager.InjectDependencies(this);

        _map = map;
        _dynamic = dynamic;
    }

    protected override void Draw(in OverlayDrawArgs args)
    {
        var mapUid = _map.GetMapOrInvalid(args.MapId);
        var invMatrix = args.Viewport.GetWorldToLocalMatrix();

        var position = args.Viewport.Eye?.Position.Position ?? Vector2.Zero;
        var worldHandle = args.WorldHandle;

        var layers = _dynamic.GetLayers(args.MapId);

        foreach (var layer in layers)
        {
            ShaderInstance? shader;

            if (!string.IsNullOrEmpty(layer.Config.Shader))
                shader = _prototypeManager.Index<ShaderPrototype>(layer.Config.Shader).Instance();
            else
                shader = null;

            worldHandle.UseShader(shader);
            var tex = layer.Texture;

            var size = (tex.Size / (float)EyeManager.PixelsPerMeter) * layer.Config.Scale;
            var home = layer.Config.WorldHomePosition + _parallax.ParallaxAnchor;

            var originBL = (position - home) * layer.Config.Slowness;
            originBL += home;
            originBL -= size / 2;

            if (layer.Config.Tiled)
            {
                var flooredBL = args.WorldAABB.BottomLeft - originBL;
                flooredBL = (flooredBL / size).Floored() * size;
                flooredBL += originBL;

                for (var x = flooredBL.X; x < args.WorldAABB.Right; x += size.X)
                {
                    for (var y = flooredBL.Y; y < args.WorldAABB.Top; y += size.Y)
                    {
                        var box = new Box2Rotated(Box2.FromDimensions(new Vector2(x, y), size), layer.Config.Rotation, new Vector2(x, y));
                        worldHandle.DrawTextureRect(tex, box);
                    }
                }
            }
            else
            {
                var box = new Box2Rotated(Box2.FromDimensions(originBL, size), layer.Config.Rotation, originBL + size / 2);
                worldHandle.DrawTextureRect(tex, box);
            }
        }

        worldHandle.UseShader(null);
    }
}
