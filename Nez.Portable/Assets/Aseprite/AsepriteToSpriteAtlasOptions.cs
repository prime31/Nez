namespace Nez.Aseprite
{
	/// <summary>
	/// Defines the options to adhere to when generating a sprite atlas from an Aseprite file.
	/// </summary>
	public class AsepriteToSpriteAtlasOptions
	{
		/// <summary>
		/// Indicates whether only visible layers in the Aseprite file should be processed when generating the sprite
		/// atlas
		/// </summary>
		public bool OnlyVisibleLayers = true;

		/// <summary>
		/// The amount of transparent pixels to add between each frame and the edge of the generated texture for the
		/// sprite atlas.
		/// </summary>
		public int BorderPadding = 0;

		/// <summary>
		/// The amount of transparent pixels to add between each frame of the generated texture for the sprite atlas.
		/// </summary>
		public int Spacing = 0;

		/// <summary>
		/// The amount of transparent pixels to add to the inside of each frame's edge of the generated texture for the
		/// sprite atlas.
		/// </summary>
		public int InnerPadding = 0;
	}
}