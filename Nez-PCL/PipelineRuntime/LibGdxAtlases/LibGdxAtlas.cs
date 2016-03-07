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


		public Subtexture getSubtexture( string name )
		{
			for( var i = 0; i < atlases.Count; i++ )
			{
				if( atlases[i].containsSubtexture( name ) )
					return atlases[i].getSubtexture( name );
			}

			return null;
		}
	}
}

