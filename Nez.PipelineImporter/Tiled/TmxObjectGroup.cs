using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxObjectGroup
	{
		[XmlAttribute( AttributeName = "offsetx" )]
		public float offsetx;

		[XmlAttribute( AttributeName = "offsety" )]
		public float offsety;

		[XmlAttribute( AttributeName = "name" )]
		public string name;

		[XmlAttribute( AttributeName = "color" )]
		public string color;

		[XmlAttribute( AttributeName = "opacity" )]
		public float opacity = 1f;

		[XmlAttribute( AttributeName = "visible" )]
		public bool visible = true;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties = new List<TmxProperty>();

		[XmlElement( ElementName = "object" )]
		public List<TmxObject> objects = new List<TmxObject>();


		public override string ToString()
		{
			return string.Format( "[TmxObjectGroup] name: {0}, offsetx: {1}, offsety: {2}", name, offsetx, offsety );
		}

	}
}