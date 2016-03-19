using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxObjectGroup
	{
		public TmxObjectGroup()
		{}
			

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
			return name;
		}

	}
}