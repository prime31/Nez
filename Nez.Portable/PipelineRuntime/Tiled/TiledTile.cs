using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Tiled
{
	public class TiledTile
	{
		/// <summary>
		/// returns the Subtexture that maps to this particular tile
		/// </summary>
		/// <value>The texture region.</value>
		public Subtexture TextureRegion
		{
			get { return Tileset.GetTileTextureRegion(Id); }
		}

		/// <summary>
		/// gets the TiledtilesetTile for this TiledTile if it exists. TiledtilesetTile only exist for animated tiles and tiles with attached
		/// properties.
		/// </summary>
		/// <value>The tileset tile.</value>
		public TiledTilesetTile TilesetTile
		{
			get
			{
				if (!_tilesetTileIndex.HasValue)
				{
					_tilesetTileIndex = -1;
					for (var i = 0; i < Tileset.Tiles.Count; i++)
					{
						// id is a gid so we need to subtract the tileset.firstId to get a local id
						if (Tileset.Tiles[i].Id == Id - Tileset.FirstId)
							_tilesetTileIndex = i;
					}
				}

				if (_tilesetTileIndex.Value < 0)
					return null;

				return Tileset.Tiles[_tilesetTileIndex.Value];
			}
		}

		public int Id;
		public int X;
		public int Y;
		public bool FlippedHorizonally;
		public bool FlippedVertically;
		public bool FlippedDiagonally;
		public TiledTileset Tileset;

		/// <summary>
		/// we use this for 3 states: HasValue is false means we havent yet checked for the TiledTilesetTile, less than 0 means there is no
		/// TiledTilesetTile for this, and 0+ means we have a TiledTilesetTile.
		/// </summary>
		int? _tilesetTileIndex;


		public TiledTile(int id)
		{
			this.Id = id;
		}


		/// <summary>
		/// sets a new Tile id for this tile and invalidates the previous tilesetTileIndex
		/// </summary>
		/// <returns>The tile identifier.</returns>
		/// <param name="id">Identifier.</param>
		public void SetTileId(int id)
		{
			this.Id = id;
			_tilesetTileIndex = null;
		}


		/// <summary>
		/// Rectangle that encompases this tile with origin on the top left
		/// </summary>
		/// <returns>The tile rectangle.</returns>
		/// <param name="tilemap">Tilemap.</param>
		public Rectangle GetTileRectangle(TiledMap tilemap)
		{
			return new Rectangle(X * tilemap.TileWidth, Y * tilemap.TileHeight, tilemap.TileWidth, tilemap.TileHeight);
		}


		/// <summary>
		/// note that the origin is the top left so this position will represent that
		/// </summary>
		/// <returns>The world position.</returns>
		/// <param name="tilemap">Tilemap.</param>
		public Vector2 GetWorldPosition(TiledMap tilemap)
		{
			return new Vector2(X * tilemap.TileWidth, Y * tilemap.TileHeight);
		}


		public override string ToString()
		{
			return string.Format("[TiledTile] id: {0}, x: {1}, y: {2}", Id, X, Y);
		}
	}
}