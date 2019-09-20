using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;


namespace Nez.Svg
{
	/// <summary>
	/// handles parsing out groups, paths, rects, lines, circles, ellipses, polygons, polylines and images. This is just a small subset of the SVG
	/// spec! Only the basics are parsed out since this is not designed to be an image viewer.
	/// </summary>
	[XmlRoot(ElementName = "svg", Namespace = "http://www.w3.org/2000/svg")]
	public class SvgDocument : SvgGroup
	{
		[XmlAttribute("width")]
		public string WidthAttribute
		{
			get => null;
			set => Width = int.Parse(Regex.Replace(value, @"[^\d]", string.Empty));
		}

		public int Width;

		[XmlAttribute("height")]
		public string HeightAttribute
		{
			get => null;
			set => Height = int.Parse(Regex.Replace(value, @"[^\d]", string.Empty));
		}

		public int Height;


		public static SvgDocument Open(Stream stream)
		{
			var serializer = new XmlSerializer(typeof(SvgDocument));
			return (SvgDocument) serializer.Deserialize(stream);
		}
	}
}