using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerMeta
	{
		[JsonProperty( "app" )]
		public string app;

		[JsonProperty( "version" )]
		public string version;

		[JsonProperty( "image" )]
		public string image;

		[JsonProperty( "format" )]
		public string format;

		[JsonProperty( "size" )]
		public TexturePackerSize size;

		[JsonProperty( "scale" )]
		public float scale;

		[JsonProperty( "smartupdate" )]
		public string smartUpdate;


		public override string ToString()
		{
			return image;
		}

	}
}