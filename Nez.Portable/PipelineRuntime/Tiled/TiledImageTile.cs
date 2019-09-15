using System;


namespace Nez.Tiled
{
	public class TiledImageTile : TiledTile
	{
		public new TiledTilesetTile TilesetTile;
		public string ImageSource;


		public TiledImageTile(int id, TiledTilesetTile tilesetTile, string imageSource) : base(id)
		{
			TilesetTile = tilesetTile;
			ImageSource = imageSource;
		}
	}
}