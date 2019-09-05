using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledMap
	{
		#region Fields and Properties

		public readonly int FirstGid;
		public readonly int Width;
		public readonly int Height;
		public readonly int TileWidth;
		public readonly int TileHeight;
		public Color? BackgroundColor;
		public TiledRenderOrder RenderOrder;
		public readonly TiledMapOrientation Orientation;
		public readonly Dictionary<string, string> Properties = new Dictionary<string, string>();
		public readonly List<TiledLayer> Layers = new List<TiledLayer>();
		public readonly List<TiledImageLayer> ImageLayers = new List<TiledImageLayer>();
		public readonly List<TiledObjectGroup> ObjectGroups = new List<TiledObjectGroup>();
		public readonly List<TiledTileset> Tilesets = new List<TiledTileset>();

		public int WidthInPixels
		{
			get { return Width * TileWidth; }
		}

		public int HeightInPixels
		{
			get { return Height * TileHeight; }
		}

		public int LargestTileWidth;
		public int LargestTileHeight;

		internal bool requiresLargeTileCulling;
		internal List<TiledAnimatedTile> _animatedTiles = new List<TiledAnimatedTile>();

		#endregion


		public TiledMap(
			int firstGid,
			int width,
			int height,
			int tileWidth,
			int tileHeight,
			TiledMapOrientation orientation = TiledMapOrientation.Orthogonal)
		{
			this.FirstGid = firstGid;
			this.Width = width;
			this.Height = height;
			this.TileWidth = tileWidth;
			this.TileHeight = tileHeight;
			this.Orientation = orientation;
		}


		#region Tileset and Layer creation

		public TiledTileset CreateTileset(Texture2D texture, int firstId, int tileWidth, int tileHeight,
		                                  bool isStandardTileset, int spacing = 2, int margin = 2, int tileCount = 1,
		                                  int columns = 1)
		{
			TiledTileset tileset;
			if (isStandardTileset)
				tileset = new TiledTileset(texture, firstId, tileWidth, tileHeight, spacing, margin, tileCount,
					columns);
			else
				tileset = new TiledImageCollectionTileset(texture, firstId);

			if (tileset.TileWidth > LargestTileWidth)
				LargestTileWidth = tileset.TileWidth;

			if (tileset.TileHeight > LargestTileHeight)
				LargestTileHeight = tileset.TileHeight;

			Tilesets.Add(tileset);

			return tileset;
		}


		public TiledLayer CreateTileLayer(string name, int width, int height)
		{
			if (Orientation == TiledMapOrientation.Orthogonal)
			{
				var layer = new TiledTileLayer(this, name, width, height);
				Layers.Add(layer);
				return layer;
			}

			if (Orientation == TiledMapOrientation.Isometric)
			{
				var layer = new TiledIsometricTiledLayer(this, name, width, height);
				Layers.Add(layer);
				return layer;
			}

			throw new NotImplementedException();
		}


		public TiledLayer CreateTileLayer(string name, int width, int height, TiledTile[] tiles)
		{
			if (Orientation == TiledMapOrientation.Orthogonal)
			{
				var layer = new TiledTileLayer(this, name, width, height, tiles);
				Layers.Add(layer);
				return layer;
			}

			if (Orientation == TiledMapOrientation.Isometric)
			{
				var layer = new TiledIsometricTiledLayer(this, name, width, height, tiles);
				Layers.Add(layer);
				return layer;
			}

			throw new NotImplementedException();
		}


		public TiledImageLayer CreateImageLayer(string name, Texture2D texture)
		{
			var layer = new TiledImageLayer(name, texture);
			Layers.Add(layer);
			return layer;
		}

		#endregion


		#region Tileset and Layer getters

		/// <summary>
		/// gets the TiledTileset for the given tileId
		/// </summary>
		/// <returns>The tileset for tile identifier.</returns>
		/// <param name="tileId">Identifier.</param>
		public TiledTileset GetTilesetForTileId(int tileId)
		{
			for (var i = Tilesets.Count - 1; i >= 0; i--)
			{
				if (Tilesets[i].FirstId <= tileId)
					return Tilesets[i];
			}

			throw new Exception(string.Format("tileId {0} was not foind in any tileset", tileId));
		}


		/// <summary>
		/// returns the TiledTilesetTile for the given id or null if none exists. TiledTilesetTiles exist only for animated tiles and tiles with
		/// properties set.
		/// </summary>
		/// <returns>The tileset tile.</returns>
		/// <param name="id">Identifier.</param>
		public TiledTilesetTile GetTilesetTile(int id)
		{
			for (var i = Tilesets.Count - 1; i >= 0; i--)
			{
				if (Tilesets[i].FirstId <= id)
				{
					for (var j = 0; j < Tilesets[i].Tiles.Count; j++)
					{
						// id is a gid so we need to subtract the tileset.firstId to get a local id
						if (Tilesets[i].Tiles[j].Id == id - Tilesets[i].FirstId)
							return Tilesets[i].Tiles[j];
					}
				}
			}

			return null;
		}


		/// <summary>
		/// gets the index in the layers List of the layer with name
		/// </summary>
		/// <returns>The layer index.</returns>
		/// <param name="name">Name.</param>
		public int GetLayerIndex(string name)
		{
			for (var i = 0; i < Layers.Count; i++)
			{
				if (Layers[i].Name == name)
					return i;
			}

			throw new Exception("could not find the layer: " + name);
		}


		/// <summary>
		/// gets the TiledLayer by name
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="name">Name.</param>
		public TiledLayer GetLayer(string name)
		{
			for (var i = 0; i < Layers.Count; i++)
			{
				if (Layers[i].Name == name)
					return Layers[i];
			}

			return null;
		}


		/// <summary>
		/// gets the TiledLayer at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <returns>The Layer.</returns>
		public TiledLayer GetLayer(int index)
		{
			return Layers[index];
		}


		/// <summary>
		/// gets the TiledLayer by index
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetLayer<T>(int index) where T : TiledLayer
		{
			return (T) GetLayer(index);
		}


		/// <summary>
		/// gets the TiledLayer by name
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetLayer<T>(string name) where T : TiledLayer
		{
			return (T) GetLayer(name);
		}


		/// <summary>
		/// gets the TiledObjectGroup with the given name
		/// </summary>
		/// <returns>The object group.</returns>
		/// <param name="name">Name.</param>
		public TiledObjectGroup GetObjectGroup(string name)
		{
			for (var i = 0; i < ObjectGroups.Count; i++)
			{
				if (ObjectGroups[i].Name == name)
					return ObjectGroups[i];
			}

			return null;
		}

		#endregion


		/// <summary>
		/// handles calling update on all animated tiles
		/// </summary>
		public void Update()
		{
			for (var i = 0; i < _animatedTiles.Count; i++)
				_animatedTiles[i].Update();
		}


		/// <summary>
		/// calls draw on each visible layer
		/// </summary>
		/// <param name="batcher">Sprite batch.</param>
		/// <param name="position">Position.</param>
		/// <param name="layerDepth">Layer depth.</param>
		/// <param name="cameraClipBounds">Camera clip bounds.</param>
		public void Draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds)
		{
			Draw(batcher, position, Vector2.One, layerDepth, cameraClipBounds);
		}


		public void Draw(Batcher batcher, Vector2 position, Vector2 scale, float layerDepth,
		                 RectangleF cameraClipBounds)
		{
			// render any visible image or tile layer
			foreach (var layer in Layers)
			{
				if (!layer.Visible)
					continue;

				layer.Draw(batcher, position, scale, layerDepth, cameraClipBounds);
			}
		}


		#region world/local conversion

		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="pos">Position.</param>
		public Point WorldToTilePosition(Vector2 pos, bool clampToTilemapBounds = true)
		{
			return new Point(WorldToTilePositionX(pos.X, clampToTilemapBounds),
				WorldToTilePositionY(pos.Y, clampToTilemapBounds));
		}

		/// <summary>
		/// converts from world to tile position for isometric map clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="pos">Position.</param>
		public Point IsometricWorldToTilePosition(Vector2 pos, bool clampToTilemapBounds = true)
		{
			return IsometricWorldToTilePosition(pos.X, pos.Y, clampToTilemapBounds);
		}

		/// <summary>
		/// converts from world to tile position for isometric map clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public Point IsometricWorldToTilePosition(float x, float y, bool clampToTilemapBounds = true)
		{
			x -= (Height - 1) * TileWidth / 2;
			var tileX = Mathf.FastFloorToInt((y / TileHeight) + (x / TileWidth));
			var tileY = Mathf.FastFloorToInt((-x / TileWidth) + (y / TileHeight));
			if (!clampToTilemapBounds)
				return new Point(tileX, tileY);

			return new Point(Mathf.Clamp(tileX, 0, Width - 1), Mathf.Clamp(tileY, 0, Height - 1));
		}

		/// converts from isometric tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="pos">Position.</param>
		public Vector2 IsometricTileToWorldPosition(Point pos)
		{
			return IsometricTileToWorldPosition(pos.X, pos.Y);
		}

		/// <summary>
		/// converts from isometric tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public Vector2 IsometricTileToWorldPosition(int x, int y)
		{
			var worldX = x * TileWidth / 2 - y * TileWidth / 2 + (Height - 1) * TileWidth / 2;
			var worldY = y * TileHeight / 2 + x * TileHeight / 2;
			return new Vector2(worldX, worldY);
		}

		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position x.</returns>
		/// <param name="x">The x coordinate.</param>
		public int WorldToTilePositionX(float x, bool clampToTilemapBounds = true)
		{
			var tileX = Mathf.FastFloorToInt(x / TileWidth);
			if (!clampToTilemapBounds)
				return tileX;

			return Mathf.Clamp(tileX, 0, Width - 1);
		}


		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position y.</returns>
		/// <param name="y">The y coordinate.</param>
		public int WorldToTilePositionY(float y, bool clampToTilemapBounds = true)
		{
			var tileY = Mathf.FastFloorToInt(y / TileHeight);
			if (!clampToTilemapBounds)
				return tileY;

			return Mathf.Clamp(tileY, 0, Height - 1);
		}

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="pos">Position.</param>
		public Vector2 TileToWorldPosition(Point pos)
		{
			return new Vector2(TileToWorldPositionX(pos.X), TileToWorldPositionY(pos.Y));
		}

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position x.</returns>
		/// <param name="x">The x coordinate.</param>
		public int TileToWorldPositionX(int x)
		{
			return x * TileWidth;
		}


		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position y.</returns>
		/// <param name="y">The y coordinate.</param>
		public int TileToWorldPositionY(int y)
		{
			return y * TileHeight;
		}

		#endregion
	}
}