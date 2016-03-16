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


		/// <summary>
		/// gets the Subtexture with name or returns null if it cant be found
		/// </summary>
		/// <param name="name">Name.</param>
		public Subtexture get( string name )
		{
			for( var i = 0; i < atlases.Count; i++ )
			{
				if( atlases[i].containsSubtexture( name ) )
					return atlases[i].getSubtexture( name );
			}

			return null;
		}


		/// <summary>
		/// gets the Subtexture with name or returns null if it cant be found
		/// </summary>
		/// <param name="name">Name.</param>
		public NinePatchSubtexture getNinePatch( string name )
		{
			for( var i = 0; i < atlases.Count; i++ )
			{
				if( atlases[i].containsSubtexture( name ) )
					return atlases[i].getSubtexture( name ) as NinePatchSubtexture;
			}

			return null;
		}


		/// <summary>
		/// alias for LibGdxAtlas.get
		/// </summary>
		/// <returns>The subtexture.</returns>
		/// <param name="name">Name.</param>
		public Subtexture getSubtexture( string name )
		{
			return get( name );
		}
	}
}

