using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez.Systems
{
	/// <summary>
	/// basic CoroutineManager. Coroutines can do the following:
	/// - yield return null (tick again the next frame)
	/// - yield return 3 (tick again after a 3 second delay)
	/// - yield return 5.5 (tick again after a 5.5 second delay)
	/// - yield return startCoroutine( another() ) (wait for the other coroutine before getting ticked again)
	/// </summary>
	public class CoroutineManager
	{
		/// <summary>
		/// internal class used by the CoroutineManager to hide the data it requires for a Coroutine
		/// </summary>
		class CoroutineImpl : ICoroutine
		{
			public IEnumerator enumerator;
			public float waitTimer;
			public bool isDone;
			public CoroutineImpl waitForCoroutine;


			public void stop()
			{
				isDone = true;
			}
		}


		Stack<CoroutineImpl> _coroutineCache = new Stack<CoroutineImpl>();
		List<CoroutineImpl> _unblockedCoroutines = new List<CoroutineImpl>();
		List<CoroutineImpl> _shouldRunNextFrame = new List<CoroutineImpl>();


		/// <summary>
		/// adds the IEnumerator to the CoroutineManager. Coroutines get ticked before Update is called each frame.
		/// </summary>
		/// <returns>The coroutine.</returns>
		/// <param name="enumerator">Enumerator.</param>
		public ICoroutine startCoroutine( IEnumerator enumerator )
		{
			// find or create a CoroutineImpl
			CoroutineImpl coroutine = null;
			if( _coroutineCache.Count > 0 )
				coroutine = _coroutineCache.Pop();
			else
				coroutine = new CoroutineImpl();

			// setup the coroutine and add it
			coroutine.enumerator = enumerator;
			coroutine.waitTimer = 0;
			coroutine.isDone = false;
			coroutine.waitForCoroutine = null;

			_shouldRunNextFrame.Add( coroutine );

			return coroutine;
		}


		public void update()
		{
			for( var i = 0; i < _unblockedCoroutines.Count; i++ )
			{
				var coroutine = _unblockedCoroutines[i];

				// check for stopped coroutines
				if( coroutine.isDone )
				{
					coroutine.isDone = true;
					_coroutineCache.Push( coroutine );
					continue;
				}

				// are we waiting for any other coroutines to finish?
				if( coroutine.waitForCoroutine != null )
				{
					if( coroutine.waitForCoroutine.isDone )
					{
						coroutine.waitForCoroutine = null;
					}
					else
					{
						_shouldRunNextFrame.Add( coroutine );
						continue;
					}
				}

				// deal with timers if we have them
				if( coroutine.waitTimer > 0 )
				{
					// still has time left. decrement and run again next frame
					coroutine.waitTimer -= Time.deltaTime;
					_shouldRunNextFrame.Add( coroutine );
					continue;
				}


				// This coroutine has finished
				if( !coroutine.enumerator.MoveNext() )
				{
					coroutine.isDone = true;
					_coroutineCache.Push( coroutine );
					continue;
				}

				if( coroutine.enumerator.Current is int )
				{
					var wait = (int)coroutine.enumerator.Current;
					coroutine.waitTimer = wait;
					_shouldRunNextFrame.Add( coroutine );
				}
				else if( coroutine.enumerator.Current is float )
				{
					var wait = (float)coroutine.enumerator.Current;
					coroutine.waitTimer = wait;
					_shouldRunNextFrame.Add( coroutine );
				}
				else if( coroutine.enumerator.Current is CoroutineImpl )
				{
					coroutine.waitForCoroutine = coroutine.enumerator.Current as CoroutineImpl;
					_shouldRunNextFrame.Add( coroutine );
				}
				else
				{
					// This coroutine yielded null, or some other value we don't understand; run it next frame.
					_shouldRunNextFrame.Add( coroutine );
				}
			}

			_unblockedCoroutines.Clear();
			_unblockedCoroutines.AddRange( _shouldRunNextFrame );
			_shouldRunNextFrame.Clear();
		}
	}
}

