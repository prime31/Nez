using System;
using System.Collections.Generic;


namespace Nez.Timers
{
	/// <summary>
	/// allows delayed and repeated execution of an Action
	/// </summary>
	public class TimerManager : GlobalManager
	{
		List<Timer> _timers = new List<Timer>();


		public override void Update()
		{
			for (var i = _timers.Count - 1; i >= 0; i--)
			{
				// tick our timer. if it returns true it is done so we remove it
				if (_timers[i].Tick())
				{
					_timers[i].Unload();
					_timers.RemoveAt(i);
				}
			}
		}

		/// <summary>
		/// schedules a one-time or repeating timer that will call the passed in Action
		/// </summary>
		/// <param name="timeInSeconds">Time in seconds.</param>
		/// <param name="repeats">If set to <c>true</c> repeats.</param>
		/// <param name="context">Context.</param>
		/// <param name="onTime">On time.</param>
		internal ITimer Schedule(float timeInSeconds, bool repeats, object context, Action<ITimer> onTime)
		{
			var timer = new Timer();
			timer.Initialize(timeInSeconds, repeats, context, onTime);
			_timers.Add(timer);

			return timer;
		}
	}
}