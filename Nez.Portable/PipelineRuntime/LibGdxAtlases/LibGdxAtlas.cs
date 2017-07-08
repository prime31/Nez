﻿using System;
using System.Collections;
using System.Collections.Generic;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Textures;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlas
	{
		public List<TextureAtlas> atlases = new List<TextureAtlas>();
		public Dictionary<string, List<Subtexture>> animations = new Dictionary<string, List<Subtexture>>();


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
		/// <summary>
		/// gets the sprite animation frames for a given name
		/// </summary>
		/// <param name="name">name of the anmation</param>
		/// <returns></returns>
		public List<Subtexture> getAnimation( string name )
		{
			return animations[name];
		}
	}
}

