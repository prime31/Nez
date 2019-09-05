using System.Xml.Serialization;


namespace Nez.Svg
{
	public class SvgEllipse : SvgElement
	{
		[XmlAttribute("rx")] public float RadiusX;

		[XmlAttribute("ry")] public float RadiusY;

		[XmlAttribute("cy")] public float CenterY;

		[XmlAttribute("cx")] public float CenterX;
	}
}