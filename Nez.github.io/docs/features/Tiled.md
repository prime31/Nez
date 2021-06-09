---
id: Tiled
title: Tiled
---

TiledMaps are widely used in 2D game development. TiledMaps build up the game world from small images, called tiles. This leads to better performance and less memory usage. Because you don't need large-scale image files to render your map.

# Tools

For creating TiledMaps, I recommend [Tiled](https://www.mapeditor.org/).
Tiled is a free and open-source, easy-to-use, and flexible Tile editor.

For drawing tilesets, you can use the following tools.

| **Tool**    | **Price** |
|-------------|-----------|
| [Aseprite](https://www.aseprite.org/) | Paid (unless you compile it yourself) |
| [Libresprite](https://libresprite.github.io/) | Free |
| [Pyxel Edit](https://pyxeledit.com/) | Free |
| [Krita](https://krita.org/en/) | Free |

:::info
The assets from this example can be downloaded [here](../assets/tiledmap_assets.zip).
:::

# Example

## Loading

You can load a TiledMap using the `Content.LoadTiledMap` method.
```cs
var map = Content.LoadTiledMap("Content/TiledMapDocumentation/samplemap.tmx");
```	

## Rendering

For rendering the tiledmap, you can use the `TiledMapRenderer` component.
```cs
var tiledEntity = CreateEntity("tiled-map");
tiledEntity.AddComponent(new TiledMapRenderer(map));
```

By default the `TiledMapRenderer` renders all layers, you can set these yourself with `SetLayersToRender`.

```cs
tiledMapRenderer.SetLayersToRender("ground", "grass");
```

`SetLayersToRender` does not adjust the render order. For that, you need to use multiple `TiledMapRenderers` with different RenderLayers

### Materials

You can also use `Materials` with `TiledMapRenderer`. Check out the [Stencil docs](Graphics/Stencil.md) on how to add stencil shadows to your TiledMap.

## Camera

Adding a camera is very simple. First we need to define the boundaries of our camera, using the tiledmap size.

```cs
var topLeft = Vector2.Zero;
var bottomRight = new Vector2(
    map.TileWidth * map.Width, 
    map.TileWidth * map.Height);
tiledEntity.AddComponent(new CameraBounds(topLeft, bottomRight));
```

Finaly we add an entity that will be tracked by our camera.
```cs
Entity character = CreateEntity("character");
character.AddComponent(new PrototypeSpriteRenderer(25, 25));
character.AddComponent(new SimpleMover());
```

```cs
Camera.Entity.AddComponent(new FollowCamera(character));
```

## Collision

Collision is very easy to add by assigning a TiledMap layer as a collision layer. All non-empty tiles in this layer will then act as colliders.

For convenience, I'll use the water layer. Since the water layer also has non-empty tiles under the bridges, we can unfortunately no longer use them.
```cs
var tiledMapRenderer = tiledEntity.AddComponent(new TiledMapRenderer(map, "water"));
```

And we wrap this up by adding a `BoxCollider` to our `Character`.
```cs
character.AddComponent(new BoxCollider(25, 25));
```