using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez.Tweens
{
	public enum LoopType
	{
		None,
		RestartFromBeginning,
		PingPong
	}


	public abstract class Tween<T> : ITweenable, ITween<T> where T : struct
	{
		protected enum TweenState
		{
			Running,
			Paused,
			Complete
		}


		protected ITweenTarget<T> _target;
		protected bool _isFromValueOverridden;
		protected T _fromValue;
		protected T _toValue;
		protected EaseType _easeType;
		protected bool _shouldRecycleTween = true;
		protected bool _isRelative;
		protected Action<ITween<T>> _completionHandler;
		protected Action<ITween<T>> _loopCompleteHandler;
		protected ITweenable _nextTween;

		// tween state
		protected TweenState _tweenState = TweenState.Complete;
		bool _isTimeScaleIndependent;
		protected float _delay;
		protected float _duration;
		protected float _timeScale = 1f;
		protected float _elapsedTime;
		
		// loop state
		protected LoopType _loopType;
		protected int _loops;
		protected float _delayBetweenLoops;
		bool _isRunningInReverse;



		#region ITweenT implementation

		public object context { get; protected set; }


		public ITween<T> setEaseType( EaseType easeType )
		{
			_easeType = easeType;
			return this;
		}


		public ITween<T> setDelay( float delay )
		{
			_delay = delay;
			_elapsedTime = -_delay;
			return this;
		}


		public ITween<T> setDuration( float duration )
		{
			_duration = duration;
			return this;
		}


		public ITween<T> setTimeScale( float timeScale )
		{
			_timeScale = timeScale;
			return this;
		}


		public ITween<T> setIsTimeScaleIndependent()
		{
			_isTimeScaleIndependent = true;
			return this;
		}
		

		public ITween<T> setCompletionHandler( Action<ITween<T>> completionHandler )
		{
			_completionHandler = completionHandler;
			return this;
		}
		

		public ITween<T> setLoops( LoopType loopType, int loops = 1, float delayBetweenLoops = 0f )
		{
			_loopType = loopType;
			_delayBetweenLoops = delayBetweenLoops;

			// double the loop count for ping-pong
			if( loopType == LoopType.PingPong )
				loops = loops * 2;
			_loops = loops;

			return this;
		}
		

		public ITween<T> setLoopCompletionHandler( Action<ITween<T>> loopCompleteHandler )
		{
			_loopCompleteHandler = loopCompleteHandler;
			return this;
		}


		public ITween<T> setFrom( T from )
		{
			_isFromValueOverridden = true;
			_fromValue = from;
			return this;
		}


		public ITween<T> prepareForReuse( T from, T to, float duration )
		{
			initialize( _target, to, duration );
			return this;
		}


		public ITween<T> setRecycleTween( bool shouldRecycleTween )
		{
			_shouldRecycleTween = shouldRecycleTween;
			return this;
		}


		abstract public ITween<T> setIsRelative();


		public ITween<T> setContext( object context )
		{
			this.context = context;
			return this;
		}


		public ITween<T> setNextTween( ITweenable nextTween )
		{
			_nextTween = nextTween;
			return this;
		}

		#endregion


		#region ITweenable

		public bool tick()
		{
			if( _tweenState == TweenState.Paused )
				return false;

			// when we loop we clamp values between 0 and duration. this will hold the excess that we clamped off so it can be reapplied
			var elapsedTimeExcess = 0f;
			if( !_isRunningInReverse && _elapsedTime >= _duration )
			{
				elapsedTimeExcess = _elapsedTime - _duration;
				_elapsedTime = _duration;
				_tweenState = TweenState.Complete;
			}
			else if( _isRunningInReverse && _elapsedTime <= 0 )
			{
				elapsedTimeExcess = 0 - _elapsedTime;
				_elapsedTime = 0f;
				_tweenState = TweenState.Complete;
			}

			// elapsed time will be negative while we are delaying the start of the tween so dont update the value
			if( _elapsedTime >= 0 && _elapsedTime <= _duration )
				updateValue();

			// if we have a loopType and we are Complete (meaning we reached 0 or duration) handle the loop.
			// handleLooping will take any excess elapsedTime and factor it in and call udpateValue if necessary to keep
			// the tween perfectly accurate.
			if( _loopType != LoopType.None && _tweenState == TweenState.Complete && _loops > 0 )
				handleLooping( elapsedTimeExcess );

			var deltaTime = _isTimeScaleIndependent ? Time.unscaledDeltaTime : Time.deltaTime;
			deltaTime *= _timeScale;

			// running in reverse? then we need to subtract deltaTime
			if( _isRunningInReverse )
				_elapsedTime -= deltaTime;
			else
				_elapsedTime += deltaTime;

			if( _tweenState == TweenState.Complete )
			{
				if( _completionHandler != null )
					_completionHandler( this );

				// if we have a nextTween add it to TweenManager so that it can start running
				if( _nextTween != null )
				{
					_nextTween.start();
					_nextTween = null;
				}

				return true;
			}

			return false;
		}


		public virtual void recycleSelf()
		{
			if( _shouldRecycleTween )
			{
				_target = null;
				_nextTween = null;
			}
		}


		public bool isRunning()
		{
			return _tweenState == TweenState.Running;
		}


		public virtual void start()
		{
			if( !_isFromValueOverridden )
				_fromValue = _target.getTweenedValue();
			
			if( _tweenState == TweenState.Complete )
			{				
				_tweenState = TweenState.Running;
				TweenManager.addTween( this );
			}
		}


		public void pause()
		{
			_tweenState = TweenState.Paused;
		}


		public void resume()
		{
			_tweenState = TweenState.Running;
		}


		public void stop( bool bringToCompletion = false )
		{
			_tweenState = TweenState.Complete;

			if( bringToCompletion )
			{
				// if we are running in reverse we finish up at 0 else we go to duration
				_elapsedTime = _isRunningInReverse ? 0f : _duration;
				_loopType = LoopType.None;
				_loops = 0;

				// TweenManager will handle removal on the next tick
			}
			else
			{
				TweenManager.removeTween( this );
			}
		}
			
		#endregion


		#region ITweenControl

		public void jumpToElapsedTime( float elapsedTime )
		{
			_elapsedTime = Mathf.clamp( elapsedTime, 0f, _duration );
			updateValue();
		}

		
		/// <summary>
		/// reverses the current tween. if it was going forward it will be going backwards and vice versa.
		/// </summary>
		public void reverseTween()
		{
			_isRunningInReverse = !_isRunningInReverse;
		}


		/// <summary>
		/// when called via StartCoroutine this will continue until the tween completes
		/// </summary>
		/// <returns>The for completion.</returns>
		public IEnumerator waitForCompletion()
		{
			while( _tweenState != TweenState.Complete )
				yield return null;
		}


		public object getTargetObject()
		{
			return _target.getTargetObject();
		}

		#endregion


		void resetState()
		{
			context = null;
			_completionHandler = _loopCompleteHandler = null;
			_isFromValueOverridden = false;
			_isTimeScaleIndependent = false;
			_tweenState = TweenState.Complete;
			// TODO: I don't think we should ever flip the flag from _shouldRecycleTween = false without the user's consent. Needs research and some thought
			//_shouldRecycleTween = true;
			_isRelative = false;
			_easeType = TweenManager.defaultEaseType;

			if( _nextTween != null )
			{
				_nextTween.recycleSelf();
				_nextTween = null;
			}
			
			_delay = 0f;
			_duration = 0f;
			_timeScale = 1f;
			_elapsedTime = 0f;
			_loopType = LoopType.None;
			_delayBetweenLoops = 0f;
			_loops = 0;
			_isRunningInReverse = false;
		}


		/// <summary>
		/// resets all state to defaults and sets the initial state based on the paramaters passed in. This method serves
		/// as an entry point so that Tween subclasses can call it and so that tweens can be recycled. When recycled,
		/// the constructor will not be called again so this method encapsulates what the constructor would be doing.
		/// </summary>
		/// <param name="target">Target.</param>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public void initialize( ITweenTarget<T> target, T to, float duration )
		{
			// reset state in case we were recycled
			resetState();

			_target = target;
			_toValue = to;
			_duration = duration;
		}

		
		/// <summary>
		/// handles loop logic
		/// </summary>
		void handleLooping( float elapsedTimeExcess )
		{
			_loops--;
			if( _loopType == LoopType.PingPong )
			{
				reverseTween();
			}

			if( _loopType == LoopType.RestartFromBeginning || _loops % 2 == 0 )
			{
				if( _loopCompleteHandler != null )
					_loopCompleteHandler( this );
			}

			// if we have loops left to process reset our state back to Running so we can continue processing them
			if( _loops > 0 )
			{
				_tweenState = TweenState.Running;

				// now we need to set our elapsed time and factor in our elapsedTimeExcess
				if( _loopType == LoopType.RestartFromBeginning )
				{
					_elapsedTime = elapsedTimeExcess - _delayBetweenLoops;
				}
				else
				{
					if( _isRunningInReverse )
						_elapsedTime += _delayBetweenLoops - elapsedTimeExcess;
					else
						_elapsedTime = elapsedTimeExcess - _delayBetweenLoops;
				}
					
				// if we had an elapsedTimeExcess and no delayBetweenLoops update the value
				if( _delayBetweenLoops == 0f && elapsedTimeExcess > 0f )
					updateValue();
			}


		}
			

		abstract protected void updateValue();

	}
}
