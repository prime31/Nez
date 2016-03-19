using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerSize
	{
		[JsonProperty( "w" )]
		public int width;

		[JsonProperty( "h" )]
		public int height;


		public override string ToString()
		{
			return string.Format( "{0} {1}", width, height );
		}
	}
}