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
	}
}

