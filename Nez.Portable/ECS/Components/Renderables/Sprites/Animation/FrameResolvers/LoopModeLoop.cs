namespace Nez.Sprites
{
	public class LoopModeLoop : ISpriteAnimationLoopMode
	{
		/// <summary>
		/// Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
		/// </summary>
		public void NextFrame(SpriteAnimator animator)
		{
			animator.SetFrame((animator.CurrentFrame + 1) % animator.FrameCount);
		}
	}
}