using Microsoft.Xna.Framework;


namespace Nez.Splines
{
	/// <summary>
	/// houses a series of cubic bezier points and provides helper methods to access the bezier
	/// </summary>
	public class BezierSpline
	{
		FastList<Vector2> _points = new FastList<Vector2>();
		int _curveCount;


		/// <summary>
		/// helper that gets the bezier point index at time t. t is modified in the process to be in the range of the curve segment.
		/// </summary>
		/// <returns>The index at time.</returns>
		/// <param name="t">T.</param>
		int pointIndexAtTime( ref float t )
		{
			int i;
			if( t >= 1f )
			{
				t = 1f;
				i = _points.length - 4;
			}
			else
			{
				t = Mathf.clamp01( t ) * _curveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return i;
		}


		/// <summary>
		/// sets a control point taking into account if this is a shared point and adjusting appropriately if it is
		/// </summary>
		/// <param name="index">Index.</param>
		/// <param name="point">Point.</param>
		public void setControlPoint( int index, Vector2 point )
		{
			if( index % 3 == 0 )
			{
				var delta = point - _points[index];
				if( index > 0 )
					_points.buffer[index - 1] += delta;

				if( index + 1 < _points.length )
					_points.buffer[index + 1] += delta;
			}
			_points.buffer[index] = point;
		}


		/// <summary>
		/// gets the point on the bezier at time t
		/// </summary>
		/// <returns>The point at time.</returns>
		/// <param name="t">T.</param>
		public Vector2 getPointAtTime( float t )
		{
			var i = pointIndexAtTime( ref t );
			return Bezier.getPoint( _points.buffer[i], _points.buffer[i + 1], _points.buffer[i + 2], _points.buffer[i + 3], t );
		}


		/// <summary>
		/// gets the velocity (first derivative) of the bezier at time t
		/// </summary>
		/// <returns>The velocity at time.</returns>
		/// <param name="t">T.</param>
		public Vector2 getVelocityAtTime( float t )
		{
			var i = pointIndexAtTime( ref t );
			return Bezier.getFirstDerivative( _points.buffer[i], _points.buffer[i + 1], _points.buffer[i + 2], _points.buffer[i + 3], t );
		}


		/// <summary>
		/// gets the direction (normalized first derivative) of the bezier at time t
		/// </summary>
		/// <returns>The direction at time.</returns>
		/// <param name="t">T.</param>
		public Vector2 getDirectionAtTime( float t )
		{
			return Vector2.Normalize( getVelocityAtTime( t ) );
		}


		/// <summary>
		/// adds a curve to the bezier
		/// </summary>
		/// <param name="start">Start.</param>
		/// <param name="firstControlPoint">First control point.</param>
		/// <param name="secondControlPoint">Second control point.</param>
		public void addCurve( Vector2 start, Vector2 firstControlPoint, Vector2 secondControlPoint, Vector2 end )
		{
			// we only add the start point if this is the first curve. For all other curves the previous end should equal the start of the new curve.
			if( _points.length == 0 )
				_points.add( start );
			
			_points.add( firstControlPoint );
			_points.add( secondControlPoint );
			_points.add( end );

			_curveCount = ( _points.length - 1 ) / 3;
		}


		/// <summary>
		/// resets the bezier removing all points
		/// </summary>
		public void reset()
		{
			_points.clear();
		}


		/// <summary>
		/// breaks up the spline into totalSegments parts and returns all the points required to draw using lines
		/// </summary>
		/// <returns>The drawing points.</returns>
		/// <param name="totalSegments">Total segments.</param>
		public Vector2[] getDrawingPoints( int totalSegments )
		{
			var points = new Vector2[totalSegments];
			for( var i = 0; i < totalSegments; i++ )
			{
				var t = i / (float)totalSegments;
				points[i] = getPointAtTime( t );
			}

			return points;
		}

	}
}
