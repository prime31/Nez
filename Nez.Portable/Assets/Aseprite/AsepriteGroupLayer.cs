using System.Collections.Generic;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a group layer in an Aseprite file that contains child layers.
	/// </summary>
	public sealed class AsepriteGroupLayer : AsepriteLayer
	{
		/// <summary>
		/// A collection of all child layer elements grouped into this group layer.  Order of layer elements is from
		/// bottom most layer to top most layer in the group.
		/// </summary>
		public readonly List<AsepriteLayer> Children;

		internal AsepriteGroupLayer(bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
			: base(isVisible, isBackground, isReference, childLevel, blendMode, opacity, name)
		{
			Children = new List<AsepriteLayer>();
		}
	}
}
