namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a layer in an Aseprite file.
	/// </summary>
	public abstract class AsepriteLayer : IAsepriteUserData
	{
		/// <summary>
		/// Indicates whether this layer is visible.
		/// </summary>
		public readonly bool IsVisible;

		/// <summary>
		/// Indicates whether this layer was marked as the background layer in Aseprite.
		/// </summary>
		public readonly bool IsBackgroundLayer;

		/// <summary>
		/// Indicates whether this layer was marked as a reference layer containing a reference image in Aseprite.
		/// </summary>
		public readonly bool IsReferenceLayer;

		/// <summary>
		/// Indicates the level of this layer in relation to its parent.
		/// </summary>
		/// <remarks>
		/// See <see href="https://github.com/aseprite/aseprite/blob/main/docs/ase-file-specs.md#note1"/> for more
		/// information.
		/// </remarks>
		public readonly int ChildLevel;

		/// <summary>
		/// Indicates the blend mode used by cels on this layer when cels blend with the cels on the layer below them
		/// </summary>
		public readonly AsepriteBlendMode BlendMode;

		/// <summary>
		/// Indicates the opacity level set for this layer in Aseprite, in the range of 0 - 255.
		/// </summary>
		public readonly int Opacity;

		/// <summary>
		/// The name given this layer in Aseprite.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The custom user data that was set in the properties for this layer in Aseprite.
		/// </summary>
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