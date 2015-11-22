using System;
using System.Collections.Generic;


namespace Nez.Timers
{
	public class TimerManager
	{
		List<Timer> _timers = new List<Timer>();

		
		internal void update()
		{
			for( var i = _timers.Count - 1; i >= 0; i-- )
			{
				// tick our timer. if it returns true it is done so we remove it
				if( _timers[i].tick() )
					_timers.RemoveAt( i );
			}
		}


		internal ITimer schedule( float timeInSeconds, bool repeats, object context, Action<ITimer> onTime )
		{
			var timer = QuickCache<Timer>.pop();
			timer.initialize( timeInSeconds, repeats, context, onTime );
			_timers.Add( timer );

			return timer;
		}
	}
}

