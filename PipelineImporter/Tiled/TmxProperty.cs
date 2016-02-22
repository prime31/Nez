using System.Xml.Serialization;


namespace Nez.TiledMaps
{
    public class TmxProperty
    {
		public TmxProperty()
		{}
			

		[XmlAttribute(AttributeName = "name")]
		public string name;

		[XmlAttribute(AttributeName = "value")]
		public string value;


        public override string ToString()
        {
            return string.Format("{0}: {1}", name, value);
        }
    }
}