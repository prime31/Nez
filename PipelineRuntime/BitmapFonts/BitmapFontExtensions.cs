using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;


namespace Nez.BitmapFonts
{
	public static class BitmapFontExtensions
	{
		public static void drawString( this SpriteBatch spriteBatch, BitmapFont bitmapFont, string text, Vector2 position, Color color )
		{
			var dx = position.X;
			var dy = position.Y;

			foreach( char character in text )
			{
				var fontRegion = bitmapFont.getCharacterRegion( character );

				if( fontRegion != null )
				{
					var charPosition = new Vector2( dx + fontRegion.xOffset, dy + fontRegion.yOffset );

					spriteBatch.Draw( fontRegion.textureRegion.texture2D, charPosition, fontRegion.textureRegion.sourceRect, color );
					//spriteBatch.Draw( fontRegion.textureRegion, charPosition, color );
					dx += fontRegion.xAdvance;
				}

				if( character == '\n' )
				{
					dy += bitmapFont.lineHeight;
					dx = position.X;
				}
			}
		}


		public static void drawString( this SpriteBatch spriteBatch, BitmapFont bitmapFont, string text, Vector2 position, Color color, int wrapWidth )
		{
			var dx = position.X;
			var dy = position.Y;
			var sentences = text.Split( new[] { '\n' }, StringSplitOptions.None );

			foreach( var sentence in sentences )
			{
				var words = sentence.Split( new[] { ' ' }, StringSplitOptions.None );

				for( var i = 0; i < words.Length; i++ )
				{
					var word = words[i];
					var size = bitmapFont.getStringRectangle( word, Vector2.Zero );

					if( i != 0 && dx + size.Width >= wrapWidth )
					{
						dy += bitmapFont.lineHeight;
						dx = position.X;
					}

					drawString( spriteBatch, bitmapFont, word, new Vector2( dx, dy ), color );
					dx += size.Width + bitmapFont.getCharacterRegion( ' ' ).xAdvance;
				}

				dx = position.X;
				dy += bitmapFont.lineHeight;
			}
		}

	}
}