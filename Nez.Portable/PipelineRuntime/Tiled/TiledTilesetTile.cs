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
		public readonly int Id;
		public readonly TiledMap TiledMap;
		public List<TiledObjectGroup> ObjectGroups;
		public List<TiledTileAnimationFrame> AnimationFrames;
		public Dictionary<string, string> Properties = new Dictionary<string, string>();

		/// <summary>
		/// returns the value of an "nez:isDestructable" property if present in the properties dictionary
		/// </summary>
		/// <value><c>true</c> if is destructable; otherwise, <c>false</c>.</value>
		public bool IsDestructable;

		/// <summary>
		/// returns the value of a "nez:isSlope" property if present in the properties dictionary
		/// </summary>
		/// <value>The is slope.</value>
		public bool IsSlope;

		/// <summary>
		/// returns the value of a "nez:isOneWayPlatform" property if present in the properties dictionary
		/// </summary>
		public bool IsOneWayPlatform;

		/// <summary>
		/// returns the value of a "nez:slopeTopLeft" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top left.</value>
		public int SlopeTopLeft;

		/// <summary>
		/// returns the value of a "nez:slopeTopRight" property if present in the properties dictionary
		/// </summary>
		/// <value>The slope top right.</value>
		public int SlopeTopRight;


		public TiledTilesetTile(int id, TiledMap tiledMap)
		{
			this.Id = id;
			this.TiledMap = tiledMap;
		}


		internal void ProcessProperties()
		{
			string value;
			if (Properties.TryGetValue("nez:isDestructable", out value))
				IsDestructable = bool.Parse(value);

			if (Properties.TryGetValue("nez:isSlope", out value))
				IsSlope = bool.Parse(value);

			if (Properties.TryGetValue("nez:isOneWayPlatform", out value))
				IsOneWayPlatform = bool.Parse(value);

			if (Properties.TryGetValue("nez:slopeTopLeft", out value))
				SlopeTopLeft = int.Parse(value);

			if (Properties.TryGetValue("nez:slopeTopRight", out value))
				SlopeTopRight = int.Parse(value);
		}
	}
}