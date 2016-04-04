using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Tiled
{
	/// <summary>
	/// these exist only for tiles with properties or animations
	/// </summary>
	public class TiledTilesetTile
	{
		public readonly int id;
		public List<TiledTileAnimationFrame> animationFrames;
		public Dictionary<string,string> properties = new Dictionary<string,string>();


		public TiledTilesetTile( int id )
		{
			this.id = id;
		}
	}
}

