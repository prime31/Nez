using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez.Systems
{
	/// <summary>
	/// basic CoroutineManager. Coroutines can do the following:
	/// - yield return null (tick again the next frame)
	/// - yield return Coroutine.waitForSeconds( 3 ) (tick again after a 3 second delay)
	/// - yield return Coroutine.waitForSeconds( 5.5f ) (tick again after a 5.5 second delay)
	/// - yield return startCoroutine( another() ) (wait for the other coroutine before getting ticked again)
	/// </summary>
	public class CoroutineManager : IUpdatableManager
	{
		/// <summary>
		/// internal class used by the CoroutineManager to hide the data it requires for a Coroutine
		/// </summary>
		class CoroutineImpl : ICoroutine, IPoolable
		{
			public IEnumerator enumerator;
			/// <summary>
			/// anytime a delay is yielded it is added to the waitTimer which tracks the delays
			/// </summary>
			public float waitTimer;
			public bool isDone;
			public CoroutineImpl waitForCoroutine;
			public bool useUnscaledDeltaTime = false;


			public void stop()
			{
				isDone = true;
			}


			public ICoroutine setUseUnscaledDeltaTime( bool useUnscaledDeltaTime )
			{
				this.useUnscaledDeltaTime = useUnscaledDeltaTime;
				return this;
			}


			internal void prepareForReuse()
			{
				isDone = false;
			}


			void IPoolable.reset()
			{
				isDone = true;
				waitTimer = 0;
				waitForCoroutine = null;
				enumerator = null;
				useUnscaledDeltaTime = false;
			}
		}


		/// <summary>
		/// flag to keep track of when we are in our update loop. If a new coroutine is started during the update loop we have to stick
		/// it in the shouldRunNextFrame List to avoid modifying a List while we iterate.
		/// </summary>
		bool _isInUpdate;
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
			var coroutine = Pool<CoroutineImpl>.obtain();
			coroutine.prepareForReuse();

			// setup the coroutine and add it
			coroutine.enumerator = enumerator;
			var shouldContinueCoroutine = tickCoroutine( coroutine );

			// guard against empty coroutines
			if( !shouldContinueCoroutine )
				return null;

			if( _isInUpdate )
				_shouldRunNextFrame.Add( coroutine );
			else
				_unblockedCoroutines.Add( coroutine );

			return coroutine;
		}


		void IUpdatableManager.update()
		{
			_isInUpdate = true;
			for( var i = 0; i < _unblockedCoroutines.Count; i++ )
			{
				var coroutine = _unblockedCoroutines[i];

				// check for stopped coroutines
				if( coroutine.isDone )
				{
					Pool<CoroutineImpl>.free( coroutine );
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
					// still has time left. decrement and run again next frame being sure to decrement with the appropriate deltaTime.
					coroutine.waitTimer -= coroutine.useUnscaledDeltaTime ? Time.unscaledDeltaTime : Time.deltaTime;
					_shouldRunNextFrame.Add( coroutine );
					continue;
				}

				if( tickCoroutine( coroutine ) )
					_shouldRunNextFrame.Add( coroutine );
			}

			_unblockedCoroutines.Clear();
			_unblockedCoroutines.AddRange( _shouldRunNextFrame );
			_shouldRunNextFrame.Clear();

			_isInUpdate = false;
		}


		/// <summary>
		/// ticks a coroutine. returns true if the coroutine should continue to run next frame. This method will put finished coroutines
		/// back in the Pool!
		/// </summary>
		/// <returns><c>true</c>, if coroutine was ticked, <c>false</c> otherwise.</returns>
		/// <param name="coroutine">Coroutine.</param>
		bool tickCoroutine( CoroutineImpl coroutine )
		{
			// This coroutine has finished
			if( !coroutine.enumerator.MoveNext() || coroutine.isDone )
			{
				Pool<CoroutineImpl>.free( coroutine );
				return false;
			}

			if( coroutine.enumerator.Current == null )
			{
				// yielded null. run again next frame
				return true;
			}

			if( coroutine.enumerator.Current is WaitForSeconds )
			{
				coroutine.waitTimer = ( coroutine.enumerator.Current as WaitForSeconds ).waitTime;
				return true;
			}

			#if DEBUG
			// deprecation warning for yielding an int/float
			if( coroutine.enumerator.Current is int )
			{
				Debug.error( "yield Coroutine.waitForSeconds instead of an int. Yielding an int will not work in a release build." );
				coroutine.waitTimer = (int)coroutine.enumerator.Current;
				return true;
			}

			if( coroutine.enumerator.Current is float )
			{
				Debug.error( "yield Coroutine.waitForSeconds instead of a float. Yielding a float will not work in a release build." );
				coroutine.waitTimer = (float)coroutine.enumerator.Current;
				return true;
			}
			#endif

			if( coroutine.enumerator.Current is CoroutineImpl )
			{
				coroutine.waitForCoroutine = coroutine.enumerator.Current as CoroutineImpl;
				return true;
			}
			else
			{
				// This coroutine yielded some value we don't understand. run it next frame.
				return true;
			}
		}
	
	}
}

