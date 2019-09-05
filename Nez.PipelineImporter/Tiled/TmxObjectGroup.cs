using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxObjectGroup
	{
		[XmlAttribute(AttributeName = "offsetx")]
		public float Offsetx;

		[XmlAttribute(AttributeName = "offsety")]
		public float Offsety;

		[XmlAttribute(AttributeName = "name")] public string Name;

		[XmlAttribute(AttributeName = "color")]
		public string Color;

		[XmlAttribute(AttributeName = "opacity")]
		public float Opacity = 1f;

		[XmlAttribute(AttributeName = "visible")]
		public bool Visible = true;

		[XmlArray("properties")] [XmlArrayItem("property")]
		public List<TmxProperty> Properties = new List<TmxProperty>();

		[XmlElement(ElementName = "object")] public List<TmxObject> Objects = new List<TmxObject>();


		public override string ToString()
		{
			return string.Format("[TmxObjectGroup] name: {0}, offsetx: {1}, offsety: {2}", Name, Offsetx, Offsety);
		}
	}
}