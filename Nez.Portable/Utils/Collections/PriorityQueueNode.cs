namespace System.Collections.Generic
{
	public class PriorityQueueNode
	{
		/// <summary>
		/// The Priority to insert this node at.  Must be set BEFORE adding a node to the queue
		/// </summary>
		public int Priority { get; set; }

		/// <summary>
		/// <b>Used by the priority queue - do not edit this value.</b>
		/// Represents the order the node was inserted in
		/// </summary>
		public long InsertionIndex { get; set; }

		/// <summary>
		/// <b>Used by the priority queue - do not edit this value.</b>
		/// Represents the current position in the queue
		/// </summary>
		public int QueueIndex { get; set; }
	}
}