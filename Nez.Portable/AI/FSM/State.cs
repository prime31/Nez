namespace Nez.AI.FSM
{
	public abstract class State<T>
	{
		protected StateMachine<T> _machine;
		protected T _context;


		public void SetMachineAndContext(StateMachine<T> machine, T context)
		{
			_machine = machine;
			_context = context;
			OnInitialized();
		}


		/// <summary>
		/// called directly after the machine and context are set allowing the state to do any required setup
		/// </summary>
		public virtual void OnInitialized()
		{
		}


		/// <summary>
		/// called when the state becomes the active state
		/// </summary>
		public virtual void Begin()
		{
		}


		/// <summary>
		/// called before update allowing the state to have one last chance to change state
		/// </summary>
		public virtual void Reason()
		{
		}


		/// <summary>
		/// called every frame this state is the active state
		/// </summary>
		/// <param name="deltaTime">Delta time.</param>
		public abstract void Update(float deltaTime);


		/// <summary>
		/// called when this state is no longer the active state
		/// </summary>
		public virtual void End()
		{
		}
	}
}