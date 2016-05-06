## Nez

Nez aims to be a lightweight 2D framework that sits on top of MonoGame/FNA. It provides a solid base for you to build a 2D game on. Some of the many features it includes are:

- Scene/Entity/Component system with Component render layer tracking and optional entity systems (an implementation that operates on a group of entities that share a specific set of components)
- SpatialHash for super fast broadphase physics lookups. You won't ever see it since it works behind the scenes but you'll love it nonetheless since it makes finding everything in your proximity crazy fast. (note that Nez does not provide any physics engine. It provides collision data that you can do whatever you want to with and some example Components showing how to implement things: Mover and ArcadeRigidBody).
- AABB, circle and polygon collision/trigger detection along with linecasts against the SpatialHash
- efficient coroutines for breaking up large tasks across multiple frames or animation timing (Core.startCoroutine)
- in-game debug console extendable by adding an attribute to any static method. Just press the tilde key like in the old days with Quake. Out of the box, it includes a visual physics debugging system, asset tracker, basic profiler and more. Just type 'help' to see all the commands or type 'help COMMAND' to see specific hints.
- extensible rendering system. Add/remove renderers and post processors as needed.
- deferred lighting engine with normal map support
- tween system. Tween any int/float/Vector/quaternion/color/rectangle field or property.
- sprites with sprite animations, scrolling sprites and repeating sprites
- kick-ass particle system with added support for importing [Particle Designer](https://71squared.com/particledesigner) files
- optimized event emitter for core events that you can also add to any class of your own
- scheduler for delayed and repeating tasks (Core.schedule method)
- per-scene content managers. Load your scene-specific content then forget about it. Nez will unload it for you when you change scenes.
- synchronous or asynchronous asset loading
- customizeable Scene transition system with several built in transitions
- tons more stuff


Samples Repository
==========
You can find the samples repo [here](https://github.com/prime31/Nez-Samples). It contains a variety of sample scenes that demonstrate the basics of getting stuff done with Nez.



IMPORTANT: READ THIS FIRST
==========

Nez is a very young beast and should be considered pre-alpha. There will without a doubt be bugs present. Some things might not be implemented. Some things might be incomplete.



Nez Systems
==========

There are various systems documented separately in the FAQs folder. They go into a bit more detail on the different sub-systems that make up Nez.

- [Nez-Core](FAQs/Nez-Core.md)
- [Scene-Entity-Component](FAQs/Scene-Entity-Component.md)
- [Rendering](FAQs/Rendering.md)
- [Content Management](FAQs/ContentManagement.md)
- [Physics/Collisions](FAQs/Physics.md)
- [Entity Processing Systems](FAQs/EntitySystems.md)
- [Nez.UI](FAQs/UI.md)
- [AI (FSM, Behavior Tree, GOAP, Utility AI)](FAQs/AI.md)
- [Deferred Lighting](FAQs/DeferredLighting.md)
- [Pipeline Importers](FAQs/PipelineImporters.md)
- [Samples](FAQs/Samples.md)



Setup
==========

All Nez shaders are compiled for OpenGL. If you are on Windows make sure you start from a DesktopGL template, not DirectX! Nez only supports OpenGL out of the box to keep things compatible accross Android/iOS/Mac/Linux/Windows. Nez needs to have the DefaultContent/NezDefaultBMFont.xnb file placed in your project's Content/nez folder. It sets up the font as the default for all text in it's debug console and for your own use. If you intend to use any of the built in Effects or PostProcessors you should also copy the DefaultContent/effects folder contents into your projects Content/nez/effects folder. Be sure to set the Build Action to Content so they get copied into your compiled game. Your Game class should then subclass Core and call it's constructor.

If you are on Windows (or using MonoGame 3.5), there is one more thing you have to do. In your Game class constructor or Initialize method add the following line: `Window.ClientSizeChanged += Core.onClientSizeChanged;` This will wire up the window resize listener that Nez requires.



Using Nez with FNA
==========

Getting up and running with Nez and FNA requires some minor changes to be made to work with the default FNA install. Once you have your FNA project working the following steps should be taken:

- define the symbol "FNA" for both Debug and Release configurations
- create a new folder named "Nez" in your project and add all the files from the Nez-PCL folder to it
- (optional) if you use the included Nez Pipeline Tool FNA needs to be able to link up the correct assembly so in your projects configuration change the assembly name to "Nez"



Tutorials
==========

[The wiki](https://github.com/prime31/Nez/wiki) contains a few basic tutorials littered with code snippets that should be enough to get you rolling your own games. If you have a suggestion for a new tutorial feel free to open an Issue with the details. More quickie tutorials will be added in the future and a full repository of sample code will be uploaded also if all goes well.



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
- **XMLTemplateMaker**: this isn't so much an imoporter as a helper to make your own importer. Pass it a class and it spits out an XML template that you can use for your own custom XML-to-object importers.



### Acknowledgements/Attributions

Bits and pieces of Nez were cherry-picked from various places around the internet. If you see something in Nez that looks familiar open an issue with the details so that we can properly attribute the code.

I want to extend a special thanks to the three people and their repos listed below. The Monocle Engine and MonoGame.Extended allowed me to get up and running with MonoGame nearly instantly when I was first evaluating if it would be a good alternative to use for making games. [libGDX](https://github.com/libgdx/libgdx) scene2D UI was ported over to Nez to get a jump start on a UI as well. Nez uses a bunch of concepts and code from all three of these repos.

Matt Thorson's fantastic [Monocle Engine](https://bitbucket.org/MattThorson/monocle-engine)

Dylan Wilson's excellent [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)

Nathan Sweet's libGDX Scene2D UI [libGDX](https://github.com/libgdx/libgdx)
