using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxTerrain
	{
		[XmlAttribute(AttributeName = "name")] public string Name;

		[XmlAttribute(AttributeName = "tile")] public int TileId;

		[XmlArray("properties")] [XmlArrayItem("property")]
		public List<TmxProperty> Properties = new List<TmxProperty>();


		public override string ToString()
		{
			return Name;
		}
	}
}