using System.Collections.Generic;
using System.Xml.Linq;

namespace Nez.Tiled
{
	public class TmxImageLayer : ITmxLayer
	{
		public readonly TmxMap Map;
		public string Name { get; private set; }
		public bool Visible { get; private set; }
		public float Opacity { get; private set; }
		public float OffsetX { get; private set; }
		public float OffsetY { get; private set; }

		public int? Width;
		public int? Height;

		public TmxImage Image;

		public Dictionary<string, string> Properties { get; private set; }

		public TmxImageLayer(TmxMap map, XElement xImageLayer, string tmxDir = "")
		{
			Map = map;
			Name = (string)xImageLayer.Attribute("name");

			Width = (int?)xImageLayer.Attribute("width");
			Height = (int?)xImageLayer.Attribute("height");
			Visible = (bool?)xImageLayer.Attribute("visible") ?? true;
			Opacity = (float?)xImageLayer.Attribute("opacity") ?? 1.0f;
			OffsetX = (float?)xImageLayer.Attribute("offsetx") ?? 0.0f;
			OffsetY = (float?)xImageLayer.Attribute("offsety") ?? 0.0f;

			var xImage = xImageLayer.Element("image");
			if (xImage != null)
				Image = new TmxImage(xImage, tmxDir);

			Properties = PropertyDict.ParsePropertyDict(xImageLayer.Element("properties"));
		}
	}
}
