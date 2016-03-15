using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Content;
using Nez.Textures;


namespace Nez.BitmapFonts
{
	public class BitmapFontReader : ContentTypeReader<BitmapFont>
	{
		protected override BitmapFont Read( ContentReader reader, BitmapFont existingInstance )
		{
			var totalTextures = reader.ReadInt32();
			var textures = new Texture2D[totalTextures];
			for( var i = 0; i < totalTextures; i++ )
				textures[i] = reader.ReadObject<Texture2D>();

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
				var textureRegion = new Subtexture( textures[textureIndex], x, y, width, height );
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