using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public class TmxObjectGroup : ITmxLayer
	{
		public readonly TmxMap Map;
		public string Name { get; private set; }
		public float Opacity { get; private set; }
		public bool Visible { get; private set; }
		public float OffsetX { get; private set; }
		public float OffsetY { get; private set; }

		public Color Color;
		public DrawOrderType DrawOrder;

		public TmxList<TmxObject> Objects;
		public Dictionary<string, string> Properties { get; private set; }

		public TmxObjectGroup(TmxMap map, XElement xObjectGroup)
		{
			Map = map;
			Name = (string)xObjectGroup.Attribute("name") ?? string.Empty;
			Color = TmxColor.ParseColor(xObjectGroup.Attribute("color"));
			Opacity = (float?)xObjectGroup.Attribute("opacity") ?? 1.0f;
			Visible = (bool?)xObjectGroup.Attribute("visible") ?? true;
			OffsetX = (float?)xObjectGroup.Attribute("offsetx") ?? 0.0f;
			OffsetY = (float?)xObjectGroup.Attribute("offsety") ?? 0.0f;

			var drawOrderDict = new Dictionary<string, DrawOrderType> {
				{"unknown", DrawOrderType.UnknownOrder},
				{"topdown", DrawOrderType.IndexOrder},
				{"index", DrawOrderType.TopDown}
			};

			var drawOrderValue = (string)xObjectGroup.Attribute("draworder");
			if (drawOrderValue != null)
				DrawOrder = drawOrderDict[drawOrderValue];

			Objects = new TmxList<TmxObject>();
			foreach (var e in xObjectGroup.Elements("object"))
				Objects.Add(new TmxObject(map, e));

			Properties = PropertyDict.ParsePropertyDict(xObjectGroup.Element("properties"));
		}
	}


	public class TmxObject : ITmxElement
	{
		public int Id;
		public string Name { get; private set; }
		public TmxObjectType ObjectType;
		public string Type;
		public float X;
		public float Y;
		public float Width;
		public float Height;
		public float Rotation;
		public TmxLayerTile Tile;
		public bool Visible;
		public TmxText Text;

		public Vector2[] Points;
		public Dictionary<string, string> Properties;

		public TmxObject(TmxMap map, XElement xObject)
		{
			Id = (int?)xObject.Attribute("id") ?? 0;
			Name = (string)xObject.Attribute("name") ?? string.Empty;
			X = (float)xObject.Attribute("x");
			Y = (float)xObject.Attribute("y");
			Width = (float?)xObject.Attribute("width") ?? 0.0f;
			Height = (float?)xObject.Attribute("height") ?? 0.0f;
			Type = (string)xObject.Attribute("type") ?? string.Empty;
			Visible = (bool?)xObject.Attribute("visible") ?? true;
			Rotation = (float?)xObject.Attribute("rotation") ?? 0.0f;

			// Assess object type and assign appropriate content
			var xGid = xObject.Attribute("gid");
			var xEllipse = xObject.Element("ellipse");
			var xPolygon = xObject.Element("polygon");
			var xPolyline = xObject.Element("polyline");
			var xText = xObject.Element("text");
			var xPoint = xObject.Element("point");

			if (xGid != null)
			{
				Tile = new TmxLayerTile(map, (uint)xGid, Convert.ToInt32(Math.Round(X)), Convert.ToInt32(Math.Round(X)));
				ObjectType = TmxObjectType.Tile;
			}
			else if (xEllipse != null)
			{
				ObjectType = TmxObjectType.Ellipse;
			}
			else if (xPolygon != null)
			{
				Points = ParsePoints(xPolygon);
				ObjectType = TmxObjectType.Polygon;
			}
			else if (xPolyline != null)
			{
				Points = ParsePoints(xPolyline);
				ObjectType = TmxObjectType.Polyline;
			}
			else if (xText != null)
			{
				Text = new TmxText(xText);
				ObjectType = TmxObjectType.Text;
			}
			else if (xPoint != null)
			{
				ObjectType = TmxObjectType.Point;
			}
			else
			{
				ObjectType = TmxObjectType.Basic;
			}

			Properties = PropertyDict.ParsePropertyDict(xObject.Element("properties"));
		}

		public Vector2[] ParsePoints(XElement xPoints)
		{
			var pointString = (string)xPoints.Attribute("points");
			var pointStringPair = pointString.Split(' ');
			var points = new Vector2[pointStringPair.Length];

			var index = 0;
			foreach (var s in pointStringPair)
				points[index++] = TmxObjectPoint.ParsePoint(s); ;
			return points;
		}
	}


	public static class TmxObjectPoint
	{
		public static Vector2 ParsePoint(string s)
		{
			var pt = s.Split(',');
			var x = float.Parse(pt[0], NumberStyles.Float, CultureInfo.InvariantCulture);
			var y = float.Parse(pt[1], NumberStyles.Float, CultureInfo.InvariantCulture);
			return new Vector2(x, y);
		}
	}


	public class TmxText
	{
		public string FontFamily;
		public int PixelSize;
		public bool Wrap;
		public Color Color;
		public bool Bold;
		public bool Italic;
		public bool Underline;
		public bool Strikeout;
		public bool Kerning;
		public TmxAlignment Alignment;
		public string Value;

		public TmxText(XElement xText)
		{
			FontFamily = (string)xText.Attribute("fontfamily") ?? "sans-serif";
			PixelSize = (int?)xText.Attribute("pixelsize") ?? 16;
			Wrap = (bool?)xText.Attribute("wrap") ?? false;
			Color = TmxColor.ParseColor(xText.Attribute("color"));
			Bold = (bool?)xText.Attribute("bold") ?? false;
			Italic = (bool?)xText.Attribute("italic") ?? false;
			Underline = (bool?)xText.Attribute("underline") ?? false;
			Strikeout = (bool?)xText.Attribute("strikeout") ?? false;
			Kerning = (bool?)xText.Attribute("kerning") ?? true;
			Alignment = new TmxAlignment(xText.Attribute("halign"), xText.Attribute("valign"));
			Value = xText.Value;
		}
	}


	public class TmxAlignment
	{
		public TmxHorizontalAlignment Horizontal;
		public TmxVerticalAlignment Vertical;

		public TmxAlignment(XAttribute halign, XAttribute valign)
		{
			var xHorizontal = (string)halign ?? "Left";
			Horizontal = (TmxHorizontalAlignment)Enum.Parse(typeof(TmxHorizontalAlignment), FirstLetterToUpperCase(xHorizontal));

			var xVertical = (string)valign ?? "Top";
			Vertical = (TmxVerticalAlignment)Enum.Parse(typeof(TmxVerticalAlignment), FirstLetterToUpperCase(xVertical));
		}

		private string FirstLetterToUpperCase(string str)
		{
			if (string.IsNullOrEmpty(str))
				return str;
			return str[0].ToString().ToUpper() + str.Substring(1);
		}
	}

	public enum TmxObjectType
	{
		Basic,
		Point,
		Tile,
		Ellipse,
		Polygon,
		Polyline,
		Text
	}

	public enum DrawOrderType
	{
		UnknownOrder = -1,
		TopDown,
		IndexOrder
	}

	public enum TmxHorizontalAlignment
	{
		Left,
		Center,
		Right,
		Justify
	}

	public enum TmxVerticalAlignment
	{
		Top,
		Center,
		Bottom
	}
}
