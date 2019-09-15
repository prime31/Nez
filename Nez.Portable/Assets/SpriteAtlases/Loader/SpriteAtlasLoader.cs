using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Nez.SpriteAtlases
{
	public static class SpriteAtlasLoader
	{
		static byte[] NewLine = Encoding.ASCII.GetBytes(Environment.NewLine);

		public static SpriteAtlasData ParseSpriteAtlasData(string dataFile)
		{
			var spriteAtlas = new SpriteAtlasData();

			using (var stream = TitleContainer.OpenStream(dataFile))
			{

			}
			return spriteAtlas;
		}

		public static SpriteAtlas ParseSpriteAtlas(string dataFile)
		{
			var spriteAtlas = ParseSpriteAtlasData(dataFile);
			using (var stream = TitleContainer.OpenStream(dataFile.Replace(".atlas", ".png")))
				return spriteAtlas.AsSpriteAtlas(Texture2D.FromStream(Core.GraphicsDevice, stream));
		}
	}
}
