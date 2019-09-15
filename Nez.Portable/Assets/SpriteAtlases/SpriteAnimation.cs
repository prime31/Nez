using Nez.Textures;

namespace Nez.SpriteAtlases
{
	public class SpriteAnimation
	{
		public Subtexture[] Subtextures;
		public float FrameRate;

		public SpriteAnimation(Subtexture[] subtextures, float frameRate)
		{
			Subtextures = subtextures;
			FrameRate = frameRate;
		}
	}
}
