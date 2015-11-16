using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.Content.Pipeline.Tiled
{
	public class TmxObjectGroup
	{
		[XmlAttribute( AttributeName = "name" )]
		public string name;

		[XmlAttribute( AttributeName = "color" )]
		public string color;

		[XmlAttribute( AttributeName = "opacity" )]
		public float opacity;

		[XmlAttribute( AttributeName = "visible" )]
		public bool visible;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties;

		[XmlElement( ElementName = "object" )]
		public List<TmxObject> objects;


		public TmxObjectGroup()
		{
			opacity = 1.0f;
			visible = true;
			properties = new List<TmxProperty>();
			objects = new List<TmxObject>();
		}

		public override string ToString()
		{
			return name;
		}

	}
}