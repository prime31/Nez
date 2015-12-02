using System;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Sprites
{
	/// <summary>
	/// holds a Subtexture and origin for use by the SpriteAnimation component
	/// </summary>
	public class SpriteAnimationFrame
	{
		public Subtexture subtexture;
		public Vector2 origin;


		public SpriteAnimationFrame( Subtexture subtexture, Vector2 origin )
		{
			this.subtexture = subtexture;
			this.origin = origin;
		}


		public SpriteAnimationFrame( Subtexture subtexture ) : this( subtexture, subtexture.center )
		{}
	}
}

