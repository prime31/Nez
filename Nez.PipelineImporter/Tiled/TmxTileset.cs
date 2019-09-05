using System.Collections.Generic;
using System.Xml.Serialization;
using System.IO;
using System;
using Nez.PipelineImporter;


namespace Nez.TiledMaps
{
	[XmlRoot(ElementName = "tileset")]
	public class TmxTileset
	{
		// we need this for tilesets that have no image. they use image collections and we need the path to save the new atlas we generate.
		public string MapFolder;
		public bool IsStandardTileset = true;

		[XmlAttribute(AttributeName = "firstgid")]
		public int FirstGid;

		[XmlAttribute(AttributeName = "source")]
		public string Source;

		[XmlAttribute(AttributeName = "name")] public string Name;

		[XmlAttribute(AttributeName = "tilewidth")]
		public int TileWidth;

		[XmlAttribute(AttributeName = "tileheight")]
		public int TileHeight;

		[XmlAttribute(AttributeName = "spacing")]
		public int Spacing;

		[XmlAttribute(AttributeName = "margin")]
		public int Margin;

		[XmlAttribute(AttributeName = "tilecount")]
		public int TileCount;

		[XmlAttribute(AttributeName = "columns")]
		public int Columns;

		[XmlElement(ElementName = "tileoffset")]
		public TmxTileOffset TileOffset;

		[XmlElement(ElementName = "tile")] public List<TmxTilesetTile> Tiles;

		[XmlArray("properties")] [XmlArrayItem("property")]
		public List<TmxProperty> Properties;

		[XmlElement(ElementName = "image")] public TmxImage Image;

		[XmlArray("terraintypes")] [XmlArrayItem("terrain")]
		public List<TmxTerrain> TerrainTypes;


		public TmxTileset()
		{
			TileOffset = new TmxTileOffset();
			Tiles = new List<TmxTilesetTile>();
			Properties = new List<TmxProperty>();
			TerrainTypes = new List<TmxTerrain>();
		}


		public void FixImagePath(string mapPath, string tilesetSource)
		{
			var mapDirectory = Path.GetDirectoryName(mapPath);
			var tilesetDirectory = Path.GetDirectoryName(tilesetSource);
			var imageDirectory = Path.GetDirectoryName(this.Image.Source);
			var imageFile = Path.GetFileName(this.Image.Source);

			var newPath = Path.GetFullPath(Path.Combine(mapDirectory, tilesetDirectory, imageDirectory, imageFile));
			Image.Source = Path.Combine(PathHelper.MakeRelativePath(mapPath, newPath));
		}


		public override string ToString()
		{
			return string.Format("{0}: {1}", Name, Image);
		}
	}
}