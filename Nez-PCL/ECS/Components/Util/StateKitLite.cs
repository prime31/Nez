using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// Simple state machine with an enum or int constraint. There are some rules you must follow when using this:
	/// - before update is called initialState must be set (use the constructor, onAddedToEntity or onAwake)
	/// - if you implement update in your subclass you must call base.update()
	/// 
	/// Note: if you use an enum as the contraint you can avoid allocations/boxing in Mono by doing what the Core
	/// Emitter does for its enum
	/// </summary>
	public class StateKitLite<TEnum> : Component where TEnum : struct, IConvertible, IComparable, IFormattable
	{
		class StateMethodCache
		{
			public Action enterState;
			public Action tick;
			public Action exitState;
		}

		StateMethodCache _stateMethods;
		protected float elapsedTimeInState = 0f;
		protected TEnum previousState;
		Dictionary<TEnum,StateMethodCache> _stateCache = new Dictionary<TEnum,StateMethodCache>();

		TEnum _currentState;
		protected TEnum currentState
		{
			get
			{
				return _currentState;
			}
			set
			{
				if( _currentState.Equals( value ) )
					return;

				// swap previous/current
				previousState = _currentState;
				_currentState = value;

				// exit the state, fetch the next cached state methods then enter that state
				if( _stateMethods.exitState != null )
					_stateMethods.exitState();

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


		public StateKitLite()
		{
			Debug.assertIsTrue( typeof( TEnum ).IsEnum, "[StateKitLite] TEnum generic constraint failed! You must use an enum when declaring your subclass!" );

			// cache all of our state methods
			var enumValues = (TEnum[])Enum.GetValues( typeof( TEnum ) );
			foreach( var e in enumValues )
				configureAndCacheState( e );
		}


		public override void update()
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
			var methodInfo = GetType().GetMethod( methodName,
				System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic );

			if( methodInfo != null )
				return Delegate.CreateDelegate( typeof( Action ), this, methodInfo ) as Action;

			return null;
		}

	}
}

