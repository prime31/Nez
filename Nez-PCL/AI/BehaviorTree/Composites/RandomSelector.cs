using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// Same as Selector except it shuffles the children when started
	/// </summary>
	public class RandomSelector<T> : Selector<T>
	{
		public override void onStart()
		{
			_children.shuffle();
		}
	}
}

