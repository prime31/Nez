﻿using System;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.BitmapFonts;


namespace Nez
{
	/// <summary>
	/// helper methods for drawing text with NezSpriteFonts
	/// </summary>
	public static class BatcherSpriteFontExt
	{
		public static void drawString( this Batcher batcher, NezSpriteFont spriteFont, StringBuilder text, Vector2 position, Color color )
		{
			batcher.drawString( spriteFont, text, position, color, 0.0f, Vector2.Zero, new Vector2( 1.0f ), SpriteEffects.None, 0.0f );
		}


		public static void drawString( this Batcher batcher, NezSpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth )
		{
			batcher.drawString( spriteFont, text, position, color, rotation, origin, new Vector2( scale ), effects, layerDepth );
		}


		public static void drawString( this Batcher batcher, NezSpriteFont spriteFont, string text, Vector2 position, Color color )
		{
			batcher.drawString( spriteFont, text, position, color, 0.0f, Vector2.Zero, new Vector2( 1.0f ), SpriteEffects.None, 0.0f );
		}


		public static void drawString( this Batcher batcher, NezSpriteFont spriteFont, string text, Vector2 position, Color color, float rotation,
			Vector2 origin, float scale, SpriteEffects effects, float layerDepth )
		{
			batcher.drawString( spriteFont, text, position, color, rotation, origin, new Vector2( scale ), effects, layerDepth );
		}


		public static void drawString( this Batcher batcher, NezSpriteFont spriteFont, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth )
		{
			Assert.isFalse( text == null );

			if( text.Length == 0 )
				return;

			var source = new FontCharacterSource( text );
			spriteFont.drawInto( batcher, ref source, position, color, rotation, origin, scale, effects, layerDepth );
		}


		public static void drawString( this Batcher batcher, NezSpriteFont spriteFont, string text, Vector2 position, Color color, float rotation,
			Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth )
		{
			Assert.isFalse( text == null );

			if( text.Length == 0 )
				return;

			var source = new FontCharacterSource( text );
			spriteFont.drawInto( batcher, ref source, position, color, rotation, origin, scale, effects, layerDepth );
		}
	
	}
}

