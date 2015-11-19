using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Tiled
{
	public class TiledObjectGroup
	{
		public string name;
		public Color color;
		public float opacity;
		public bool visible;
		public Dictionary<string,string> properties = new Dictionary<string,string>();
		public TiledObject[] objects;


		public TiledObjectGroup( string name, Color color, bool visible, float opacity )
		{
			this.name = name;
			this.color = color;
			this.visible = visible;
			this.opacity = opacity;
		}

	}
}

