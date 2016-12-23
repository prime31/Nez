using System;


namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// encapsulates an Action and generates a score that a Reasoner can use to decide which Consideration to use
	/// </summary>
	public interface IConsideration<T>
	{
		IAction<T> action { get; set; }

		float getScore( T context );
	}
}

