using System;
using System.Collections;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// very basic wrapper around an array that auto-expands it when it reaches capacity. Note that when iterating it should be done
	/// like this accessing the buffer directly but using the FastList.length field:
	/// 
	/// for( var i = 0; i &lt;= list.length; i++ )
	/// 	var item = list.buffer[i];
	/// </summary>
	public class FastList<T>
	{
		/// <summary>
		/// direct access to the backing buffer. Do not use buffer.Length! Use FastList.length
		/// </summary>
		public T[] Buffer;

		/// <summary>
		/// direct access to the length of the filled items in the buffer. Do not change.
		/// </summary>
		public int Length = 0;


		public FastList(int size)
		{
			Buffer = new T[size];
		}


		public FastList() : this(5)
		{
		}


		/// <summary>
		/// provided for ease of access though it is recommended to just access the buffer directly.
		/// </summary>
		/// <param name="index">Index.</param>
		public T this[int index] => Buffer[index];


		/// <summary>
		/// clears the list and nulls out all items in the buffer
		/// </summary>
		public void Clear()
		{
			Array.Clear(Buffer, 0, Length);
			Length = 0;
		}


		/// <summary>
		/// works just like clear except it does not null our all the items in the buffer. Useful when dealing with structs.
		/// </summary>
		public void Reset()
		{
			Length = 0;
		}


		/// <summary>
		/// adds the item to the list
		/// </summary>
		public void Add(T item)
		{
			if (Length == Buffer.Length)
				Array.Resize(ref Buffer, Math.Max(Buffer.Length << 1, 10));
			Buffer[Length++] = item;
		}


		/// <summary>
		/// removes the item from the list
		/// </summary>
		/// <param name="item">Item.</param>
		public void Remove(T item)
		{
			var comp = EqualityComparer<T>.Default;
			for (var i = 0; i < Length; ++i)
			{
				if (comp.Equals(Buffer[i], item))
				{
					RemoveAt(i);
					return;
				}
			}
		}


		/// <summary>
		/// removes the item at the given index from the list
		/// </summary>
		public void RemoveAt(int index)
		{
			Insist.IsTrue(index < Length, "Index out of range!");

			Length--;
			if (index < Length)
				Array.Copy(Buffer, index + 1, Buffer, index, Length - index);
			Buffer[Length] = default(T);
		}


		/// <summary>
		/// removes the item at the given index from the list but does NOT maintain list order
		/// </summary>
		/// <param name="index">Index.</param>
		public void RemoveAtWithSwap(int index)
		{
			Insist.IsTrue(index < Length, "Index out of range!");

			Buffer[index] = Buffer[Length - 1];
			Buffer[Length - 1] = default(T);
			--Length;
		}


		/// <summary>
		/// checks to see if item is in the FastList
		/// </summary>
		/// <param name="item">Item.</param>
		public bool Contains(T item)
		{
			var comp = EqualityComparer<T>.Default;
			for (var i = 0; i < Length; ++i)
			{
				if (comp.Equals(Buffer[i], item))
					return true;
			}

			return false;
		}


		/// <summary>
		/// if the buffer is at its max more space will be allocated to fit additionalItemCount
		/// </summary>
		public void EnsureCapacity(int additionalItemCount = 1)
		{
			if (Length + additionalItemCount >= Buffer.Length)
				Array.Resize(ref Buffer, Math.Max(Buffer.Length << 1, Length + additionalItemCount));
		}


		/// <summary>
		/// adds all items from array
		/// </summary>
		/// <param name="array">Array.</param>
		public void AddRange(IEnumerable<T> array)
		{
			foreach (var item in array)
				Add(item);
		}


		/// <summary>
		/// sorts all items in the buffer up to length
		/// </summary>
		public void Sort()
		{
			Array.Sort(Buffer, 0, Length);
		}


		/// <summary>
		/// sorts all items in the buffer up to length
		/// </summary>
		public void Sort(IComparer comparer)
		{
			Array.Sort(Buffer, 0, Length, comparer);
		}


		/// <summary>
		/// sorts all items in the buffer up to length
		/// </summary>
		public void Sort(IComparer<T> comparer)
		{
			Array.Sort(Buffer, 0, Length, comparer);
		}
	}
}