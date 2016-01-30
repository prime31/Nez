using System;


namespace Nez
{
	public static class ArrayExt
	{
		public static bool contains( this int[] source, int value )
		{
			for( var i = 0; i < source.Length; i++ )
			{
				if( source[i] == value )
					return true;
			}
			return false;
		}
	}
}

