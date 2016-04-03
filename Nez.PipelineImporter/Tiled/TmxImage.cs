using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxImage
	{
		[XmlAttribute( AttributeName = "source" )]
		public string source;

		[XmlAttribute( AttributeName = "width" )]
		public int width;

		[XmlAttribute( AttributeName = "height" )]
		public int height;

		[XmlAttribute( AttributeName = "format" )]
		public string format;

		[XmlAttribute( AttributeName = "trans" )]
		public string trans;

		[XmlElement( ElementName = "data" )]
		public TmxData data;


		public override string ToString()
		{
			return source;
		}
	}
}