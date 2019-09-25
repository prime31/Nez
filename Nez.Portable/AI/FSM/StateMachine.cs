using System;
using System.Collections.Generic;


namespace Nez.AI.FSM
{
	public class StateMachine<T>
	{
		public event Action OnStateChanged;

		public State<T> CurrentState => _currentState;

		public State<T> PreviousState;
		public float ElapsedTimeInState = 0f;

		protected State<T> _currentState;
		protected T _context;
		Dictionary<Type, State<T>> _states = new Dictionary<Type, State<T>>();


		public StateMachine(T context, State<T> initialState)
		{
			_context = context;

			// setup our initial state
			AddState(initialState);
			_currentState = initialState;
			_currentState.Begin();
		}


		/// <summary>
		/// adds the state to the machine
		/// </summary>
		public void AddState(State<T> state)
		{
			state.SetMachineAndContext(this, _context);
			_states[state.GetType()] = state;
		}


		/// <summary>
		/// ticks the state machine with the provided delta time
		/// </summary>
		public virtual void Update(float deltaTime)
		{
			ElapsedTimeInState += deltaTime;
			_currentState.Reason();
			_currentState.Update(deltaTime);
		}

		/// <summary>
		/// Gets a specific state from the machine without having to
		/// change to it.
		/// </summary>
		public virtual R GetState<R>() where R : State<T>
		{
			var type = typeof(R);
			Insist.IsTrue(_states.ContainsKey(type),
				"{0}: state {1} does not exist. Did you forget to add it by calling addState?", GetType(), type);

			return (R)_states[type];
		}


		/// <summary>
		/// changes the current state
		/// </summary>
		public R ChangeState<R>() where R : State<T>
		{
			// avoid changing to the same state
			var newType = typeof(R);
			if (_currentState.GetType() == newType)
				return _currentState as R;

			// only call end if we have a currentState
			if (_currentState != null)
				_currentState.End();

			Insist.IsTrue(_states.ContainsKey(newType),
				"{0}: state {1} does not exist. Did you forget to add it by calling addState?", GetType(), newType);

			// swap states and call begin
			ElapsedTimeInState = 0f;
			PreviousState = _currentState;
			_currentState = _states[newType];
			_currentState.Begin();

			// fire the changed event if we have a listener
			if (OnStateChanged != null)
				OnStateChanged();

			return _currentState as R;
		}
	}
}