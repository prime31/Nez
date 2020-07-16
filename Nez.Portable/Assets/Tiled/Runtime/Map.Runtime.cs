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
		public Vector2 TileToWorldPosition(Vector2 pos) => new Vector2(TileToWorldPositionX((int)pos.X), TileToWorldPositionY((int)pos.Y));

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position x.</returns>
		/// <param name="x">The x coordinate.</param>
		public int TileToWorldPositionX(int x) => x * TileWidth;

		/// <summary>
		/// converts from tile to world position
		/// </summary>
		/// <returns>The to world position y.</returns>
		/// <param name="y">The y coordinate.</param>
		public int TileToWorldPositionY(int y) => y * TileHeight;

		#endregion

	}
}