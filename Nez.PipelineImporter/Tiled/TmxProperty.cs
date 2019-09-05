using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxProperty
	{
		public TmxProperty()
		{
		}


		[XmlAttribute(AttributeName = "name")] public string Name;

		[XmlAttribute(AttributeName = "value")]
		public string Value;


		public override string ToString()
		{
			return string.Format("{0}: {1}", Name, Value);
		}
	}
}