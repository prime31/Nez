using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Nez.BitmapFonts
{
	/// <summary>
	/// Legacy MonoGame Pipeline reader. This only exists as legacy support to load the embedded Nez default font into the
	/// new runtime BitmapFont class
	/// </summary>
	/// <typeparam name="BitmapFont"></typeparam>
	public class BitmapFontReader : ContentTypeReader<BitmapFont>
	{
		protected override BitmapFont Read(ContentReader reader, BitmapFont existingInstance)
		{
			Texture2D[] textures = null;
			Vector2[] atlasOrigins = null;

			var hasEmbeddedTextures = reader.ReadBoolean();
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
				atlasOrigins = new Vector2[totalTextureNames];
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
			var descent = reader.ReadInt32();

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

				// Subtexture textureRegion = null;
				// if (hasEmbeddedTextures)
				// 	textureRegion = new Subtexture(textures[textureIndex], x, y, width, height);
				// else
				// 	textureRegion = new Subtexture(textures[textureIndex], atlasOrigins[textureIndex].x + x, atlasOrigins[textureIndex].y + y, width, height);

				characters[character.Char] = character;
			}

			var font = new BitmapFont();
			font.Kernings = new Dictionary<Kerning, int>();
			font.Textures = textures;
			font.LineHeight = lineHeight;
			font.Padding = new Padding(padLeft, padTop, padRight, padBottom);
			font.Characters = characters;
			font.DefaultCharacter = font[' '];
			font._spaceWidth = font.DefaultCharacter.Bounds.Width + font.DefaultCharacter.XAdvance;

			return font;
		}
	}
}