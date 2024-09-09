namespace Nez.Sprites
{
	public class LoopModeControllerPingPongOnce : ILoopModeController
	{
		private enum State
		{
			Ping,
			Pong
		}

		private State _currentState = State.Ping;
		private bool _started = false;
		
		/// <summary>
		/// Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
		/// </summary>
		public void NextFrame(SpriteAnimator animator)
		{
			// If there's only one frame, no need to change it.
			if (animator.FrameCount == 1 || animator.CurrentFrame == 0)
			{
				if (_started)
				{
					animator.SetCompleted(true);
					return;
				}

				_started = true;
			}
			
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