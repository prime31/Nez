---
layout: docs
permalink: /documentation/tutorials/controlling-rendering
---

Controlling Rendering
==========

First and foremost with regard to rendering is the type of filtering used for your textures. Nez has several subsystems (such as Renderers, Scenes and PostProcessors) that all need to know how you want your textures to look. Everything is configurable on a per object basis but you will want to set a default as well so you don't have to bother changing the SamplerState all over the place. `Core.defaultSamplerState` should be set before you create your first Scene. It defaults to SamplerState.PointClamp which is good for pixel art. If you are using high-def art then make sure you set it to SamplerState.LinearClamp so you don't get ugly results.

`RenderableComponent` subclasses are responsible for all objects that are rendered on screen. As we have already seen in the previous example, the `Sprite` class is one of these. It renders a Texture2D for you. There are other `RenderableComponent` subclasses included with Nez that you may wish to explore including `ScrollingSprite`, `TiledSprite`, `SpriteTrail` and `TrailRibbon`. There are some common properties on the `RenderableComponent` class that all of these contain that control how things are rendered.

In a 2D game one of the first things that comes up often is the order of rendering. We want some sprites to be rendered in front of others. Nez provides 2 different methods of sorting the render order of your sprites. The first is the `renderLayer` property. Lower render layers are in front and higher in the back. Renderables of the same `renderLayer` are then sorted by their `layerDepth`.


Materials
==========

Each `RenderableComponent` has an optional material field. The material lets you set per-renderable details such as the BlendState (Alpha by default), DepthStencilState (None by default), SamplerState (PointClamp by default) and Effect (null by default).



Sprite Atlases
==========

Nez has Pipeline Tool importers included for [TexturePacker](https://www.codeandweb.com/texturepacker), [LibGDX Atlases](https://github.com/libgdx/libgdx/wiki/Texture-packer) (highly recommended for UI since it handles nine slice textures. Tip: Overlap2D makes building atlases a snap.) and it has its own atlas generator (which also handles nine slice textures). There are 2 ways that you can use the atlas generator: you can list each image individually that you would like in the atlas or you can specify a root folder which will be traversed to locate the images (and create animations automatically for each folder).

Creating atlases is an important topic so lets take a look at examples of each method. For either one, the steps are the same:

- open your Content.mgcb file with the Pipeline tool
- right-click, choose "add new item" and choose the "xml content" type
- open the XML file and edit it as below

Add each image individually to the XML file.
```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:ns="Microsoft.Xna.Framework">
  <Asset Type="System.String[]">
      <Item>Images/TextureAtlas/background.png</Item>
      <Item>Images/TextureAtlas/tree.png</Item>
	  <Item>Images/TextureAtlas/Ninja_Idle_0.png</Item>
	  <Item>Images/TextureAtlas/Ninja_Idle_1.png</Item>
	  <Item>Images/TextureAtlas/Ninja_Idle_2.png</Item>
	  <Item>Images/TextureAtlas/Ninja_Idle_3.png</Item>
  </Asset>
</XnaContent>
```

Add the root folder for your images to the XML file. Each subfolder will have an animation setup automatically for you.
```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:ns="Microsoft.Xna.Framework">
  <Asset Type="System.String[]">
      <Item>Images/AnotherAtlas</Item>
  </Asset>
</XnaContent>
```

Now that we have our texture atlases generated lets get to work and use them. You can load individual images from the atlas or (if you added a folder to the XML file) you can load a sprite animation.

```cs
// load up the TextureAtlas that we generated with the Pipeline tool specifying individual files
var textureAtlas = scene.content.Load<TextureAtlas>( "AtlasImages" );

// fetch a Subtexture from the atlas. A Subtexture consists of the Texture2D and the rect on the Texture2D this particular image ended up
var subtexture = textureAtlas.getSubtexture( "Ninja_Idle_0" );

// now we can create an Entity and add a Sprite which knows how to render a Subtexture
var entity = scene.createEntity( "entity" );
entity.addComponent( new Sprite( subtexture ) );
```


```cs
// load up the TextureAtlas that we generated with the Pipeline tool specifying a folder
var textureAtlas = scene.content.Load<TextureAtlas>( "AtlasFolder" );

// fetch the hardLanding animation. This animation will consist of all the images that were in the hardLanding folder
var hardLandingAnimation = textureAtlas.getSpriteAnimation( "hardLanding" );

// create a SpriteT. SpriteT is like a normal Sprite except that it knows about animations. The type passed to it when creating it lets
// you decide how animations are identified. Enum or int are good options. In this case we chose int and we are identifying the
// hardLanding animation by the key 3.
var sprite = new Sprite<int>( 3, hardLandingAnimation );

// create an Entity and add the sprite to it
var entity = myScene.createEntity( "entity" );
entity.addComponent( sprite );

// play the animation using the key. We chose to use an int for this SpriteT and used 3 as the key for the animation.
sprite.play( 3 );
```