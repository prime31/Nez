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
	public class SimpleStateMachine<TEnum> : Component, IUpdatable where TEnum : struct, IComparable, IFormattable
	{
		class StateMethodCache
		{
			public Action enterState;
			public Action tick;
			public Action exitState;
		}

		protected float elapsedTimeInState = 0f;
		protected TEnum previousState;
		Dictionary<TEnum,StateMethodCache> _stateCache;
		StateMethodCache _stateMethods;

		TEnum _currentState;
		protected TEnum currentState
		{
			get
			{
				return _currentState;
			}
			set
			{
				// dont change to the current state
				if( _stateCache.Comparer.Equals( _currentState, value ) )
					return;
				
				// swap previous/current
				previousState = _currentState;
				_currentState = value;

				// exit the state, fetch the next cached state methods then enter that state
				if( _stateMethods.exitState != null )
					_stateMethods.exitState();

				elapsedTimeInState = 0f;
				_stateMethods = _stateCache[_currentState];

				if( _stateMethods.enterState != null )
					_stateMethods.enterState();
			}
		}

		protected TEnum initialState
		{
			set
			{
				_currentState = value;
				_stateMethods = _stateCache[_currentState];

				if( _stateMethods.enterState != null )
					_stateMethods.enterState();
			}
		}


		public SimpleStateMachine( IEqualityComparer<TEnum> customComparer = null )
		{
			_stateCache = new Dictionary<TEnum,StateMethodCache>( customComparer );

			// cache all of our state methods
			var enumValues = (TEnum[])Enum.GetValues( typeof( TEnum ) );
			foreach( var e in enumValues )
				configureAndCacheState( e );
		}


		public virtual void update()
		{
			elapsedTimeInState += Time.deltaTime;

			if( _stateMethods.tick != null )
				_stateMethods.tick();
		}


		void configureAndCacheState( TEnum stateEnum )
		{
			var stateName = stateEnum.ToString();

			var state = new StateMethodCache();
			state.enterState = getDelegateForMethod( stateName + "_Enter" );
			state.tick = getDelegateForMethod( stateName + "_Tick" );
			state.exitState = getDelegateForMethod( stateName + "_Exit" );

			_stateCache[stateEnum] = state;
		}


		Action getDelegateForMethod( string methodName )
		{
			var methodInfo = ReflectionUtils.getMethodInfo( this, methodName );
			if( methodInfo != null )
				return ReflectionUtils.createDelegate<Action>( this, methodInfo );

			return null;
		}

	}
}

