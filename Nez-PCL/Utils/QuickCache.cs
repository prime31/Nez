using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// simple static class that can be used to pool any object. this is meant for use with non-Unity classes such as tweens.
	/// </summary>
	public static class QuickCache<T> where T : new()
	{
		private static Stack<T> _objectStack = new Stack<T>( 10 );


		/// <summary>
		/// warms up the cache filling it with a max of howMany objects
		/// </summary>
		/// <param name="howMany">How many.</param>
		public static void warmCache( int howMany = 3 )
		{
			howMany -= _objectStack.Count;
			if( howMany > 0 )
			{
				for( var i = 0; i < howMany; i++ )
					_objectStack.Push( new T() );
			}
		}


		/// <summary>
		/// pops an item off the stack if available creating a new item as necessary
		/// </summary>
		public static T pop()
		{
			if( _objectStack.Count > 0 )
				return _objectStack.Pop();

			return new T();
		}


		/// <summary>
		/// pushes an item back on the stack
		/// </summary>
		/// <param name="obj">Object.</param>
		public static void push( T obj )
		{
			_objectStack.Push( obj );
		}
	}
}