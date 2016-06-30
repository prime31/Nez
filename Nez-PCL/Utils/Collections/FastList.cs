using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// very basic wrapper around an array that auto-expands it when it reaches capacity
	/// </summary>
	public class FastList<T>
	{
		/// <summary>
		/// direct access to the backing buffer. Do not use buffer.Length! Use FastList.length
		/// </summary>
		public T[] buffer;

		/// <summary>
		/// direct access to the length of the filled items in the buffer. Do not change.
		/// </summary>
		public int length = 0;


		public FastList( int size )
		{
			buffer = new T[size];
		}


		public FastList() : this( 10 )
		{}


		/// <summary>
		/// clears the list and nulls out all items in the buffer
		/// </summary>
		public void clear()
		{
			for( var i = 0; i < buffer.Length; i++ )
				buffer[i] = default(T);
			length = 0;
		}


		/// <summary>
		/// adds the item to the list
		/// </summary>
		public void add( T item )
		{
			if( length == buffer.Length )
				Array.Resize( ref buffer, Math.Max( buffer.Length << 1, 10 ) );
			buffer[length++] = item;
		}


		/// <summary>
		/// removes the item from the list
		/// </summary>
		/// <param name="item">Item.</param>
		public void remove( T item )
		{
			var comp = EqualityComparer<T>.Default;
			for( var i = 0; i < length; ++i )
			{
				if( comp.Equals( buffer[i], item ) )
				{
					removeAt( i );
					return;
				}
			}
		}


		/// <summary>
		/// removes the item at the given index from the list
		/// </summary>
		public void removeAt( int index )
		{
			if( index < length )
			{
				--length;
				buffer[index] = default( T );
				for( var b = index; b < length; ++b )
					buffer[b] = buffer[b + 1];
			}
		}

	}
}

