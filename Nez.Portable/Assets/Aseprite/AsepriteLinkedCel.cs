using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a cel in an Aseprite file that is linked to another cel.
	/// </summary>
	public sealed class AsepriteLinkedCel : AsepriteCel
	{
		/// <summary>
		/// The cel that this cel is linked to.
		/// </summary>
		public AsepriteCel Cel;

		internal AsepriteLinkedCel(AsepriteCel other, AsepriteLayer layer, Point position, int opacity)
			: base(layer, position, opacity)
		{
			Cel = other;
		}
	}
}