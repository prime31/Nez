using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text;


namespace Nez
{
	public interface IFont
	{
		/// <summary>
		/// returns the size in pixels of text when rendered in this font
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="text">Text.</param>
		Vector2 measureString( string text );

		/// <summary>
		/// returns the size in pixels of text when rendered in this font
		/// </summary>
		/// <returns>The string.</returns>
		/// <param name="text">Text.</param>
		Vector2 measureString( StringBuilder text );

		/// <summary>
		/// returns true if the character exists in the font or false if it does not
		/// </summary>
		/// <returns><c>true</c>, if character was hased, <c>false</c> otherwise.</returns>
		/// <param name="c">C.</param>
		bool hasCharacter( char c );

		void drawInto( Batcher batcher, string text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth );

		void drawInto( Batcher batcher, StringBuilder text, Vector2 position, Color color,
			float rotation, Vector2 origin, Vector2 scale, SpriteEffects effect, float depth );
	}
}

