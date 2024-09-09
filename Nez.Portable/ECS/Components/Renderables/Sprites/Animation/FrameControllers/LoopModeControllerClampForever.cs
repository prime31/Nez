namespace Nez.Sprites
{
	public class LoopModeControllerClampForever : ILoopModeController
	{
		/// <summary>
		/// Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing
		/// </summary>
		public void NextFrame(SpriteAnimator animator)
		{
			var newFrame = animator.CurrentFrame + 1;
			if (newFrame >= animator.FrameCount)
			{
				animator.SetCompleted();
			}
			else
			{
				animator.SetFrame(newFrame);
			}
		}
	}
}