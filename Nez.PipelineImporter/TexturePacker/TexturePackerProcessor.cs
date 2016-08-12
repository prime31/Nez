﻿using System;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.TexturePackerImporter
{
	[ContentProcessor( DisplayName = "TexturePacker Processor" )]
	public class TexturePackerProcessor : ContentProcessor<TexturePackerFile, TexturePackerFile>
	{
		public static ContentBuildLogger logger;


		public override TexturePackerFile Process( TexturePackerFile input, ContentProcessorContext context )
		{
			logger = context.Logger;
			return input;
		}
	}
}