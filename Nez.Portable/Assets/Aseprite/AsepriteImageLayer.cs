namespace Nez.Aseprite
{
	public sealed class AsepriteImageLayer : AsepriteLayer
	{
		internal AsepriteImageLayer(bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
			: base(isVisible, isBackground, isReference, childLevel, blendMode, opacity, name) { }
	}
}