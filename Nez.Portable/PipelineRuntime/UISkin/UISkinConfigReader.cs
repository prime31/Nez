using System;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class UISkinConfigReader : ContentTypeReader<UISkinConfig>
	{
		protected override UISkinConfig Read( ContentReader reader, UISkinConfig existingInstance )
		{
			var skinConfig = new UISkinConfig();

			if( reader.ReadBoolean() )
				skinConfig.colors = reader.ReadObject<Dictionary<string,Color>>();

			if( reader.ReadBoolean() )
				skinConfig.textureAtlases = reader.ReadObject<string[]>();

			if( reader.ReadBoolean() )
				skinConfig.libGdxAtlases = reader.ReadObject<string[]>();

			if( reader.ReadBoolean() )
				skinConfig.styles = reader.ReadObject<UISkinStyleConfig>();

			return skinConfig;
		}
	}
}

