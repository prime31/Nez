using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	[XmlInclude(typeof(TmxTileLayer))]
	[XmlInclude(typeof(TmxImageLayer))]
	public abstract class TmxLayer
	{
		[XmlAttribute(AttributeName = "offsetx")]
		public float Offsetx;

		[XmlAttribute(AttributeName = "offsety")]
		public float Offsety;

		[XmlAttribute(AttributeName = "name")] public string Name;

		[XmlAttribute(AttributeName = "opacity")]
		public float Opacity = 1f;

		[XmlAttribute(AttributeName = "visible")]
		public bool Visible = true;

		[XmlArray("properties")] [XmlArrayItem("property")]
		public List<TmxProperty> Properties = new List<TmxProperty>();


		public override string ToString()
		{
			return string.Format("[TmxLayer] name: {0}, visible: {1}", Name, Visible);
		}
	}
}