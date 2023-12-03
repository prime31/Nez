using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents the custom user data set for an element in Aseprite.
	/// </summary>
	public sealed class AsepriteUserData
	{
		/// <summary>
		/// Returns a value tha indicates whether custom text was set for this user data.
		/// </summary>
		public bool HasText
		{
			get
			{
				return string.IsNullOrEmpty(Text);
			}
		}

		/// <summary>
		/// Returns a value that indicates whether color was set for this user data.
		/// </summary>
		public bool HasColor
		{
			get
			{
				return Color != null;
			}
		}

		/// <summary>
		/// The text that was set for this user data in Aseprite.
		/// </summary>
		public string Text;

		/// <summary>
		/// The color that was set for this user data in Aseprite, if a color was set; otherwise, null.
		/// </summary>
		public Color? Color;

		internal AsepriteUserData() { }
	}
}