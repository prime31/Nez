Pipeline Importers
==========
Nez provides a plethora of Pipeline Tool importers out of the box. Importers take data such as Tiled maps and convert them into a binary format that is much faster and more efficient to use at runtime.



## Tiled Tilemaps
Imports [Tiled](http://www.mapeditor.org/) maps. Covers tile, image and object layers along with image collections. All custom properties are also imported. The `TiledMapComponent` can render with full culling support built-in along with optimized collider generation.



## BM Font
BMFont processing can be done in two different ways. In the importer settings there is a bool (`packTexturesIntoXnb`) to toggle how the processor handles the files. If true (the default), the texture will be packed right in with the BMFont data in a single xnb file. If false, the texture will not be packed in the xnb. Setting it to false lets you use a shared texture atlas that includes the font atlas. It requires an extra bit of setup. In the .fnt file locate the *pages* element. Each page needs to have the **file** element correctly set to point to your atlas image. Additionally, two new XMLelements need to be added manually: **x** and **y** indicating the top-left point in the atlas that the font texture is located.



## Overlap2D
Imports [Overlap2D](http://overlap2d.com/) projects. Imports most of the data but currently only offers renderers for the basics (no fancy stuff like Spriter animations, lights, etc). To use the importer the .atlas (atlas files should be renamed to packatlas.atlas to avoid xnb clashes with the pack.png file that Overlap2D exports) and any of your scene files (*.dt) should be processed by the Pipeline Tool. Your project.dt file must also be present (it can be in the same folder as your scene files or one folder up as it is in the default Overlap2D export) but it should not be processed. The importer will take care of converting layers to renderLayers and it will calculate layerDepths for all your objects. Composites and primitive shapes (polygons) will also be imported


## LibGDX Atlases
LibGDX atlases go hand-in-hand with Overlap2D or they can be used directly. You can even use Overlap2D to create your LibGDX atlas by just importing all your images and then using the File -> Export menu item. Note that Overlap2D (and the LibGDX atlas builder program) will export the files pack.atlas and pack.png. MonoGame requires that different types have different names since all imported files will have an .xnb extension. To avoid the name clash just rename pack.atlas to packatlas.atlas.



## Texture Atlas Generator
Give it a directory or a list of files and it will combine them all into a single atlas and provide easy access to the source images at runtime. Supports nine patch sprites as well in the [Android style](http://developer.android.com/tools/help/draw9patch.html) (single pixel border with black lines representing the patches). See also [this generator](https://romannurik.github.io/AndroidAssetStudio/nine-patches.html). The Texture Atlas Generator also includes a per-folder sprite animation generation. The atlas generator uses an XML file as input with an Asset Type of System.String[]. The string array should specify the folder or folders where the source images are located.



## UI Skin
Imports uiskin files which are JSON files that define the various styles for UI elements. See the [UI page](FAQs/UI.md) for details on the file format and an example.



## Particle Designer
Imports [Particle Designer](https://71squared.com/particledesigner) particle systems for use with the Nez particle system.



## Texture Packer
Imports [Texture Packer](https://www.codeandweb.com/texturepacker) json files for creating sprite atlases.



## Normal Map Generator
The Normal Map Generator uses the TextureImporter in the Pipeline Tool. Select NormalMapProcessor as the Processor to generate a normal map. Several options are available that affect how the normal map is generated. The order of operations is the following:

- (optional) flatten image to two colors (one for transparent and one for opaque: `opaqueColor` and `transparentColor` params. This is for generating rim lighting normal maps)
- (optional) blur in color or grayscale (`blurType` and `blurDeviation` params)
- generate normal map using sobel or 5 tap (both methods have invertX/invertY params. `useSobelFilter` uses the `sobelStrength` and the 5 tap method uses the `nonSobelBias` param)




## XMLTemplateMaker
This isn't so much an importer as a helper to make your own importer. It does not create any xnb files. The XML file passed to this processor should just be a System.string with the namespace.class of the type that you want a template for, like below:

```xml
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:ns="Microsoft.Xna.Framework">
  <Asset Type="System.string">MyNamespace.MyClass</Asset>
</XnaContent>
```

The template will be dumped to the Pipeline console but note that it will have utf-16 instead of utf-8 so you need to change that. Copy/paste the template XML into a new XML file and change the values as required. This is the XML file that you will be adding to the Pipeline tool to actually create your xnb file.

You will first need to create your own super simple `ContentProcessor` to handle converting the XML into an xnb:

```csharp
[ContentProcessor( DisplayName = "XML File Processor" )]
public class MyClassProcessor : ContentProcessor<MyClass,MyClass>
{
	public override ParticleType Process( MyClass input, ContentProcessorContext context )
	{
		return input;
	}
}
```

With that all set up, add the XML file to the Pipeline tool and choose the XML File Processor that you just made in Settings -> Processor and you are all set.

You can then access the data at runtime like so: `var data = content.Load<MyClass>( "LocationOfXnbFile" );`

