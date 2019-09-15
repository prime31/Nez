using Nez.Textures;

namespace Nez.SpriteAtlases
{
	public class SpriteAnimation
	{
		public Sprite[] Sprites;
		public float FrameRate;

		public SpriteAnimation(Sprite[] sprites, float frameRate)
		{
			Sprites = sprites;
			FrameRate = frameRate;
		}
	}
}
