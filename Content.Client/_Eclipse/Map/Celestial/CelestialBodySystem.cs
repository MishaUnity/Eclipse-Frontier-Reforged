using Content.Client._Eclipse.Parallax;
using Content.Shared._Eclipse.Map.Celestial;
using Content.Shared._Eclipse.Maps.Celestial;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Content.Client._Eclipse.Map.Celestial;

public sealed partial class CelestialBodySystem : SharedCelestialBodySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SpriteSystem _sprite = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GetDymanicParallaxLayersEvent>(OnGetParallaxLayers);
        SubscribeLocalEvent<CelestialBodyComponent, GetCelestialBodyLayersEvent>(OnGetBodyLayers);
    }

    private void OnGetParallaxLayers(ref GetDymanicParallaxLayersEvent args)
    {
        var query = EntityQueryEnumerator<CelestialBodyComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var _, out var transform))
        {
            if (transform.MapID != args.MapId)
                continue;

            var ev = new GetCelestialBodyLayersEvent();
            RaiseLocalEvent(uid, ref ev);

            foreach (var layer in ev.Layers)
                args.AddLayer(layer);
        }
    }

    private void OnGetBodyLayers(EntityUid uid, CelestialBodyComponent component, ref GetCelestialBodyLayersEvent args)
    {
        if (component.ParallaxPrototype == null ||
            !_prototype.TryIndex<DynamicParallaxPrototype>(component.ParallaxPrototype, out var parallax))
            return;

        foreach (var layer in parallax.Layers)
        {
            var config = layer.Config;
            config.WorldHomePosition += _transform.GetWorldPosition(uid);
            config.Rotation += _transform.GetWorldRotation(uid);

            args.AddLayer(new RenderedDynamicParallaxLayer()
            {
                Config = config,
                Texture = _sprite.GetFrame(layer.Visual, _timing.CurTime)
            });
        }
    }
}

/// <summary>
/// Rises on celestial body, when the system receives its visual
/// </summary>
[ByRefEvent]
public record struct GetCelestialBodyLayersEvent
{
    public List<RenderedDynamicParallaxLayer> Layers;

    public GetCelestialBodyLayersEvent()
    {
        Layers = new();
    }

    public void AddLayer(RenderedDynamicParallaxLayer layer, int zIndex = 0)
    {
        Layers.Add(layer);
    }
}
