using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nez.Pipeline.Content;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Tiled
{
	public class TiledMapReader : ContentTypeReader<TiledMap>
	{
		protected override TiledMap Read(ContentReader reader, TiledMap existingInstance)
		{
			var backgroundColor = reader.ReadColor();
			var renderOrder = (TiledRenderOrder) Enum.Parse(typeof(TiledRenderOrder), reader.ReadString(), true);
			var tiledMap = new TiledMap(firstGid: reader.ReadInt32(),
				width: reader.ReadInt32(),
				height: reader.ReadInt32(),
				tileWidth: reader.ReadInt32(),
				tileHeight: reader.ReadInt32(),
				orientation: (TiledMapOrientation) reader.ReadInt32())
			{
				BackgroundColor = backgroundColor,
				RenderOrder = renderOrder
			};
			tiledMap.LargestTileWidth = reader.ReadInt32();
			tiledMap.LargestTileHeight = reader.ReadInt32();

			// determine if we have some tiles that are larger than our standard tile size and if so mark this map for requiring culling
			if (tiledMap.LargestTileWidth > tiledMap.TileWidth || tiledMap.LargestTileHeight > tiledMap.TileHeight)
				tiledMap.requiresLargeTileCulling = true;

			ReadCustomProperties(reader, tiledMap.Properties);

			var tilesetCount = reader.ReadInt32();
			for (var i = 0; i < tilesetCount; i++)
			{
				var isStandardTileset = reader.ReadBoolean();

				// image collections will have not texture associated with them
				var textureName = reader.ReadString();
				Texture2D texture = null;
				if (textureName != string.Empty)
				{
					var textureAssetName = reader.GetRelativeAssetPath(textureName);
					texture = reader.ContentManager.Load<Texture2D>(textureAssetName);
				}

				var tileset = tiledMap.CreateTileset(
					texture: texture,
					firstId: reader.ReadInt32(),
					tileWidth: reader.ReadInt32(),
					tileHeight: reader.ReadInt32(),
					isStandardTileset: isStandardTileset,
					spacing: reader.ReadInt32(),
					margin: reader.ReadInt32(),
					tileCount: reader.ReadInt32(),
					columns: reader.ReadInt32());
				ReadCustomProperties(reader, tileset.Properties);

				// tiledset tile array
				var tileCount = reader.ReadInt32();
				for (var j = 0; j < tileCount; j++)
				{
					var tile = new TiledTilesetTile(reader.ReadInt32(), tiledMap);

					var tileAnimationFrameCount = reader.ReadInt32();
					if (tileAnimationFrameCount > 0)
						tile.AnimationFrames = new List<TiledTileAnimationFrame>(tileAnimationFrameCount);

					for (var k = 0; k < tileAnimationFrameCount; k++)
						tile.AnimationFrames.Add(new TiledTileAnimationFrame(reader.ReadInt32(), reader.ReadSingle()));

					// image source is optional
					var isFromImageCollection = reader.ReadBoolean();
					if (isFromImageCollection)
					{
						var rect = new Rectangle(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(),
							reader.ReadInt32());
						((TiledImageCollectionTileset) tileset).SetTileTextureRegion(tileset.FirstId + tile.Id, rect);
					}

					ReadCustomProperties(reader, tile.Properties);

					var tileObjectGroupCount = reader.ReadInt32();
					if (tileObjectGroupCount > 0)
						tile.ObjectGroups = new List<TiledObjectGroup>(tileObjectGroupCount);
					for (var k = 0; k < tileObjectGroupCount; k++)
						tile.ObjectGroups.Add(ReadObjectGroup(reader, tiledMap));

					// give the TiledTilesetTile a chance to process and cache any data required
					tile.ProcessProperties();

					// if this tile is from an image collection and it has no properties there is no need to keep it around since we
					// already grabbed the image source above
					if (!(isFromImageCollection && tile.Properties.Count == 0))
						tileset.Tiles.Add(tile);
				}
			}

			var layerCount = reader.ReadInt32();
			for (var i = 0; i < layerCount; i++)
			{
				var layer = ReadLayer(reader, tiledMap);
				ReadCustomProperties(reader, layer.Properties);
			}


			var objectGroupCount = reader.ReadInt32();
			for (var i = 0; i < objectGroupCount; i++)
				tiledMap.ObjectGroups.Add(ReadObjectGroup(reader, tiledMap));

			return tiledMap;
		}


		static void ReadCustomProperties(ContentReader reader, Dictionary<string, string> properties)
		{
			var count = reader.ReadInt32();
			for (var i = 0; i < count; i++)
				properties.Add(reader.ReadString(), reader.ReadString());
		}


		static TiledLayer ReadLayer(ContentReader reader, TiledMap tiledMap)
		{
			var layerName = reader.ReadString();
			var visible = reader.ReadBoolean();
			var opacity = reader.ReadSingle();
			var offset = reader.ReadVector2();
			var layerType = (TiledLayerType) reader.ReadInt32();

			TiledLayer layer;
			if (layerType == TiledLayerType.Tile)
				layer = ReadTileLayer(reader, tiledMap, layerName);
			else if (layerType == TiledLayerType.Image)
				layer = ReadImageLayer(reader, tiledMap, layerName);
			else
				throw new NotSupportedException(string.Format("Layer type {0} with name {1} is not supported",
					layerType, layerName));

			layer.Offset = offset;
			layer.Visible = visible;
			layer.Opacity = opacity;

			return layer;
		}


		static TiledLayer ReadTileLayer(ContentReader reader, TiledMap tileMap, string layerName)
		{
			var tileCount = reader.ReadInt32();
			var tiles = new TiledTile[tileCount];

			for (var i = 0; i < tileCount; i++)
			{
				var tileId = reader.ReadInt32();
				var flippedHorizonally = reader.ReadBoolean();
				var flippedVertically = reader.ReadBoolean();
				var flippedDiagonally = reader.ReadBoolean();

				// dont add null tiles
				if (tileId != 0)
				{
					var tilesetTile = tileMap.GetTilesetTile(tileId);
					if (tilesetTile != null && tilesetTile.AnimationFrames != null)
					{
						if (tilesetTile.AnimationFrames.Count > 0)
						{
							tiles[i] = new TiledAnimatedTile(tileId, tilesetTile)
							{
								FlippedHorizonally = flippedHorizonally,
								FlippedVertically = flippedVertically,
								FlippedDiagonally = flippedDiagonally
							};
							tileMap._animatedTiles.Add(tiles[i] as TiledAnimatedTile);
						}
					}
					else
					{
						tiles[i] = new TiledTile(tileId)
						{
							FlippedHorizonally = flippedHorizonally,
							FlippedVertically = flippedVertically,
							FlippedDiagonally = flippedDiagonally
						};
					}

					tiles[i].Tileset = tileMap.GetTilesetForTileId(tileId);
				}
				else
				{
					tiles[i] = null;
				}
			}

			return tileMap.CreateTileLayer(
				name: layerName,
				width: reader.ReadInt32(),
				height: reader.ReadInt32(),
				tiles: tiles);
		}


		static TiledImageLayer ReadImageLayer(ContentReader reader, TiledMap tileMap, string layerName)
		{
			var assetName = reader.GetRelativeAssetPath(reader.ReadString());
			var texture = reader.ContentManager.Load<Texture2D>(assetName);

			return tileMap.CreateImageLayer(layerName, texture);
		}


		static TiledObjectGroup ReadObjectGroup(ContentReader reader, TiledMap tiledMap)
		{
			var objectGroup = new TiledObjectGroup(
				reader.ReadString(), reader.ReadColor(), reader.ReadBoolean(), reader.ReadSingle());

			ReadCustomProperties(reader, objectGroup.Properties);

			var objectCount = reader.ReadInt32();
			objectGroup.Objects = new TiledObject[objectCount];
			for (var i = 0; i < objectCount; i++)
			{
				var obj = new TiledObject()
				{
					Id = reader.ReadInt32(),
					Name = reader.ReadString(),
					Type = reader.ReadString(),
					X = reader.ReadInt32(),
					Y = reader.ReadInt32(),
					Width = reader.ReadInt32(),
					Height = reader.ReadInt32(),
					Rotation = reader.ReadInt32(),
					Gid = reader.ReadInt32(),
					Visible = reader.ReadBoolean()
				};

				var tiledObjectType = reader.ReadString();
				if (tiledObjectType == "ellipse")
				{
					// ellipse has no extra props
					obj.TiledObjectType = TiledObjectType.Ellipse;
				}
				else if (tiledObjectType == "image")
				{
					obj.TiledObjectType = TiledObjectType.Image;
					throw new NotImplementedException("Image objects are not yet implemented");
				}
				else if (tiledObjectType == "polygon")
				{
					obj.TiledObjectType = TiledObjectType.Polygon;
					obj.PolyPoints = ReadVector2Array(reader);
				}
				else if (tiledObjectType == "polyline")
				{
					obj.TiledObjectType = TiledObjectType.Polyline;
					obj.PolyPoints = ReadVector2Array(reader);
				}
				else
				{
					obj.TiledObjectType = TiledObjectType.None;
				}

				obj.ObjectType = reader.ReadString();

				ReadCustomProperties(reader, obj.Properties);

				objectGroup.Objects[i] = obj;
			}

			return objectGroup;
		}


		static Vector2[] ReadVector2Array(ContentReader reader)
		{
			var pointCount = reader.ReadInt32();
			var points = new Vector2[pointCount];

			for (var i = 0; i < pointCount; i++)
				points[i] = reader.ReadVector2();

			return points;
		}
	}
}