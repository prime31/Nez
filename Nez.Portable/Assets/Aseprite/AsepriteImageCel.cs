using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a single cel in a frame in Aseprite that contains image data.
	/// </summary>
	public sealed class AsepriteImageCel : AsepriteCel
	{
		/// <summary>
		/// The width, in pixels, of this cel.
		/// </summary>
		public int Width;

		/// <summary>
		/// The height, in pixels, of this cel.
		/// </summary>
		public int Height;

		/// <summary>
		/// An array of color elements that represents the pixel data that makes up the image for this cel.  Order of
		/// the color elements starts with the top-left most pixel and is read left-to-right from top-to-bottom.
		/// </summary>
		public readonly Color[] Pixels;

		internal AsepriteImageCel(int width, int height, Color[] pixels, AsepriteLayer layer, Point position, int opacity)
			: base(layer, position, opacity)
		{
			Width = width;
			Height = height;
			Pixels = pixels;
		}

		/// <summary>
		/// Translates the pixel data of this cel into a new sprite instance.
		/// </summary>
		/// <returns>
		/// A new instance of the <see cref="Sprite"/> class initialized with a texture generated from the pixel data
		/// of this cel.
		/// </returns>
		public Sprite ToSprite()
		{
			Texture2D texture = new Texture2D(Core.GraphicsDevice, Width, Height);
			texture.SetData<Color>(Pixels);
			return new Sprite(texture);
		}
	}
}