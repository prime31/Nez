using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxTileLayer : TmxLayer
	{
		[XmlAttribute(AttributeName = "x")] public int X;

		[XmlAttribute(AttributeName = "y")] public int Y;

		[XmlAttribute(AttributeName = "width")]
		public int Width;

		[XmlAttribute(AttributeName = "height")]
		public int Height;

		[XmlElement(ElementName = "data")] public TmxData Data;
	}
}