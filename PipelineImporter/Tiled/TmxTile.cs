using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxTile
	{
		public TmxTile()
		{}
			

		[XmlAttribute( AttributeName = "id" )]
		public int id;

		[XmlElement( ElementName = "terrain" )]
		public TmxTerrain terrain;

		[XmlAttribute( AttributeName = "probability" )]
		public float probability = 1f;

		[XmlElement( ElementName = "image" )]
		public TmxImage image;

		[XmlElement( ElementName = "objectgroup" )]
		public List<TmxObjectGroup> objectGroups;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties = new List<TmxProperty>();


		public override string ToString()
		{
			return id.ToString();
		}
	}
}