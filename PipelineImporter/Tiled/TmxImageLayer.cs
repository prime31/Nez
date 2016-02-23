using System.Collections.Generic;
using System.Xml.Serialization;


namespace Nez.TiledMaps
{
	public class TmxImageLayer : TmxLayer
	{
		public TmxImageLayer()
		{}


		[XmlAttribute( AttributeName = "x" )]
		public int x;

		[XmlAttribute( AttributeName = "y" )]
		public int y;

		[XmlElement( ElementName = "image" )]
		public TmxImage image;


		public override string ToString()
		{
			return string.Format( "[TmxImageLayer] name: {0}, image: {1}, visible: {2}", name, image, visible );
		}
	}
}