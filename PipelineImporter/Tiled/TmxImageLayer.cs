using System.Collections.Generic;
using System.Xml.Serialization;

namespace Nez.Content.Pipeline.Tiled
{
    public class TmxImageLayer : TmxLayer
    {
        public TmxImageLayer()
        {
            opacity = 1.0f;
            visible = true;
            properties = new List<TmxProperty>();
        }

        public override string ToString()
        {
            return string.Format("{0}: {1}", name, image);
        }

        [XmlAttribute(AttributeName = "x")]
        public int x;

        [XmlAttribute(AttributeName = "y")]
        public int y;

        [XmlElement(ElementName = "image")]
        public TmxImage image;
    }
}