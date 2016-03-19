using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerRegion
	{
		[JsonProperty( "filename" )]
		public string filename;

		[JsonProperty( "frame" )]
		public TexturePackerRectangle frame;

		[JsonProperty( "rotated" )]
		public bool isRotated;

		[JsonProperty( "trimmed" )]
		public bool isTrimmed;

		[JsonProperty( "spriteSourceSize" )]
		public TexturePackerRectangle sourceRectangle;

		[JsonProperty( "sourceSize" )]
		public TexturePackerSize sourceSize;

		[JsonProperty( "pivot" )]
		public TexturePackerPoint pivotPoint;


		public override string ToString()
		{
			return string.Format( "{0} {1}", filename, frame );
		}
	}
}