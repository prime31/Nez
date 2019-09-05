using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Nez.Tiled;


namespace Nez.TiledMaps
{
	[ContentTypeWriter]
	public class TiledMapWriter : ContentTypeWriter<TmxMap>
	{
		const uint FLIPPED_VERTICALLY_FLAG = 0x40000000;
		const uint FLIPPED_HORIZONTALLY_FLAG = 0x80000000;
		const uint FLIPPED_DIAGONALLY_FLAG = 0x20000000;


		protected override void Write(ContentWriter writer, TmxMap map)
		{
			int largestTileWidth = 0, largestTileHeight = 0;
			foreach (var tileset in map.Tilesets)
			{
				largestTileWidth = Math.Max(largestTileWidth, tileset.TileWidth);
				largestTileHeight = Math.Max(largestTileHeight, tileset.TileHeight);
			}

			writer.Write(HexToColor(map.BackgroundColor));
			writer.Write(map.RenderOrder.ToString());
			writer.Write(map.FirstGid);
			writer.Write(map.Width);
			writer.Write(map.Height);
			writer.Write(map.TileWidth);
			writer.Write(map.TileHeight);
			writer.Write(Convert.ToInt32(map.Orientation));
			writer.Write(largestTileWidth);
			writer.Write(largestTileHeight);
			WriteCustomProperties(writer, map.Properties);

			writer.Write(map.Tilesets.Count);
			foreach (var tileset in map.Tilesets)
			{
				if (tileset.Image != null)
					TiledMapProcessor.Logger.LogMessage("\nExpecting texture asset: {0}\n", tileset.Image.Source);

				writer.Write(tileset.IsStandardTileset);

				if (tileset.Image != null)
					writer.Write(RemoveExtension(tileset.Image.Source));
				else
					writer.Write(string.Empty);

				writer.Write(tileset.FirstGid);
				writer.Write(tileset.TileWidth);
				writer.Write(tileset.TileHeight);
				writer.Write(tileset.Spacing);
				writer.Write(tileset.Margin);
				writer.Write(tileset.TileCount);
				writer.Write(tileset.Columns);
				WriteCustomProperties(writer, tileset.Properties);

				writer.Write(tileset.Tiles.Count);
				foreach (var tile in tileset.Tiles)
				{
					TiledMapProcessor.Logger.LogMessage("writing tile: {0}", tile);
					writer.Write(tile.Id);

					// animation frames
					writer.Write(tile.AnimationFrames.Count);
					foreach (var anim in tile.AnimationFrames)
					{
						writer.Write(anim.TileId);
						writer.Write(anim.Duration);
					}

					// image is optional
					if (tile.Image != null)
					{
						writer.Write(true);
						writer.Write(tile.SourceRect.X);
						writer.Write(tile.SourceRect.Y);
						writer.Write(tile.SourceRect.Width);
						writer.Write(tile.SourceRect.Height);
					}
					else
					{
						writer.Write(false);
					}

					WriteCustomProperties(writer, tile.Properties);
					WriteObjectGroups(writer, tile.ObjectGroups);
				}
			}


			writer.Write(map.Layers.Count);
			foreach (var layer in map.Layers)
			{
				writer.Write(layer.Name);
				writer.Write(layer.Visible);
				writer.Write(layer.Opacity);
				writer.Write(new Vector2(layer.Offsetx, layer.Offsety));

				var tileLayer = layer as TmxTileLayer;
				if (tileLayer != null)
				{
					writer.Write((int) TiledLayerType.Tile);
					writer.Write(tileLayer.Data.Tiles.Count);
					foreach (var tile in tileLayer.Data.Tiles)
					{
						// Read out the flags
						var flippedHorizontally = (tile.Gid & FLIPPED_HORIZONTALLY_FLAG) != 0;
						var flippedVertically = (tile.Gid & FLIPPED_VERTICALLY_FLAG) != 0;
						var flippedDiagonally = (tile.Gid & FLIPPED_DIAGONALLY_FLAG) != 0;

						if (flippedHorizontally || flippedVertically || flippedDiagonally)
						{
							// Clear the flags
							tile.Gid &= ~(FLIPPED_HORIZONTALLY_FLAG | FLIPPED_VERTICALLY_FLAG |
							              FLIPPED_DIAGONALLY_FLAG);
							tile.FlippedHorizontally = flippedHorizontally;
							tile.FlippedVertically = flippedVertically;
							tile.FlippedDiagonally = flippedDiagonally;
						}

						writer.Write(tile.Gid);
						writer.Write(tile.FlippedHorizontally);
						writer.Write(tile.FlippedVertically);
						writer.Write(tile.FlippedDiagonally);
					}

					writer.Write(tileLayer.Width);
					writer.Write(tileLayer.Height);
				}

				var imageLayer = layer as TmxImageLayer;
				if (imageLayer != null)
				{
					writer.Write((int) TiledLayerType.Image);
					writer.Write(RemoveExtension(imageLayer.Image.Source));

					TiledMapProcessor.Logger.LogMessage("Expecting texture asset: {0}\n", imageLayer.Image.Source);
				}

				WriteCustomProperties(writer, layer.Properties);
				TiledMapProcessor.Logger.LogMessage("done writing Layer: {0}", layer);
			}

			WriteObjectGroups(writer, map.ObjectGroups);
		}

		static void WriteObjectGroups(ContentWriter writer, List<TmxObjectGroup> objectGroups)
		{
			writer.Write(objectGroups.Count);
			foreach (var group in objectGroups)
			{
				writer.Write(group.Name ?? string.Empty);
				writer.Write(HexToColor(group.Color));
				writer.Write(group.Visible);
				writer.Write(group.Opacity);

				WriteCustomProperties(writer, group.Properties);

				writer.Write(group.Objects.Count);
				foreach (var obj in group.Objects)
				{
					writer.Write(obj.Id);
					writer.Write(obj.Name ?? string.Empty);
					writer.Write(obj.Type ?? string.Empty);
					writer.Write((int) obj.X);
					writer.Write((int) obj.Y);
					writer.Write((int) obj.Width);
					writer.Write((int) obj.Height);
					writer.Write(obj.Rotation);
					writer.Write(obj.Gid);
					writer.Write(obj.Visible);

					if (obj.Ellipse != null)
					{
						writer.Write("ellipse");
					}
					else if (obj.Image != null)
					{
						writer.Write("image");
					}
					else if (obj.Polygon != null)
					{
						writer.Write("polygon");
						WritePointList(writer, obj, obj.Polygon.Points);
					}
					else if (obj.Polyline != null)
					{
						writer.Write("polyline");
						WritePointList(writer, obj, obj.Polyline.Points);
					}
					else
					{
						writer.Write("none");
					}

					writer.Write(obj.Type ?? string.Empty);

					WriteCustomProperties(writer, obj.Properties);
				}

				TiledMapProcessor.Logger.LogMessage("done writing ObjectGroup: {0}", group);
			}
		}


		static void WritePointList(ContentWriter writer, TmxObject obj, List<Vector2> points)
		{
			writer.Write(points.Count);
			for (var i = 0; i < points.Count; i++)
			{
				// here we offset each point by the actual x/y value of the object so they can be used directly
				var pt = points[i];
				pt.X += obj.X;
				pt.Y += obj.Y;
				writer.Write(pt);
			}
		}


		static void WriteCustomProperties(ContentWriter writer, List<TmxProperty> properties)
		{
			writer.Write(properties.Count);
			foreach (var mapProperty in properties)
			{
				writer.Write(mapProperty.Name);
				writer.Write(mapProperty.Value);
			}
		}


		static Color HexToColor(string hexValue)
		{
			if (string.IsNullOrEmpty(hexValue))
				return new Color(128, 128, 128);

			hexValue = hexValue.TrimStart('#');
			var r = int.Parse(hexValue.Substring(0, 2), NumberStyles.HexNumber);
			var g = int.Parse(hexValue.Substring(2, 2), NumberStyles.HexNumber);
			var b = int.Parse(hexValue.Substring(4, 2), NumberStyles.HexNumber);
			return new Color(r, g, b);
		}


		static string RemoveExtension(string path)
		{
			var dotIndex = path.LastIndexOf('.');
			return dotIndex > 0 ? path.Substring(0, dotIndex) : path;
		}


		public override string GetRuntimeType(TargetPlatform targetPlatform)
		{
			return typeof(Nez.Tiled.TiledMap).AssemblyQualifiedName;
		}


		public override string GetRuntimeReader(TargetPlatform targetPlatform)
		{
			return typeof(Nez.Tiled.TiledMapReader).AssemblyQualifiedName;
		}
	}
}