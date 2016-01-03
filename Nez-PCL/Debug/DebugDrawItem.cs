using System;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	internal class DebugDrawItem
	{
		enum DebugDrawType
		{
			Line,
			HollowRectangle,
			BitmapFontText,
			SpriteFontText
		}

		// used for Line items
		public Vector2 start;
		public Vector2 end;
		public Rectangle rectangle;

		// used for Text items
		public string text;
		public BitmapFont bitmapFont;
		public SpriteFont spriteFont;
		public Vector2 position;
		public float scale;

		// shared by multiple items
		public Color color;
		public float duration;

		DebugDrawType _drawType;


		public DebugDrawItem( Vector2 start, Vector2 end, Color color, float duration )
		{
			this.start = start;
			this.end = end;
			this.color = color;
			this.duration = duration;
			_drawType = DebugDrawType.Line;
		}


		public DebugDrawItem( Rectangle rectangle, Color color, float duration )
		{
			this.rectangle = rectangle;
			this.color = color;
			this.duration = duration;
			_drawType = DebugDrawType.HollowRectangle;
		}


		public DebugDrawItem( BitmapFont bitmapFont, String text, Vector2 position, Color color, float duration, float scale )
		{
			this.bitmapFont = bitmapFont;
			this.text = text;
			this.position = position;
			this.color = color;
			this.scale = scale;
			this.duration = duration;
			_drawType = DebugDrawType.BitmapFontText;
		}


		public DebugDrawItem( SpriteFont spriteFont, String text, Vector2 position, Color color, float duration, float scale )
		{
			this.spriteFont = spriteFont;
			this.text = text;
			this.position = position;
			this.color = color;
			this.scale = scale;
			this.duration = duration;
			_drawType = DebugDrawType.SpriteFontText;
		}


		/// <summary>
		/// returns true if we are done with this debug draw item
		/// </summary>
		public bool draw( Graphics graphics )
		{
			switch( _drawType )
			{
				case DebugDrawType.Line:
					graphics.spriteBatch.drawLine( start, end, color );
					break;
				case DebugDrawType.HollowRectangle:
					graphics.spriteBatch.drawHollowRect( rectangle, color );
					break;
				case DebugDrawType.BitmapFontText:
					graphics.spriteBatch.DrawString( bitmapFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f );
					break;
				case DebugDrawType.SpriteFontText:
					graphics.spriteBatch.DrawString( spriteFont, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f );
				break;
			}

			duration -= Time.deltaTime;

			return duration < 0f;
		}
	}
}

