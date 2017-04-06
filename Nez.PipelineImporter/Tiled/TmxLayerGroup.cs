using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Nez.TiledMaps
{
    public class TmxLayerGroup
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

        [XmlElement( ElementName = "layer", Type = typeof( TmxTileLayer ) )]
        [XmlElement( ElementName = "imagelayer", Type = typeof( TmxImageLayer ) )]
        public List<TmxLayer> layer = new List<TmxLayer>();

        [XmlElement( ElementName = "group" )]
        public List<TmxLayerGroup> groups = new List<TmxLayerGroup>();

        public override string ToString()
        {
            return string.Format( "[TmxLayerGroup] name: {0}, offsetx: {1}, offsety: {2}", name, offsetx, offsety );
        }

    }
}
