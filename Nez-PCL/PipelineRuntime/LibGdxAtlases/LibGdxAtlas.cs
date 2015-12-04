using System;
using System.Collections;
using System.Collections.Generic;
using Nez.TextureAtlases;
using Nez.Textures;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlas
	{
		public List<TextureAtlas> atlases = new List<TextureAtlas>();


		public LibGdxAtlas()
		{}


		public Subtexture getSubtexture( string name )
		{
			foreach( var a in atlases )
			{
				if( a.containsSubtexture( name ) )
					return a.getSubtexture( name );
			}
			return null;
		}
	}
}

