using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class Vector2Ext
	{
		/// <summary>
		/// temporary workaround to Vector2.Normalize screwing up the 0,0 vector
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void normalize( ref Vector2 vec )
		{
			var magnitude = Mathf.sqrt( ( vec.X * vec.X ) + ( vec.Y * vec.Y ) );
			if( magnitude > Mathf.epsilon )
				vec /= magnitude;
			else
				vec.X = vec.Y = 0;
		}


		/// <summary>
		/// temporary workaround to Vector2.Normalize screwing up the 0,0 vector
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 normalize( Vector2 vec )
		{
			var magnitude = Mathf.sqrt( ( vec.X * vec.X ) + ( vec.Y * vec.Y ) );
			if( magnitude > Mathf.epsilon )
				vec /= magnitude;
			else
				vec.X = vec.Y = 0;

			return vec;
		}


		/// <summary>
		/// rounds the x and y values
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 round( this Vector2 vec )
		{
			return new Vector2( Mathf.round( vec.X ), Mathf.round( vec.Y ) );
		}


		/// <summary>
		/// rounds the x and y values in place
		/// </summary>
		/// <param name="vec">Vec.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void round( ref Vector2 vec )
		{
			vec.X = Mathf.round( vec.X );
			vec.Y = Mathf.round( vec.Y );
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public void floor( ref Vector2 val )
		{
			val.X = (int)val.X;
			val.Y = (int)val.Y;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		static public Vector2 floor( Vector2 val )
		{
			return new Vector2( (int)val.X, (int)val.Y );
		}


		/// <summary>
		/// returns a 0.5, 0.5 vector
		/// </summary>
		/// <returns>The vector.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 halfVector()
		{
			return new Vector2( 0.5f, 0.5f );
		}


		/// <summary>
		/// compute the 2d pseudo cross product Dot( Perp( u ), v )
		/// </summary>
		/// <param name="u">U.</param>
		/// <param name="v">V.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float cross( Vector2 u, Vector2 v )
		{
			return u.Y * v.X - u.X * v.Y;
		}


		/// <summary>
		/// returns the vector perpendicular to the passed in vectors
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 perpendicular( ref Vector2 first, ref Vector2 second )
		{
			return new Vector2( -1f * ( second.Y - first.Y ), second.X - first.X );
		}


		/// <summary>
		/// returns the vector perpendicular to the passed in vectors
		/// </summary>
		/// <param name="first">First.</param>
		/// <param name="second">Second.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 perpendicular( Vector2 first, Vector2 second )
		{
			return new Vector2( -1f * ( second.Y - first.Y ), second.X - first.X );
		}


		/// <summary>
		/// flips the x/y values and inverts the y to get the perpendicular
		/// </summary>
		/// <param name="original">Original.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 perpendicular( Vector2 original )
		{
			return new Vector2( -original.Y, original.X );
		}


		/// <summary>
		/// returns the angle between the two vectors in degrees
		/// </summary>
		/// <param name="from">From.</param>
		/// <param name="to">To.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float angle( Vector2 from, Vector2 to )
		{
			normalize( ref from );
			normalize( ref to );
			return Mathf.acos( Mathf.clamp( Vector2.Dot( from, to ), -1f, 1f ) ) * Mathf.rad2Deg;
		}


		/// <summary>
		/// returns the angle between left and right with self being the center point in degrees
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="left">V left.</param>
		/// <param name="right">V right.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static float angleBetween( this Vector2 self, Vector2 left, Vector2 right )
		{
			var one = left - self;
			var two = right - self;
			return angle( one, two );
		}


		/// <summary>
		/// given two lines (ab and cd) finds the intersection point
		/// </summary>
		/// <returns>The ray intersection.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="c">C.</param>
		/// <param name="d">D.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool getRayIntersection( Vector2 a, Vector2 b, Vector2 c, Vector2 d, out Vector2 intersection )
		{
			var dy1 = b.Y - a.Y;
			var dx1 = b.X - a.X;
			var dy2 = d.Y - c.Y;
			var dx2 = d.X - c.X;

			if( dy1 * dx2 == dy2 * dx1 )
			{
				intersection = new Vector2( float.NaN, float.NaN );
				return false;
			}

			var x = ( ( c.Y - a.Y ) * dx1 * dx2 + dy1 * dx2 * a.X - dy2 * dx1 * c.X ) / ( dy1 * dx2 - dy2 * dx1 );
			var y = a.Y + ( dy1 / dx1 ) * ( x - a.X );

			intersection = new Vector2( x, y );
			return true;
		}

		/// <summary>
		/// rounds and converts a Vector2 to a Point
		/// </summary>
		/// <returns>The point.</returns>
		/// <param name="vec">Vec.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Point roundToPoint( this Vector2 vec )
		{
			var roundedVec = vec.round();
			return new Point((int) roundedVec.X, (int) roundedVec.Y);
		}


		/// <summary>
		/// converts a Vector2 to a Vector3 with a 0 z-position
		/// </summary>
		/// <returns>The vector3.</returns>
		/// <param name="vec">Vec.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector3 toVector3( this Vector2 vec )
		{
			return new Vector3( vec, 0 );
		}


		/// <summary>
		/// checks if a triangle is CCW or CW
		/// </summary>
		/// <returns><c>true</c>, if triangle ccw was ised, <c>false</c> otherwise.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="center">Center.</param>
		/// <param name="c">C.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static bool isTriangleCCW( Vector2 a, Vector2 center, Vector2 c )
		{
			return cross( center - a, c - center ) < 0;
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <returns>Transformed <see cref="Vector2"/>.</returns>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 transform( Vector2 position, Matrix2D matrix )
		{
			return new Vector2( ( position.X * matrix.M11 ) + ( position.Y * matrix.M21 ) + matrix.M31, ( position.X * matrix.M12 ) + ( position.Y * matrix.M22 ) + matrix.M32 );
		}


		/// <summary>
		/// Creates a new <see cref="Vector2"/> that contains a transformation of 2d-vector by the specified <see cref="Matrix"/>.
		/// </summary>
		/// <param name="position">Source <see cref="Vector2"/>.</param>
		/// <param name="matrix">The transformation <see cref="Matrix"/>.</param>
		/// <param name="result">Transformed <see cref="Vector2"/> as an output parameter.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void transform( ref Vector2 position, ref Matrix2D matrix, out Vector2 result )
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void transform( Vector2[] sourceArray, int sourceIndex, ref Matrix2D matrix, Vector2[] destinationArray, int destinationIndex, int length )
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static void transform( Vector2[] sourceArray, ref Matrix2D matrix, Vector2[] destinationArray )
		{
			transform( sourceArray, 0, ref matrix, destinationArray, 0, sourceArray.Length );
		}

	}
}

