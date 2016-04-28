using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple static class that can be used to pool Lists
	/// </summary>
	public static class ListPool<T> where T : new()
	{
		static Stack<List<T>> _objectStack = new Stack<List<T>>();


		/// <summary>
		/// warms up the cache filling it with a max of cacheCount objects
		/// </summary>
		/// <param name="cacheCount">new cache count</param>
		public static void warmCache( int cacheCount )
		{
			cacheCount -= _objectStack.Count;
			if( cacheCount > 0 )
			{
				for( var i = 0; i < cacheCount; i++ )
					_objectStack.Push( new List<T>() );
			}
		}


		/// <summary>
		/// trims the cache down to cacheCount items
		/// </summary>
		/// <param name="cacheCount">Cache count.</param>
		public static void trimCache( int cacheCount )
		{
			while( cacheCount > _objectStack.Count )
				_objectStack.Pop();
		}


		/// <summary>
		/// clears out the cache
		/// </summary>
		public static void clearCache()
		{
			_objectStack.Clear();
		}


		/// <summary>
		/// pops an item off the stack if available creating a new item as necessary
		/// </summary>
		public static List<T> obtain()
		{
			if( _objectStack.Count > 0 )
				return _objectStack.Pop();

			return new List<T>();
		}


		/// <summary>
		/// pushes an item back on the stack
		/// </summary>
		/// <param name="obj">Object.</param>
		public static void free( List<T> obj )
		{
			_objectStack.Push( obj );
			obj.Clear();
		}
	}
}