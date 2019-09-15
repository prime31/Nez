using System;
using Nez.Textures;

namespace Nez.SpriteAtlases
{
	public class SpriteAtlas : IDisposable
	{
		public string[] Names;
		public Subtexture[] Subtextures;

		public string[] AnimationNames;
		public SpriteAnimation[] SpriteAnimations;

		public Subtexture GetSprite(string name)
		{
			var index = Array.IndexOf(Names, name);
			return Subtextures[index];
		}

		public SpriteAnimation GetAnimation(string name)
		{
			var index = Array.IndexOf(AnimationNames, name);
			return SpriteAnimations[index];
		}

		void IDisposable.Dispose()
		{
			// all our Subtextures use the same Texture so we only need to dispose one of them
			if (Subtextures != null)
			{
				Subtextures[0].Texture2D.Dispose();
				Subtextures = null;
			}
		}
	}
}
