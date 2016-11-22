using System.Xml.Serialization;


namespace Nez.Svg
{
	/// <summary>
	/// container in SVG. The 'g' XML tag.
	/// </summary>
	public class SvgGroup : SvgElement
	{
		[XmlElement( "title" )]
		public string title;

		[XmlElement( "g" )]
		public SvgGroup[] groups;

		[XmlElement( "path" )]
		public SvgPath[] paths;

		[XmlElement( "rect" )]
		public SvgRectangle[] rectangles;

		[XmlElement( "line" )]
		public SvgLine[] lines;

		[XmlElement( "circle" )]
		public SvgCircle[] circles;

		[XmlElement( "ellipse" )]
		public SvgEllipse[] ellipses;

		[XmlElement( "polygon" )]
		public SvgPolygon[] polygons;

		[XmlElement( "polyline" )]
		public SvgPolyline[] polylines;

		[XmlElement( "image" )]
		public SvgImage[] images;
	}
}
