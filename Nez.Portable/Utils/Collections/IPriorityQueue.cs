﻿namespace System.Collections.Generic
{
	/// <summary>
	/// The IPriorityQueue interface.  This is mainly here for purists, and in case I decide to add more implementations later.
	/// For speed purposes, it is actually recommended that you *don't* access the priority queue through this interface, since the JIT can
	/// (theoretically?) optimize method calls from concrete-types slightly better.
	/// </summary>
	public interface IPriorityQueue<T> : IEnumerable<T>
	{
		/// <summary>
		/// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
		/// See implementation for how duplicates are handled.
		/// </summary>
		void Enqueue(T node, int priority);

		/// <summary>
		/// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
		/// </summary>
		T Dequeue();

		/// <summary>
		/// Removes every node from the queue.
		/// </summary>
		void Clear();

		/// <summary>
		/// Returns whether the given node is in the queue.
		/// </summary>
		bool Contains(T node);

		/// <summary>
		/// Removes a node from the queue.  The node does not need to be the head of the queue.  
		/// </summary>
		void Remove(T node);

		/// <summary>
		/// Call this method to change the priority of a node.  
		/// </summary>
		void UpdatePriority(T node, int priority);

		/// <summary>
		/// Returns the head of the queue, without removing it (use Dequeue() for that).
		/// </summary>
		T First { get; }

		/// <summary>
		/// Returns the number of nodes in the queue.
		/// </summary>
		int Count { get; }
	}
}