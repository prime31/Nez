namespace Nez
{
	public interface ITimer
	{
		object Context { get; }


		/// <summary>
		/// call stop to stop this timer from being run again. This has no effect on a non-repeating timer.
		/// </summary>
		void Stop();

		/// <summary>
		/// resets the elapsed time of the timer to 0
		/// </summary>
		void Reset();

		/// <summary>
		/// Returns time reamaing on the timer
		/// </summary>
		float RemainingTime();

		/// <summary>
		/// returns the context casted to T as a convenience
		/// </summary>
		/// <returns>The context.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T GetContext<T>();
	}
}