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
		public string Text;

		/// <summary>
		/// is this text an error
		/// </summary>
		public bool IsError;


		public LogAction(string text)
		{
			Text = text;
		}


		public override TaskStatus Update(T context)
		{
			if (IsError)
				Debug.Error(Text);
			else
				Debug.Log(Text);

			return TaskStatus.Success;
		}
	}
}