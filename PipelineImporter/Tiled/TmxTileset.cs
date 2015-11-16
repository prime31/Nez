using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.Content.Pipeline.Tiled
{
	[XmlRoot( ElementName = "tileset" )]
	public class TmxTileset
	{
		public TmxTileset()
		{
			tileOffset = new TmxTileOffset();
			tiles = new List<TmxTile>();
			properties = new List<TmxProperty>();
			terrainTypes = new List<TmxTerrain>();

		}

		public override string ToString()
		{
			return string.Format( "{0}: {1}", name, image );
		}

		[XmlAttribute( AttributeName = "firstgid" )]
		public int firstGid;

		[XmlAttribute( AttributeName = "source" )]
		public string source;

		[XmlAttribute( AttributeName = "name" )]
		public string name;

		[XmlAttribute( AttributeName = "tilewidth" )]
		public int tileWidth;

		[XmlAttribute( AttributeName = "tileheight" )]
		public int tileHeight;

		[XmlAttribute( AttributeName = "spacing" )]
		public int spacing;

		[XmlAttribute( AttributeName = "margin" )]
		public int margin;

		[XmlElement( ElementName = "tileoffset" )]
		public TmxTileOffset tileOffset;

		[XmlElement( ElementName = "tile" )]
		public List<TmxTile> tiles;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties;

		[XmlElement( ElementName = "image" )]
		public TmxImage image;

		[XmlArray( "terraintypes" )]
		[XmlArrayItem( "terrain" )]
		public List<TmxTerrain> terrainTypes;

	}
}