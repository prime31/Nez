using System;
using System.Text;


namespace Nez
{
	/// <summary>
	/// utility methods that don't yet have a proper home that makes sense
	/// </summary>
	public static class Utils
	{
		public static string randomString( int size = 38 )
		{
			var builder = new StringBuilder();

			char ch;
			for( int i = 0; i < size; i++ )
			{
				ch = Convert.ToChar( Convert.ToInt32( Math.Floor( 26 * Random.nextFloat() + 65 ) ) );
				builder.Append( ch );
			}

			return builder.ToString();
		}


		/// <summary>
		/// swaps the two objects
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void swap<T>( T first, T second )
		{
			T temp = first;
			first = second;
			second = temp;
		}


		/// <summary>
		/// swaps the two value types
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static void swap<T>( ref T first, ref T second )
		{
			T temp = first;
			first = second;
			second = temp;
		}


		/// <summary>
		/// randomly returns one of the given values
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T choose<T>( T first, T second )
		{
			if( Random.nextFloat( 1 ) == 1 )
				return first;
			return second;
		}


		/// <summary>
		/// randomly returns one of the given values
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="third">Third.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T choose<T>( T first, T second, T third )
		{
			switch( Random.nextInt( 3 ) )
			{
				case 0:
					return first;
				case 1:
					return second;
				default:
					return third;
			}
		}


		/// <summary>
		/// randomly returns one of the given values
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		/// <param name="third">Third.</param>
		/// <param name="fourth">Fourth.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public static T choose<T>( T first, T second, T third, T fourth )
		{
			switch( Random.nextInt( 4 ) )
			{
				case 0:
					return first;
				case 1:
					return second;
				case 2:
					return third;
				default:
				return fourth;
			}
		}

	}
}

