using System.Xml.Serialization;


namespace Nez.Content.Pipeline.Tiled
{
    public class TmxProperty
    {
		[XmlAttribute(AttributeName = "name")]
		public string name;

		[XmlAttribute(AttributeName = "value")]
		public string value;


        public TmxProperty()
        {}


        public override string ToString()
        {
            return string.Format("{0}: {1}", name, value);
        }
    }
}