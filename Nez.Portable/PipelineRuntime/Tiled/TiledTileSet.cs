using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Tiled
{
	public class TiledTileset
	{
		public Texture2D Texture;
		public readonly int FirstId;
		public readonly int TileWidth;
		public readonly int TileHeight;
		public int Spacing;
		public int Margin;
		public Dictionary<string, string> Properties = new Dictionary<string, string>();
		public List<TiledTilesetTile> Tiles = new List<TiledTilesetTile>();

		protected readonly Dictionary<int, Sprite> _regions;


		public TiledTileset(Texture2D texture, int firstId)
		{
			Texture = texture;
			FirstId = firstId;
			_regions = new Dictionary<int, Sprite>();
		}


		public TiledTileset(Texture2D texture, int firstId, int tileWidth, int tileHeight, int spacing = 2,
		                    int margin = 2, int tileCount = 2, int columns = 2)
		{
			Texture = texture;
			FirstId = firstId;
			TileWidth = tileWidth;
			TileHeight = tileHeight;
			Spacing = spacing;
			Margin = margin;

			var id = firstId;
			_regions = new Dictionary<int, Sprite>();
			for (var y = margin; y < texture.Height - margin; y += tileHeight + spacing)
			{
				var column = 0;

				for (var x = margin; x < texture.Width - margin; x += tileWidth + spacing)
				{
					_regions.Add(id, new Sprite(texture, x, y, tileWidth, tileHeight));
					id++;

					if (++column >= columns)
						break;
				}
			}
		}


		/// <summary>
		/// gets the Sprite for the tile with id
		/// </summary>
		/// <returns>The tile texture region.</returns>
		/// <param name="id">Identifier.</param>
		public virtual Sprite GetTileTextureRegion(int id)
		{
			return _regions[id];
		}
	}
}