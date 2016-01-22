## Nez

Nez aims to be a lightweight 2D game engine that sits on top of MonoGame/FNA. It provides a solid base for you to build a 2D game on. Some of the many features it includes are:

- Scene/Entity/Component system with automatic Entity tag tracking and Component render layer tracking
- SpatialHash for super fast broadphase physics lookups (note that Nez does not provide any physics engine. It provides collision data that you can do whatever you want to with it.)
- AABB, polygon and circle collision/trigger detection along with raycasts against the SpatialHash
- efficient coroutines for breaking up large tasks across multiple frames or animation timing
- in-game debug console extendable by adding an attribute to any static method
- extensible rendering system. Add/remove renderers and post processors as needed.
- tweening system. Tween any int/float/Vector/quaternion/color/rectangle field or property
- sprites with sprite animations
- kick-ass particle system with added support for importing Particle Designer files
- optimized event emitter for core events that you can also add to any class of your own
- scheduler for delayed and repeating tasks
- synchronous or asynchronous asset loading
- tons more stuff



### Setup

Nez needs to have the DefaultContent/Fonts/NezDefaultBMFont.xnb file placed in your projects Content folder. It sets up the font as the default for all text in it's debug console and for your own use.


### Systems

There are various systems documented separately in the FAQs folder. They go into a bit more detail on the different sub-systems that make up Nez.

- [Nez-Core](FAQs/Nez-Core.md)
- [Scene-Entity-Component](FAQs/Scene-Entity-Component.md)
- [Rendering](FAQs/Rendering.md)
- [Physics/Collisions](FAQs/Physics.md)
- [Entity System](FAQs/Systems.md)
- [Samples](FAQs/Samples.md)


### Pipeline Importers

Nez comes stock with a decent bunch of Pipeline tool importers including:

- **Texture Atlas Generator**: give it a directory or a list of files and it will combine them all into a single atlas and provide easy access to the source UVs of each image. Also includes a per-folder sprite animation generation.
- **Tiled**: import [Tiled](http://www.mapeditor.org/) maps. Covers tile, image and object layers and rendering with full culling support built-in along with optimized collider generation.
- **Bitmap Fonts**: imports BMFont files (from programs like [Glyph Designer](https://71squared.com/glyphdesigner), [Littera](http://kvazars.com/littera/), etc). Outputs a single xnb file and includes SpriteBatch extension methods to display text the directly match the SpriteFont methods.
- **LibGdxAtlases**: imports libGDX texture atlases
- **Overlap2D**: imports [Overlap2D](http://overlap2d.com/) projects. Imports almost all of the data but currently only offers renderers for the basics (no fancy stuff like Spriter animations, lights, etc).
- **Texture Packer**: imports a [TexturePacker](https://www.codeandweb.com/texturepacker) atlas and JSON file
- **Particle Designer Importer**: imports [Particle Designer](https://71squared.com/particledesigner) particle systems for use with the Nez particle system
- **XMLTemplateMaker**: this isn't so much an imoporter as a helper to make your own importer. Pass it a class and it spits out an XML template that you can use for your own custom XML-to-object importers.



### Acknowledgements/Attributions

Bits and pieces of Nez were cherry-picked from various places around the internet. If you see something in Nez that looks familiar open an issue with the details so that we can properly attribute the code.

I want to extend a special thanks to the two people and their repos listed below. The Monocle Engine and MonoGame.Extended repo allowed me to get up and running with MonoGame nearly instantly when I was first evaluating if it would be a good alternative to use for making games. Nez uses a bunch of concepts and code from both of these repos.

Matt Thorson's fantastic [Monocle Engine](https://bitbucket.org/MattThorson/monocle-engine)

Dylan Wilson's excellent [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)