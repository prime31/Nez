using System;
using System.Collections;
using System.Collections.Generic;
using Nez.Sprites;
using Nez.TextureAtlases;
using Nez.Textures;


namespace Nez.LibGdxAtlases
{
	public class LibGdxAtlas
	{
		public List<TextureAtlas> Atlases = new List<TextureAtlas>();
		public Dictionary<string, List<Subtexture>> Animations = new Dictionary<string, List<Subtexture>>();


		/// <summary>
		/// gets the Subtexture with name or returns null if it cant be found
		/// </summary>
		/// <param name="name">Name.</param>
		public Subtexture Get(string name)
		{
			for (var i = 0; i < Atlases.Count; i++)
			{
				if (Atlases[i].ContainsSubtexture(name))
					return Atlases[i].GetSubtexture(name);
			}

			return null;
		}


		/// <summary>
		/// gets the Subtexture with name or returns null if it cant be found
		/// </summary>
		/// <param name="name">Name.</param>
		public NinePatchSubtexture GetNinePatch(string name)
		{
			for (var i = 0; i < Atlases.Count; i++)
			{
				if (Atlases[i].ContainsSubtexture(name))
					return Atlases[i].GetSubtexture(name) as NinePatchSubtexture;
			}

			return null;
		}


		/// <summary>
		/// alias for LibGdxAtlas.get
		/// </summary>
		/// <returns>The subtexture.</returns>
		/// <param name="name">Name.</param>
		public Subtexture GetSubtexture(string name)
		{
			return Get(name);
		}

		/// <summary>
		/// gets the sprite animation frames for a given name
		/// </summary>
		/// <param name="name">name of the anmation</param>
		/// <returns></returns>
		public List<Subtexture> GetAnimation(string name)
		{
			return Animations[name];
		}
	}
}