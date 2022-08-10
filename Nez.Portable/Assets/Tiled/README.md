Tiled Map Integration
==========

Heavily modified fork of [TiledSharp](https://github.com/marshallward/TiledSharp)


Load and render a map:

```csharp
var map = new TmxMap();
map.LoadTmxMap("some_map.tmx");

// in Draw function:

Graphics.Instance.Batcher.Begin();
            

var layerDepth = 0;
var scale = new Vector2(1f);
var position = new Vector2(0f);

foreach (var layer in map.Layers)
{
    if (layer is TmxGroup tmxGroup)
        TiledRendering.RenderGroup(tmxGroup, Graphics.Instance.Batcher, position, scale, layerDepth);

    if (layer is TmxObjectGroup tmxObjGroup)
        TiledRendering.RenderObjectGroup(tmxObjGroup, Graphics.Instance.Batcher, position, scale, layerDepth);

    if (layer is TmxLayer tmxLayer)
        TiledRendering.RenderLayer(tmxLayer, Graphics.Instance.Batcher, position, scale, layerDepth);
    
    if (layer is TmxImageLayer tmxImageLayer)
        TiledRendering.RenderImageLayer(tmxImageLayer, Graphics.Instance.Batcher, position, scale, layerDepth);
}

Graphics.Instance.Batcher.End();
```
