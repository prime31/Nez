namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a layer in an Aseprite file that image cels are placed on.
	/// </summary>
	public sealed class AsepriteImageLayer : AsepriteLayer
	{
		internal AsepriteImageLayer(bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
			: base(isVisible, isBackground, isReference, childLevel, blendMode, opacity, name) { }
	}
}