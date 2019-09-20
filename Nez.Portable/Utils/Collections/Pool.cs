using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple static class that can be used to pool any object
	/// </summary>
	public static class Pool<T> where T : new()
	{
		private static Queue<T> _objectQueue = new Queue<T>(10);


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
					_objectQueue.Enqueue(new T());
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
		public static T Obtain()
		{
			if (_objectQueue.Count > 0)
				return _objectQueue.Dequeue();

			return new T();
		}


		/// <summary>
		/// pushes an item back on the stack
		/// </summary>
		/// <param name="obj">Object.</param>
		public static void Free(T obj)
		{
			_objectQueue.Enqueue(obj);

			if (obj is IPoolable)
				((IPoolable) obj).Reset();
		}
	}


	/// <summary>
	/// Objects implementing this interface will have {@link #reset()} called when passed to {@link #push(Object)}
	/// </summary>
	public interface IPoolable
	{
		/// <summary>
		/// Resets the object for reuse. Object references should be nulled and fields may be set to default values
		/// </summary>
		void Reset();
	}
}