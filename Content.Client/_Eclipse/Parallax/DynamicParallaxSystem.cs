using Content.Client.Parallax;
using Content.Shared._Eclipse.Parallax;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Client._Eclipse.Parallax;

public sealed partial class DynamicParallaxSystem : EntitySystem
{
    [Dependency] private readonly IOverlayManager _overlay = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ParallaxSystem _parallax = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;

    public override void Initialize()
    {
        base.Initialize();
        _overlay.AddOverlay(new DynamicParallaxOverlay(this, _map));

        SubscribeLocalEvent<GetDymanicParallaxLayersEvent>(OnGetLayers);
    }

    public List<RenderedDynamicParallaxLayer> GetLayers(MapId map)
    {
        var ev = new GetDymanicParallaxLayersEvent(map);

        _map.GetMap(map);
        RaiseLocalEvent(ref ev);

        return ev.Layers;
    }

    private void OnGetLayers(ref GetDymanicParallaxLayersEvent args)
    {
        var mapUid = _map.GetMap(args.MapId);
        if (!TryComp<DynamicParallaxComponent>(mapUid, out var component))
            return;

        var prototype = _prototype.Index<DynamicParallaxPrototype>(component.Parallax);
        foreach (var layer in prototype.Layers)
        {
            args.AddLayer(new RenderedDynamicParallaxLayer()
            {
                Config = layer.Config,
                Texture = _sprite.GetFrame(layer.Visual, _timing.CurTime)
            });
        }
    }
}

[ByRefEvent]
public record struct GetDymanicParallaxLayersEvent
{
    public List<RenderedDynamicParallaxLayer> Layers;
    public MapId MapId;

    public GetDymanicParallaxLayersEvent(MapId mapId)
    {
        Layers = new();
        MapId = mapId;
    }

    public void AddLayer(RenderedDynamicParallaxLayer layer)
    {
        Layers.Add(layer);
    }
}
