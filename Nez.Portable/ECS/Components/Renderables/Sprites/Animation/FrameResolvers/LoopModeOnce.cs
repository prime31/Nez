namespace Nez.Sprites
{
	public class LoopModeOnce : ISpriteAnimationLoopMode
	{
		/// <summary>
		/// Play the sequence once [A][B][C] then pause and set time to 0 [A]
		/// </summary>
		public void NextFrame(SpriteAnimator animator)
		{
			var newFrame = animator.CurrentFrame + 1;
			if (newFrame >= animator.FrameCount)
			{
				animator.SetCompleted(true);
			}
			else
			{
				animator.SetFrame(newFrame);
			}
		}
	}
}