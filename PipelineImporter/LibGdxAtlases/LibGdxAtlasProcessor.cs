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
			return new LibGdxAtlasProcessorResult { data = input };
		}
	}
}
