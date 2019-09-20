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


		public ConditionalDecorator(IConditional<T> conditional, bool shouldReevalute)
		{
			Insist.IsTrue(conditional is IConditional<T>, "conditional must implment IConditional");
			_conditional = conditional;
			_shouldReevaluate = shouldReevalute;
		}


		public ConditionalDecorator(IConditional<T> conditional) : this(conditional, true)
		{
		}


		public override void Invalidate()
		{
			base.Invalidate();
			_conditionalStatus = TaskStatus.Invalid;
		}


		public override void OnStart()
		{
			_conditionalStatus = TaskStatus.Invalid;
		}


		public override TaskStatus Update(T context)
		{
			Insist.IsNotNull(Child, "child must not be null");

			// evalute the condition if we need to
			_conditionalStatus = ExecuteConditional(context);

			if (_conditionalStatus == TaskStatus.Success)
				return Child.Tick(context);

			return TaskStatus.Failure;
		}


		/// <summary>
		/// executes the conditional either following the shouldReevaluate flag or with an option to force an update. Aborts will force the
		/// update to make sure they get the proper data if a Conditional changes.
		/// </summary>
		/// <returns>The conditional.</returns>
		/// <param name="context">Context.</param>
		/// <param name="forceUpdate">If set to <c>true</c> force update.</param>
		internal TaskStatus ExecuteConditional(T context, bool forceUpdate = false)
		{
			if (forceUpdate || _shouldReevaluate || _conditionalStatus == TaskStatus.Invalid)
				_conditionalStatus = _conditional.Update(context);
			return _conditionalStatus;
		}
	}
}