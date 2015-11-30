## Nez



### Setup

Nez needs to have the DefaultContent/Fonts/NezFont.xnb file placed in your projects Content folder.



### Pipeline Importers

Nez comes stock with a decent bunch of Pipeline tool importors including:

- **Texture Atlas Generator**: give it a directory or a list of files and it will combine them all into a single atlas and provide easy access to the source UVs of each image. Also includes a per-folder sprite animation generation.
- **Tiled**: import [Tiled](http://www.mapeditor.org/) maps. Covers tile, image and object layers and rendering with full culling support built in.
- **Bitmap Fonts**: imports BMFont files (from programs like [Glyph Designer](https://71squared.com/glyphdesigner), [Littera](http://kvazars.com/littera/), etc). Outputs a single xnb file and includes SpriteBatch extension methods to display text.
- **LibGdxAtlases**: imports libGDX texture atlases
- **Overlap2D**: imports [Overlap2D](http://overlap2d.com/) projects. Imports almost all of the data but currently only offers renderers for the basics (no fancy stuff like Spriter animations, lights, etc).
- **Texture Packer**: imports a [TexturePacker](https://www.codeandweb.com/texturepacker) atlas and JSON file
- **PartcleType XML Importer**: generates an XML template that is used for defining the details of a particle system. While useful on its own, this is also here to provide a concrete example of how to make your own XML-to-object importers. It is super simple being just a single line of code.
- **XMLTemplateMaker**: this isn't so much an imoporter as a helper to make your own importer (or to use the ParticleType Importer). Pass it a class and it spits out an XML template that you can use for your own custom XML-to-object importers.



### Acknowledgements/Attributions

Bits and pieces of Nez were cherry-picked from various places around the internet. If you see something in Nez that looks familiar open an issue with the details so that we can properly attribute the code.

I want to extend a special thanks to the two people and their repos listed below. The Monocle Engine and MonoGame.Extended repo allowed me to get up and running with MonoGame nearly instantly when I was first evaluating if it would be a good alternative to use for making games. Nez uses a bunch of concepts and code from both of these repos.

Matt Thorson's fantastic [Monocle Engine](https://bitbucket.org/MattThorson/monocle-engine)

Dylan Wilson's excellent [MonoGame.Extended](https://github.com/craftworkgames/MonoGame.Extended)