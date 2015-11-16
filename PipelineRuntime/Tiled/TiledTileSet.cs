using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Nez.TextureAtlases;


namespace Nez.Tiled
{
	public class TiledTileset
	{
		private readonly Dictionary<int,Subtexture> _regions;

		public Texture2D texture { get; private set; }
		public int firstId { get; private set; }
		public int tileWidth { get; private set; }
		public int tileHeight { get; private set; }
		public int spacing { get; private set; }
		public int margin { get; private set; }
		public Dictionary<string,string> properties { get; private set; }


		public TiledTileset( Texture2D texture, int firstId, int tileWidth, int tileHeight, int spacing = 2, int margin = 2 )
		{
			this.texture = texture;
			this.firstId = firstId;
			this.tileWidth = tileWidth;
			this.tileHeight = tileHeight;
			this.spacing = spacing;
			this.margin = margin;
			properties = new Dictionary<string,string>();

			var id = firstId;
			_regions = new Dictionary<int,Subtexture>();

			for( var y = margin; y < texture.Height - margin; y += tileHeight + spacing )
			{
				for( var x = margin; x < texture.Width - margin; x += tileWidth + spacing )
				{
					_regions.Add( id, new Subtexture( texture, x, y, tileWidth, tileHeight ) );
					id++;
				}
			}
		}
			

		public Subtexture getTileRegion( int id )
		{
			return _regions[id];
		}
	}
}