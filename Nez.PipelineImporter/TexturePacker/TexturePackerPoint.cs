using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerPoint
	{
		[JsonProperty("x")] public double X;

		[JsonProperty("y")] public double Y;


		public override string ToString()
		{
			return string.Format("{0} {1}", X, Y);
		}
	}
}