namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// Same as Sequence except it shuffles the children when started
	/// </summary>
	public class RandomSequence<T> : Sequence<T>
	{
		public override void OnStart()
		{
			_children.Shuffle();
		}
	}
}