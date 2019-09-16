using System;
using Nez.Textures;

namespace Nez.Sprites
{
	public class SpriteAtlas : IDisposable
	{
		public string[] Names;
		public Sprite[] Sprites;

		public string[] AnimationNames;
		public SpriteAnimation[] SpriteAnimations;

		public Sprite GetSprite(string name)
		{
			var index = Array.IndexOf(Names, name);
			return Sprites[index];
		}

		public SpriteAnimation GetAnimation(string name)
		{
			var index = Array.IndexOf(AnimationNames, name);
			return SpriteAnimations[index];
		}

		void IDisposable.Dispose()
		{
			// all our Sprites use the same Texture so we only need to dispose one of them
			if (Sprites != null)
			{
				Sprites[0].Texture2D.Dispose();
				Sprites = null;
			}
		}
	}
}
