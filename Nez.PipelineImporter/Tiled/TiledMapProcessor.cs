using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Zlib;
using Microsoft.Xna.Framework.Content.Pipeline;
using System.ComponentModel;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework;
using Nez.TextureAtlasGenerator;
using Microsoft.Xna.Framework.Graphics;
using Nez.PipelineImporter;


namespace Nez.TiledMaps
{
	[ContentProcessor(DisplayName = "Tiled Map Processor")]
	public class TiledMapProcessor : ContentProcessor<TmxMap, TmxMap>
	{
		public static ContentBuildLogger Logger;


		public override TmxMap Process(TmxMap map, ContentProcessorContext context)
		{
			Logger = context.Logger;
			foreach (var layer in map.Layers.OfType<TmxTileLayer>())
			{
				var data = layer.Data;

				if (data.Encoding == "csv")
				{
					data.Tiles = data.Value
						.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
						.Select(uint.Parse)
						.Select(gid => new TmxDataTile((uint) gid))
						.ToList();
				}
				else if (data.Encoding == "base64")
				{
					var encodedData = data.Value.Trim();
					var decodedData = Convert.FromBase64String(encodedData);

					using (var stream = OpenStream(decodedData, data.Compression))
					using (var reader = new BinaryReader(stream))
					{
						data.Tiles = new List<TmxDataTile>();

						for (var y = 0; y < layer.Width; y++)
						{
							for (var x = 0; x < layer.Height; x++)
							{
								var gid = reader.ReadUInt32();
								data.Tiles.Add(new TmxDataTile(gid));
							}
						}
					}
				}
			}

			// deal with tilesets that have image collections
			foreach (var tileset in map.Tilesets)
				SetTilesetTextureIfNecessary(tileset, context);

			return map;
		}


		static void SetTilesetTextureIfNecessary(TmxTileset tileset, ContentProcessorContext context)
		{
			if (tileset.Image != null)
				return;

			tileset.IsStandardTileset = false;

			var imagePaths = new List<string>();
			foreach (var tile in tileset.Tiles)
			{
				if (tile.Image != null && !imagePaths.Contains(tile.Image.Source))
					imagePaths.Add(tile.Image.Source);
			}

			context.Logger.LogMessage("\n\t --- need to pack images: {0}\n", imagePaths.Count);
			var sourceSprites = new List<BitmapContent>();

			// Loop over each input sprite filename
			foreach (var inputFilename in imagePaths)
			{
				// Store the name of this sprite.
				var spriteName = Path.GetFileName(inputFilename);

				var absolutePath = PathHelper.GetAbsolutePath(inputFilename, tileset.MapFolder);
				context.Logger.LogMessage("Adding texture: {0}", spriteName);

				// Load the sprite texture into memory.
				var textureReference = new ExternalReference<TextureContent>(absolutePath);
				var texture =
					context.BuildAndLoadAsset<TextureContent, TextureContent>(textureReference, "TextureProcessor");

				sourceSprites.Add(texture.Faces[0][0]);
			}

			var spriteRectangles = new List<Rectangle>();

			// pack all the sprites into a single large texture.
			var packedSprites = TextureAtlasPacker.PackSprites(sourceSprites, spriteRectangles, false, context);
			context.Logger.LogMessage("packed: {0}", packedSprites);

			// save out a PNG with our atlas
			var bm = new System.Drawing.Bitmap(packedSprites.Width, packedSprites.Height);
			for (var x = 0; x < packedSprites.Width; x++)
			{
				for (var y = 0; y < packedSprites.Height; y++)
				{
					var col = packedSprites.GetPixel(x, y);
					var color = System.Drawing.Color.FromArgb(col.A, col.R, col.G, col.B);
					bm.SetPixel(x, y, color);
				}
			}

			var atlasFilename = tileset.Name + "-atlas.png";
			bm.Save(Path.Combine(tileset.MapFolder, atlasFilename), System.Drawing.Imaging.ImageFormat.Png);
			context.Logger.LogImportantMessage("\n-- generated atlas {0}. Make sure you add it to the Pipeline tool!",
				atlasFilename);

			// set the new atlas as our tileset source image
			tileset.Image = new TmxImage();
			tileset.Image.Source = atlasFilename;

			// last step: set the new atlas info and source rectangle for each tile
			foreach (var tile in tileset.Tiles)
			{
				if (tile.Image == null)
					continue;

				tile.SourceRect = spriteRectangles[imagePaths.IndexOf(tile.Image.Source)];
			}
		}


		private static Stream OpenStream(byte[] decodedData, string compressionMode)
		{
			var memoryStream = new MemoryStream(decodedData, writable: false);

			if (compressionMode == "gzip")
				return new GZipStream(memoryStream, CompressionMode.Decompress);

			if (compressionMode == "zlib")
				return new ZlibStream(memoryStream, CompressionMode.Decompress);

			return memoryStream;
		}
	}
}