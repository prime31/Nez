using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public abstract class AsepriteCel : IAsepriteUserData
	{
		public readonly AsepriteLayer Layer;
		public readonly Point Position;
		public readonly int Opacity;
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