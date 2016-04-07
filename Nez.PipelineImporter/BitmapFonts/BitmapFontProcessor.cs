using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Nez.PipelineImporter;


namespace Nez.BitmapFontImporter
{
	[ContentProcessor( DisplayName = "BMFont Processor" )]
	public class BitmapFontProcessor : ContentProcessor<BitmapFontFile,BitmapFontProcessorResult>
	{
		[Description( "If true, the textures will be packed into the xnb, else you will need to process them separately" )]
		public bool packTexturesIntoXnb { get; set; } = true;

		[Description( "Enable/disable texture compression" )]
		public bool compressTextures { get; set; }


		public override BitmapFontProcessorResult Process( BitmapFontFile bitmapFontFile, ContentProcessorContext context )
		{
			try
			{
				context.Logger.LogMessage( "Processing BMFont, Kerning pairs: {0}", bitmapFontFile.kernings.Count );
				var result = new BitmapFontProcessorResult( bitmapFontFile );
				result.packTexturesIntoXnb = packTexturesIntoXnb;

				foreach( var fontPage in bitmapFontFile.pages )
				{
					// find our texture so we can embed it in the xnb asset
					var imageFile = Path.Combine( Path.GetDirectoryName( bitmapFontFile.file ), fontPage.file );
					context.Logger.LogMessage( "looking for image file: {0}", imageFile );

					if( !File.Exists( imageFile ) )
						throw new Exception( string.Format( "Could not locate font atlas file {0} relative to folder {1}", fontPage.file, Directory.GetCurrentDirectory() ) );

					if( packTexturesIntoXnb )
					{
						context.Logger.LogMessage( "Found texture: {0}. Packing into xnb", imageFile );
						var textureReference = new ExternalReference<TextureContent>( imageFile );
						var texture = context.BuildAndLoadAsset<TextureContent,TextureContent>( textureReference, "TextureProcessor" );

						var textureContent = new Texture2DContent();
						textureContent.Mipmaps.Add( texture.Faces[0][0] );

						if( compressTextures )
							textureContent.ConvertBitmapType( typeof( Dxt5BitmapContent ) );

						result.textures.Add( textureContent );
					}
					else
					{
						var textureFilename = PathHelper.makeRelativePath( Directory.GetCurrentDirectory(), imageFile ).Replace( "Content/", string.Empty );
						textureFilename = Path.ChangeExtension( textureFilename, null );
						context.Logger.LogMessage( "Not writing texture but it is expected to exist and be processed: {0}", textureFilename );
						result.textureNames.Add( textureFilename );
						result.textureOrigins.Add( new Vector2( fontPage.x, fontPage.y ) );
					}
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