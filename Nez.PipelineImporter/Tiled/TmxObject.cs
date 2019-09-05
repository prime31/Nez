using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.TiledMaps
{
	public class TmxObject
	{
		[XmlAttribute(DataType = "int", AttributeName = "id")]
		public int Id;

		[XmlAttribute(DataType = "string", AttributeName = "name")]
		public string Name;

		[XmlAttribute(DataType = "string", AttributeName = "type")]
		public string Type;

		[XmlAttribute(DataType = "float", AttributeName = "x")]
		public float X;

		[XmlAttribute(DataType = "float", AttributeName = "y")]
		public float Y;

		[XmlAttribute(DataType = "float", AttributeName = "width")]
		public float Width;

		[XmlAttribute(DataType = "float", AttributeName = "height")]
		public float Height;

		[XmlAttribute(DataType = "int", AttributeName = "rotation")]
		public int Rotation;

		[XmlAttribute(DataType = "int", AttributeName = "gid")]
		public int Gid;

		[XmlAttribute(DataType = "boolean", AttributeName = "visible")]
		public bool Visible = true;

		[XmlElement(ElementName = "image")] public TmxImage Image;

		[XmlElement(ElementName = "ellipse")] public TmxEllipse Ellipse;

		[XmlElement(ElementName = "polygon")] public TmxPolygon Polygon;

		[XmlElement(ElementName = "polyline")] public TmxPolyline Polyline;

		[XmlArray("properties")] [XmlArrayItem("property")]
		public List<TmxProperty> Properties;
	}


	public class TmxEllipse
	{
	}


	public class TmxPolygon
	{
		[XmlAttribute(DataType = "string", AttributeName = "points")]
		public string TempPoints
		{
			get { return string.Empty; }
			set
			{
				var parts = value.Split(new char[] {' '});
				foreach (var p in parts)
				{
					var pair = p.Split(new char[] {','});
					Points.Add(new Vector2(float.Parse(pair[0], System.Globalization.CultureInfo.InvariantCulture),
						float.Parse(pair[1], System.Globalization.CultureInfo.InvariantCulture)));
				}
			}
		}

		public List<Vector2> Points = new List<Vector2>();


		public override string ToString()
		{
			return string.Format("[TmxPolygon] point count: {0}", Points.Count);
		}
	}


	public class TmxPolyline : TmxPolygon
	{
		public override string ToString()
		{
			return string.Format("[TmxPolyline] point count: {0}", Points.Count);
		}
	}
}