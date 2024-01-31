using System;
using System.Collections.Generic;


namespace Nez.AI.FSM
{
	/// <summary>
	/// Simple state machine with an enum constraint. There are some rules you must follow when using this:
	/// - before update is called initialState must be set (use the constructor or onAddedToEntity)
	/// - if you implement update in your subclass you must call base.update()
	/// 
	/// Note: if you use an enum as the contraint you can avoid allocations/boxing in Mono by doing what the Core
	/// Emitter does for its enum: pass in a IEqualityComparer to the constructor.
	/// </summary>
	public abstract class SimpleStateMachine<TEnum> : Component, IUpdatable
		where TEnum : struct, IComparable, IFormattable
	{
		class StateMethodCache
		{
			public Action EnterState;
			public Action Tick;
			public Action ExitState;
		}

		protected float elapsedTimeInState = 0f;
		protected TEnum previousState;
		Dictionary<TEnum, StateMethodCache> _stateCache;
		StateMethodCache _stateMethods;

		TEnum _currentState;

		public TEnum CurrentState
		{
			get => _currentState;
			set
			{
				// dont change to the current state
				if (_stateCache.Comparer.Equals(_currentState, value))
					return;

				// swap previous/current
				previousState = _currentState;
				_currentState = value;

				// exit the state, fetch the next cached state methods then enter that state
				if (_stateMethods.ExitState != null)
					_stateMethods.ExitState();

				elapsedTimeInState = 0f;
				_stateMethods = _stateCache[_currentState];

				if (_stateMethods.EnterState != null)
					_stateMethods.EnterState();
			}
		}

		protected TEnum InitialState
		{
			set
			{
				_currentState = value;
				_stateMethods = _stateCache[_currentState];

				if (_stateMethods.EnterState != null)
					_stateMethods.EnterState();
			}
		}


		public SimpleStateMachine(IEqualityComparer<TEnum> customComparer = null)
		{
			_stateCache = new Dictionary<TEnum, StateMethodCache>(customComparer);

			// cache all of our state methods
			var enumValues = (TEnum[]) Enum.GetValues(typeof(TEnum));
			foreach (var e in enumValues)
				ConfigureAndCacheState(e);
		}

		public virtual void Update()
		{
			elapsedTimeInState += Time.DeltaTime;

			if (_stateMethods.Tick != null)
				_stateMethods.Tick();
		}

		void ConfigureAndCacheState(TEnum stateEnum)
		{
			var stateName = stateEnum.ToString();

			var state = new StateMethodCache();
			state.EnterState = GetDelegateForMethod(stateName + "_Enter");
			state.Tick = GetDelegateForMethod(stateName + "_Tick");
			state.ExitState = GetDelegateForMethod(stateName + "_Exit");

			_stateCache[stateEnum] = state;
		}

		Action GetDelegateForMethod(string methodName)
		{
			var methodInfo = ReflectionUtils.GetMethodInfo(this, methodName);
			if (methodInfo != null)
				return ReflectionUtils.CreateDelegate<Action>(this, methodInfo);

			return null;
		}
	}
}
