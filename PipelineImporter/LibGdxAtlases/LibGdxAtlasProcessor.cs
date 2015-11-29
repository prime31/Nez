using System;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.LibGdxAtlases
{
	[ContentProcessor( DisplayName = "libGDX Atlas Processor" )]
	public class LibGdxAtlasProcessor : ContentProcessor<LibGdxAtlasFile, LibGdxAtlasProcessorResult>
	{
		public static ContentBuildLogger logger;

		public override LibGdxAtlasProcessorResult Process( LibGdxAtlasFile input, ContentProcessorContext context )
		{
			logger = context.Logger;
			try
			{
				var output = new LibGdxAtlasProcessorResult { Data = input };
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
