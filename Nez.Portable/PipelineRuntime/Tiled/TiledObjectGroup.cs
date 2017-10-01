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


		/// <summary>
		/// gets the first TiledObject with the given name
		/// </summary>
		/// <returns>The with name.</returns>
		/// <param name="name">Name.</param>
		public TiledObject objectWithName( string name )
		{
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i].name == name )
					return objects[i];
			}
			return null;
		}


		/// <summary>
		/// gets all the TiledObjects with the given name
		/// </summary>
		/// <returns>The objects with matching names.</returns>
		/// <param name="name">Name.</param>
		public List<TiledObject> objectsWithName( string name )
		{
			var list = new List<TiledObject>();
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i].name == name )
					list.Add( objects[i] );
			}
			return list;
		}

		/// <summary>
		/// gets all the TiledObjects with the given type
		/// </summary>
		/// <returns>The objects with matching types.</returns>
		/// <param name="type">Type.</param>
		public List<TiledObject> objectsWithType( string type )
		{
			var list = new List<TiledObject>();
			for( int i = 0; i < objects.Length; i++ )
			{
				if( objects[i].type == type )
					list.Add( objects[i] );
			}
			return list;
		}

	}
}

