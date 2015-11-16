using System.Xml.Serialization;


namespace Nez.Content.Pipeline.Tiled
{
	[XmlRoot( ElementName = "tileoffset" )]
	public class TmxTileOffset
	{
		public TmxTileOffset()
		{
		}

		public override string ToString()
		{
			return string.Format( "{0}, {1}", X, Y );
		}

		[XmlAttribute( AttributeName = "x" )]
		public int X;

		[XmlAttribute( AttributeName = "y" )]
		public int Y;
	}
}