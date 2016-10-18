using System.Xml.Serialization;


namespace Nez.Svg
{
	public class SvgEllipse : SvgElement
	{
		[XmlAttribute( "rx" )]
		public float radiusX;

		[XmlAttribute( "ry" )]
		public float radiusY;

		[XmlAttribute( "cy" )]
		public float centerY;

		[XmlAttribute( "cx" )]
		public float centerX;
	}
}
