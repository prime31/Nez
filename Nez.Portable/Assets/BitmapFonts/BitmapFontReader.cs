using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;


namespace Nez.BitmapFonts
{
    /// <summary>
    /// Legacy MonoGame Pipeline reader. This exists as legacy support to load the embedded Nez default font into the
    /// new runtime BitmapFont class
    /// </summary>
    public class BitmapFontReader : ContentTypeReader<BitmapFont>
	{
		protected override BitmapFont Read(ContentReader reader, BitmapFont existingInstance)
		{
            var hasEmbeddedTextures = reader.ReadBoolean();
            Texture2D[] textures;
            if (hasEmbeddedTextures)
            {
                var totalTextures = reader.ReadInt32();
                textures = new Texture2D[totalTextures];
                for (var i = 0; i < totalTextures; i++)
                    textures[i] = reader.ReadObject<Texture2D>();
            }
            else
            {
                var totalTextureNames = reader.ReadInt32();
                Vector2[] atlasOrigins = new Vector2[totalTextureNames];
                textures = new Texture2D[totalTextureNames];
                for (var i = 0; i < totalTextureNames; i++)
                {
                    var textureName = reader.ReadString();
                    atlasOrigins[i].X = reader.ReadSingle();
                    atlasOrigins[i].Y = reader.ReadSingle();
                    textures[i] = reader.ContentManager.Load<Texture2D>(textureName);
                }
            }

            var lineHeight = reader.ReadInt32();
			var padTop = reader.ReadInt32();
			var padLeft = reader.ReadInt32();
			var padBottom = reader.ReadInt32();
			var padRight = reader.ReadInt32();
			reader.ReadInt32(); // was descent in old style format

			var regionCount = reader.ReadInt32();
			var characters = new Dictionary<char, Character>();
			for (var r = 0; r < regionCount; r++)
			{
				var character = new Character();

				character.Char = (char) reader.ReadInt32();
				character.TexturePage = reader.ReadInt32();
				character.Bounds.X = reader.ReadInt32();
				character.Bounds.Y = reader.ReadInt32();
				character.Bounds.Width = reader.ReadInt32();
				character.Bounds.Height = reader.ReadInt32();
				character.Offset.X = reader.ReadInt32();
				character.Offset.Y = reader.ReadInt32();
				character.XAdvance = reader.ReadInt32();

				characters[character.Char] = character;
			}

            var font = new BitmapFont
            {
                Kernings = new Dictionary<Kerning, int>(),
                Textures = textures,
                LineHeight = lineHeight,
                Padding = new Padding(padLeft, padTop, padRight, padBottom),
                Characters = characters
            };
            font.DefaultCharacter = font[' '];
			font._spaceWidth = font.DefaultCharacter.Bounds.Width + font.DefaultCharacter.XAdvance;

			return font;
		}
	}
}