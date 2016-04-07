using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using System;
using Nez.PipelineImporter;


namespace Nez.BitmapFontImporter
{
	[ContentTypeWriter]
	public class BitmapFontWriter : ContentTypeWriter<BitmapFontProcessorResult>
	{
		protected override void Write( ContentWriter writer, BitmapFontProcessorResult result )
		{
			writer.Write( result.packTexturesIntoXnb );

			// write our textures if we should else write the texture names
			if( result.packTexturesIntoXnb )
			{
				writer.Write( result.textures.Count );
				foreach( var tex in result.textures )
					writer.WriteObject( tex );
			}
			else
			{
				writer.Write( result.textureNames.Count );
				for( var i = 0; i < result.textureNames.Count; i++ )
				{
					writer.Write( result.textureNames[i] );
					writer.Write( result.textureOrigins[i] );
				}
			}

			// write the font data
			var fontFile = result.fontFile;
			writer.Write( fontFile.common.lineHeight );

			var padding = fontFile.info.padding.Split( new char[] { ',' } );
			if( padding.Length != 4 )
				throw new PipelineException( "font padding is invalid! It should contain 4 values" );

			writer.Write( int.Parse( padding[0] ) ); // top
			writer.Write( int.Parse( padding[1] ) ); // left
			writer.Write( int.Parse( padding[2] ) ); // bottom
			writer.Write( int.Parse( padding[3] ) ); // right

			writer.Write( getDescent( fontFile, int.Parse( padding[3] ) ) );

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


		int getDescent( BitmapFontFile fontFile, int padBottom )
		{
			var descent = 0;

			foreach( var c in fontFile.chars )
			{
				if( c.width > 0 && c.height > 0 )
					descent = Math.Min( fontFile.common.base_ + c.yOffset, descent );
			}

			return descent + padBottom;
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