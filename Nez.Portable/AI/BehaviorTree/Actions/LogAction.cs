using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// simple task which will output the specified text and return success. It can be used for debugging.
	/// </summary>
	public class LogAction<T> : Behavior<T>
	{
		/// <summary>
		/// text to log
		/// </summary>
		public string text;

		/// <summary>
		/// is this text an error
		/// </summary>
		public bool isError;


		public LogAction( string text )
		{
			this.text = text;
		}


		public override TaskStatus update( T context )
		{
			if( isError )
				Debug.error( text );
			else
				Debug.log( text );

			return TaskStatus.Success;
		}
	}
}

