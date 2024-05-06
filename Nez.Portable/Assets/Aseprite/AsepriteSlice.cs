using System.Collections.Generic;

namespace Nez.Aseprite
{
	/// <summary>
	/// Represents a slice element in an Aseprite file.
	/// </summary>
	public sealed class AsepriteSlice : IAsepriteUserData
	{
		/// <summary>
		/// A collection of all slice key elements for this slice.  Each key is similar to an animation key frame in
		/// that it defines the properties of this slice starting on a specified frame.
		/// </summary>
		public readonly List<AsepriteSliceKey> Keys;

		/// <summary>
		/// Indicates whether this slice was marked as a nine patch slice in Aseprite.
		/// </summary>
		public readonly bool IsNinePatch;

		/// <summary>
		/// Indicates whether this slice was marked to have a pivot point in Aseprite.
		/// </summary>
		public readonly bool HasPivot;

		/// <summary>
		/// The name this slice was given in Aseprite.
		/// </summary>
		public readonly string Name;

		/// <summary>
		/// The custom user data that was set in the properties for this slice in Aseprite.
		/// </summary>
		public AsepriteUserData UserData { get; }

		internal AsepriteSlice(bool isNinePatch, bool hasPivot, string name)
		{
			IsNinePatch = isNinePatch;
			HasPivot = hasPivot;
			Name = name;
			Keys = new List<AsepriteSliceKey>();
            UserData = new AsepriteUserData();
		}
	}
}
