namespace Nez.Sprites
{
	public class LoopModePingPong : ISpriteAnimationLoopMode
	{
		private enum State
		{
			Ping,
			Pong
		}

		private State _currentState = State.Ping;
		
		/// <summary>
		/// Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
		/// </summary>
		public void NextFrame(SpriteAnimator animator)
		{
			// If there's only one frame, no need to change it.
			if (animator.FrameCount == 1) return;
			
			switch (_currentState)
			{
				case State.Ping:
					ParsePing(animator);
					break;
				case State.Pong:
					ParsePong(animator);
					break;
			}
		}

		private void ParsePing(SpriteAnimator animator)
		{
			var newFrame = animator.CurrentFrame + 1;
			if (newFrame >= animator.FrameCount)
			{
				_currentState = State.Pong;
				ParsePong(animator);
			}
			else
			{
				animator.SetFrame(newFrame);
			}
		}
		
		private void ParsePong(SpriteAnimator animator)
		{
			var newFrame = animator.CurrentFrame - 1;
			if (newFrame < 0)
			{
				_currentState = State.Ping;
				ParsePing(animator);
			}
			else
			{
				animator.SetFrame(newFrame);
			}
		}
	}
}