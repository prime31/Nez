using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Vector2Ext
	{
		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static Vector2 round( this Vector2 vec )
		{
			return new Vector2( Mathf.round( vec.X ), Mathf.round( vec.Y ) );
		}


		/// <summary>
		/// rounds the x and y values in place
		/// </summary>
		/// <param name="vec">Vec.</param>
		public static void round( ref Vector2 vec )
		{
			vec.X = Mathf.round( vec.X );
			vec.Y = Mathf.round( vec.Y );
		}


		/// <summary>
		/// returns a 0.5, 0.5 vector
		/// </summary>
		/// <returns>The vector.</returns>
		public static Vector2 halfVector()
		{
			return new Vector2( 0.5f, 0.5f );
		}


		/// <summary>
		/// compute the 2d pseudo cross product Dot( Perp( u ), v )
		/// </summary>
		/// <param name="u">U.</param>
		/// <param name="v">V.</param>
		public static float cross( Vector2 u, Vector2 v )
		{
			return u.Y * v.X - u.X * v.Y;
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <returns>Transformed <see cref="Vector2"/>.</returns>
		public static Vector2 Transform( Vector2 position, Matrix2D matrix )
		{
			return new Vector2( ( position.X * matrix.M11 ) + ( position.Y * matrix.M21 ) + matrix.M31, ( position.X * matrix.M12 ) + ( position.Y * matrix.M22 ) + matrix.M32 );
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="result">Transformed <see cref="Vector2"/> as an output parameter.</param>
		public static void Transform( ref Vector2 position, ref Matrix2D matrix, out Vector2 result )
		{
			var x = ( position.X * matrix.M11 ) + ( position.Y * matrix.M21 ) + matrix.M31;
			var y = ( position.X * matrix.M12 ) + ( position.Y * matrix.M22 ) + matrix.M32;
			result.X = x;
			result.Y = y;
		}


		/// <summary>
		/// Apply transformation on vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="sourceIndex">The starting index of transformation in the source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		/// <param name="destinationIndex">The starting index in the destination array, where the first <see cref="Vector2"/> should be written.</param>
		/// <param name="length">The number of vectors to be transformed.</param>
		public static void Transform( Vector2[] sourceArray, int sourceIndex, ref Matrix2D matrix, Vector2[] destinationArray, int destinationIndex, int length )
		{
			for( var i = 0; i < length; i++ )
			{
				var position = sourceArray[sourceIndex + i];
				var destination = destinationArray[destinationIndex + i];
				destination.X = ( position.X * matrix.M11 ) + ( position.Y * matrix.M21 ) + matrix.M31;
				destination.Y = ( position.X * matrix.M12 ) + ( position.Y * matrix.M22 ) + matrix.M32;
				destinationArray[destinationIndex + i] = destination;
			}
		}


		/// <summary>
		/// Apply transformation on all vectors within array of <see cref="Vector2"/> by the specified <see cref="Matrix"/> and places the results in an another array.
		/// </summary>
		/// <param name="sourceArray">Source array.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="destinationArray">Destination array.</param>
		public static void Transform( Vector2[] sourceArray, ref Matrix2D matrix, Vector2[] destinationArray )
		{
			Transform( sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length );
		}

	}
}

