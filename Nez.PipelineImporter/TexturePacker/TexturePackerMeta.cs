using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerMeta
	{
		[JsonProperty("app")] public string App;

		[JsonProperty("version")] public string Version;

		[JsonProperty("image")] public string Image;

		[JsonProperty("format")] public string Format;

		[JsonProperty("size")] public TexturePackerSize Size;

		[JsonProperty("scale")] public float Scale;

		[JsonProperty("smartupdate")] public string SmartUpdate;


		public override string ToString()
		{
			return Image;
		}
	}
}