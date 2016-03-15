using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple static class that can be used to pool any object. this is meant for use with non-Unity classes such as tweens.
	/// </summary>
	public static class Pool<T> where T : new()
	{
		private static Stack<T> _objectStack = new Stack<T>( 10 );


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
					_objectStack.Push( new T() );
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
		public static T obtain()
		{
			if( _objectStack.Count > 0 )
				return _objectStack.Pop();

			return new T();
		}


		/// <summary>
		/// pushes an item back on the stack
		/// </summary>
		/// <param name="obj">Object.</param>
		public static void free( T obj )
		{
			_objectStack.Push( obj );

			if( obj is IPoolable )
				( (IPoolable)obj ).reset();
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
		void reset();
	}
}