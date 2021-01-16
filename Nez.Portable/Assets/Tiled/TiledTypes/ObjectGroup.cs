using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	public class TmxObjectGroup : ITmxLayer
	{
		public TmxMap Map;
		public string Name { get; set; }
		public float Opacity { get; set; }
		public bool Visible { get; set; }
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		public float ParallaxFactorX { get; set; }
		public float ParallaxFactorY { get; set; }

		public Color Color;
		public DrawOrderType DrawOrder;

		public TmxList<TmxObject> Objects;
		public Dictionary<string, string> Properties { get; set; }
	}


	public class TmxObject : ITmxElement
	{
		public int Id;
		public string Name { get; set; }
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
	}


	public class TmxAlignment
	{
		public TmxHorizontalAlignment Horizontal;
		public TmxVerticalAlignment Vertical;
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
