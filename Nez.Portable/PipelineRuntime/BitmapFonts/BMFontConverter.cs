using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;
using Nez;

namespace Nez.BitmapFonts
{
    public class BMFontConverter
    {
        /// <summary>
        /// Converts a BitmapFont to a SriteFont
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static SpriteFont LoadSpriteFontFromBitmapFont(string filename)
        {
            var fontData = BitmapFontLoader.LoadFontFromFile(filename);
            if (fontData.pages.Length > 1)
                throw new Exception($"Found multiple textures in font file {filename}. Only single texture fonts are supported.");

            var texture = Texture2D.FromStream(Core.graphicsDevice, File.OpenRead(fontData.pages[0].filename));
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

            var characters = font.characters.Values.OrderBy(c => c.character);
            foreach (var character in characters)
            {
                var bounds = character.bounds;
                glyphBounds.Add(bounds);
                cropping.Add(new Rectangle(character.offset.X, character.offset.Y, bounds.Width, bounds.Height));
                chars.Add(character.character);
                kerning.Add(new Vector3(0, character.bounds.Width, character.xAdvance - character.bounds.Width));
            }

            var constructorInfo = typeof(SpriteFont).GetTypeInfo().DeclaredConstructors.First();
            var result = (SpriteFont)constructorInfo.Invoke(new object[]
            {
                texture, glyphBounds, cropping,
                chars, font.lineHeight, 0, kerning, ' '
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
				var subtexture = new Subtexture(texture, bounds);
				var region = new BitmapFonts.BitmapFontRegion(subtexture, (char)key, character.XOffset, character.YOffset, character.XAdvance);
				regions[index++] = region;
			}

			return new Nez.BitmapFonts.BitmapFont(regions, ttf.Height);
		}
#endif
    }
}
