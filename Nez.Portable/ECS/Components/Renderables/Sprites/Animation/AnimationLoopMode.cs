using System;

namespace Nez.Sprites
{
	public partial class SpriteAnimator
	{
		public enum LoopMode
		{
			/// <summary>
			/// Play the sequence in a loop forever [A][B][C][A][B][C][A][B][C]...
			/// </summary>
			Loop,

			/// <summary>
			/// Play the sequence once [A][B][C] then pause and set time to 0 [A]
			/// </summary>
			Once,

			/// <summary>
			/// Plays back the animation once, [A][B][C]. When it reaches the end, it will keep playing the last frame and never stop playing
			/// </summary>
			ClampForever,

			/// <summary>
			/// Play the sequence in a ping pong loop forever [A][B][C][B][A][B][C][B]...
			/// </summary>
			PingPong,

			/// <summary>
			/// Play the sequence once forward then back to the start [A][B][C][B][A] then pause and set time to 0
			/// </summary>
			PingPongOnce
		}

		public static ILoopModeController GetNewController(LoopMode loopMode)
		{
			switch (loopMode)
			{
				case LoopMode.Once:
					return new LoopModeControllerOnce();
				case LoopMode.ClampForever:
					return new LoopModeControllerClampForever();
				case LoopMode.PingPong:
					return new LoopModeControllerPingPong();
				case LoopMode.PingPongOnce:
					return new LoopModeControllerPingPongOnce();
				case LoopMode.Loop:
				default:
					return new LoopModeControllerLoop();
			}
		}
	}
}