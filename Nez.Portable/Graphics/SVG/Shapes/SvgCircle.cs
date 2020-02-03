using System.Xml.Serialization;


namespace Nez.Svg
{
	public class SvgCircle : SvgElement
	{
		[XmlAttribute("r")] public float Radius;

		[XmlAttribute("cy")] public float CenterY;

		[XmlAttribute("cx")] public float CenterX;
	}
}