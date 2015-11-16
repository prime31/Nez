using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Tiled
{
	public class TiledObject
	{
		public enum TiledObjectType
		{
			None,
			Ellipse,
			Image,
			Polygon,
			Polyline
		}

		public int gid;
		public string name;
		public string type;
		public int x;
		public int y;
		public int width;
		public int height;
		public int rotation;
		public bool visible;
		public TiledObjectType tiledObjectType;
		public string objectType;
		public Vector2[] polyPoints;
		public Dictionary<string,string> properties = new Dictionary<string,string>();


		public TiledObject()
		{}
	}
}

