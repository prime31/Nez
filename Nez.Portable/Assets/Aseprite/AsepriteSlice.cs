using System.Collections.Generic;

namespace Nez.Aseprite
{

	public sealed class AsepriteSlice : IAsepriteUserData
	{
		public readonly List<AsepriteSliceKey> Keys;
		public readonly bool IsNinePatch;
		public readonly bool HasPivot;
		public readonly string Name;
		public AsepriteUserData UserData { get; }

		internal AsepriteSlice(bool isNinePatch, bool hasPivot, string name)
		{
			IsNinePatch = isNinePatch;
			HasPivot = hasPivot;
			Name = name;
			Keys = new List<AsepriteSliceKey>();
		}
	}
}