using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;


namespace Nez.BitmapFonts
{
	/// <summary>
	/// provides the full SpriteFont assortment of drawString methods
	/// </summary>
	public static class BatcherBitmapFontExt
	{
        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="bitmapFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public static void DrawString(this Batcher batcher, BitmapFont bitmapFont, string text, Vector2 position,
		                              Color color)
		{
			var source = new FontCharacterSource(text);
			bitmapFont.DrawInto(batcher, ref source, position, color, 0, Vector2.Zero, new Vector2(1),
				SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="bitmapFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public static void DrawString(this Batcher batcher, BitmapFont bitmapFont, string text, Vector2 position,
		                              Color color,
		                              float rotation, Vector2 origin, float scale, SpriteEffects effects,
		                              float layerDepth)
		{
			var scaleVec = new Vector2(scale, scale);
			var source = new FontCharacterSource(text);
			bitmapFont.DrawInto(batcher, ref source, position, color, rotation, origin, scaleVec, effects, layerDepth);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="bitmapFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public static void DrawString(this Batcher batcher, BitmapFont bitmapFont, string text, Vector2 position,
		                              Color color,
		                              float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects,
		                              float layerDepth)
		{
			var source = new FontCharacterSource(text);
			bitmapFont.DrawInto(batcher, ref source, position, color, rotation, origin, scale, effects, layerDepth);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="bitmapFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        public static void DrawString(this Batcher batcher, BitmapFont bitmapFont, StringBuilder text, Vector2 position,
		                              Color color)
		{
			var source = new FontCharacterSource(text);
			bitmapFont.DrawInto(batcher, ref source, position, color, 0, Vector2.Zero, new Vector2(1),
				SpriteEffects.None, 0f);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="bitmapFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public static void DrawString(this Batcher batcher, BitmapFont bitmapFont, StringBuilder text, Vector2 position,
		                              Color color,
		                              float rotation, Vector2 origin, float scale, SpriteEffects effects,
		                              float layerDepth)
		{
			var scaleVec = new Vector2(scale, scale);
			var source = new FontCharacterSource(text);
			bitmapFont.DrawInto(batcher, ref source, position, color, rotation, origin, scaleVec, effects, layerDepth);
		}

        /// <summary>
        /// Submit a text string of sprites for drawing in the current batch.
        /// </summary>
        /// <param name="bitmapFont">A font.</param>
        /// <param name="text">The text which will be drawn.</param>
        /// <param name="position">The drawing location on screen.</param>
        /// <param name="color">A color mask.</param>
        /// <param name="rotation">A rotation of this string.</param>
        /// <param name="origin">Center of the rotation. 0,0 by default.</param>
        /// <param name="scale">A scaling of this string.</param>
        /// <param name="effects">Modificators for drawing. Can be combined.</param>
        /// <param name="layerDepth">A depth of the layer of this string.</param>
        public static void DrawString(this Batcher batcher, BitmapFont bitmapFont, StringBuilder text, Vector2 position,
		                              Color color, float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects,
		                              float layerDepth)
		{
			var source = new FontCharacterSource(text);
			bitmapFont.DrawInto(batcher, ref source, position, color, rotation, origin, scale, effects, layerDepth);
		}

		public static void DrawGlyphs(this Batcher batcher, BitmapFontEnumerator glyphs, Vector2 position, Color color,
		                              float rotation, Vector2 origin, Vector2 scale, float layerDepth)
		{
			foreach (var glyph in glyphs)
			{
				var characterOrigin = origin - glyph.Position;
				Graphics.Instance.Batcher.Draw(glyph.Texture, position, glyph.Character.Bounds, color, rotation,
					characterOrigin, scale, SpriteEffects.None, layerDepth);
			}
		}
	}
}