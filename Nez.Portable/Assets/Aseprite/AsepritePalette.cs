using System;
using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents the palette in an Aseprtie file.
	/// </summary>
	public sealed class AsepritePalette
	{
		/// <summary>
		/// An array of color elements that contains the colors of this palette.  Order of color elements is the same as
		/// the order of colors in the palette in Aseprite.
		/// </summary>
		public Color[] Colors;

		/// <summary>
		/// The index of the color element in this palette that should be interpreted as a transparent color.
		/// </summary>
		/// <remarks>
		/// This value is only valid when the color depth mode of the Aseprite image was set to Indexed.
		/// </remarks>
		public int TransparentIndex;

		internal AsepritePalette(int transparentIndex)
		{
			TransparentIndex = transparentIndex;
			Colors = Array.Empty<Color>();
		}
	}
}