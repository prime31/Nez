using System.Runtime.CompilerServices;


namespace Nez
{
	/// <summary>
	/// provides frame timing information
	/// </summary>
	public static class Time
	{
		/// <summary>
		/// total time the game has been running
		/// </summary>
		public static float time;

		/// <summary>
		/// delta time from the previous frame to the current, scaled by timeScale
		/// </summary>
		public static float deltaTime;

		/// <summary>
		/// unscaled version of deltaTime. Not affected by timeScale
		/// </summary>
		public static float unscaledDeltaTime;

		/// <summary>
		/// secondary deltaTime for use when you need to scale two different deltas simultaneously
		/// </summary>
		public static float altDeltaTime;

		/// <summary>
		/// total time since the Scene was loaded
		/// </summary>
		public static float timeSinceSceneLoad;

		/// <summary>
		/// time scale of deltaTime
		/// </summary>
		public static float timeScale = 1f;

		/// <summary>
		/// time scale of altDeltaTime
		/// </summary>
		public static float altTimeScale = 1f;

		/// <summary>
		/// total number of frames that have passed
		/// </summary>
		public static uint frameCount;


		internal static void update( float dt )
		{
			time += dt;
			deltaTime = dt * timeScale;
			altDeltaTime = dt * altTimeScale;
			unscaledDeltaTime = dt;
			timeSinceSceneLoad += dt;
			frameCount++;
		}


		internal static void sceneChanged()
		{
			timeSinceSceneLoad = 0f;
		}


		/// <summary>
		/// Allows to check in intervals. Should only be used with interval values above deltaTime,
		/// otherwise it will always return true.
		/// </summary>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool checkEvery( float interval )
		{
			// we subtract deltaTime since timeSinceSceneLoad already includes this update ticks deltaTime
			return (int)( timeSinceSceneLoad / interval ) > (int)( ( timeSinceSceneLoad - deltaTime ) / interval );
		}

	}
}

