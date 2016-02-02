Controlling Rendering
==========

`RenderableComponent` subclasses are responsible for all objects that are rendered on screen. As we have already seen in the previous example, the `Sprite` class is one of these. It renders a Texture2D for you. There are other `RenderableComponent` subclasses included with Nez that you may wish to explore including `ScrollingSprite`, `TiledSprite` and `SpriteTrail`. There are some common properties on the `RenderableComponent` class that all of these contain that control how things are rendered.

In a 2D game one of the first things that comes up is the order of rendering. We want some sprites to be rendered in front of others. Nez provides 2 different methods of sorting the render order of your sprites. The first is the `renderLayer` property. Lower render layers are in front and higher in the back. Renderables of the same `renderLayer` are then sorted by their `layerDepth`.


RenderState
==========

Each `RenderableComponent` has an optional renderState field. The renderState lets you set per-renderable details such as the BlendState (Alpha by default), DepthStencilState (None by default), SamplerState (PointClamp by default) and Effect (null by default).
