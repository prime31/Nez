using System.Collections.Generic;
using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerFile
	{
		[JsonProperty( "frames" )]
		public List<TexturePackerRegion> regions;

		[JsonProperty( "meta" )]
		public TexturePackerMeta metadata;
	}
}