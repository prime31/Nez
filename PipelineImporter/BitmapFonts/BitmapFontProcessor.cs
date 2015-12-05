using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Nez.BitmapFontImporter
{
	[ContentProcessor( DisplayName = "BMFont Processor" )]
	public class BitmapFontProcessor : ContentProcessor<BitmapFontFile,BitmapFontProcessorResult>
	{
		bool _compress;
		[Description( "Enable/disable texture compression" )]
		[DefaultValue( false )]
		public bool CompressTexture
		{
			get { return _compress; }
			set { _compress = value; }
		}


		public override BitmapFontProcessorResult Process( BitmapFontFile bitmapFontFile, ContentProcessorContext context )
		{
			try
			{
				context.Logger.LogMessage( "Processing BMFont, Kerning pairs: {0}", bitmapFontFile.kernings.Count );
				var result = new BitmapFontProcessorResult( bitmapFontFile );

				foreach( var fontPage in bitmapFontFile.pages )
				{
					var assetName = Path.GetFileNameWithoutExtension( fontPage.file );
					context.Logger.LogMessage( "Expecting texture asset: {0}", assetName );

					// find our texture so we can embed it in the xnb asset
					var imageFiles = Directory.GetFiles( ".", fontPage.file, SearchOption.AllDirectories );
					if( imageFiles.Length == 0 )
						throw new Exception( string.Format( "Could not locate font atlas file {0} in any subdirectory of {1}", fontPage.file, Directory.GetCurrentDirectory() ) );
						
					context.Logger.LogMessage( "Found texture: {0}", imageFiles[0] );
					var textureReference = new ExternalReference<TextureContent>( imageFiles[0] );
					var texture = context.BuildAndLoadAsset<TextureContent,TextureContent>( textureReference, "TextureProcessor" );

					var textureContent = new Texture2DContent();
					textureContent.Mipmaps.Add( texture.Faces[0][0] );

					if( _compress )
						textureContent.ConvertBitmapType( typeof( Dxt5BitmapContent ) );
					
					result.textures.Add( textureContent );
				}

				return result;

			}
			catch( Exception ex )
			{
				context.Logger.LogMessage( "Error {0}", ex );
				throw;
			}
		}
	}
}