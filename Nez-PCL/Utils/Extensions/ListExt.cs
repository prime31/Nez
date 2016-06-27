using System;
using System.Collections.Generic;


namespace Nez
{
	public static class ListExt
	{
		public static void shuffle<T>( this IList<T> list )
		{
			var n = list.Count;
			while( n > 1 )
			{
				n--;
				int k = Nez.Random.range( 0, n + 1 );
				T value = list[k];
				list[k] = list[n];
				list[n] = value;
			}  
		}


		/// <summary>
		/// returns false if the item is already in the List and true if it was successfully added.
		/// </summary>
		/// <returns>The if not present.</returns>
		/// <param name="list">List.</param>
		/// <param name="item">Item.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static bool addIfNotPresent<T>( this IList<T> list, T item )
		{
			if( list.Contains( item ) )
				return false;

			list.Add( item );
			return true;
		}
	}
}

