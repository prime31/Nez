using System.Collections.Generic;

namespace Nez.Tiled
{
	public class TmxGroup : ITmxLayer
	{
		public TmxMap map;
		public string Name { get; set; }
		public float Opacity { get; set; }
		public bool Visible { get; set; }
		public float OffsetX { get; set; }
		public float OffsetY { get; set; }

		public TmxList<ITmxLayer> Layers;

		public TmxList<TmxLayer> TileLayers;
		public TmxList<TmxObjectGroup> ObjectGroups;
		public TmxList<TmxImageLayer> ImageLayers;
		public TmxList<TmxGroup> Groups;

		public Dictionary<string, string> Properties { get; set; }
	}
}
