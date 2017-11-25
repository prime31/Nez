using Microsoft.Xna.Framework;


namespace Nez.PhysicsShapes
{
	public static partial class ShapeCollisions
	{
		public static bool lineToPoly( Vector2 start, Vector2 end, Polygon polygon, out RaycastHit hit )
		{
			hit = new RaycastHit();
			var normal = Vector2.Zero;
			var intersectionPoint = Vector2.Zero;
			var fraction = float.MaxValue;
			var hasIntersection = false;

			for( int j = polygon.points.Length - 1, i = 0; i < polygon.points.Length; j = i, i++ )
			{
				var edge1 = polygon.position + polygon.points[j];
				var edge2 = polygon.position + polygon.points[i];
				Vector2 intersection;
				if( lineToLine( edge1, edge2, start, end, out intersection ) )
				{
					hasIntersection = true;

					// TODO: is this the correct and most efficient way to get the fraction?
					// check x fraction first. if it is NaN use y instead
					var distanceFraction = ( intersection.X - start.X ) / ( end.X - start.X );
					if( float.IsNaN( distanceFraction ) || float.IsInfinity( distanceFraction ) )
						distanceFraction = ( intersection.Y - start.Y ) / ( end.Y - start.Y );

					if( distanceFraction < fraction )
					{
						var edge = edge2 - edge1;
						normal = new Vector2( edge.Y, -edge.X );
						fraction = distanceFraction;
						intersectionPoint = intersection;
					}
				}
			}

			if( hasIntersection )
			{
				normal.Normalize();
				float distance;
				Vector2.Distance( ref start, ref intersectionPoint, out distance );
				hit.setValues( fraction, distance, intersectionPoint, normal );
				return true;
			}

			return false;
		}


		public static bool lineToCircle( Vector2 start, Vector2 end, Circle s, out RaycastHit hit )
		{
			hit = new RaycastHit();

			// calculate the length here and normalize d separately since we will need it to get the fraction if we have a hit
			var lineLength = Vector2.Distance( start, end );
			var d = ( end - start ) / lineLength;
			var m = start - s.position;
			var b = Vector2.Dot( m, d );
			var c = Vector2.Dot( m, m ) - s.radius * s.radius;

			// exit if r's origin outside of s (c > 0) and r pointing away from s (b > 0)
			if( c > 0f && b > 0f )
				return false;

			var discr = b * b - c;

			// a negative descriminant means the line misses the circle
			if( discr < 0 )
				return false;

			// ray intersects circle. calculate details now.
			hit.fraction = -b - Mathf.sqrt( discr );

			// if fraction is negative, ray started inside circle so clamp fraction to 0
			if( hit.fraction < 0 )
				hit.fraction = 0;

			hit.point = start + hit.fraction * d;
			Vector2.Distance( ref start, ref hit.point, out hit.distance );
			hit.normal = Vector2.Normalize( hit.point - s.position );
			hit.fraction = hit.distance / lineLength;

			return true;
		}


		public static bool lineToLine( Vector2 a1, Vector2 a2, Vector2 b1, Vector2 b2, out Vector2 intersection )
		{
			intersection = Vector2.Zero;

			var b = a2 - a1;
			var d = b2 - b1;
			var bDotDPerp = b.X * d.Y - b.Y * d.X;

			// if b dot d == 0, it means the lines are parallel so have infinite intersection points
			if( bDotDPerp == 0 )
				return false;

			var c = b1 - a1;
			var t = ( c.X * d.Y - c.Y * d.X ) / bDotDPerp;
			if( t < 0 || t > 1 )
				return false;

			var u = ( c.X * b.Y - c.Y * b.X ) / bDotDPerp;
			if( u < 0 || u > 1 )
				return false;

			intersection = a1 + t * b;

			return true;
		}

	}
}
