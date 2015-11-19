using System;


namespace Nez
{
	public static class Time
	{
		public static float time;
		public static float deltaTime;
		public static float unscaledDeltaTime;
		public static float timeSinceSceneLoad;
		public static float timeScale = 1f;
		/// <summary>
		/// total number of frames that have passed
		/// </summary>
		public static uint frameCount;


		public static void update( float dt )
		{
			time += dt;
			deltaTime = dt * timeScale;
			unscaledDeltaTime = dt;
			timeSinceSceneLoad += dt;
			frameCount++;
		}


		public static void sceneChanged()
		{
			timeSinceSceneLoad = 0f;
		}

	}
}

