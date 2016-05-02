using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// will keep executing its child task until the child task returns failure
	/// </summary>
	public class UntilFail<T> : Decorator<T>
	{
		public override TaskStatus update( T context )
		{
			Assert.isNotNull( child, "child must not be null" );

			var status = child.update( context );

			if( status != TaskStatus.Failure )
				return TaskStatus.Running;

			return TaskStatus.Success;
		}
	}
}

