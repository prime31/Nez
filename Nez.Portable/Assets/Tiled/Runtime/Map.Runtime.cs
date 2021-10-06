using System;
using Microsoft.Xna.Framework;

namespace Nez.Tiled
{
	/// <summary>
	/// contains runtime querying and other helper methods seperate from the tmx parsing in the other partial
	/// </summary>
	public partial class TmxMap : TmxDocument
	{
		#region Tileset and Layer getters

		/// <summary>
		/// gets the TiledTileset for the given tileId
		/// </summary>
		/// <returns>The tileset for tile identifier.</returns>
		/// <param name="gid">Identifier.</param>
		public TmxTileset GetTilesetForTileGid(int gid)
		{
			if (gid == 0)
				return null;

			for (var i = Tilesets.Count - 1; i >= 0; i--)
			{
				if (Tilesets[i].FirstGid <= gid)
					return Tilesets[i];
			}

			throw new Exception(string.Format("tile gid {0} was not found in any tileset", gid));
		}

		/// <summary>
		/// returns the TmxTilesetTile for the given id or null if none exists. TmxTilesetTiles exist only for animated tiles
		/// and tiles with properties set.
		/// </summary>
		/// <returns>The tileset tile.</returns>
		/// <param name="gid">Identifier.</param>
		public TmxTilesetTile GetTilesetTile(int gid)
		{
			for (var i = Tilesets.Count - 1; i >= 0; i--)
			{
				if (Tilesets[i].FirstGid <= gid)
				{
					if (Tilesets[i].Tiles.TryGetValue(gid - Tilesets[i].FirstGid, out var tilesetTile))
						return tilesetTile;
				}
			}

			return null;
		}

		/// <summary>
		/// gets the TiledLayer by name
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="name">Name.</param>
		public ITmxLayer GetLayer(string name) => Layers.Contains(name) ? Layers[name] : null;

		/// <summary>
		/// gets the ITmxLayer by index
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="index">Index.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetLayer<T>(int index) where T : ITmxLayer => (T)Layers[index];

		/// <summary>
		/// gets the ITmxLayer by name
		/// </summary>
		/// <returns>The layer.</returns>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetLayer<T>(string name) where T : ITmxLayer => (T)GetLayer(name);

		/// <summary>
		/// gets the TmxObjectGroup with the given name
		/// </summary>
		/// <returns>The object group.</returns>
		/// <param name="name">Name.</param>
		public TmxObjectGroup GetObjectGroup(string name) => ObjectGroups.Contains(name) ? ObjectGroups[name] : null;

		#endregion

		#region world/local conversion

		/// <summary>
		/// converts from world to tile position clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="pos">Position.</param>
		public Point WorldToTilePosition(Vector2 pos, bool clampToTilemapBounds = true)
		{
			if (Orientation == OrientationType.Isometric)
			{
				return IsometricWorldToTilePosition(pos, clampToTilemapBounds);
			}

			return new Point(WorldToTilePositionX(pos.X, clampToTilemapBounds), WorldToTilePositionY(pos.Y, clampToTilemapBounds));
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
			var tileY = Mathf.FloorToInt(y / TileHeight);
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
			if (Orientation == OrientationType.Isometric)
			{
				return IsometricTileToWorldPosition(pos);
			}

			return new Vector2(TileToWorldPositionX((int)pos.X), TileToWorldPositionY((int)pos.Y));
		}

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position x.</returns>
		/// <param name="x">The x coordinate.</param>
		public int TileToWorldPositionX(int x)
		{
			if (Orientation == OrientationType.Isometric)
			{
				throw new InvalidOperationException("Cannot convert tile position to world position for isometric maps with just an X coordinate.");
			}

			return x * TileWidth;
		}

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position y.</returns>
		/// <param name="y">The y coordinate.</param>
		public int TileToWorldPositionY(int y)
		{
			if (Orientation == OrientationType.Isometric)
			{
				throw new InvalidOperationException("Cannot convert tile position to world position for isometric maps with just an Y coordinate.");
			}

			return y * TileHeight;
		}

		/// <summary>
		/// converts from world to tile position for isometric map clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="pos">Position.</param>
		private Point IsometricWorldToTilePosition(Vector2 pos, bool clampToTilemapBounds = true)
		{
			return IsometricWorldToTilePosition(pos.X, pos.Y, clampToTilemapBounds);
		}

		/// <summary>
		/// converts from world to tile position for isometric map clamping to the tilemap bounds
		/// </summary>
		/// <returns>The to tile position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		private Point IsometricWorldToTilePosition(float x, float y, bool clampToTilemapBounds = true)
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
		private Vector2 IsometricTileToWorldPosition(Point pos)
		{
			return IsometricTileToWorldPosition(pos.X, pos.Y);
		}

		/// <summary>
		/// converts from isometric tile to world position
		/// </summary>
		/// <returns>The to world position.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		private Vector2 IsometricTileToWorldPosition(int x, int y)
		{
			var worldX = x * TileWidth / 2 - y * TileWidth / 2 + (Height - 1) * TileWidth / 2;
			var worldY = y * TileHeight / 2 + x * TileHeight / 2;
			return new Vector2(worldX, worldY);
		}

		/// <summary>
		/// Converts a position to world position with respect to the Map's projection. Useful for placing Tiled Objects accurately.
		/// </summary>
		/// <param name="pos">The position.</param>
		/// <returns>The world position.</returns>
		public Vector2 ToWorldPosition(Vector2 pos)
		{
			if (Orientation == OrientationType.Orthogonal
				|| Orientation == OrientationType.Unknown)
			{
				return pos;
			}
			else if (Orientation == OrientationType.Isometric)
			{
				// Code below taken from Tiled's conversion of pixel to screen coordinates
				// See: https://github.com/mapeditor/tiled/blob/12ca7077d3d5cd9655929030092681a986dd749a/src/libtiled/isometricrenderer.cpp#L495-L505
				var originX = Height * TileWidth / 2;

				var tileX = pos.X / TileHeight;
				var tileY = pos.Y / TileHeight;

				var worldPos = new Vector2(
					(tileX - tileY) * TileWidth / 2 + originX,
					(tileX + tileY) * TileHeight / 2);

				// Subtract half the tile height to accommodate for Tiled's bottm-left coordinate system
				worldPos.Y -= TileHeight / 2;

				return worldPos;
			}

			throw new NotImplementedException("Map orientation not supported for conversion of position to world coordinates.");
		}

		#endregion

	}
}