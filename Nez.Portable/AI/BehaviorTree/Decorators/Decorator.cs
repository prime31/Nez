namespace Nez.AI.BehaviorTrees
{
	public abstract class Decorator<T> : Behavior<T>
	{
		public Behavior<T> Child;


		public override void Invalidate()
		{
			base.Invalidate();
			Child.Invalidate();
		}
	}
}