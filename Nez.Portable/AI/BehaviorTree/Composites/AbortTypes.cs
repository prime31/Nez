using System;


namespace Nez.AI.BehaviorTrees
{
	[Flags]
	public enum AbortTypes
	{
		/// <summary>
		/// no abort type. the current action will always run even if other conditionals change state
		/// </summary>
		None,

		/// <summary>
		/// If a more important conditional task changes status it can issue an abort that will stop the lower priority tasks from running
		/// and shift control back to the higher priority branch. This type should be set on Composites that are children of the evaulating
		/// Composite. The parent Composite will check it's children to see if they have a LowerPriority abort.
		/// </summary>
		LowerPriority,

		/// <summary>
		/// The Conditional task can only abort an Action task if they are both children of the Composite. This AbortType only affects the
		/// actual Composite that it is set on unlike LowerPriority which affects its parent Composite.
		/// </summary>
		Self,

		/// <summary>
		/// both LowerPriority and Self aborts are checked
		/// </summary>
		Both = Self | LowerPriority
	}


	public static class AbortTypesExt
	{
		public static bool Has(this AbortTypes self, AbortTypes check)
		{
			return (self & check) == check;
		}
	}
}