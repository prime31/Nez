namespace Nez.Aseprite
{
	public abstract class AsepriteLayer : IAsepriteUserData
	{
		public readonly bool IsVisible;
		public readonly bool IsBackgroundLayer;
		public readonly bool IsReferenceLayer;
		public readonly int ChildLevel;
		public readonly AsepriteBlendMode BlendMode;
		public readonly int Opacity;
		public readonly string Name;
		public AsepriteUserData UserData { get; }

		internal AsepriteLayer(bool isVisible, bool isBackground, bool isReference, int childLevel, AsepriteBlendMode blendMode, int opacity, string name)
		{
			IsVisible = isVisible;
			IsBackgroundLayer = isBackground;
			IsReferenceLayer = isReference;
			ChildLevel = childLevel;
			BlendMode = blendMode;
			Opacity = opacity;
			Name = name;
		}

	}
}