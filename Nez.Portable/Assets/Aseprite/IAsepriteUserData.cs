namespace Nez.Aseprite
{
	/// <summary>
	/// Represents an element that can contain custom users data in Aseprite.
	/// </summary>
	public interface IAsepriteUserData
	{
		/// <summary>
		/// When implemented, contains the custom user data set for the element in Aseprite.
		/// </summary>
		AsepriteUserData UserData { get; }
	}
}