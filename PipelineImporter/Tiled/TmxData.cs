using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxData
	{
		public TmxData()
		{}


		[XmlAttribute( AttributeName = "encoding" )]
		public string encoding;

		[XmlAttribute( AttributeName = "compression" )]
		public string compression;

		[XmlElement( ElementName = "tile" )]
		public List<TmxDataTile> tiles = new List<TmxDataTile>();

		[XmlText]
		public string value;


		public override string ToString()
		{
			return string.Format( "{0} {1}", encoding, compression );
		}
	}
}