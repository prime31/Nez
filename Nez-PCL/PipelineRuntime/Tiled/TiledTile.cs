using Microsoft.Xna.Framework;


namespace Nez.Tiled
{
	public class TiledTile
	{
		public int id { get; private set; }
		public int x;
		public int y;
		public bool flippedHorizonally;
		public bool flippedVertically;
		public bool flippedDiagonally;


		public TiledTile( int id )
		{
			this.id = id;
		}


		public TiledTile( int id, int x, int y )
		{
			this.id = id;
			this.x = x;
			this.y = y;
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