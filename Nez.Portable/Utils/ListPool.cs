using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple static class that can be used to pool Lists
	/// </summary>
	public static class ListPool<T>
	{
		static readonly Queue<List<T>> _listQueue = new Queue<List<T>>();


		/// <summary>
		/// warms up the cache filling it with a max of cacheCount lists
		/// </summary>
		/// <param name="cacheCount">new cache count</param>
		public static void warmCache( int cacheCount )
		{
			cacheCount -= _listQueue.Count;
			if( cacheCount > 0 )
			{
				for( var i = 0; i < cacheCount; i++ )
					_listQueue.Enqueue( new List<T>() );
			}
		}


		/// <summary>
		/// trims the cache down to cacheCount lists
		/// </summary>
		/// <param name="cacheCount">Cache count.</param>
		public static void trimCache( int cacheCount )
		{
			while( cacheCount > _listQueue.Count )
				_listQueue.Dequeue();
		}


		/// <summary>
		/// clears out the cache
		/// </summary>
		public static void clearCache()
		{
			_listQueue.Clear();
		}


		/// <summary>
		/// pops a list off the stack if available creating a new list as necessary
		/// </summary>
		public static List<T> obtain()
		{
			if( _listQueue.Count > 0 )
				return _listQueue.Dequeue();

			return new List<T>();
		}


		/// <summary>
		/// pushes an list back on the stack
		/// </summary>
		/// <param name="list">List.</param>
		public static void free( List<T> list )
		{
			_listQueue.Enqueue( list );
			list.Clear();
		}
	}
}