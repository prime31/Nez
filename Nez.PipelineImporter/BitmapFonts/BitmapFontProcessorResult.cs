using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;


namespace Nez.BitmapFontImporter
{
	public class BitmapFontProcessorResult
	{
		public List<Texture2DContent> textures = new List<Texture2DContent>();
		public BitmapFontFile fontFile;


		public BitmapFontProcessorResult( BitmapFontFile fontFile )
		{
			this.fontFile = fontFile;
		}
	}
}