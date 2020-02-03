namespace System.Collections.Generic
{
	public sealed class SimplePriorityQueue<T> : IPriorityQueue<T>
	{
		class SimpleNode : PriorityQueueNode
		{
			public T Data { get; set; }

			public SimpleNode(T data)
			{
				Data = data;
			}
		}


		const int INITIAL_QUEUE_SIZE = 10;
		readonly PriorityQueue<SimpleNode> _queue;


		public SimplePriorityQueue()
		{
			_queue = new PriorityQueue<SimpleNode>(INITIAL_QUEUE_SIZE);
		}

		/// <summary>
		/// Given an item of type T, returns the exist SimpleNode in the queue
		/// </summary>
		SimpleNode GetExistingNode(T item)
		{
			var comparer = EqualityComparer<T>.Default;
			foreach (var node in _queue)
			{
				if (comparer.Equals(node.Data, item))
				{
					return node;
				}
			}

			throw new InvalidOperationException("Item cannot be found in queue: " + item);
		}

		/// <summary>
		/// Returns the number of nodes in the queue.
		/// O(1)
		/// </summary>
		public int Count
		{
			get
			{
				lock (_queue)
				{
					return _queue.Count;
				}
			}
		}


		/// <summary>
		/// Returns the head of the queue, without removing it (use Dequeue() for that).
		/// Throws an exception when the queue is empty.
		/// O(1)
		/// </summary>
		public T First
		{
			get
			{
				lock (_queue)
				{
					if (_queue.Count <= 0)
					{
						throw new InvalidOperationException("Cannot call .First on an empty queue");
					}

					SimpleNode first = _queue.First;
					return (first != null ? first.Data : default(T));
				}
			}
		}

		/// <summary>
		/// Removes every node from the queue.
		/// O(n)
		/// </summary>
		public void Clear()
		{
			lock (_queue)
			{
				_queue.Clear();
			}
		}

		/// <summary>
		/// Returns whether the given item is in the queue.
		/// O(n)
		/// </summary>
		public bool Contains(T item)
		{
			lock (_queue)
			{
				var comparer = EqualityComparer<T>.Default;
				foreach (var node in _queue)
				{
					if (comparer.Equals(node.Data, item))
					{
						return true;
					}
				}

				return false;
			}
		}

		/// <summary>
		/// Removes the head of the queue (node with minimum priority; ties are broken by order of insertion), and returns it.
		/// If queue is empty, throws an exception
		/// O(log n)
		/// </summary>
		public T Dequeue()
		{
			lock (_queue)
			{
				if (_queue.Count <= 0)
				{
					throw new InvalidOperationException("Cannot call Dequeue() on an empty queue");
				}

				SimpleNode node = _queue.Dequeue();
				return node.Data;
			}
		}

		/// <summary>
		/// Enqueue a node to the priority queue.  Lower values are placed in front. Ties are broken by first-in-first-out.
		/// This queue automatically resizes itself, so there's no concern of the queue becoming 'full'.
		/// Duplicates are allowed.
		/// O(log n)
		/// </summary>
		public void Enqueue(T item, int priority)
		{
			lock (_queue)
			{
				SimpleNode node = new SimpleNode(item);
				if (_queue.Count == _queue.MaxSize)
				{
					_queue.Resize(_queue.MaxSize * 2 + 1);
				}

				_queue.Enqueue(node, priority);
			}
		}

		/// <summary>
		/// Removes an item from the queue.  The item does not need to be the head of the queue.  
		/// If the item is not in the queue, an exception is thrown.  If unsure, check Contains() first.
		/// If multiple copies of the item are enqueued, only the first one is removed. 
		/// O(n)
		/// </summary>
		public void Remove(T item)
		{
			lock (_queue)
			{
				try
				{
					_queue.Remove(GetExistingNode(item));
				}
				catch (InvalidOperationException ex)
				{
					throw new InvalidOperationException("Cannot call Remove() on a node which is not enqueued: " + item,
						ex);
				}
			}
		}

		/// <summary>
		/// Call this method to change the priority of an item.
		/// Calling this method on a item not in the queue will throw an exception.
		/// If the item is enqueued multiple times, only the first one will be updated.
		/// (If your requirements are complex enough that you need to enqueue the same item multiple times <i>and</i> be able
		/// to update all of them, please wrap your items in a wrapper class so they can be distinguished).
		/// O(n)
		/// </summary>
		public void UpdatePriority(T item, int priority)
		{
			lock (_queue)
			{
				try
				{
					SimpleNode updateMe = GetExistingNode(item);
					_queue.UpdatePriority(updateMe, priority);
				}
				catch (InvalidOperationException ex)
				{
					throw new InvalidOperationException(
						"Cannot call UpdatePriority() on a node which is not enqueued: " + item, ex);
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			List<T> queueData = new List<T>();
			lock (_queue)
			{
				//Copy to a separate list because we don't want to 'yield return' inside a lock
				foreach (var node in _queue)
				{
					queueData.Add(node.Data);
				}
			}

			return queueData.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public bool IsValidQueue()
		{
			lock (_queue)
			{
				return _queue.IsValidQueue();
			}
		}
	}
}