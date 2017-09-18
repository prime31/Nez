using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez.Tiled
{
	public class TiledTileset
	{
		public Texture2D texture;
		public readonly int firstId;
		public readonly int tileWidth;
		public readonly int tileHeight;
		public int spacing;
		public int margin;
		public Dictionary<string,string> properties = new Dictionary<string,string>();
		public List<TiledTilesetTile> tiles = new List<TiledTilesetTile>();

		protected readonly Dictionary<int,Subtexture> _regions;


		public TiledTileset( Texture2D texture, int firstId )
		{
			this.texture = texture;
			this.firstId = firstId;
			_regions = new Dictionary<int,Subtexture>();
		}


		public TiledTileset( Texture2D texture, int firstId, int tileWidth, int tileHeight, int spacing = 2, int margin = 2, int tileCount = 2, int columns = 2 )
		{
			this.texture = texture;
			this.firstId = firstId;
			this.tileWidth = tileWidth;
			this.tileHeight = tileHeight;
			this.spacing = spacing;
			this.margin = margin;

			var id = firstId;
			_regions = new Dictionary<int,Subtexture>();
			for( var y = margin; y < texture.Height - margin; y += tileHeight + spacing )
			{
				var column = 0;

				for( var x = margin; x < texture.Width - margin; x += tileWidth + spacing )
				{
					_regions.Add( id, new Subtexture( texture, x, y, tileWidth, tileHeight ) );
					id++;

					if( ++column >= columns ) 
						break;
				}
			}
		}
			

		/// <summary>
		/// gets the Subtexture for the tile with id
		/// </summary>
		/// <returns>The tile texture region.</returns>
		/// <param name="id">Identifier.</param>
		public virtual Subtexture getTileTextureRegion( int id )
		{
			return _regions[id];
		}
	}
}