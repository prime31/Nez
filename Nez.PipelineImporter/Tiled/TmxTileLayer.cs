using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxTileLayer : TmxLayer
	{
		[XmlAttribute( AttributeName = "x" )]
		public int x;

		[XmlAttribute( AttributeName = "y" )]
		public int y;

		[XmlAttribute( AttributeName = "width" )]
		public int width;

		[XmlAttribute( AttributeName = "height" )]
		public int height;

		[XmlElement( ElementName = "data" )]
		public TmxData data;
	}
}