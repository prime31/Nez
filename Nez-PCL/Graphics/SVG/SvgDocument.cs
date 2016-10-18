using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace Nez.Svg
{
	/// <summary>
	/// handles parsing out groups, paths, rects, lines, circles, ellipses, polygons, polylines and images. This is just a small subset of the SVG
	/// spec! Only the basics are parsed out since this is not designed to be an image viewer.
	/// </summary>
	[XmlRoot( ElementName = "svg", Namespace = "http://www.w3.org/2000/svg" )]
	public class SvgDocument
	{
		[XmlAttribute( "width" )]
		public string widthAttribute
		{
			get { return null; }
			set { width = int.Parse( Regex.Replace( value, @"[^\d]", string.Empty ) ); }
		}
		public int width;

		[XmlAttribute( "height" )]
		public string heightAttribute
		{
			get { return null; }
			set { height = int.Parse( Regex.Replace( value, @"[^\d]", string.Empty ) ); }
		}
		public int height;

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


		public static SvgDocument open( Stream stream )
		{
			var serializer = new XmlSerializer( typeof( SvgDocument ) );
			return (SvgDocument)serializer.Deserialize( stream );
		}

	}
}
