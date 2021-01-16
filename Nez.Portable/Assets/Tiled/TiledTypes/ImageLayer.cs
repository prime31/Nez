using System.Collections.Generic;

namespace Nez.Tiled
{
	public class TmxImageLayer : ITmxLayer
	{
		public TmxMap Map;
		public string Name { get; set; }
		public bool Visible { get; set; }
		public float Opacity { get; set; }
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }
		public float ParallaxFactorX { get; set; }
		public float ParallaxFactorY { get; set; }

		public int? Width;
		public int? Height;

		public TmxImage Image;

		public Dictionary<string, string> Properties { get; set; }
	}
}
