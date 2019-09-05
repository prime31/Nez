using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxImage
	{
		[XmlAttribute(AttributeName = "source")]
		public string Source;

		[XmlAttribute(AttributeName = "width")]
		public int Width;

		[XmlAttribute(AttributeName = "height")]
		public int Height;

		[XmlAttribute(AttributeName = "format")]
		public string Format;

		[XmlAttribute(AttributeName = "trans")]
		public string Trans;

		[XmlElement(ElementName = "data")] public TmxData Data;


		public override string ToString()
		{
			return Source;
		}
	}
}