using System.Collections.Generic;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.BitmapFontImporter
{
	public class BitmapFontProcessorResult
	{
		public List<Texture2DContent> Textures = new List<Texture2DContent>();
		public List<string> TextureNames = new List<string>();
		public List<Vector2> TextureOrigins = new List<Vector2>();
		public BitmapFontFile FontFile;
		public bool PackTexturesIntoXnb;


		public BitmapFontProcessorResult(BitmapFontFile fontFile)
		{
			this.FontFile = fontFile;
		}
	}
}