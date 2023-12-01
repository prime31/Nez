using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	public sealed class AsepriteLinkedCel : AsepriteCel
	{
		public AsepriteCel Cel;
		internal AsepriteLinkedCel(AsepriteCel other, AsepriteLayer layer, Point position, int opacity)
			: base(layer, position, opacity)
		{
			Cel = other;
		}
	}
}