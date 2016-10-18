using System.Xml.Serialization;


namespace Nez.Svg
{
	public class SvgCircle : SvgElement
	{
		[XmlAttribute( "r" )]
		public float radius;

		[XmlAttribute( "cy" )]
		public float centerY;

		[XmlAttribute( "cx" )]
		public float centerX;
	}
}
