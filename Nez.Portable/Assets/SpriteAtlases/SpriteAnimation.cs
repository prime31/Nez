using System;
using Nez.Textures;

namespace Nez.Sprites
{
	public class SpriteAnimation
	{
		public readonly Sprite[] Sprites;
		public readonly float[] FrameRates;

		public SpriteAnimation(Sprite[] sprites, float frameRate)
		{
			Sprites = sprites;
			FrameRates = new float[sprites.Length];
			for(int i = 0; i < FrameRates.Length; ++i)
			{
				FrameRates[i] = frameRate;
			}
		}

		public SpriteAnimation(Sprite[] sprites, float[] frameRates)
		{
			Sprites = sprites;
			FrameRates = frameRates;
		}
	}
}
