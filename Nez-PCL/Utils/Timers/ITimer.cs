using System;


namespace Nez
{
	public interface ITimer
	{
		object context { get; }


		/// <summary>
		/// call stop to stop this timer from being run again. This has no effect on a non-repeating timer.
		/// </summary>
		void stop();
	}
}

