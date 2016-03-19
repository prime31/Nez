using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	[XmlRoot( ElementName = "tileoffset" )]
	public class TmxTileOffset
	{
		public TmxTileOffset()
		{}
			

		[XmlAttribute( AttributeName = "x" )]
		public int X;

		[XmlAttribute( AttributeName = "y" )]
		public int Y;


		public override string ToString()
		{
			return string.Format( "{0}, {1}", X, Y );
		}
	}
}