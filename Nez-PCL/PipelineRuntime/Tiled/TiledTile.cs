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
		public Subtexture textureRegion { get { return tileset.getTileTextureRegion( id ); } }

		public int id;
		public int x;
		public int y;
		public bool flippedHorizonally;
		public bool flippedVertically;
		public bool flippedDiagonally;
		internal TiledTileset tileset;


		public TiledTile( int id )
		{
			this.id = id;
		}


		/// <summary>
		/// Rectangle that encompases this tile with origin on the top left
		/// </summary>
		/// <returns>The tile rectangle.</returns>
		/// <param name="tilemap">Tilemap.</param>
		public Rectangle getTileRectangle( TiledMap tilemap )
		{
			return new Rectangle( x * tilemap.tileWidth, y * tilemap.tileHeight, tilemap.tileWidth, tilemap.tileHeight );
		}


		/// <summary>
		/// note that the origin is the top left so this position will represent that
		/// </summary>
		/// <returns>The world position.</returns>
		/// <param name="tilemap">Tilemap.</param>
		public Vector2 getWorldPosition( TiledMap tilemap )
		{
			return new Vector2( x * tilemap.tileWidth, y * tilemap.tileHeight );
		}


		public override string ToString()
		{
			return string.Format( "[TiledTile] id: {0}, x: {1}, y: {2}", id, x, y );
		}

	}
}