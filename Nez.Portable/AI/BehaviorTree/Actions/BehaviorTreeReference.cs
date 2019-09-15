namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// runs an entire BehaviorTree as a child and returns success
	/// </summary>
	public class BehaviorTreeReference<T> : Behavior<T>
	{
		BehaviorTree<T> _childTree;


		public BehaviorTreeReference(BehaviorTree<T> tree)
		{
			_childTree = tree;
		}


		public override TaskStatus Update(T context)
		{
			_childTree.Tick();
			return TaskStatus.Success;
		}
	}
}