using System;
using System.Collections.Generic;


namespace Nez.AI.FSM
{
	public class StateMachine<T>
	{
		protected T _context;
		public event Action onStateChanged;

		public State<T> currentState { get { return _currentState; } }
		public State<T> previousState;
		public float elapsedTimeInState = 0f;


		private Dictionary<System.Type,State<T>> _states = new Dictionary<System.Type,State<T>>();
		private State<T> _currentState;


		public StateMachine( T context, State<T> initialState )
		{
			this._context = context;

			// setup our initial state
			addState( initialState );
			_currentState = initialState;
			_currentState.begin();
		}


		/// <summary>
		/// adds the state to the machine
		/// </summary>
		public void addState( State<T> state )
		{
			state.setMachineAndContext( this, _context );
			_states[state.GetType()] = state;
		}


		/// <summary>
		/// ticks the state machine with the provided delta time
		/// </summary>
		public void update( float deltaTime )
		{
			elapsedTimeInState += deltaTime;
			_currentState.reason();
			_currentState.update( deltaTime );
		}


		/// <summary>
		/// changes the current state
		/// </summary>
		public R changeState<R>() where R : State<T>
		{
			// avoid changing to the same state
			var newType = typeof( R );
			if( _currentState.GetType() == newType )
				return _currentState as R;

			// only call end if we have a currentState
			if( _currentState != null )
				_currentState.end();

			Assert.isTrue( _states.ContainsKey( newType ), "{0}: state {1} does not exist. Did you forget to add it by calling addState?", GetType(), newType );

			// swap states and call begin
			elapsedTimeInState = 0f;
			previousState = _currentState;
			_currentState = _states[newType];
			_currentState.begin();

			// fire the changed event if we have a listener
			if( onStateChanged != null )
				onStateChanged();

			return _currentState as R;
		}
	}
}

