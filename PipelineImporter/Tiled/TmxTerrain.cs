using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.Content.Pipeline.Tiled
{
	public class TmxTerrain
	{
		public TmxTerrain()
		{
			Properties = new List<TmxProperty>();
		}

		public override string ToString()
		{
			return Name;
		}

		[XmlAttribute( AttributeName = "name" )]
		public string Name;

		[XmlAttribute( AttributeName = "tile" )]
		public string TileId;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> Properties;
	}
}