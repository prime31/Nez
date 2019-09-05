using System;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.LibGdxAtlases
{
	[ContentProcessor(DisplayName = "libGDX Atlas Processor")]
	public class LibGdxAtlasProcessor : ContentProcessor<LibGdxAtlasFile, LibGdxAtlasProcessorResult>
	{
		public static ContentBuildLogger Logger;

		public override LibGdxAtlasProcessorResult Process(LibGdxAtlasFile input, ContentProcessorContext context)
		{
			Logger = context.Logger;
			return new LibGdxAtlasProcessorResult {Data = input};
		}
	}
}