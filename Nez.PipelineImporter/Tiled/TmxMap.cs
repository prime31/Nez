using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	[XmlRoot( ElementName = "map" )]
	public class TmxMap
	{
		public TmxMap()
		{
			properties = new List<TmxProperty>();
			tilesets = new List<TmxTileset>();
			layers = new List<TmxLayer>();
			objectGroups = new List<TmxObjectGroup>();
		}

		[XmlAttribute( AttributeName = "version" )]
		public string version;

		[XmlAttribute( AttributeName = "orientation" )]
		public TmxOrientation orientation;

		[XmlAttribute( AttributeName = "renderorder" )]
		public TmxRenderOrder renderOrder;

		[XmlAttribute( AttributeName = "backgroundcolor" )]
		public string backgroundColor;

		[XmlAttribute( AttributeName = "firstgid" )]
		public int firstGid;

		[XmlAttribute( AttributeName = "width" )]
		public int width;

		[XmlAttribute( AttributeName = "height" )]
		public int height;

		[XmlAttribute( AttributeName = "tilewidth" )]
		public int tileWidth;

		[XmlAttribute( AttributeName = "tileheight" )]
		public int tileHeight;

		[XmlAttribute( "nextobjectid" )]
		public int nextObjectId;

		[XmlElement( ElementName = "tileset" )]
		public List<TmxTileset> tilesets;

		[XmlElement( ElementName = "objectgroup" )]
		public List<TmxObjectGroup> objectGroups;

		[XmlElement( ElementName = "layer", Type = typeof( TmxTileLayer ) )]
		[XmlElement( ElementName = "imagelayer", Type = typeof( TmxImageLayer ) )]
		public List<TmxLayer> layers;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties;
	}
}