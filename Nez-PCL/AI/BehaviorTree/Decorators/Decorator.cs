using System;


namespace Nez.AI.BehaviorTrees
{
	public abstract class Decorator<T> : Behavior<T>
	{
		public Behavior<T> child;


		public override void invalidate()
		{
			base.invalidate();
			child.invalidate();
		}
	}
}

