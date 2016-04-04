using System;


namespace Nez.Tiled
{
	public class TiledImageTile : TiledTile
	{
		public new TiledTilesetTile tilesetTile;
		public string imageSource;


		public TiledImageTile( int id, TiledTilesetTile tilesetTile, string imageSource ) : base( id )
		{
			this.tilesetTile = tilesetTile;
			this.imageSource = imageSource;
		}
	}
}

