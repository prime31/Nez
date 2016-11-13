using System;


namespace Nez.AI.BehaviorTrees
{
	/// <summary>
	/// decorator that will only run its child if a condition is met. By default, the condition will be reevaluated every tick.
	/// </summary>
	public class ConditionalDecorator<T> : Decorator<T>, IConditional<T>
	{
		IConditional<T> _conditional;
		bool _shouldReevaluate;
		TaskStatus _conditionalStatus;


		public ConditionalDecorator( IConditional<T> conditional, bool shouldReevalute )
		{
			Assert.isTrue( conditional is IConditional<T>, "conditional must implment IConditional" );
			_conditional = conditional;
			_shouldReevaluate = shouldReevalute;
		}


		public ConditionalDecorator( IConditional<T> conditional ) : this( conditional, true )
		{}


		public override void invalidate()
		{
			base.invalidate();
			_conditionalStatus = TaskStatus.Invalid;
		}


		public override void onStart()
		{
			_conditionalStatus = TaskStatus.Invalid;
		}

		
		public override TaskStatus update( T context )
		{
			Assert.isNotNull( child, "child must not be null" );

			// evalute the condition if we need to
			_conditionalStatus = executeConditional( context );
			
			if( _conditionalStatus == TaskStatus.Success )
				return child.tick( context );

			return TaskStatus.Failure;
		}


		/// <summary>
		/// executes the conditional either following the shouldReevaluate flag or with an option to force an update. Aborts will force the
		/// update to make sure they get the proper data if a Conditional changes.
		/// </summary>
		/// <returns>The conditional.</returns>
		/// <param name="context">Context.</param>
		/// <param name="forceUpdate">If set to <c>true</c> force update.</param>
		internal TaskStatus executeConditional( T context, bool forceUpdate = false )
		{
			if( forceUpdate || _shouldReevaluate || _conditionalStatus == TaskStatus.Invalid )
				_conditionalStatus = _conditional.update( context );
			return _conditionalStatus;
		}

	}
}

