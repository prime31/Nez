using Microsoft.Xna.Framework;


namespace Nez.BitmapFonts
{
    /// <summary>
    /// Represents the definition of a single character in a <see cref="BitmapFont"/>
    /// </summary>
    public class Character
	{
		/// <summary>
		/// bounds of the character image in the source texture.
		/// </summary>
		public Rectangle Bounds;

		/// <summary>
		/// texture channel where the character image is found.
		/// </summary>
		public int Channel;

		/// <summary>
		/// character.
		/// </summary>
		public char Char;

		/// <summary>
		/// offset when copying the image from the texture to the screen.
		/// </summary>
		public Point Offset;

		/// <summary>
		/// texture page where the character image is found.
		/// </summary>
		public int TexturePage;

		/// <summary>
		/// value used to advance the current position after drawing the character.
		/// </summary>
		public int XAdvance;

		public override string ToString() => Char.ToString();
	}
}