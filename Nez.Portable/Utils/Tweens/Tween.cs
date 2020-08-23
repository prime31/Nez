using System;
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

		public object Context { get; protected set; }


		public ITween<T> SetEaseType(EaseType easeType)
		{
			_easeType = easeType;
			return this;
		}


		public ITween<T> SetDelay(float delay)
		{
			_delay = delay;
			_elapsedTime = -_delay;
			return this;
		}


		public ITween<T> SetDuration(float duration)
		{
			_duration = duration;
			return this;
		}


		public ITween<T> SetTimeScale(float timeScale)
		{
			_timeScale = timeScale;
			return this;
		}


		public ITween<T> SetIsTimeScaleIndependent()
		{
			_isTimeScaleIndependent = true;
			return this;
		}


		public ITween<T> SetCompletionHandler(Action<ITween<T>> completionHandler)
		{
			_completionHandler = completionHandler;
			return this;
		}


		public ITween<T> SetLoops(LoopType loopType, int loops = 1, float delayBetweenLoops = 0f)
		{
			_loopType = loopType;
			_delayBetweenLoops = delayBetweenLoops;

			// negative for infinite loop, force -1
			if (loops < 0)
				loops = -1;

			// double the loop count for ping-pong
			if (loopType == LoopType.PingPong)
				loops = loops * 2;

			_loops = loops;

			return this;
		}


		public ITween<T> SetLoopCompletionHandler(Action<ITween<T>> loopCompleteHandler)
		{
			_loopCompleteHandler = loopCompleteHandler;
			return this;
		}


		public ITween<T> SetFrom(T from)
		{
			_isFromValueOverridden = true;
			_fromValue = from;
			return this;
		}


		public ITween<T> PrepareForReuse(T from, T to, float duration)
		{
			Initialize(_target, to, duration);
			return this;
		}


		public ITween<T> SetRecycleTween(bool shouldRecycleTween)
		{
			_shouldRecycleTween = shouldRecycleTween;
			return this;
		}


		abstract public ITween<T> SetIsRelative();


		public ITween<T> SetContext(object context)
		{
			Context = context;
			return this;
		}


		public ITween<T> SetNextTween(ITweenable nextTween)
		{
			_nextTween = nextTween;
			return this;
		}

		#endregion


		#region ITweenable

		public bool Tick()
		{
			if (_tweenState == TweenState.Paused)
				return false;

			// when we loop we clamp values between 0 and duration. this will hold the excess that we clamped off so it can be reapplied
			var elapsedTimeExcess = 0f;
			if (!_isRunningInReverse && _elapsedTime >= _duration)
			{
				elapsedTimeExcess = _elapsedTime - _duration;
				_elapsedTime = _duration;
				_tweenState = TweenState.Complete;
			}
			else if (_isRunningInReverse && _elapsedTime <= 0)
			{
				elapsedTimeExcess = 0 - _elapsedTime;
				_elapsedTime = 0f;
				_tweenState = TweenState.Complete;
			}

			// elapsed time will be negative while we are delaying the start of the tween so dont update the value
			if (_elapsedTime >= 0 && _elapsedTime <= _duration)
				UpdateValue();

			// if we have a loopType and we are Complete (meaning we reached 0 or duration) handle the loop.
			// handleLooping will take any excess elapsedTime and factor it in and call udpateValue if necessary to keep
			// the tween perfectly accurate.
			if (_loopType != LoopType.None && _tweenState == TweenState.Complete && _loops != 0)
				HandleLooping(elapsedTimeExcess);

			var deltaTime = _isTimeScaleIndependent ? Time.UnscaledDeltaTime : Time.DeltaTime;
			deltaTime *= _timeScale;

			// running in reverse? then we need to subtract deltaTime
			if (_isRunningInReverse)
				_elapsedTime -= deltaTime;
			else
				_elapsedTime += deltaTime;

			if (_tweenState == TweenState.Complete)
			{
				if (_completionHandler != null)
					_completionHandler(this);

				// if we have a nextTween add it to TweenManager so that it can start running
				if (_nextTween != null)
				{
					_nextTween.Start();
					_nextTween = null;
				}

				return true;
			}

			return false;
		}


		public virtual void RecycleSelf()
		{
			if (_shouldRecycleTween)
			{
				_target = null;
				_nextTween = null;
			}
		}


		public bool IsRunning()
		{
			return _tweenState == TweenState.Running;
		}


		public virtual void Start()
		{
			if (!_isFromValueOverridden)
				_fromValue = _target.GetTweenedValue();

			if (_tweenState == TweenState.Complete)
			{
				_tweenState = TweenState.Running;
				TweenManager.AddTween(this);
			}
		}


		public void Pause()
		{
			_tweenState = TweenState.Paused;
		}


		public void Resume()
		{
			_tweenState = TweenState.Running;
		}


		public void Stop(bool bringToCompletion = false)
		{
			_tweenState = TweenState.Complete;

			if (bringToCompletion)
			{
				// if we are running in reverse we finish up at 0 else we go to duration
				_elapsedTime = _isRunningInReverse ? 0f : _duration;
				_loopType = LoopType.None;
				_loops = 0;

				// TweenManager will handle removal on the next tick
			}
			else
			{
				TweenManager.RemoveTween(this);
			}
		}

		#endregion


		#region ITweenControl

		public void JumpToElapsedTime(float elapsedTime)
		{
			_elapsedTime = Mathf.Clamp(elapsedTime, 0f, _duration);
			UpdateValue();
		}


		/// <summary>
		/// reverses the current tween. if it was going forward it will be going backwards and vice versa.
		/// </summary>
		public void ReverseTween()
		{
			_isRunningInReverse = !_isRunningInReverse;
		}


		/// <summary>
		/// when called via StartCoroutine this will continue until the tween completes
		/// </summary>
		/// <returns>The for completion.</returns>
		public IEnumerator WaitForCompletion()
		{
			while (_tweenState != TweenState.Complete)
				yield return null;
		}


		public object GetTargetObject()
		{
			return _target.GetTargetObject();
		}

		#endregion


		void ResetState()
		{
			Context = null;
			_completionHandler = _loopCompleteHandler = null;
			_isFromValueOverridden = false;
			_isTimeScaleIndependent = false;
			_tweenState = TweenState.Complete;

			// TODO: I don't think we should ever flip the flag from _shouldRecycleTween = false without the user's consent. Needs research and some thought
			//_shouldRecycleTween = true;
			_isRelative = false;
			_easeType = TweenManager.DefaultEaseType;

			if (_nextTween != null)
			{
				_nextTween.RecycleSelf();
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
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public void Initialize(ITweenTarget<T> target, T to, float duration)
		{
			// reset state in case we were recycled
			ResetState();

			_target = target;
			_toValue = to;
			_duration = duration;
		}


		/// <summary>
		/// handles loop logic
		/// </summary>
		void HandleLooping(float elapsedTimeExcess)
		{
			_loops--;
			if (_loopType == LoopType.PingPong)
			{
				ReverseTween();
			}

			if (_loopType == LoopType.RestartFromBeginning || _loops % 2 == 0)
			{
				if (_loopCompleteHandler != null)
					_loopCompleteHandler(this);
			}

			// if we have loops left to process reset our state back to Running so we can continue processing them
			if (_loops != 0)
			{
				_tweenState = TweenState.Running;

				// now we need to set our elapsed time and factor in our elapsedTimeExcess
				if (_loopType == LoopType.RestartFromBeginning)
				{
					_elapsedTime = elapsedTimeExcess - _delayBetweenLoops;
				}
				else
				{
					if (_isRunningInReverse)
						_elapsedTime += _delayBetweenLoops - elapsedTimeExcess;
					else
						_elapsedTime = elapsedTimeExcess - _delayBetweenLoops;
				}

				// if we had an elapsedTimeExcess and no delayBetweenLoops update the value
				if (_delayBetweenLoops == 0f && elapsedTimeExcess > 0f)
					UpdateValue();
			}
		}


		abstract protected void UpdateValue();
	}
}