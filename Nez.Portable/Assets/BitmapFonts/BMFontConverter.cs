using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.BitmapFonts
{
    public static class BMFontConverter
	{
		/// <summary>
		/// Converts a BitmapFont to a SpriteFont
		/// </summary>
		/// <param name="filename"></param>
		/// <returns></returns>
		public static SpriteFont LoadSpriteFontFromBitmapFont(string filename)
		{
			var fontData = BitmapFontLoader.LoadFontFromFile(filename);
			if (fontData.Pages.Length > 1)
				throw new Exception(
					$"Found multiple textures in font file {filename}. Only single texture fonts are supported.");

			var texture = Texture2D.FromStream(Core.GraphicsDevice, File.OpenRead(fontData.Pages[0].Filename));
			return LoadSpriteFontFromBitmapFont(fontData, texture);
		}

		/// <summary>
		/// converts a BitmapFont to a SpriteFont
		/// </summary>
		/// <param name="font"></param>
		/// <param name="texture"></param>
		/// <returns></returns>
		public static SpriteFont LoadSpriteFontFromBitmapFont(BitmapFont font, Texture2D texture)
		{
			var glyphBounds = new List<Rectangle>();
			var cropping = new List<Rectangle>();
			var chars = new List<char>();
			var kerning = new List<Vector3>();

			var characters = font.Characters.Values.OrderBy(c => c.Char);
			foreach (var character in characters)
			{
				var bounds = character.Bounds;
				glyphBounds.Add(bounds);
				cropping.Add(new Rectangle(character.Offset.X, character.Offset.Y, bounds.Width, bounds.Height));
				chars.Add(character.Char);
				kerning.Add(new Vector3(0, character.Bounds.Width, character.XAdvance - character.Bounds.Width));
			}

			var constructorInfo = typeof(SpriteFont).GetTypeInfo().DeclaredConstructors.First();
			var result = (SpriteFont) constructorInfo.Invoke(new object[]
			{
				texture, glyphBounds, cropping,
				chars, font.LineHeight, 0, kerning, ' '
			});

			return result;
		}

#if SPRITEFONTPLUS
		public static Nez.BitmapFonts.BitmapFont NezBitmapFromBakedTTF(SpriteFontPlus.TtfFontBakerResult ttf)
		{
			var rgb = new Color[ttf.Width * ttf.Height];
			for (var i = 0; i < ttf.Pixels.Length; ++i)
			{
				var b = ttf.Pixels[i];
				rgb[i].R = b;
				rgb[i].G = b;
				rgb[i].B = b;

				rgb[i].A = b;
			}

			var texture = new Texture2D(Core.graphicsDevice, ttf.Width, ttf.Height);
			texture.SetData(rgb);

			var regions = new BitmapFonts.BitmapFontRegion[ttf.Glyphs.Count];
			var index = 0;
			var orderedKeys = ttf.Glyphs.Keys.OrderBy(a => a);
			foreach (var key in orderedKeys)
			{
				var character = ttf.Glyphs[key];
				var bounds = new Rectangle(character.X, character.Y,
					character.Width,
					character.Height);
				var sprite = new Sprite(texture, bounds);
				var region =
 new BitmapFonts.BitmapFontRegion(sprite, (char)key, character.XOffset, character.YOffset, character.XAdvance);
				regions[index++] = region;
			}

			return new Nez.BitmapFonts.BitmapFont(regions, ttf.Height);
		}
#endif
	}
}