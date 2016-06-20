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
		public readonly TiledMap tiledMap;
		public List<TiledTileAnimationFrame> animationFrames;
		public Dictionary<string, string> properties = new Dictionary<string, string>();

		/// <summary>
		/// returns the value of an "nez:isDestructable" property if present in the properties dictionary
		/// </summary>
		/// <value><c>true</c> if is destructable; otherwise, <c>false</c>.</value>
		public bool isDestructable;

		/// <summary>
		/// returns the value of an "nez:isSlope" property if present in the properties dictionary
		/// </summary>
		/// <value>The is slope.</value>
		public bool isSlope;

		/// <summary>
		/// returns the value of an "nez:isOneWayPlatform" property if present in the properties dictionary
		/// </summary>
		public bool isOneWayPlatform;

		/// <summary>
		/// returns the value of an "nez:slopeTopLeft" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top left.</value>
		public int slopeTopLeft;

		/// <summary>
		/// returns the value of an "nez:slopeTopRight" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top right.</value>
		public int slopeTopRight;


		public TiledTilesetTile( int id, TiledMap tiledMap )
		{
			this.id = id;
			this.tiledMap = tiledMap;
		}


		internal void processProperties()
		{
			string value;
			if( properties.TryGetValue( "nez:isDestructable", out value ) )
				isDestructable = bool.Parse( value );

			if( properties.TryGetValue( "nez:isSlope", out value ) )
				isSlope = bool.Parse( value );

			if( properties.TryGetValue( "nez:isOneWayPlatform", out value ) )
				isOneWayPlatform = bool.Parse( value );

			if( properties.TryGetValue( "nez:slopeTopLeft", out value ) )
				slopeTopLeft = int.Parse( value );

			if( properties.TryGetValue( "nez:slopeTopRight", out value ) )
				slopeTopRight = int.Parse( value );
		}

	}
}

