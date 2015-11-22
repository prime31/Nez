using System;


namespace Nez.Timers
{
	/// <summary>
	/// private class hiding the implementation of ITimer
	/// </summary>
	class Timer : ITimer
	{
		public object context { get; set; }

		float _timeInSeconds;
		bool _repeats;
		Action<ITimer> _onTime;
		bool _isDone;
		float _elapsedTime;


		public void stop()
		{
			_isDone = true;
		}


		internal bool tick()
		{
			if( _elapsedTime > _timeInSeconds )
			{
				_elapsedTime -= _timeInSeconds;
				_onTime( this );

				if( !_isDone && !_repeats )
					_isDone = true;
			}

			_elapsedTime += Time.deltaTime;

			return _isDone;
		}


		internal void initialize( float timeInSeconds, bool repeats, object context, Action<ITimer> onTime )
		{
			_timeInSeconds = timeInSeconds;
			_repeats = repeats;
			this.context = context;
			_onTime = onTime;
		}
	}
}

