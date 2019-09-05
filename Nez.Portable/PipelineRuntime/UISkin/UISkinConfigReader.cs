using System;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class UISkinConfigReader : ContentTypeReader<UISkinConfig>
	{
		protected override UISkinConfig Read(ContentReader reader, UISkinConfig existingInstance)
		{
			var skinConfig = new UISkinConfig();

			if (reader.ReadBoolean())
				skinConfig.Colors = reader.ReadObject<Dictionary<string, Color>>();

			if (reader.ReadBoolean())
				skinConfig.TextureAtlases = reader.ReadObject<string[]>();

			if (reader.ReadBoolean())
				skinConfig.LibGdxAtlases = reader.ReadObject<string[]>();

			if (reader.ReadBoolean())
				skinConfig.Styles = reader.ReadObject<UISkinStyleConfig>();

			return skinConfig;
		}
	}
}