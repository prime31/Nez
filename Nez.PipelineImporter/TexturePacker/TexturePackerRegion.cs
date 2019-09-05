using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerRegion
	{
		[JsonProperty("filename")] public string Filename;

		[JsonProperty("frame")] public TexturePackerRectangle Frame;

		[JsonProperty("rotated")] public bool IsRotated;

		[JsonProperty("trimmed")] public bool IsTrimmed;

		[JsonProperty("spriteSourceSize")] public TexturePackerRectangle SourceRectangle;

		[JsonProperty("sourceSize")] public TexturePackerSize SourceSize;

		[JsonProperty("pivot")] public TexturePackerPoint PivotPoint;


		public override string ToString()
		{
			return string.Format("{0} {1}", Filename, Frame);
		}
	}
}