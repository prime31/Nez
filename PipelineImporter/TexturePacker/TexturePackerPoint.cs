using Newtonsoft.Json;


namespace Nez.TexturePackerImporter
{
	public class TexturePackerPoint
	{
		[JsonProperty( "x" )]
		public double x;

		[JsonProperty( "y" )]
		public double y;


		public override string ToString()
		{
			return string.Format( "{0} {1}", x, y );
		}

	}
}