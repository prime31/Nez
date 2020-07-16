using System.Xml.Serialization;


namespace Nez.Svg
{
	/// <summary>
	/// container in SVG. The 'g' XML tag.
	/// </summary>
	public class SvgGroup : SvgElement
	{
		[XmlElement("title")] public string Title;

		[XmlElement("g")] public SvgGroup[] Groups;

		[XmlElement("path")] public SvgPath[] Paths;

		[XmlElement("rect")] public SvgRectangle[] Rectangles;

		[XmlElement("line")] public SvgLine[] Lines;

		[XmlElement("circle")] public SvgCircle[] Circles;

		[XmlElement("ellipse")] public SvgEllipse[] Ellipses;

		[XmlElement("polygon")] public SvgPolygon[] Polygons;

		[XmlElement("polyline")] public SvgPolyline[] Polylines;

		[XmlElement("image")] public SvgImage[] Images;
	}
}