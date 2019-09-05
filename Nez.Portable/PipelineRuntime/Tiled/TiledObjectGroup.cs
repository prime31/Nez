using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Tiled
{
	public class TiledObjectGroup
	{
		public string Name;
		public Color Color;
		public float Opacity;
		public bool Visible;
		public Dictionary<string, string> Properties = new Dictionary<string, string>();
		public TiledObject[] Objects;


		public TiledObjectGroup(string name, Color color, bool visible, float opacity)
		{
			this.Name = name;
			this.Color = color;
			this.Visible = visible;
			this.Opacity = opacity;
		}


		/// <summary>
		/// gets the first TiledObject with the given name
		/// </summary>
		/// <returns>The with name.</returns>
		/// <param name="name">Name.</param>
		public TiledObject ObjectWithName(string name)
		{
			for (int i = 0; i < Objects.Length; i++)
			{
				if (Objects[i].Name == name)
					return Objects[i];
			}

			return null;
		}


		/// <summary>
		/// gets all the TiledObjects with the given name
		/// </summary>
		/// <returns>The objects with matching names.</returns>
		/// <param name="name">Name.</param>
		public List<TiledObject> ObjectsWithName(string name)
		{
			var list = new List<TiledObject>();
			for (int i = 0; i < Objects.Length; i++)
			{
				if (Objects[i].Name == name)
					list.Add(Objects[i]);
			}

			return list;
		}

		/// <summary>
		/// gets all the TiledObjects with the given type
		/// </summary>
		/// <returns>The objects with matching types.</returns>
		/// <param name="type">Type.</param>
		public List<TiledObject> ObjectsWithType(string type)
		{
			var list = new List<TiledObject>();
			for (int i = 0; i < Objects.Length; i++)
			{
				if (Objects[i].Type == type)
					list.Add(Objects[i]);
			}

			return list;
		}
	}
}