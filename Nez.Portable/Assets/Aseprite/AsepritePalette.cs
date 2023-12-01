using System;
using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepritePalette
	{
		public Color[] Colors;
		public int TransparentIndex;

		internal AsepritePalette(int transparentIndex)
		{
			TransparentIndex = transparentIndex;
			Colors = Array.Empty<Color>();
		}
	}
}