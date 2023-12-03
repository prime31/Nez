using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Defines a single cel within a frame in Aseprite.
	/// </summary>
	public abstract class AsepriteCel : IAsepriteUserData
	{
		/// <summary>
		/// The layer that this cel exists on.
		/// </summary>
		public readonly AsepriteLayer Layer;

		/// <summary>
		/// The x- and y- coordinate position of this cel relative to the bounds of the frame.
		/// </summary>
		public readonly Point Position;

		/// <summary>
		/// The opacity level of this cel, in the range of 0 - 255
		/// </summary>
		public readonly int Opacity;

		/// <summary>
		/// The custom user data tha was set in the properties for this cel in Aseprite.
		/// </summary>
		public AsepriteUserData UserData { get; }

		internal AsepriteCel(AsepriteLayer layer, Point position, int opacity)
		{
			Layer = layer;
			Position = position;
			Opacity = opacity;
			UserData = new AsepriteUserData();
		}
	}
}