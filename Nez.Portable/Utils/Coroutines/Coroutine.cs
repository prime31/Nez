namespace Nez
{
	/// <summary>
	/// interface returned by startCoroutine which provides the ability to stop the coroutine mid-flight
	/// </summary>
	public interface ICoroutine
	{
		/// <summary>
		/// stops the Coroutine
		/// </summary>
		void Stop();

		/// <summary>
		/// sets whether the Coroutine should use deltaTime or unscaledDeltaTime for timing
		/// </summary>
		/// <returns>The use unscaled delta time.</returns>
		/// <param name="useUnscaledDeltaTime">If set to <c>true</c> use unscaled delta time.</param>
		ICoroutine SetUseUnscaledDeltaTime(bool useUnscaledDeltaTime);
	}


	public static class Coroutine
	{
		/// <summary>
		/// causes a Coroutine to pause for the specified duration. Yield on Coroutine.waitForSeconds in a coroutine to use.
		/// </summary>
		/// <returns>The for seconds.</returns>
		/// <param name="seconds">Seconds.</param>
		public static object WaitForSeconds(float seconds)
		{
			return Nez.WaitForSeconds.waiter.Wait(seconds);
		}
	}


	/// <summary>
	/// helper class for when a coroutine wants to pause for some duration. Returning Coroutine.waitForSeconds returns one of these
	/// to avoid having to return an int/float and paying the boxing penalty.
	/// </summary>
	class WaitForSeconds
	{
		internal static WaitForSeconds waiter = new WaitForSeconds();
		internal float waitTime;

		internal WaitForSeconds Wait(float seconds)
		{
			waiter.waitTime = seconds;
			return waiter;
		}
	}
}