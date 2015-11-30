using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Nez.BitmapFontImporter
{
	[ContentTypeWriter]
	public class BitmapFontWriter : ContentTypeWriter<BitmapFontProcessorResult>
	{
		protected override void Write( ContentWriter writer, BitmapFontProcessorResult result )
		{
			// write our textures
			writer.Write( result.textures.Count );
			foreach( var tex in result.textures )
				writer.WriteObject( tex );

			// write the font data
			var fontFile = result.fontFile;
			writer.Write( fontFile.common.lineHeight );
			writer.Write( fontFile.chars.Count );

			foreach( var c in fontFile.chars )
			{
				writer.Write( c.id );
				writer.Write( c.page );
				writer.Write( c.x );
				writer.Write( c.y );
				writer.Write( c.width );
				writer.Write( c.height );
				writer.Write( c.xOffset );
				writer.Write( c.yOffset );
				writer.Write( c.xAdvance );
			}
		}


		public override string GetRuntimeType( TargetPlatform targetPlatform )
		{
			return typeof( Nez.BitmapFonts.BitmapFont ).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader( TargetPlatform targetPlatform )
		{
			return typeof( Nez.BitmapFonts.BitmapFontReader ).AssemblyQualifiedName;
		}
	}
}