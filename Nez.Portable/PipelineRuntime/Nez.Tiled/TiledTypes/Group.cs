using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Nez.Tiled
{
	public class TmxGroup : ITmxLayer
	{
		public readonly TmxMap map;
		public string Name { get; private set; }
		public float Opacity { get; private set; }
		public bool Visible { get; private set; }
		public float OffsetX { get; private set; }
		public float OffsetY { get; private set; }

		public TmxList<ITmxLayer> layers;

		public TmxList<TmxLayer> tileLayers;
		public TmxList<TmxObjectGroup> objectGroups;
		public TmxList<TmxImageLayer> imageLayers;
		public TmxList<TmxGroup> groups;
		public Dictionary<string, string> Properties { get; private set; }

		public TmxGroup(TmxMap map, XElement xGroup, int width, int height, string tmxDirectory)
		{
			this.map = map;
			Name = (string)xGroup.Attribute("name") ?? string.Empty;
			Opacity = (float?)xGroup.Attribute("opacity") ?? 1.0f;
			Visible = (bool?)xGroup.Attribute("visible") ?? true;
			OffsetX = (float?)xGroup.Attribute("offsetx") ?? 0.0f;
			OffsetY = (float?)xGroup.Attribute("offsety") ?? 0.0f;

			Properties = PropertyDict.ParsePropertyDict(xGroup.Element("properties"));

			layers = new TmxList<ITmxLayer>();
			tileLayers = new TmxList<TmxLayer>();
			objectGroups = new TmxList<TmxObjectGroup>();
			imageLayers = new TmxList<TmxImageLayer>();
			groups = new TmxList<TmxGroup>();
			foreach (var e in xGroup.Elements().Where(x => x.Name == "layer" || x.Name == "objectgroup" || x.Name == "imagelayer" || x.Name == "group"))
			{
				ITmxLayer layer;
				switch (e.Name.LocalName)
				{
					case "layer":
						var tileLayer = new TmxLayer(map, e, width, height);
						layer = tileLayer;
						tileLayers.Add(tileLayer);
						break;
					case "objectgroup":
						var objectgroup = new TmxObjectGroup(map, e);
						layer = objectgroup;
						objectGroups.Add(objectgroup);
						break;
					case "imagelayer":
						var imagelayer = new TmxImageLayer(map, e, tmxDirectory);
						layer = imagelayer;
						imageLayers.Add(imagelayer);
						break;
					case "group":
						var group = new TmxGroup(map, e, width, height, tmxDirectory);
						layer = group;
						groups.Add(group);
						break;
					default:
						throw new InvalidOperationException();
				}
				layers.Add(layer);
			}
		}

	}
}
