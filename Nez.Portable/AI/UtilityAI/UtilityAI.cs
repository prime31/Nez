namespace Nez.AI.UtilityAI
{
	public class UtilityAI<T>
	{
		/// <summary>
		/// how often the behavior tree should update. An updatePeriod of 0.2 will make the tree update 5 times a second.
		/// </summary>
		public float UpdatePeriod;

		/// <summary>
		/// The context should contain all the data needed to run the tree
		/// </summary>
		T _context;

		Reasoner<T> _rootReasoner;
		float _elapsedTime;


		public UtilityAI(T context, Reasoner<T> rootSelector, float updatePeriod = 0.2f)
		{
			_rootReasoner = rootSelector;
			_context = context;
			UpdatePeriod = _elapsedTime = updatePeriod;
		}


		public void Tick()
		{
			_elapsedTime -= Time.DeltaTime;
			while (_elapsedTime <= 0)
			{
				_elapsedTime += UpdatePeriod;
				var action = _rootReasoner.Select(_context);
				if (action != null)
					action.Execute(_context);
			}
		}
	}
}