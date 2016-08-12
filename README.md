![Nez](FAQs/images/nez-logo-black.png)

Nez aims to be a lightweight 2D framework that sits on top of MonoGame/FNA. It provides a solid base for you to build a 2D game on. Some of the many features it includes are:

- Scene/Entity/Component system with Component render layer tracking and optional entity systems (an implementation that operates on a group of entities that share a specific set of components)
- SpatialHash for super fast broadphase physics lookups. You won't ever see it since it works behind the scenes but you'll love it nonetheless since it makes finding everything in your proximity crazy fast. (note that Nez does not provide any physics engine. It provides collision data that you can do whatever you want to with and some example Components showing how to implement things: Mover and ArcadeRigidBody).
- AABB, circle and polygon collision/trigger detection along with linecasts against the SpatialHash
- efficient coroutines for breaking up large tasks across multiple frames or animation timing (Core.startCoroutine)
- in-game debug console extendable by adding an attribute to any static method. Just press the tilde key like in the old days with Quake. Out of the box, it includes a visual physics debugging system, asset tracker, basic profiler and more. Just type 'help' to see all the commands or type 'help COMMAND' to see specific hints.
- in-game Component inspector. Open the debug console and use the command `inspect entity_name` to display and edit fields/properties and call methods with a button click.
- extensible rendering system. Add/remove renderers and post processors as needed. Renderables are sorted by render layer first then layer depth for maximum flexibility out of the box.
- pathfinding support via Astar and Breadth First Search
- deferred lighting engine with normal map support and both runtime and offline normal map generation
- tween system. Tween any int/float/Vector/quaternion/color/rectangle field or property.
- sprites with sprite animations, scrolling sprites, repeating sprites and sprite trails
- kick-ass particle system with added support for importing [Particle Designer](https://71squared.com/particledesigner) files
- optimized event emitter for core events that you can also add to any class of your own
- scheduler for delayed and repeating tasks (`Core.schedule` method)
- per-scene content managers. Load your scene-specific content then forget about it. Nez will unload it for you when you change scenes.
- synchronous or asynchronous asset loading
- customizable Scene transition system with several built in transitions
- tons more stuff


Samples Repository
==========
You can find the samples repo [here](https://github.com/prime31/Nez-Samples). It contains a variety of sample scenes that demonstrate the basics of getting stuff done with Nez.



Nez Systems
==========
There are various systems documented separately on the [Nez website docs.](http://prime31.github.io/Nez/documentation/setup/installation)


Setup
==========
### Quick version:

- clone or download the Nez repository
- add the `Nez-PCL/Nez.csproj` project to your solution and add a reference to it in your main project
- make your main Game class (`Game1.cs` in a default project) subclass `Nez.Core`


### (optional) Pipeline Tool setup for access to the Nez Pipeline importers

- add the `Nez.PipelineImporter/Nez.PipelineImporter.csproj` project to your solution
- open the `Nez.PipelineImporter` references dialog and add a reference to the Nez project
- build the `Nez.PipelineImporter` project to generate the DLLs
- open the Pipeline Tool by double-clicking your `Content.mgcb` file and add references to `PipelineImporter.dll`, `Ionic.ZLib.dll`, `Newtonsoft.Json.dll` and `Nez.dll`.


All Nez shaders are compiled for OpenGL so be sure to use the DesktopGL template, not DirectX! Nez only supports OpenGL out of the box to keep things compatible across Android/iOS/Mac/Linux/Windows.

If you intend to use any of the built in Effects or PostProcessors you should also copy or link the DefaultContent/effects folder into your projects Content/nez/effects folder. Be sure to set the Build Action to Content and enable the "Copy to output directory" property so they get copied into your compiled game.

If you are developing a mobile application you will need to enable touch input by calling `Input.touch.enableTouchSupport()`.




Using Nez with FNA
==========
See the [Nez.FNA repo](https://github.com/prime31/Nez.FNA) for details.



Tutorials
==========
[The wiki](https://github.com/prime31/Nez/wiki) contains a few basic tutorials littered with code snippets that should be enough to get you rolling your own games. If you have a suggestion for a new tutorial feel free to open an issue with the details.



Pipeline Importers
==========
Nez comes stock with a decent bunch of Pipeline tool importers including:

- **Texture Atlas Generator**: give it a directory or a list of files and it will combine them all into a single atlas and provide easy access to the source images at runtime. Supports nine patch sprites as well in the [Android style](http://developer.android.com/tools/help/draw9patch.html) (single pixel border with black lines representing the patches). See also [this generator](https://romannurik.github.io/AndroidAssetStudio/nine-patches.html). The Texture Atlas Generator also includes a per-folder sprite animation generation. The atlas generator uses an XML file as input with an Asset Type of System.String[]. The string array should specify the folder or folders where the source images are located.
- **Tiled**: import [Tiled](http://www.mapeditor.org/) maps. Covers tile, image and object layers and rendering with full culling support built-in along with optimized collider generation.
- **Bitmap Fonts**: imports BMFont files (from programs like [Glyph Designer](https://71squared.com/glyphdesigner), [Littera](http://kvazars.com/littera/), etc). Outputs a single xnb file and includes SpriteBatch extension methods to display text the directly match the SpriteFont methods.
- **Particle Designer Importer**: imports [Particle Designer](https://71squared.com/particledesigner) particle systems for use with the Nez particle system
- **LibGdxAtlases**: imports libGDX texture atlases including nine patch support
- **Texture Packer**: imports a [TexturePacker](https://www.codeandweb.com/texturepacker) atlas and JSON file
- **Overlap2D**: imports [Overlap2D](http://overlap2d.com/) projects. Imports most of the data but currently only offers renderers for the basics (no fancy stuff like Spriter animations, lights, etc).
- **UISkin Importer**: imports uiskin files (JSON format) that are converted to UISkins. See the [UI page](FAQs/UI.md) for an example and details of the JSON format.
- **Normal Map Generator**: generates normal maps from standard textures
- **XMLTemplateMaker**: this isn't so much an importer as a helper to make your own importer. Pass it a class and it spits out an XML template that you can use for your own custom XML-to-object importers.



### Acknowledgements/Attributions
Bits and pieces of Nez were cherry-picked from various places around the internet. If you see something in Nez that looks familiar open an issue with the details so that we can properly attribute the code.

I want to extend a special thanks to the three people and their repos listed below. The Monocle Engine and MonoGame.Extended allowed me to get up and running with MonoGame nearly instantly when I was first evaluating if it would be a good alternative to use for making games. [libGDX](https://github.com/libgdx/libgdx) scene2D UI was ported over to Nez to get a jump start on a UI as well. Nez uses a bunch of concepts and code from all three of these repos.

Matt Thorson's fantastic [Monocle Engine](https://bitbucket.org/MattThorson/monocle-engine)

Dylan Wilson's excellent [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)

Nathan Sweet's libGDX Scene2D UI [libGDX](https://github.com/libgdx/libgdx). Nez UI is based on libGDX Scene2D which is [Apache licensed](UI_LICENSE).

