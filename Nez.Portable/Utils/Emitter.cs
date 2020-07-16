using System;
using System.Collections.Generic;


namespace Nez.Systems
{
	/// <summary>
	/// simple event emitter that is designed to have its generic contraint be either an int or an enum
	/// </summary>
	public class Emitter<T> where T : struct, IComparable, IFormattable
	{
		Dictionary<T, List<Action>> _messageTable;


		public Emitter()
		{
			_messageTable = new Dictionary<T, List<Action>>();
		}

		/// <summary>
		/// if using an enum as the generic constraint you may want to pass in a custom comparer to avoid boxing/unboxing. See the CoreEventsComparer
		/// for an example implementation.
		/// </summary>
		/// <param name="customComparer">Custom comparer.</param>
		public Emitter(IEqualityComparer<T> customComparer)
		{
			_messageTable = new Dictionary<T, List<Action>>(customComparer);
		}

		public void AddObserver(T eventType, Action handler)
		{
			List<Action> list = null;
			if (!_messageTable.TryGetValue(eventType, out list))
			{
				list = new List<Action>();
				_messageTable.Add(eventType, list);
			}

			Insist.IsFalse(list.Contains(handler), "You are trying to add the same observer twice");
			list.Add(handler);
		}

		public void RemoveObserver(T eventType, Action handler)
		{
			// we purposely do this in unsafe fashion so that it will throw an Exception if someone tries to remove a handler that
			// was never added
			_messageTable[eventType].Remove(handler);
		}

		public void Emit(T eventType)
		{
			List<Action> list = null;
			if (_messageTable.TryGetValue(eventType, out list))
			{
				for (var i = list.Count - 1; i >= 0; i--)
					list[i]();
			}
		}
	}


	/// <summary>
	/// simple event emitter that is designed to have its generic contraint be either an int or an enum. this variant lets you pass around
	/// data with each event. See InputEvent for an example.
	/// </summary>
	public class Emitter<T, U> where T : struct, IComparable, IFormattable
	{
		Dictionary<T, List<Action<U>>> _messageTable;


		public Emitter()
		{
			_messageTable = new Dictionary<T, List<Action<U>>>();
		}

		/// <summary>
		/// if using an enum as the generic constraint you may want to pass in a custom comparer to avoid boxing/unboxing. See the CoreEventsComparer
		/// for an example implementation.
		/// </summary>
		/// <param name="customComparer">Custom comparer.</param>
		public Emitter(IEqualityComparer<T> customComparer)
		{
			_messageTable = new Dictionary<T, List<Action<U>>>(customComparer);
		}

		public void AddObserver(T eventType, Action<U> handler)
		{
			List<Action<U>> list = null;
			if (!_messageTable.TryGetValue(eventType, out list))
			{
				list = new List<Action<U>>();
				_messageTable.Add(eventType, list);
			}

			Insist.IsFalse(list.Contains(handler), "You are trying to add the same observer twice");
			list.Add(handler);
		}

		public void RemoveObserver(T eventType, Action<U> handler)
		{
			// we purposely do this in unsafe fashion so that it will throw an Exception if someone tries to remove a handler that
			// was never added
			_messageTable[eventType].Remove(handler);
		}

		public void Emit(T eventType, U data)
		{
			List<Action<U>> list = null;
			if (_messageTable.TryGetValue(eventType, out list))
			{
				for (var i = list.Count - 1; i >= 0; i--)
					list[i](data);
			}
		}
	}
}