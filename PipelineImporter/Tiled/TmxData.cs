using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxData
	{
		public TmxData()
		{
			Tiles = new List<TmxDataTile>();
		}

		public override string ToString()
		{
			return string.Format( "{0} {1}", Encoding, Compression );
		}

		[XmlAttribute( AttributeName = "encoding" )]
		public string Encoding;

		[XmlAttribute( AttributeName = "compression" )]
		public string Compression;

		[XmlElement( ElementName = "tile" )]
		public List<TmxDataTile> Tiles;

		[XmlText]
		public string Value;
	}
}