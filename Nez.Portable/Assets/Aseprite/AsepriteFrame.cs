using System.Collections.Generic;

namespace Nez.Aseprite
{
	public sealed class AsepriteFrame
	{
		public readonly List<AsepriteCel> Cels;
		public readonly int Width;
		public readonly int Height;
		public readonly int Duration;

		internal AsepriteFrame(int duration, List<AsepriteCel> cels, int width, int height)
		{
			Duration = duration;
			Cels = cels;
			Width = width;
			Height = height;
		}
	}
}