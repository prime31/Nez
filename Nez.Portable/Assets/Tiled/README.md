Tiled Map Integration
==========

Heavily modified fork of [TiledSharp](https://github.com/marshallward/TiledSharp)


Load and render a map:

```csharp
var map = new TmxMap("some_map.tmx");

Graphics.instance.batcher.Begin();

var layerDepth = 0;
var scale = new float2(1f);

foreach (var layer in map.layers)
{
    if (layer is TmxGroup tmxGroup)
        TiledRendering.RenderGroup(tmxGroup, Graphics.instance.batcher, scale, layerDepth);

    if (layer is TmxObjectGroup tmxObjGroup)
        TiledRendering.RenderObjectGroup(tmxObjGroup, Graphics.instance.batcher, scale, layerDepth);

    if (layer is TmxLayer tmxLayer)
        TiledRendering.RenderLayer(tmxLayer, Graphics.instance.batcher, scale, layerDepth);
    
    if (layer is TmxImageLayer tmxImageLayer)
        TiledRendering.RenderImageLayer(tmxImageLayer, Graphics.instance.batcher, scale, layerDepth);
}

Graphics.instance.batcher.End();
```
