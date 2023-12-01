using System.Collections.Generic;

namespace Nez.Aseprite
{
	public sealed class AsepriteGroupLayer : AsepriteLayer
	{
		public readonly List<AsepriteLayer> Children;

		internal AsepriteGroupLayer(bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
			: base(isVisible, isBackground, isReference, childLevel, blendMode, opacity, name)
		{
			Children = new List<AsepriteLayer>();
		}
	}
}
