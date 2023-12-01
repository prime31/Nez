using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteImageCel : AsepriteCel
	{
		public int Width;
		public int Height;
		public readonly Color[] Pixels;

		internal AsepriteImageCel(int width, int height, Color[] pixels, AsepriteLayer layer, Point position, int opacity)
			: base(layer, position, opacity)
		{
			Width = width;
			Height = height;
			Pixels = pixels;
		}

	}
}