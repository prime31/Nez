using System;
using System.IO;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;


namespace Nez.BitmapFontImporter
{
	[ContentProcessor( DisplayName = "BMFont Processor" )]
	public class BitmapFontProcessor : ContentProcessor<BitmapFontFile, BitmapFontProcessorResult>
	{
		public override BitmapFontProcessorResult Process( BitmapFontFile bitmapFontFile, ContentProcessorContext context )
		{
			try
			{
				context.Logger.LogMessage( "Processing BMFont" );
				var result = new BitmapFontProcessorResult( bitmapFontFile );

				foreach( var fontPage in bitmapFontFile.pages )
				{
					var assetName = Path.GetFileNameWithoutExtension( fontPage.file );
					context.Logger.LogMessage( "Expecting texture asset: {0}", assetName );
					result.textureAssets.Add( assetName );
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