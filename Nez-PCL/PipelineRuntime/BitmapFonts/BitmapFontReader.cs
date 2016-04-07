using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Content;
using Nez.Textures;
using Microsoft.Xna.Framework;


namespace Nez.BitmapFonts
{
	public class BitmapFontReader : ContentTypeReader<BitmapFont>
	{
		protected override BitmapFont Read( ContentReader reader, BitmapFont existingInstance )
		{
			Texture2D[] textures = null;
			Vector2[] atlasOrigins = null;

			var hasTextures = reader.ReadBoolean();
			if( hasTextures )
			{
				var totalTextures = reader.ReadInt32();
				textures = new Texture2D[totalTextures];
				for( var i = 0; i < totalTextures; i++ )
					textures[i] = reader.ReadObject<Texture2D>();
			}
			else
			{
				var totalTextureNames = reader.ReadInt32();
				atlasOrigins = new Vector2[totalTextureNames];
				textures = new Texture2D[totalTextureNames];
				for( var i = 0; i < totalTextureNames; i++ )
				{
					var textureName = reader.ReadString();
					atlasOrigins[i] = reader.ReadVector2();
					textures[i] = reader.ContentManager.Load<Texture2D>( textureName );
				}
			}

			var lineHeight = reader.ReadInt32();
			var padTop = reader.ReadInt32();
			var padLeft = reader.ReadInt32();
			var padBottom = reader.ReadInt32();
			var padRight = reader.ReadInt32();
			var descent = reader.ReadInt32();

			var regionCount = reader.ReadInt32();
			var regions = new BitmapFontRegion[regionCount];
			for( var r = 0; r < regionCount; r++ )
			{
				var character = (char)reader.ReadInt32();
				var textureIndex = reader.ReadInt32();
				var x = reader.ReadInt32();
				var y = reader.ReadInt32();
				var width = reader.ReadInt32();
				var height = reader.ReadInt32();
				var xOffset = reader.ReadInt32();
				var yOffset = reader.ReadInt32();
				var xAdvance = reader.ReadInt32();

				Subtexture textureRegion = null;
				if( hasTextures )
					textureRegion = new Subtexture( textures[textureIndex], x, y, width, height );
				else
					textureRegion = new Subtexture( textures[textureIndex], atlasOrigins[textureIndex].X + x, atlasOrigins[textureIndex].Y + y, width, height );
				
				regions[r] = new BitmapFontRegion( textureRegion, character, xOffset, yOffset, xAdvance );
			}
            
			return new BitmapFont( regions, lineHeight )
			{
				padTop = padTop,
				padBottom = padBottom,
				padRight = padRight,
				padLeft = padLeft,
				descent = descent
			};
		}
	}
}