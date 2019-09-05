using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Tiled
{
	public enum TiledObjectType
	{
		None,
		Ellipse,
		Image,
		Polygon,
		Polyline
	}

	public class TiledObject
	{
		public int Id;
		public string Name;
		public string Type;
		public int X;
		public int Y;
		public int Width;
		public int Height;
		public int Rotation;
		public int Gid;
		public bool Visible;
		public TiledObjectType TiledObjectType;
		public string ObjectType;
		public Vector2[] PolyPoints;
		public Dictionary<string, string> Properties = new Dictionary<string, string>();

		/// <summary>
		/// wraps the x/y fields in a Vector
		/// </summary>
		public Vector2 Position
		{
			get { return new Vector2(X, Y); }
			set
			{
				X = (int) value.X;
				Y = (int) value.Y;
			}
		}
	}
}