using System;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.Content.Pipeline.TextureAtlases
{
	[ContentProcessor( DisplayName = "TexturePacker Processor" )]
	public class TexturePackerProcessor : ContentProcessor<TexturePackerFile, TexturePackerProcessorResult>
	{
		public override TexturePackerProcessorResult Process( TexturePackerFile input, ContentProcessorContext context )
		{
			try
			{
				var output = new TexturePackerProcessorResult { Data = input };
				return output;
			}
			catch( Exception ex )
			{
				context.Logger.LogMessage( "Error {0}", ex );
				throw;
			}
		}
	}
}