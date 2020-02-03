namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Action that calls through to another Reasoner
	/// </summary>
	public class ReasonerAction<T> : IAction<T>
	{
		Reasoner<T> _reasoner;


		public ReasonerAction(Reasoner<T> reasoner)
		{
			_reasoner = reasoner;
		}


		void IAction<T>.Execute(T context)
		{
			var action = _reasoner.Select(context);
			if (action != null)
				action.Execute(context);
		}
	}
}