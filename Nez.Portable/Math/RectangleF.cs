using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Describes a 2D-rectangle. 
	/// </summary>
	[DebuggerDisplay( "{DebugDisplayString,nq}" )]
	public struct RectangleF : IEquatable<RectangleF>
	{
		static RectangleF emptyRectangle = new RectangleF();

		/// <summary>
		/// The x coordinate of the top-left corner of this <see cref="RectangleF"/>.
		/// </summary>
		public float x;

		/// <summary>
		/// The y coordinate of the top-left corner of this <see cref="RectangleF"/>.
		/// </summary>
		public float y;

		/// <summary>
		/// The width of this <see cref="RectangleF"/>.
		/// </summary>
		public float width;

		/// <summary>
		/// The height of this <see cref="RectangleF"/>.
		/// </summary>
		public float height;


		#region Public Properties

		/// <summary>
		/// Returns a <see cref="RectangleF"/> with X=0, Y=0, Width=0, Height=0.
		/// </summary>
		public static RectangleF empty
		{
			get { return emptyRectangle; }
		}

		/// <summary>
		/// returns a RectangleF of float.Min/Max values
		/// </summary>
		/// <value>The max rect.</value>
		public static RectangleF maxRect { get { return new RectangleF( float.MinValue / 2, float.MinValue / 2, float.MaxValue, float.MaxValue ); } }

		/// <summary>
		/// Returns the x coordinate of the left edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float left
		{
			get { return this.x; }
		}

		/// <summary>
		/// Returns the x coordinate of the right edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float right
		{
			get { return ( this.x + this.width ); }
		}

		/// <summary>
		/// Returns the y coordinate of the top edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float top
		{
			get { return this.y; }
		}

		/// <summary>
		/// Returns the y coordinate of the bottom edge of this <see cref="RectangleF"/>.
		/// </summary>
		public float bottom
		{
			get { return ( this.y + this.height ); }
		}

		/// <summary>
		/// gets the max point of the rectangle, the bottom-right corner
		/// </summary>
		/// <value>The max.</value>
		public Vector2 max { get { return new Vector2( right, bottom ); } }

		/// <summary>
		/// Whether or not this <see cref="RectangleF"/> has a <see cref="Width"/> and
		/// <see cref="Height"/> of 0, and a <see cref="Location"/> of (0, 0).
		/// </summary>
		public bool isEmpty
		{
			get
			{
				return ( ( ( ( this.width == 0 ) && ( this.height == 0 ) ) && ( this.x == 0 ) ) && ( this.y == 0 ) );
			}
		}

		/// <summary>
		/// The top-left coordinates of this <see cref="RectangleF"/>.
		/// </summary>
		public Vector2 location
		{
			get
			{
				return new Vector2( this.x, this.y );
			}
			set
			{
				x = value.X;
				y = value.Y;
			}
		}

		/// <summary>
		/// The width-height coordinates of this <see cref="RectangleF"/>.
		/// </summary>
		public Vector2 size
		{
			get
			{
				return new Vector2( this.width, this.height );
			}
			set
			{
				width = value.X;
				height = value.Y;
			}
		}

		/// <summary>
		/// A <see cref="Point"/> located in the center of this <see cref="RectangleF"/>.
		/// </summary>
		/// <remarks>
		/// If <see cref="Width"/> or <see cref="Height"/> is an odd number,
		/// the center point will be rounded down.
		/// </remarks>
		public Vector2 center
		{
			get { return new Vector2( this.x + ( this.width / 2 ), this.y + ( this.height / 2 ) ); }
		}

		#endregion

		// temp Matrixes used for bounds calculation
		static Matrix2D _tempMat, _transformMat;


		internal string DebugDisplayString
		{
			get
			{
				return string.Concat(
					this.x, "  ",
					this.y, "  ",
					this.width, "  ",
					this.height
				);
			}
		}


		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
		/// position, width, and height.
		/// </summary>
		/// <param name="x">The x coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="y">The y coordinate of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="width">The width of the created <see cref="RectangleF"/>.</param>
		/// <param name="height">The height of the created <see cref="RectangleF"/>.</param>
		public RectangleF( float x, float y, float width, float height )
		{
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
		}


		/// <summary>
		/// Creates a new instance of <see cref="RectangleF"/> struct, with the specified
		/// location and size.
		/// </summary>
		/// <param name="location">The x and y coordinates of the top-left corner of the created <see cref="RectangleF"/>.</param>
		/// <param name="size">The width and height of the created <see cref="RectangleF"/>.</param>
		public RectangleF( Vector2 location, Vector2 size )
		{
			this.x = location.X;
			this.y = location.Y;
			this.width = size.X;
			this.height = size.Y;
		}


		/// <summary>
		/// creates a RectangleF given min/max points (top-left, bottom-right points)
		/// </summary>
		/// <returns>The minimum max points.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static RectangleF fromMinMax( Vector2 min, Vector2 max )
		{
			return new RectangleF( min.X, min.Y, max.X - min.X, max.Y - min.Y );
		}


		/// <summary>
		/// creates a RectangleF given min/max points (top-left, bottom-right points)
		/// </summary>
		/// <returns>The minimum max points.</returns>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		public static RectangleF fromMinMax( float minX, float minY, float maxX, float maxY )
		{
			return new RectangleF( minX, minY, maxX - minX, maxY - minY );
		}


		/// <summary>
		/// given the points of a polygon calculates the bounds
		/// </summary>
		/// <returns>The from polygon points.</returns>
		/// <param name="points">Points.</param>
		public static RectangleF rectEncompassingPoints( Vector2[] points )
		{
			// we need to find the min/max x/y values
			var minX = float.PositiveInfinity;
			var minY = float.PositiveInfinity;
			var maxX = float.NegativeInfinity;
			var maxY = float.NegativeInfinity;

			for( var i = 0; i < points.Length; i++ )
			{
				var pt = points[i];

				if( pt.X < minX )
					minX = pt.X;
				if( pt.X > maxX )
					maxX = pt.X;

				if( pt.Y < minY )
					minY = pt.Y;
				if( pt.Y > maxY )
					maxY = pt.Y;
			}

			return RectangleF.fromMinMax( minX, minY, maxX, maxY );
		}


		#region Public Methods

		/// <summary>
		/// gets the position of the specified edge
		/// </summary>
		/// <returns>The side.</returns>
		/// <param name="edge">Side.</param>
		public float getSide( Edge edge )
		{
			switch( edge )
			{
				case Edge.Top:
					return top;
				case Edge.Bottom:
					return bottom;
				case Edge.Left:
					return left;
				case Edge.Right:
					return right;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool contains( int x, int y )
		{
			return ( ( ( ( this.x <= x ) && ( x < ( this.x + this.width ) ) ) && ( this.y <= y ) ) && ( y < ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided coordinates lie within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="x">The x coordinate of the point to check for containment.</param>
		/// <param name="y">The y coordinate of the point to check for containment.</param>
		/// <returns><c>true</c> if the provided coordinates lie inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool contains( float x, float y )
		{
			return ( ( ( ( this.x <= x ) && ( x < ( this.x + this.width ) ) ) && ( this.y <= y ) ) && ( y < ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool contains( Point value )
		{
			return ( ( ( ( this.x <= value.X ) && ( value.X < ( this.x + this.width ) ) ) && ( this.y <= value.Y ) ) && ( value.Y < ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Point"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Point"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void contains( ref Point value, out bool result )
		{
			result = ( ( ( ( this.x <= value.X ) && ( value.X < ( this.x + this.width ) ) ) && ( this.y <= value.Y ) ) && ( value.Y < ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool contains( Vector2 value )
		{
			return ( ( ( ( this.x <= value.X ) && ( value.X < ( this.x + this.width ) ) ) && ( this.y <= value.Y ) ) && ( value.Y < ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="Vector2"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The coordinates to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="Vector2"/> lies inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void contains( ref Vector2 value, out bool result )
		{
			result = ( ( ( ( this.x <= value.X ) && ( value.X < ( this.x + this.width ) ) ) && ( this.y <= value.Y ) ) && ( value.Y < ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <returns><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise.</returns>
		public bool contains( RectangleF value )
		{
			return ( ( ( ( this.x <= value.x ) && ( ( value.x + value.width ) <= ( this.x + this.width ) ) ) && ( this.y <= value.y ) ) && ( ( value.y + value.height ) <= ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Gets whether or not the provided <see cref="RectangleF"/> lies within the bounds of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="value">The <see cref="RectangleF"/> to check for inclusion in this <see cref="RectangleF"/>.</param>
		/// <param name="result"><c>true</c> if the provided <see cref="RectangleF"/>'s bounds lie entirely inside this <see cref="RectangleF"/>; <c>false</c> otherwise. As an output parameter.</param>
		public void contains( ref RectangleF value, out bool result )
		{
			result = ( ( ( ( this.x <= value.x ) && ( ( value.x + value.width ) <= ( this.x + this.width ) ) ) && ( this.y <= value.y ) ) && ( ( value.y + value.height ) <= ( this.y + this.height ) ) );
		}


		/// <summary>
		/// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts. 
		/// </summary>
		/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
		/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
		public void inflate( int horizontalAmount, int verticalAmount )
		{
			x -= horizontalAmount;
			y -= verticalAmount;
			width += horizontalAmount * 2;
			height += verticalAmount * 2;
		}


		/// <summary>
		/// Adjusts the edges of this <see cref="RectangleF"/> by specified horizontal and vertical amounts. 
		/// </summary>
		/// <param name="horizontalAmount">Value to adjust the left and right edges.</param>
		/// <param name="verticalAmount">Value to adjust the top and bottom edges.</param>
		public void inflate( float horizontalAmount, float verticalAmount )
		{
			x -= horizontalAmount;
			y -= verticalAmount;
			width += horizontalAmount * 2;
			height += verticalAmount * 2;
		}


		/// <summary>
		/// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <returns><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise.</returns>
		public bool intersects( RectangleF value )
		{
			return value.left < right &&
			left < value.right &&
			value.top < bottom &&
			top < value.bottom;
		}


		/// <summary>
		/// Gets whether or not the other <see cref="RectangleF"/> intersects with this rectangle.
		/// </summary>
		/// <param name="value">The other rectangle for testing.</param>
		/// <param name="result"><c>true</c> if other <see cref="RectangleF"/> intersects with this rectangle; <c>false</c> otherwise. As an output parameter.</param>
		public void intersects( ref RectangleF value, out bool result )
		{
			result = value.left < right &&
			left < value.right &&
			value.top < bottom &&
			top < value.bottom;
		}


		/// <summary>
		/// returns true if other intersects rect
		/// </summary>
		/// <param name="other">other.</param>
		public bool intersects( ref RectangleF other )
		{
			bool result;
			intersects( ref other, out result );
			return result;
		}


		public bool rayIntersects( ref Ray2D ray, out float distance )
		{
			distance = 0f;
			var maxValue = float.MaxValue;

			if( Math.Abs( ray.direction.X ) < 1E-06f )
			{
				if( ( ray.start.X < x ) || ( ray.start.X > x + width ) )
					return false;
			}
			else
			{
				var num11 = 1f / ray.direction.X;
				var num8 = ( x - ray.start.X ) * num11;
				var num7 = ( x + width - ray.start.X ) * num11;
				if( num8 > num7 )
				{
					var num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				distance = MathHelper.Max( num8, distance );
				maxValue = MathHelper.Min( num7, maxValue );
				if( distance > maxValue )
					return false;
			}

			if( Math.Abs( ray.direction.Y ) < 1E-06f )
			{
				if( ( ray.start.Y < y ) || ( ray.start.Y > y + height ) )
				{
					return false;
				}
			}
			else
			{
				var num10 = 1f / ray.direction.Y;
				var num6 = ( y - ray.start.Y ) * num10;
				var num5 = ( y + height - ray.start.Y ) * num10;
				if( num6 > num5 )
				{
					var num13 = num6;
					num6 = num5;
					num5 = num13;
				}

				distance = MathHelper.Max( num6, distance );
				maxValue = MathHelper.Min( num5, maxValue );
				if( distance > maxValue )
					return false;
			}

			return true;
		}


		public float? rayIntersects( Ray ray )
		{
			var num = 0f;
			var maxValue = float.MaxValue;

			if( Math.Abs( ray.Direction.X ) < 1E-06f )
			{
				if( ( ray.Position.X < left ) || ( ray.Position.X > right ) )
					return null;
			}
			else
			{
				float num11 = 1f / ray.Direction.X;
				float num8 = ( left - ray.Position.X ) * num11;
				float num7 = ( right - ray.Position.X ) * num11;
				if( num8 > num7 )
				{
					float num14 = num8;
					num8 = num7;
					num7 = num14;
				}

				num = MathHelper.Max( num8, num );
				maxValue = MathHelper.Min( num7, maxValue );
				if( num > maxValue )
					return null;
			}

			if( Math.Abs( ray.Direction.Y ) < 1E-06f )
			{
				if( ( ray.Position.Y < top ) || ( ray.Position.Y > bottom ) )
					return null;
			}
			else
			{
				float num10 = 1f / ray.Direction.Y;
				float num6 = ( top - ray.Position.Y ) * num10;
				float num5 = ( bottom - ray.Position.Y ) * num10;
				if( num6 > num5 )
				{
					float num13 = num6;
					num6 = num5;
					num5 = num13;
				}

				num = MathHelper.Max( num6, num );
				maxValue = MathHelper.Min( num5, maxValue );
				if( num > maxValue )
					return null;
			}

			return new float?( num );
		}


		public Vector2 getClosestPointOnBoundsToOrigin()
		{
			var max = this.max;
			var minDist = Math.Abs( location.X );
			var boundsPoint = new Vector2( location.X, 0 );

			if( Math.Abs( max.X ) < minDist )
			{
				minDist = Math.Abs( max.X );
				boundsPoint.X = max.X;
				boundsPoint.Y = 0f;
			}

			if( Math.Abs( max.Y ) < minDist )
			{
				minDist = Math.Abs( max.Y );
				boundsPoint.X = 0f;
				boundsPoint.Y = max.Y;
			}

			if( Math.Abs( location.Y ) < minDist )
			{
				minDist = Math.Abs( location.Y );
				boundsPoint.X = 0;
				boundsPoint.Y = location.Y;
			}

			return boundsPoint;
		}


		/// <summary>
		/// returns the closest point that is in or on the RectangleF to the given point
		/// </summary>
		/// <returns>The closest point on rectangle to point.</returns>
		/// <param name="point">Point.</param>
		public Vector2 getClosestPointOnRectangleFToPoint( Vector2 point )
		{
			// for each axis, if the point is outside the box clamp it to the box else leave it alone
			var res = new Vector2();
			res.X = MathHelper.Clamp( point.X, left, right );
			res.Y = MathHelper.Clamp( point.Y, top, bottom );

			return res;
		}


		/// <summary>
		/// gets the closest point that is on the rectangle border to the given point
		/// </summary>
		/// <returns>The closest point on rectangle border to point.</returns>
		/// <param name="point">Point.</param>
		public Vector2 getClosestPointOnRectangleBorderToPoint( Vector2 point, out Vector2 edgeNormal )
		{
			edgeNormal = Vector2.Zero;

			// for each axis, if the point is outside the box clamp it to the box else leave it alone
			var res = new Vector2();
			res.X = MathHelper.Clamp( point.X, left, right );
			res.Y = MathHelper.Clamp( point.Y, top, bottom );

			// if point is inside the rectangle we need to push res to the border since it will be inside the rect
			if( contains( res ) )
			{
				var dl = res.X - left;
				var dr = right - res.X;
				var dt = res.Y - top;
				var db = bottom - res.Y;

				var min = Mathf.minOf( dl, dr, dt, db );
				if( min == dt )
				{
					res.Y = top;
					edgeNormal.Y = -1;
				}
				else if( min == db )
				{
					res.Y = bottom;
					edgeNormal.Y = 1;
				}
				else if( min == dl )
				{
					res.X = left;
					edgeNormal.X = -1;
				}
				else
				{
					res.X = right;
					edgeNormal.X = 1;
				}
			}
			else
			{
				if( res.X == left )
					edgeNormal.X = -1;
				if( res.X == right )
					edgeNormal.X = 1;
				if( res.Y == top )
					edgeNormal.Y = -1;
				if( res.Y == bottom )
					edgeNormal.Y = 1;
			}

			return res;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>Overlapping region of the two rectangles.</returns>
		public static RectangleF intersect( RectangleF value1, RectangleF value2 )
		{
			RectangleF rectangle;
			intersect( ref value1, ref value2, out rectangle );
			return rectangle;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that contains overlapping region of two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">Overlapping region of the two rectangles as an output parameter.</param>
		public static void intersect( ref RectangleF value1, ref RectangleF value2, out RectangleF result )
		{
			if( value1.intersects( value2 ) )
			{
				var right_side = Math.Min( value1.x + value1.width, value2.x + value2.width );
				var left_side = Math.Max( value1.x, value2.x );
				var top_side = Math.Max( value1.y, value2.y );
				var bottom_side = Math.Min( value1.y + value1.height, value2.y + value2.height );
				result = new RectangleF( left_side, top_side, right_side - left_side, bottom_side - top_side );
			}
			else
			{
				result = new RectangleF( 0, 0, 0, 0 );
			}
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
		public void offset( int offsetX, int offsetY )
		{
			x += offsetX;
			y += offsetY;
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="offsetX">The x coordinate to add to this <see cref="RectangleF"/>.</param>
		/// <param name="offsetY">The y coordinate to add to this <see cref="RectangleF"/>.</param>
		public void offset( float offsetX, float offsetY )
		{
			x += offsetX;
			y += offsetY;
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
		public void offset( Point amount )
		{
			x += amount.X;
			y += amount.Y;
		}


		/// <summary>
		/// Changes the <see cref="Location"/> of this <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="amount">The x and y components to add to this <see cref="RectangleF"/>.</param>
		public void offset( Vector2 amount )
		{
			x += amount.X;
			y += amount.Y;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>The union of the two rectangles.</returns>
		public static RectangleF union( RectangleF value1, RectangleF value2 )
		{
			var x = Math.Min( value1.x, value2.x );
			var y = Math.Min( value1.y, value2.y );
			return new RectangleF( x, y,
				Math.Max( value1.right, value2.right ) - x,
				Math.Max( value1.bottom, value2.bottom ) - y );
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> that completely contains two other rectangles.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">The union of the two rectangles as an output parameter.</param>
		public static void union( ref RectangleF value1, ref RectangleF value2, out RectangleF result )
		{
			result.x = Math.Min( value1.x, value2.x );
			result.y = Math.Min( value1.y, value2.y );
			result.width = Math.Max( value1.right, value2.right ) - result.x;
			result.height = Math.Max( value1.bottom, value2.bottom ) - result.y;
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> where the rectangles overlap.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <returns>The overlap of the two rectangles.</returns>
		public static RectangleF overlap( RectangleF value1, RectangleF value2 )
		{
			var x = Math.Max( Math.Max( value1.x, value2.x ), 0 );
			var y = Math.Max( Math.Max( value1.y, value2.y ), 0 );
			return new RectangleF( x, y,
				Math.Max( Math.Min( value1.right, value2.right ) - x, 0 ),
				Math.Max( Math.Min( value1.bottom, value2.bottom ) - y, 0 ) );
		}


		/// <summary>
		/// Creates a new <see cref="RectangleF"/> where the rectangles overlap.
		/// </summary>
		/// <param name="value1">The first <see cref="RectangleF"/>.</param>
		/// <param name="value2">The second <see cref="RectangleF"/>.</param>
		/// <param name="result">The overlap of the two rectangles as an output parameter.</param>
		public static void overlap( ref RectangleF value1, ref RectangleF value2, out RectangleF result )
		{
			result.x = Math.Max( Math.Max( value1.x, value2.x ), 0 );
			result.y = Math.Max( Math.Max( value1.y, value2.y ), 0 );
			result.width = Math.Max( Math.Min( value1.right, value2.right ) - result.x, 0 );
			result.height = Math.Max( Math.Min( value1.bottom, value2.bottom ) - result.y, 0 );
		}


		public void calculateBounds( Vector2 parentPosition, Vector2 position, Vector2 origin, Vector2 scale, float rotation, float width, float height )
		{
			if( rotation == 0f )
			{
				x = parentPosition.X + position.X - origin.X * scale.X;
				y = parentPosition.Y + position.Y - origin.Y * scale.Y;
				this.width = width * scale.X;
				this.height = height * scale.Y;
			}
			else
			{
				// special care for rotated bounds. we need to find our absolute min/max values and create the bounds from that
				var worldPosX = parentPosition.X + position.X;
				var worldPosY = parentPosition.Y + position.Y;

				// set the reference point to world reference taking origin into account
				Matrix2D.createTranslation( -worldPosX - origin.X, -worldPosY - origin.Y, out _transformMat );
				Matrix2D.createScale( scale.X, scale.Y, out _tempMat ); // scale ->
				Matrix2D.multiply( ref _transformMat, ref _tempMat, out _transformMat );
				Matrix2D.createRotation( rotation, out _tempMat ); // rotate ->
				Matrix2D.multiply( ref _transformMat, ref _tempMat, out _transformMat );
				Matrix2D.createTranslation( worldPosX, worldPosY, out _tempMat ); // translate back
				Matrix2D.multiply( ref _transformMat, ref _tempMat, out _transformMat );

				// TODO: this is a bit silly. we can just leave the worldPos translation in the Matrix and avoid this
				// get all four corners in world space
				var topLeft = new Vector2( worldPosX, worldPosY );
				var topRight = new Vector2( worldPosX + width, worldPosY );
				var bottomLeft = new Vector2( worldPosX, worldPosY + height );
				var bottomRight = new Vector2( worldPosX + width, worldPosY + height );

				// transform the corners into our work space
				Vector2Ext.transform( ref topLeft, ref _transformMat, out topLeft );
				Vector2Ext.transform( ref topRight, ref _transformMat, out topRight );
				Vector2Ext.transform( ref bottomLeft, ref _transformMat, out bottomLeft );
				Vector2Ext.transform( ref bottomRight, ref _transformMat, out bottomRight );

				// find the min and max values so we can concoct our bounding box
				var minX = Mathf.minOf( topLeft.X, bottomRight.X, topRight.X, bottomLeft.X );
				var maxX = Mathf.maxOf( topLeft.X, bottomRight.X, topRight.X, bottomLeft.X );
				var minY = Mathf.minOf( topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y );
				var maxY = Mathf.maxOf( topLeft.Y, bottomRight.Y, topRight.Y, bottomLeft.Y );

				location = new Vector2( minX, minY );
				this.width = maxX - minX;
				this.height = maxY - minY;
			}
		}


		/// <summary>
		/// returns a RectangleF that spans the current rect and the provided delta positions
		/// </summary>
		/// <returns>The swept broadphase box.</returns>
		/// <param name="velocityX">Velocity x.</param>
		/// <param name="velocityY">Velocity y.</param>
		public RectangleF getSweptBroadphaseBounds( float deltaX, float deltaY )
		{
			var broadphasebox = RectangleF.empty;

			broadphasebox.x = deltaX > 0 ? x : x + deltaX;
			broadphasebox.y = deltaY > 0 ? y : y + deltaY;
			broadphasebox.width = deltaX > 0 ? deltaX + width : width - deltaX;
			broadphasebox.height = deltaY > 0 ? deltaY + height : height - deltaY;

			return broadphasebox;
		}


		/// <summary>
		/// returns true if the boxes are colliding
		/// moveX and moveY will return the movement that b1 must move to avoid the collision
		/// </summary>
		/// <param name="other">Other.</param>
		/// <param name="moveX">Move x.</param>
		/// <param name="moveY">Move y.</param>
		public bool collisionCheck( ref RectangleF other, out float moveX, out float moveY )
		{
			moveX = moveY = 0.0f;

			var l = other.x - ( x + width );
			var r = ( other.x + other.width ) - x;
			var t = other.y - ( y + height );
			var b = ( other.y + other.height ) - y;

			// check that there was a collision
			if( l > 0 || r < 0 || t > 0 || b < 0 )
				return false;

			// find the offset of both sides
			moveX = Math.Abs( l ) < r ? l : r;
			moveY = Math.Abs( t ) < b ? t : b;

			// only use whichever offset is the smallest
			if( Math.Abs( moveX ) < Math.Abs( moveY ) )
				moveY = 0.0f;
			else
				moveX = 0.0f;

			return true;
		}


		/// <summary>
		/// Calculates the signed depth of intersection between two rectangles.
		/// </summary>
		/// <returns>
		/// The amount of overlap between two intersecting rectangles. These depth values can be negative depending on which sides the rectangles
		/// intersect. This allows callers to determine the correct direction to push objects in order to resolve collisions.
		/// If the rectangles are not intersecting, Vector2.Zero is returned.
		/// </returns>
		public static Vector2 getIntersectionDepth( ref RectangleF rectA, ref RectangleF rectB )
		{
			// calculate half sizes
			var halfWidthA = rectA.width / 2.0f;
			var halfHeightA = rectA.height / 2.0f;
			var halfWidthB = rectB.width / 2.0f;
			var halfHeightB = rectB.height / 2.0f;

			// calculate centers
			var centerA = new Vector2( rectA.left + halfWidthA, rectA.top + halfHeightA );
			var centerB = new Vector2( rectB.left + halfWidthB, rectB.top + halfHeightB );

			// calculate current and minimum-non-intersecting distances between centers
			var distanceX = centerA.X - centerB.X;
			var distanceY = centerA.Y - centerB.Y;
			var minDistanceX = halfWidthA + halfWidthB;
			var minDistanceY = halfHeightA + halfHeightB;

			// if we are not intersecting at all, return (0, 0)
			if( Math.Abs( distanceX ) >= minDistanceX || Math.Abs( distanceY ) >= minDistanceY )
				return Vector2.Zero;

			// calculate and return intersection depths
			var depthX = distanceX > 0 ? minDistanceX - distanceX : -minDistanceX - distanceX;
			var depthY = distanceY > 0 ? minDistanceY - distanceY : -minDistanceY - distanceY;

			return new Vector2( depthX, depthY );
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="Object"/>.
		/// </summary>
		/// <param name="obj">The <see cref="Object"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public override bool Equals( object obj )
		{
			return ( obj is RectangleF ) && this == ( (RectangleF)obj );
		}


		/// <summary>
		/// Compares whether current instance is equal to specified <see cref="RectangleF"/>.
		/// </summary>
		/// <param name="other">The <see cref="RectangleF"/> to compare.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public bool Equals( RectangleF other )
		{
			return this == other;
		}


		/// <summary>
		/// Gets the hash code of this <see cref="RectangleF"/>.
		/// </summary>
		/// <returns>Hash code of this <see cref="RectangleF"/>.</returns>
		public override int GetHashCode()
		{
			return ( (int)x ^ (int)y ^ (int)width ^ (int)height );
		}


		/// <summary>
		/// Returns a <see cref="String"/> representation of this <see cref="RectangleF"/> in the format:
		/// {X:[<see cref="X"/>] Y:[<see cref="Y"/>] Width:[<see cref="Width"/>] Height:[<see cref="Height"/>]}
		/// </summary>
		/// <returns><see cref="String"/> representation of this <see cref="RectangleF"/>.</returns>
		public override string ToString()
		{
			return string.Format( "X:{0}, Y:{1}, Width: {2}, Height: {3}", x, y, width, height );
		}

		#endregion


		#region Operators

		/// <summary>
		/// Compares whether two <see cref="RectangleF"/> instances are equal.
		/// </summary>
		/// <param name="a"><see cref="RectangleF"/> instance on the left of the equal sign.</param>
		/// <param name="b"><see cref="RectangleF"/> instance on the right of the equal sign.</param>
		/// <returns><c>true</c> if the instances are equal; <c>false</c> otherwise.</returns>
		public static bool operator ==( RectangleF a, RectangleF b )
		{
			return ( ( a.x == b.x ) && ( a.y == b.y ) && ( a.width == b.width ) && ( a.height == b.height ) );
		}


		/// <summary>
		/// Compares whether two <see cref="RectangleF"/> instances are not equal.
		/// </summary>
		/// <param name="a"><see cref="RectangleF"/> instance on the left of the not equal sign.</param>
		/// <param name="b"><see cref="RectangleF"/> instance on the right of the not equal sign.</param>
		/// <returns><c>true</c> if the instances are not equal; <c>false</c> otherwise.</returns>
		public static bool operator !=( RectangleF a, RectangleF b )
		{
			return !( a == b );
		}


		public static implicit operator Rectangle( RectangleF self )
		{
			return RectangleExt.fromFloats( self.x, self.y, self.width, self.height );
		}

		#endregion

	}
}
