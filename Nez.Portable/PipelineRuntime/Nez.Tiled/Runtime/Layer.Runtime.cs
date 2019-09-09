namespace Nez.Tiled
{
    public partial class TmxLayer : ITmxLayer
    {
        /// <summary>
		/// gets the TmxLayerTile at the x/y coordinates. Note that these are tile coordinates not world coordinates!
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public TmxLayerTile GetTile(int x, int y) => Tiles[x + y * Width];
        
		/// <summary>
		/// sets the tile and updates its tileset. If you change a tiles gid to one in a different Tileset you must
		/// call this method or update the TmxLayerTile.tileset manually!
		/// </summary>
		/// <returns>The tile.</returns>
		/// <param name="tile">Tile.</param>
		public TmxLayerTile SetTile(TmxLayerTile tile)
		{
			Tiles[tile.X + tile.Y * Width] = tile;
			tile.Tileset = Map.GetTilesetForTileGid(tile.Gid);

			return tile;
		}

		/// <summary>
		/// nulls out the tile at the x/y coordinates
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public void RemoveTile(int x, int y) => Tiles[x + y * Width] = null;

    }
}