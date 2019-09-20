namespace Nez.AI.UtilityAI
{
	/// <summary>
	/// Action that logs text
	/// </summary>
	public class LogAction<T> : IAction<T>
	{
		string _text;


		public LogAction(string text)
		{
			_text = text;
		}


		void IAction<T>.Execute(T context)
		{
			Debug.Log(_text);
		}
	}
}