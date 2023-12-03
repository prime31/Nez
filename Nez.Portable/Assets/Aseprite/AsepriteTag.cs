using Microsoft.Xna.Framework;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents an animation tag define in the Aseprite file.
	/// </summary>
	public sealed class AsepriteTag : IAsepriteUserData
	{
		private Color _oldVersionColor;

		/// <summary>
		/// The frame that the animation defined by this tag starts on.
		/// </summary>
		public readonly int From;

		/// <summary>
		/// The frame that the animation defined by this tag ends on.
		/// </summary>
		public readonly int To;

		/// <summary>
		/// The loop direction defined for the animation represented by this tag.
		/// </summary>
		public readonly AsepriteLoopDirection LoopDirection;

		/// <summary>
		/// The name given this tag in Aseprite.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The color assigned to this tag in Aseprite.
		/// </summary>
		/// <remarks>
		///	In Aseprite version <= 1.2, this will be the color value assigned to the tag directly.  In Aseprite version
		///	>= 1.3, this will be the color value assigned to the tag the tags user data.
		/// </remarks>
		public Color Color
		{
			get
			{
				if (UserData.HasColor)
				{
					return UserData.Color.Value;
				}

				return _oldVersionColor;
			}
		}

		/// <summary>
		/// The custom user data that was set in the properties for this tag in Aseprite.
		/// </summary>
		public AsepriteUserData UserData { get; }

		internal AsepriteTag(int from, int to, AsepriteLoopDirection loopDirection, Color color, string name)
		{
			From = from;
			To = to;
			LoopDirection = loopDirection;
			_oldVersionColor = color;
			Name = name;
			UserData = new AsepriteUserData();
		}
	}
}