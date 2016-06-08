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

		/// <summary>
		/// returns the value of an "isDestructable" property if present in the properties dictionary
		/// </summary>
		/// <value><c>true</c> if is destructable; otherwise, <c>false</c>.</value>
		public bool isDestructable
		{
			get
			{
				if( !_isDestructable.HasValue && properties.ContainsKey( "isDestructable" ) )
					_isDestructable = bool.Parse( properties["isDestructable"] );
				return _isDestructable.Value;
			}
		}

		/// <summary>
		/// returns the value of an "slopeTopLeft" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top left.</value>
		public int slopeTopLeft
		{
			get
			{
				if( !_slopeTopLeft.HasValue && properties.ContainsKey( "slopeTopLeft" ) )
					_slopeTopLeft = int.Parse( properties["slopeTopLeft"] );
				return _slopeTopLeft.Value;
			}
		}

		/// <summary>
		/// returns the value of an "slopeTopRight" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top right.</value>
		public int slopeTopRight
		{
			get
			{
				if( !_slopeTopRight.HasValue && properties.ContainsKey( "slopeTopRight" ) )
					_slopeTopRight = int.Parse( properties["slopeTopRight"] );
				return _slopeTopRight.Value;
			}
		}

		bool? _isDestructable;
		int? _slopeTopLeft;
		int? _slopeTopRight;



		public TiledTilesetTile( int id )
		{
			this.id = id;
		}
	}
}

