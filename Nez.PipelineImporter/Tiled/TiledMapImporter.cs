using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;


namespace Nez.TiledMaps
{
	[ContentImporter(".tmx", DefaultProcessor = "TiledMapProcessor", DisplayName = "Tiled Map Importer")]
	public class TiledMapImporter : ContentImporter<TmxMap>
	{
		public override TmxMap Import(string filename, ContentImporterContext context)
		{
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));

			using (var reader = new StreamReader(filename))
			{
				context.Logger.LogMessage("Deserializing filename: {0}", filename);

				var serializer = new XmlSerializer(typeof(TmxMap));
				var map = (TmxMap) serializer.Deserialize(reader);
				var xmlSerializer = new XmlSerializer(typeof(TmxTileset));

				foreach (var l in map.Layers)
					context.Logger.LogMessage("Deserialized Layer: {0}", l);

				foreach (var o in map.ObjectGroups)
					context.Logger.LogMessage("Deserialized ObjectGroup: {0}, object count: {1}", o.Name,
						o.Objects.Count);

				context.Logger.LogMessage("");

				for (var i = 0; i < map.Tilesets.Count; i++)
				{
					var tileset = map.Tilesets[i];
					if (!string.IsNullOrWhiteSpace(tileset.Source))
					{
						var directoryName = Path.GetDirectoryName(filename);
						var tilesetLocation = tileset.Source.Replace('/', Path.DirectorySeparatorChar);
						var filePath = Path.Combine(directoryName, tilesetLocation);

						var normExtTilesetPath = new DirectoryInfo(filePath).FullName;
						context.Logger.LogMessage("Reading External Tileset File: " + normExtTilesetPath);
						using (var file = new StreamReader(filePath))
						{
							map.Tilesets[i] = (TmxTileset) xmlSerializer.Deserialize(file);
							map.Tilesets[i].FixImagePath(filename, tileset.Source);
							map.Tilesets[i].FirstGid = tileset.FirstGid;
						}
					}
					else
					{
						tileset.MapFolder = Path.GetDirectoryName(Path.GetFullPath(filename));
					}
				}

				return map;
			}
		}
	}
}