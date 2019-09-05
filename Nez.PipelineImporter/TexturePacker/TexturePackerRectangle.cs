using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerRectangle
	{
		[JsonProperty("x")] public int X;

		[JsonProperty("y")] public int Y;

		[JsonProperty("w")] public int Width;

		[JsonProperty("h")] public int Height;


		public override string ToString()
		{
			return string.Format("{0} {1} {2} {3}", X, Y, Width, Height);
		}
	}
}