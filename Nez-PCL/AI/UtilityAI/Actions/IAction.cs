using System;


namespace Nez.AI.UtilityAI
{
	public interface IAction<T>
	{
		void execute( T context );
	}
}

