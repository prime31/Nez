namespace Nez.Aseprite
{
	/// <summary>
	///	Defines the color depth mode used by an Aseprite image.
	/// </summary>
	public enum AsepriteColorDepth : ushort
	{
		/// <summary>
		///     Defines that the Aseprite image uses an Indexed mode of 8-bits per pixel.
		/// </summary>
		Indexed = 8,

		/// <summary>
		///	Defines that the Aseprite image uses a Grayscale mode of 16-bits per pixel.
		/// </summary>
		Grayscale = 16,

		/// <summary>
		///	Defines that the Aseprite image uses an RGBA mode of 32-bits per pixel.
		/// </summary>
		RGBA = 32
	}
}