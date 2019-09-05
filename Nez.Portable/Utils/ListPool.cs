using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple static class that can be used to pool Lists
	/// </summary>
	public static class ListPool<T>
	{
		static readonly Queue<List<T>> _objectQueue = new Queue<List<T>>();


		/// <summary>
		/// warms up the cache filling it with a max of cacheCount objects
		/// </summary>
		/// <param name="cacheCount">new cache count</param>
		public static void WarmCache(int cacheCount)
		{
			cacheCount -= _objectQueue.Count;
			if (cacheCount > 0)
			{
				for (var i = 0; i < cacheCount; i++)
					_objectQueue.Enqueue(new List<T>());
			}
		}

		/// <summary>
		/// trims the cache down to cacheCount items
		/// </summary>
		/// <param name="cacheCount">Cache count.</param>
		public static void TrimCache(int cacheCount)
		{
			while (cacheCount > _objectQueue.Count)
				_objectQueue.Dequeue();
		}

		/// <summary>
		/// clears out the cache
		/// </summary>
		public static void ClearCache()
		{
			_objectQueue.Clear();
		}

		/// <summary>
		/// pops an item off the stack if available creating a new item as necessary
		/// </summary>
		public static List<T> Obtain()
		{
			if (_objectQueue.Count > 0)
				return _objectQueue.Dequeue();

			return new List<T>();
		}

		/// <summary>
		/// pushes an item back on the stack
		/// </summary>
		/// <param name="obj">Object.</param>
		public static void Free(List<T> obj)
		{
			_objectQueue.Enqueue(obj);
			obj.Clear();
		}
	}
}