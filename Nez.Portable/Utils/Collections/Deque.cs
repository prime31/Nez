using Nez;
using Nez.IEnumerableExtensions;


namespace System.Collections.Generic
{
	/// <summary>
	/// sourced from: https://github.com/tejacques/Deque
	/// A generic Deque class. It can be thought of as a double-ended queue, hence Deque. This allows for
	/// an O(1) AddFront, AddBack, RemoveFront, RemoveBack. The Deque also has O(1) indexed lookup, as it is backed
	/// by a circular array.
	/// </summary>
	/// <typeparam name="T">
	/// The type of objects to store in the deque.
	/// </typeparam>
	public class Deque<T> : IList<T>
	{
		/// <summary>
		/// The default capacity of the deque.
		/// </summary>
		const int defaultCapacity = 16;

		/// <summary>
		/// The first element offset from the beginning of the data array.
		/// </summary>
		int startOffset;

		/// <summary>
		/// The circular array holding the items.
		/// </summary>
		T[] buffer;

		/// <summary>
		/// Creates a new instance of the Deque class with
		/// the default capacity.
		/// </summary>
		public Deque() : this(defaultCapacity)
		{
		}

		/// <summary>
		/// Creates a new instance of the Deque class with
		/// the specified capacity.
		/// </summary>
		/// <param name="capacity">The initial capacity of the Deque.</param>
		public Deque(int capacity)
		{
			if (capacity < 0)
			{
				throw new ArgumentOutOfRangeException(
					"capacity", "capacity is less than 0.");
			}

			Capacity = capacity;
		}

		/// <summary>
		/// Create a new instance of the Deque class with the elements
		/// from the specified collection.
		/// </summary>
		/// <param name="collection">The co</param>
		public Deque(IEnumerable<T> collection) : this(collection.Count())
		{
			InsertRange(0, collection);
		}

		int capacityClosestPowerOfTwoMinusOne;

		/// <summary>
		/// Gets or sets the total number of elements
		/// the internal array can hold without resizing.
		/// </summary>
		public int Capacity
		{
			get => buffer.Length;

			set
			{
				if (value < 0)
				{
					throw new ArgumentOutOfRangeException(
						"value",
						"Capacity is less than 0.");
				}
				else if (value < Count)
				{
					throw new InvalidOperationException(
						"Capacity cannot be set to a value less than Count");
				}
				else if (null != buffer && value == buffer.Length)
				{
					return;
				}

				// Create a new array and copy the old values.
				var powOfTwo = Mathf.ClosestPowerOfTwoGreaterThan(value);

				value = powOfTwo;

				T[] newBuffer = new T[value];
				CopyTo(newBuffer, 0);

				// Set up to use the new buffer.
				buffer = newBuffer;
				startOffset = 0;
				capacityClosestPowerOfTwoMinusOne = powOfTwo - 1;
			}
		}

		/// <summary>
		/// Gets whether or not the Deque is filled to capacity.
		/// </summary>
		public bool IsFull => Count == Capacity;

		/// <summary>
		/// Gets whether or not the Deque is empty.
		/// </summary>
		public bool IsEmpty => 0 == Count;

		void EnsureCapacityFor(int numElements)
		{
			if (Count + numElements > Capacity)
			{
				Capacity = Count + numElements;
			}
		}

		int ToBufferIndex(int index)
		{
			int bufferIndex;

			bufferIndex = (index + startOffset)
			              & capacityClosestPowerOfTwoMinusOne;

			return bufferIndex;
		}

		void CheckIndexOutOfRange(int index)
		{
			if (index >= Count)
			{
				throw new IndexOutOfRangeException(
					"The supplied index is greater than the Count");
			}
		}

		static void CheckArgumentsOutOfRange(int length, int offset, int count)
		{
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "Invalid offset " + offset);
			}

			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "Invalid count " + count);
			}

			if (length - offset < count)
			{
				throw new ArgumentException(
					String.Format(
						"Invalid offset ({0}) or count + ({1}) "
						+ "for source length {2}",
						offset, count, length));
			}
		}

		int ShiftStartOffset(int value)
		{
			startOffset = ToBufferIndex(value);

			return startOffset;
		}

		int PreShiftStartOffset(int value)
		{
			int offset = startOffset;
			ShiftStartOffset(value);
			return offset;
		}

		int PostShiftStartOffset(int value)
		{
			return ShiftStartOffset(value);
		}


		#region IEnumerable

		/// <summary>
		/// Returns an enumerator that iterates through the Deque.
		/// </summary>
		/// <returns>
		/// An iterator that can be used to iterate through the Deque.
		/// </returns>
		public IEnumerator<T> GetEnumerator()
		{
			// The below is done for performance reasons.
			// Rather than doing bounds checking and modulo arithmetic
			// that would go along with calls to Get(index), we can skip
			// all of that by referencing the underlying array.

			if (startOffset + Count > Capacity)
			{
				for (int i = startOffset; i < Capacity; i++)
				{
					yield return buffer[i];
				}

				int endIndex = ToBufferIndex(Count);
				for (int i = 0; i < endIndex; i++)
				{
					yield return buffer[i];
				}
			}
			else
			{
				int endIndex = startOffset + Count;
				for (int i = startOffset; i < endIndex; i++)
				{
					yield return buffer[i];
				}
			}
		}

		/// <summary>
		/// Returns an enumerator that iterates through the Deque.
		/// </summary>
		/// <returns>
		/// An iterator that can be used to iterate through the Deque.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		#endregion

		#region ICollection

		/// <summary>
		/// Gets a value indicating whether the Deque is read-only.
		/// </summary>
		bool ICollection<T>.IsReadOnly => false;

		/// <summary>
		/// Gets the number of elements contained in the Deque.
		/// </summary>
		public int Count { get; set; }

		void IncrementCount(int value)
		{
			Count = Count + value;
		}

		void DecrementCount(int value)
		{
			Count = Math.Max(Count - value, 0);
		}

		/// <summary>
		/// Adds an item to the Deque.
		/// </summary>
		/// <param name="item">The object to add to the Deque.</param>
		public void Add(T item)
		{
			AddBack(item);
		}

		void ClearBuffer(int logicalIndex, int length)
		{
			int offset = ToBufferIndex(logicalIndex);
			if (offset + length > Capacity)
			{
				int len = Capacity - offset;
				Array.Clear(buffer, offset, len);

				len = ToBufferIndex(logicalIndex + length);
				Array.Clear(buffer, 0, len);
			}
			else
			{
				Array.Clear(buffer, offset, length);
			}
		}

		/// <summary>
		/// Removes all items from the Deque.
		/// </summary>
		public void Clear()
		{
			if (Count > 0)
			{
				ClearBuffer(0, Count);
			}

			Count = 0;
			startOffset = 0;
		}

		/// <summary>
		/// Determines whether the Deque contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the Deque.</param>
		/// <returns>
		/// true if item is found in the Deque; otherwise, false.
		/// </returns>
		public bool Contains(T item)
		{
			return IndexOf(item) != -1;
		}

		/// <summary>
		///     Copies the elements of the Deque to a System.Array,
		///     starting at a particular System.Array index.
		/// </summary>
		/// <param name="array">
		///     The one-dimensional System.Array that is the destination of
		///     the elements copied from the Deque. The System.Array must
		///     have zero-based indexing.
		/// </param>
		/// <param name="arrayIndex">
		///     The zero-based index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">
		///     array is null.
		/// </exception>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     arrayIndex is less than 0.
		/// </exception>
		/// <exception cref="System.ArgumentException">
		///     The number of elements in the source Deque is greater than
		///     the available space from arrayIndex to the end of the
		///     destination array.
		/// </exception>
		public void CopyTo(T[] array, int arrayIndex)
		{
			if (null == array)
			{
				throw new ArgumentNullException("array", "Array is null");
			}

			// Nothing to copy
			if (null == buffer)
			{
				return;
			}

			CheckArgumentsOutOfRange(array.Length, arrayIndex, Count);

			if (0 != startOffset
			    && startOffset + Count >= Capacity)
			{
				int lengthFromStart = Capacity - startOffset;
				int lengthFromEnd = Count - lengthFromStart;

				Array.Copy(
					buffer, startOffset, array, 0, lengthFromStart);

				Array.Copy(
					buffer, 0, array, lengthFromStart, lengthFromEnd);
			}
			else
			{
				Array.Copy(
					buffer, startOffset, array, 0, Count);
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the Deque.
		/// </summary>
		/// <param name="item">The object to remove from the Deque.</param>
		/// <returns>
		///     true if item was successfully removed from the Deque;
		///     otherwise, false. This method also returns false if item
		///     is not found in the original
		/// </returns>
		public bool Remove(T item)
		{
			bool result = true;
			int index = IndexOf(item);

			if (-1 == index)
			{
				result = false;
			}
			else
			{
				RemoveAt(index);
			}

			return result;
		}

		#endregion

		#region List<T>

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">
		///     The zero-based index of the element to get or set.
		/// </param>
		/// <returns>The element at the specified index</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">
		///     <paramref name="index"/> is not a valid index in this deque
		/// </exception>
		public T this[int index]
		{
			get => Get(index);

			set => Set(index, value);
		}

		/// <summary>
		/// Inserts an item to the Deque at the specified index.
		/// </summary>
		/// <param name="index">
		/// The zero-based index at which item should be inserted.
		/// </param>
		/// <param name="item">The object to insert into the Deque.</param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index in the Deque.
		/// </exception>
		public void Insert(int index, T item)
		{
			EnsureCapacityFor(1);

			if (index == 0)
			{
				AddFront(item);
				return;
			}
			else if (index == Count)
			{
				AddBack(item);
				return;
			}

			InsertRange(index, new[] {item});
		}

		/// <summary>
		/// Determines the index of a specific item in the deque.
		/// </summary>
		/// <param name="item">The object to locate in the deque.</param>
		/// <returns>
		/// The index of the item if found in the deque; otherwise, -1.
		/// </returns>
		public int IndexOf(T item)
		{
			int index = 0;
			foreach (var myItem in this)
			{
				if (myItem.Equals(item))
				{
					break;
				}

				++index;
			}

			if (index == Count)
			{
				index = -1;
			}

			return index;
		}

		/// <summary>
		/// Removes the item at the specified index.
		/// </summary>
		/// <param name="index">
		/// The zero-based index of the item to remove.
		/// </param>
		/// <exception cref="System.ArgumentOutOfRangeException">
		/// <paramref name="index"/> is not a valid index in the Deque.
		/// </exception>
		public void RemoveAt(int index)
		{
			if (index == 0)
			{
				RemoveFront();
				return;
			}
			else if (index == Count - 1)
			{
				RemoveBack();
				return;
			}

			RemoveRange(index, 1);
		}

		#endregion

		/// <summary>
		/// Adds the provided item to the front of the Deque.
		/// </summary>
		/// <param name="item">The item to add.</param>
		public void AddFront(T item)
		{
			EnsureCapacityFor(1);
			buffer[PostShiftStartOffset(-1)] = item;
			IncrementCount(1);
		}

		/// <summary>
		/// Adds the provided item to the back of the Deque.
		/// </summary>
		/// <param name="item">The item to add.</param>
		public void AddBack(T item)
		{
			EnsureCapacityFor(1);
			buffer[ToBufferIndex(Count)] = item;
			IncrementCount(1);
		}

		/// <summary>
		/// Removes an item from the front of the Deque and returns it.
		/// </summary>
		/// <returns>The item at the front of the Deque.</returns>
		public T RemoveFront()
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException("The Deque is empty");
			}

			T result = buffer[startOffset];
			buffer[PreShiftStartOffset(1)] = default(T);
			DecrementCount(1);
			return result;
		}

		/// <summary>
		/// Removes an item from the back of the Deque and returns it.
		/// </summary>
		/// <returns>The item in the back of the Deque.</returns>
		public T RemoveBack()
		{
			if (IsEmpty)
				throw new InvalidOperationException("The Deque is empty");

			DecrementCount(1);
			var endIndex = ToBufferIndex(Count);
			T result = buffer[endIndex];
			buffer[endIndex] = default(T);

			return result;
		}

		/// <summary>
		/// Adds a collection of items to the Deque.
		/// </summary>
		/// <param name="collection">The collection to add.</param>
		public void AddRange(IEnumerable<T> collection)
		{
			AddBackRange(collection);
		}

		/// <summary>
		/// Adds a collection of items to the front of the Deque.
		/// </summary>
		/// <param name="collection">The collection to add.</param>
		public void AddFrontRange(IEnumerable<T> collection)
		{
			AddFrontRange(collection, 0, collection.Count());
		}

		/// <summary>
		/// Adds count items from a collection of items
		/// from a specified index to the Deque.
		/// </summary>
		/// <param name="collection">The collection to add.</param>
		/// <param name="fromIndex">
		/// The index in the collection to begin adding from.
		/// </param>
		/// <param name="count">
		/// The number of items in the collection to add.
		/// </param>
		public void AddFrontRange(IEnumerable<T> collection, int fromIndex, int count)
		{
			InsertRange(0, collection, fromIndex, count);
		}

		/// <summary>
		/// Adds a collection of items to the back of the Deque.
		/// </summary>
		/// <param name="collection">The collection to add.</param>
		public void AddBackRange(IEnumerable<T> collection)
		{
			AddBackRange(collection, 0, collection.Count());
		}

		/// <summary>
		/// Adds count items from a collection of items
		/// from a specified index to the back of the Deque.
		/// </summary>
		/// <param name="collection">The collection to add.</param>
		/// <param name="fromIndex">
		/// The index in the collection to begin adding from.
		/// </param>
		/// <param name="count">
		/// The number of items in the collection to add.
		/// </param>
		public void AddBackRange(IEnumerable<T> collection, int fromIndex, int count)
		{
			InsertRange(Count, collection, fromIndex, count);
		}

		/// <summary>
		/// Inserts a collection of items into the Deque
		/// at the specified index.
		/// </summary>
		/// <param name="index">
		/// The index in the Deque to insert the collection.
		/// </param>
		/// <param name="collection">The collection to add.</param>
		public void InsertRange(int index, IEnumerable<T> collection)
		{
			var count = collection.Count();
			InsertRange(index, collection, 0, count);
		}

		/// <summary>
		/// Inserts count items from a collection of items from a specified
		/// index into the Deque at the specified index.
		/// </summary>
		/// <param name="index">
		/// The index in the Deque to insert the collection.
		/// </param>
		/// <param name="collection">The collection to add.</param>
		/// <param name="fromIndex">
		/// The index in the collection to begin adding from.
		/// </param>
		/// <param name="count">
		/// The number of items in the colleciton to add.
		/// </param>
		public void InsertRange(int index, IEnumerable<T> collection, int fromIndex, int count)
		{
			CheckIndexOutOfRange(index - 1);

			if (0 == count)
			{
				return;
			}

			// Make room
			EnsureCapacityFor(count);

			if (index < Count / 2)
			{
				// Inserting into the first half of the list

				if (index > 0)
				{
					// Move items down:
					//  [0, index) -> 
					//  [Capacity - count, Capacity - count + index)
					int copyCount = index;
					int shiftIndex = Capacity - count;
					for (int j = 0; j < copyCount; j++)
					{
						buffer[ToBufferIndex(shiftIndex + j)] =
							buffer[ToBufferIndex(j)];
					}
				}

				// shift the starting offset
				ShiftStartOffset(-count);
			}
			else
			{
				// Inserting into the second half of the list

				if (index < Count)
				{
					// Move items up:
					// [index, Count) -> [index + count, count + Count)
					int copyCount = Count - index;
					int shiftIndex = index + count;
					for (int j = 0; j < copyCount; j++)
					{
						buffer[ToBufferIndex(shiftIndex + j)] =
							buffer[ToBufferIndex(index + j)];
					}
				}
			}

			// Copy new items into place
			int i = index;
			foreach (T item in collection)
			{
				buffer[ToBufferIndex(i)] = item;
				++i;
			}

			// Adjust valid count
			IncrementCount(count);
		}

		/// <summary>
		///     Removes a range of elements from the view.
		/// </summary>
		/// <param name="index">
		///     The index into the view at which the range begins.
		/// </param>
		/// <param name="count">
		///     The number of elements in the range. This must be greater
		///     than 0 and less than or equal to <see cref="Count"/>.
		/// </param>
		public void RemoveRange(int index, int count)
		{
			if (IsEmpty)
			{
				throw new InvalidOperationException("The Deque is empty");
			}

			if (index > Count - count)
			{
				throw new IndexOutOfRangeException(
					"The supplied index is greater than the Count");
			}

			// Clear out the underlying array
			ClearBuffer(index, count);

			if (index == 0)
			{
				// Removing from the beginning: shift the start offset
				ShiftStartOffset(count);
				Count -= count;
				return;
			}
			else if (index == Count - count)
			{
				// Removing from the ending: trim the existing view
				Count -= count;
				return;
			}

			if ((index + (count / 2)) < Count / 2)
			{
				// Removing from first half of list

				// Move items up:
				//  [0, index) -> [count, count + index)
				int copyCount = index;
				int writeIndex = count;
				for (int j = 0; j < copyCount; j++)
				{
					buffer[ToBufferIndex(writeIndex + j)]
						= buffer[ToBufferIndex(j)];
				}

				// Rotate to new view
				ShiftStartOffset(count);
			}
			else
			{
				// Removing from second half of list

				// Move items down:
				// [index + collectionCount, count) ->
				// [index, count - collectionCount)
				int copyCount = Count - count - index;
				int readIndex = index + count;
				for (int j = 0; j < copyCount; ++j)
				{
					buffer[ToBufferIndex(index + j)] =
						buffer[ToBufferIndex(readIndex + j)];
				}
			}

			// Adjust valid count
			DecrementCount(count);
		}

		/// <summary>
		/// Gets the value at the specified index of the Deque
		/// </summary>
		/// <param name="index">The index of the Deque.</param>
		/// <returns></returns>
		public T Get(int index)
		{
			CheckIndexOutOfRange(index);
			return buffer[ToBufferIndex(index)];
		}

		/// <summary>
		/// Sets the value at the specified index of the
		/// Deque to the given item.
		/// </summary>
		/// <param name="index">The index of the deque to set the item.</param>
		/// <param name="item">The item to set at the specified index.</param>
		public void Set(int index, T item)
		{
			CheckIndexOutOfRange(index);
			buffer[ToBufferIndex(index)] = item;
		}
	}
}