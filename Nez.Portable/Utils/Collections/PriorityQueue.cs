using System.Runtime.CompilerServices;
using Nez;


// sourced from: https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
namespace System.Collections.Generic
{
	/// <summary>
	/// An implementation of a min-Priority Queue using a heap.  Has O(1) .Contains()!
	/// See https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp/wiki/Getting-Started for more information
	/// </summary>
	/// <typeparam name="T">The values in the queue.  Must extend the FastPriorityQueueNode class</typeparam>
	public sealed class PriorityQueue<T> : IPriorityQueue<T> where T : PriorityQueueNode
	{
		int _numNodes;
		T[] _nodes;
		long _numNodesEverEnqueued;


		/// <summary>
		/// Instantiate a new Priority Queue
		/// </summary>
		/// <param name="maxNodes">The max nodes ever allowed to be enqueued (going over this will cause undefined behavior)</param>
		public PriorityQueue(int maxNodes)
		{
			Insist.IsTrue(maxNodes > 0, "New queue size cannot be smaller than 1");

			_numNodes = 0;
			_nodes = new T[maxNodes + 1];
			_numNodesEverEnqueued = 0;
		}

		/// <summary>
		/// Returns the number of nodes in the queue.
		/// O(1)
		/// </summary>
		public int Count => _numNodes;

		/// <summary>
		/// Returns the maximum number of items that can be enqueued at once in this queue.  Once you hit this number (ie. once Count == MaxSize),
		/// attempting to enqueue another item will cause undefined behavior.  O(1)
		/// </summary>
		public int MaxSize => _nodes.Length - 1;


		/// <summary>
		/// Removes every node from the queue.
		/// O(n) (So, don't do this often!)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Clear()
		{
			Array.Clear(_nodes, 1, _numNodes);
			_numNodes = 0;
		}


		/// <summary>
		/// Returns (in O(1)!) whether the given node is in the queue.  O(1)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool Contains(T node)
		{
			Insist.IsNotNull(node, "node cannot be null");
			Insist.IsFalse(node.QueueIndex < 0 || node.QueueIndex >= _nodes.Length,
				"node.QueueIndex has been corrupted. Did you change it manually? Or add this node to another queue?");

			return (_nodes[node.QueueIndex] == node);
		}


		/// <summary>
		/// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
		/// If the queue is full, the result is undefined.
		/// If the node is already enqueued, the result is undefined.
		/// O(log n)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Enqueue(T node, int priority)
		{
			Insist.IsNotNull(node, "node cannot be null");
			Insist.IsFalse(_numNodes >= _nodes.Length - 1, "Queue is full - node cannot be added: " + node);
			Insist.IsFalse(Contains(node), "Node is already enqueued: " + node);

			node.Priority = priority;
			_numNodes++;
			_nodes[_numNodes] = node;
			node.QueueIndex = _numNodes;
			node.InsertionIndex = _numNodesEverEnqueued++;
			CascadeUp(_nodes[_numNodes]);
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void Swap(T node1, T node2)
		{
			//Swap the nodes
			_nodes[node1.QueueIndex] = node2;
			_nodes[node2.QueueIndex] = node1;

			//Swap their indicies
			int temp = node1.QueueIndex;
			node1.QueueIndex = node2.QueueIndex;
			node2.QueueIndex = temp;
		}


		//Performance appears to be slightly better when this is NOT inlined o_O
		void CascadeUp(T node)
		{
			//aka Heapify-up
			int parent = node.QueueIndex / 2;
			while (parent >= 1)
			{
				T parentNode = _nodes[parent];
				if (HasHigherPriority(parentNode, node))
					break;

				//Node has lower priority value, so move it up the heap
				Swap(node,
					parentNode); //For some reason, this is faster with Swap() rather than (less..?) individual operations, like in CascadeDown()

				parent = node.QueueIndex / 2;
			}
		}


		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void CascadeDown(T node)
		{
			//aka Heapify-down
			T newParent;
			int finalQueueIndex = node.QueueIndex;
			while (true)
			{
				newParent = node;
				int childLeftIndex = 2 * finalQueueIndex;

				//Check if the left-child is higher-priority than the current node
				if (childLeftIndex > _numNodes)
				{
					//This could be placed outside the loop, but then we'd have to check newParent != node twice
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}

				T childLeft = _nodes[childLeftIndex];
				if (HasHigherPriority(childLeft, newParent))
				{
					newParent = childLeft;
				}

				//Check if the right-child is higher-priority than either the current node or the left child
				int childRightIndex = childLeftIndex + 1;
				if (childRightIndex <= _numNodes)
				{
					T childRight = _nodes[childRightIndex];
					if (HasHigherPriority(childRight, newParent))
					{
						newParent = childRight;
					}
				}

				//If either of the children has higher (smaller) priority, swap and continue cascading
				if (newParent != node)
				{
					//Move new parent to its new index.  node will be moved once, at the end
					//Doing it this way is one less assignment operation than calling Swap()
					_nodes[finalQueueIndex] = newParent;

					int temp = newParent.QueueIndex;
					newParent.QueueIndex = finalQueueIndex;
					finalQueueIndex = temp;
				}
				else
				{
					//See note above
					node.QueueIndex = finalQueueIndex;
					_nodes[finalQueueIndex] = node;
					break;
				}
			}
		}


		/// <summary>
		/// Returns true if 'higher' has higher priority than 'lower', false otherwise.
		/// Note that calling HasHigherPriority(node, node) (ie. both arguments the same node) will return false
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool HasHigherPriority(T higher, T lower)
		{
			return (higher.Priority < lower.Priority ||
			        (higher.Priority == lower.Priority && higher.InsertionIndex < lower.InsertionIndex));
		}


		/// <summary>
		/// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
		/// If queue is empty, result is undefined
		/// O(log n)
		/// </summary>
		public T Dequeue()
		{
			Insist.IsFalse(_numNodes <= 0, "Cannot call Dequeue() on an empty queue");
			Insist.IsTrue(IsValidQueue(),
				"Queue has been corrupted (Did you update a node priority manually instead of calling UpdatePriority()?" +
				"Or add the same node to two different queues?)");

			T returnMe = _nodes[1];
			Remove(returnMe);
			return returnMe;
		}


		/// <summary>
		/// Resize the queue so it can accept more nodes.  All currently enqueued nodes are remain.
		/// Attempting to decrease the queue size to a size too small to hold the existing nodes results in undefined behavior
		/// O(n)
		/// </summary>
		public void Resize(int maxNodes)
		{
			Insist.IsFalse(maxNodes <= 0, "Queue size cannot be smaller than 1");
			Insist.IsFalse(maxNodes < _numNodes,
				"Called Resize(" + maxNodes + "), but current queue contains " + _numNodes + " nodes");

			T[] newArray = new T[maxNodes + 1];
			int highestIndexToCopy = Math.Min(maxNodes, _numNodes);
			for (int i = 1; i <= highestIndexToCopy; i++)
			{
				newArray[i] = _nodes[i];
			}

			_nodes = newArray;
		}


		/// <summary>
		/// Returns the head of the queue, without removing it (use Dequeue() for that).
		/// If the queue is empty, behavior is undefined.
		/// O(1)
		/// </summary>
		public T First
		{
			get
			{
				Insist.IsFalse(_numNodes <= 0, "Cannot call .First on an empty queue");
				return _nodes[1];
			}
		}


		/// <summary>
		/// This method must be called on a node every time its priority changes while it is in the queue.  
		/// <b>Forgetting to call this method will result in a corrupted queue!</b>
		/// Calling this method on a node not in the queue results in undefined behavior
		/// O(log n)
		/// </summary>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void UpdatePriority(T node, int priority)
		{
			Insist.IsNotNull(node, "node cannot be null");
			Insist.IsFalse(Contains(node), "Cannot call UpdatePriority() on a node which is not enqueued: " + node);

			node.Priority = priority;
			OnNodeUpdated(node);
		}


		void OnNodeUpdated(T node)
		{
			//Bubble the updated node up or down as appropriate
			int parentIndex = node.QueueIndex / 2;
			T parentNode = _nodes[parentIndex];

			if (parentIndex > 0 && HasHigherPriority(node, parentNode))
			{
				CascadeUp(node);
			}
			else
			{
				//Note that CascadeDown will be called if parentNode == node (that is, node is the root)
				CascadeDown(node);
			}
		}


		/// <summary>
		/// Removes a node from the queue.  The node does not need to be the head of the queue.  
		/// If the node is not in the queue, the result is undefined.  If unsure, check Contains() first
		/// O(log n)
		/// </summary>
		public void Remove(T node)
		{
			Insist.IsNotNull(node, "node cannot be null");
			Insist.IsTrue(Contains(node), "Cannot call Remove() on a node which is not enqueued: " + node);

			//If the node is already the last node, we can remove it immediately
			if (node.QueueIndex == _numNodes)
			{
				_nodes[_numNodes] = null;
				_numNodes--;
				return;
			}

			//Swap the node with the last node
			T formerLastNode = _nodes[_numNodes];
			Swap(node, formerLastNode);
			_nodes[_numNodes] = null;
			_numNodes--;

			//Now bubble formerLastNode (which is no longer the last node) up or down as appropriate
			OnNodeUpdated(formerLastNode);
		}


		public IEnumerator<T> GetEnumerator()
		{
			for (var i = 1; i <= _numNodes; i++)
				yield return _nodes[i];
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// <b>Should not be called in production code.</b>
		/// Checks to make sure the queue is still in a valid state.  Used for testing/debugging the queue.
		/// </summary>
		public bool IsValidQueue()
		{
			for (int i = 1; i < _nodes.Length; i++)
			{
				if (_nodes[i] != null)
				{
					int childLeftIndex = 2 * i;
					if (childLeftIndex < _nodes.Length && _nodes[childLeftIndex] != null &&
					    HasHigherPriority(_nodes[childLeftIndex], _nodes[i]))
						return false;

					int childRightIndex = childLeftIndex + 1;
					if (childRightIndex < _nodes.Length && _nodes[childRightIndex] != null &&
					    HasHigherPriority(_nodes[childRightIndex], _nodes[i]))
						return false;
				}
			}

			return true;
		}
	}
}