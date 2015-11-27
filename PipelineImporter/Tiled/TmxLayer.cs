using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	[XmlInclude( typeof( TmxTileLayer ) )]
	[XmlInclude( typeof( TmxImageLayer ) )]
	public abstract class TmxLayer
	{
		protected TmxLayer()
		{
			opacity = 1.0f;
			visible = true;
			properties = new List<TmxProperty>();
		}

		public override string ToString()
		{
			return name;
		}

		[XmlAttribute( AttributeName = "name" )]
		public string name;

		[XmlAttribute( AttributeName = "opacity" )]
		public float opacity;

		[XmlAttribute( AttributeName = "visible" )]
		public bool visible;

		[XmlArray( "properties" )]
		[XmlArrayItem( "property" )]
		public List<TmxProperty> properties;

	}
}