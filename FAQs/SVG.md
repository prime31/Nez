Nez SVG Support
==========
Nez includes some rudimentary support for importing SVG files. The Nez SVG features are *not* designed to make full SVG image viewers. The main purpose of the SVG support is to get at shapes such as rectangles and bezier paths. These can be used for level layout, AI paths or a variety of other purposes.

The SVG parser supports groups, paths, rects, lines, circles, ellipses, polygons, polylines and images. Images can be embedded, accessible via URL or in your Content folder. A debug renderer component is included as well for testing out SVG files. You can use it by just adding it to an Entity in your scene like so:

```csharp
var svgEntity = CreateEntity( "svg" );
svgEntity.AddComponent( new SvgDebugComponent( "mySvgFile.svg" ) );
```

The `SvgDebugComponent` is the best example of what shapes are available and how to access them. It should be considered the documentation until this page is fleshed out. Each of the supported shapes has its own render method in the class so that it is very easy to follow and see how to access the raw data from the SVG file.


## Making Paths Faster
If you are using paths at all the first thing you will want to do is not use the default `ISvgPathBuilder`. Nez includes a terribly slow implementation (also used by SvgDebugComponent) just so that things work out of the box. The reason for this is that PCLs cannot access System.Drawing so an ISvgPathBuilder that uses relection was made.

To get things working at the proper speed do the following:
- add a link in your main project to the Graphics/SVG/Shapes/Paths/SvgPathBuilder.cs file
- when you create your SvgDebugComponent pass an instance of the `SvgPathBuilder` as the second parameter
- enjoy 80x speed improvements creating bezier and other paths


## Parsing Paths Directly
You can access just paths from an SVG file as well without having to parse the whole file. To do so you will just need to fetch the contents of the 'd' attribute in a 'path' element from the SVG file. This lets you get at bezier and other path data quickly and easily. Sample code to do so is below:

```csharp
var svgPath = new SvgPath();
svgPath.d = "the contents of the 'd' attribute in the SVG file";
var points = svgPath.GetTransformedDrawingPoints( new SvgPathBuilder() );
```