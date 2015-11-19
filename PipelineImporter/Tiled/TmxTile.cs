using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez
{
	public class TmxTile
	{
		public TmxTile()
		{
			Probability = 1.0f;
			Properties = new List<TmxProperty>();
		}

		public override string ToString()
		{
			return Id;
		}

		[XmlAttribute( AttributeName = "id" )]
		public string Id;

		[XmlElement( ElementName = "terrain" )]
		public TmxTerrain Terrain;

		[XmlAttribute( AttributeName = "probability" )]
		public float Probability;

		[XmlElement( ElementName = "image" )]
		public TmxImage Image;

		[XmlElement( ElementName = "objectgroup" )]
		public List<TmxObjectGroup> ObjectGroups;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> Properties;
	}
}