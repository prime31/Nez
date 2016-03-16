using System;
using Microsoft.Xna.Framework.Input;


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


		public static bool contains( this string[] source, string value )
		{
			for( var i = 0; i < source.Length; i++ )
			{
				if( source[i] == value )
					return true;
			}
			return false;
		}


		public static bool contains( this Keys[] source, Keys value )
		{
			for( var i = 0; i < source.Length; i++ )
			{
				if( source[i] == value )
					return true;
			}
			return false;
		}


		public static bool contains<T>( this T[] source, T value )
		{
			for( var i = 0; i < source.Length; i++ )
			{
				if( source[i].Equals( value ) )
					return true;
			}
			return false;
		}
	}
}

