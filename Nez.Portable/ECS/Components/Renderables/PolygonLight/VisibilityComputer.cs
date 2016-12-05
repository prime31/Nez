using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;


namespace Nez.Shadows
{
	/// <summary>
	/// Class which computes a mesh that represents which regions are visibile from the origin point given a set of occluders. Usage is as
	/// follows:
	/// 
	/// - call begin
	/// - add any occluders
	/// - call end to get the visibility polygon. When end is called all internal storage is cleared.
	/// 
	/// based on: http://www.redblobgames.com/articles/visibility/ and http://roy-t.nl/index.php/2014/02/27/2d-lighting-and-shadows-preview/
	/// </summary>
	public class VisibilityComputer
	{
		float _radius;
		Vector2 _origin;

		// TODO: use FastList and convert EndPoint and Segment to structs
		List<EndPoint> _endpoints = new List<EndPoint>();
		List<Segment> _segments = new List<Segment>();
		EndPointComparer _radialComparer;

		static Vector2[] _cornerCache = new Vector2[4];
		static LinkedList<Segment> _openSegments = new LinkedList<Segment>();


		public VisibilityComputer()
		{
			_radialComparer = new EndPointComparer();
		}


		public VisibilityComputer( Vector2 origin, float radius ) : this()
		{
			_origin = origin;
			_radius = radius;
		}


		/// <summary>
		/// Add a square shaped occluder
		/// </summary>        
		public void addSquareOccluder( Vector2 position, float width, float rotation )
		{
			var x = position.X;
			var y = position.Y;

			// The distance to each corner is half of the width times sqrt(2)
			var radius = width * 0.5f * 1.41f;

			// Add Pi/4 to get the corners
			rotation += MathHelper.PiOver4;

			for( var i = 0; i < 4; i++ )
			{
				_cornerCache[i] = new Vector2(
					(float)Math.Cos( rotation + i * Math.PI * 0.5 ) * radius + x,
					(float)Math.Sin( rotation + i * Math.PI * 0.5 ) * radius + y
				);
			}

			addSegment( _cornerCache[0], _cornerCache[1] );
			addSegment( _cornerCache[1], _cornerCache[2] );
			addSegment( _cornerCache[2], _cornerCache[3] );
			addSegment( _cornerCache[3], _cornerCache[0] );
		}


		/// <summary>
		/// Add a square shaped occluder
		/// </summary>        
		public void addSquareOccluder( RectangleF bounds )
		{
			var tr = new Vector2( bounds.right, bounds.top );
			var bl = new Vector2( bounds.left, bounds.bottom );
			var br = new Vector2( bounds.right, bounds.bottom );

			addSegment( bounds.location, tr );
			addSegment( tr, br );
			addSegment( br, bl );
			addSegment( bl, bounds.location );
		}


		/// <summary>
		/// Add a line shaped occluder
		/// </summary>        
		public void addLineOccluder( Vector2 p1, Vector2 p2 )
		{
			addSegment( p1, p2 );
		}


		// Add a segment, where the first point shows up in the
		// visualization but the second one does not. (Every endpoint is
		// part of two segments, but we want to only show them once.)
		void addSegment( Vector2 p1, Vector2 p2 )
		{
			var segment = new Segment();
			var endPoint1 = new EndPoint();
			var endPoint2 = new EndPoint();

			endPoint1.position = p1;
			endPoint1.segment = segment;

			endPoint2.position = p2;
			endPoint2.segment = segment;

			segment.p1 = endPoint1;
			segment.p2 = endPoint2;

			_segments.Add( segment );
			_endpoints.Add( endPoint1 );
			_endpoints.Add( endPoint2 );
		}


		/// <summary>
		/// Remove all occluders
		/// </summary>
		public void clearOccluders()
		{
			_segments.Clear();
			_endpoints.Clear();
		}


		public void begin( Vector2 origin, float radius )
		{
			_origin = origin;
			_radius = radius;

			loadBoundaries();
		}


		/// <summary>
		/// Helper function to construct segments along the outside perimiter in order to limit the radius of the light
		/// </summary>        
		void loadBoundaries()
		{
			//Top
			addSegment( new Vector2( _origin.X - _radius, _origin.Y - _radius ),
				new Vector2( _origin.X + _radius, _origin.Y - _radius ) );

			//Bottom
			addSegment( new Vector2( _origin.X - _radius, _origin.Y + _radius ),
				new Vector2( _origin.X + _radius, _origin.Y + _radius ) );

			//Left
			addSegment( new Vector2( _origin.X - _radius, _origin.Y - _radius ),
				new Vector2( _origin.X - _radius, _origin.Y + _radius ) );

			//Right
			addSegment( new Vector2( _origin.X + _radius, _origin.Y - _radius ),
				new Vector2( _origin.X + _radius, _origin.Y + _radius ) );
		}


		/// <summary>
		/// Processess segments so that we can sort them later
		/// </summary>
		void updateSegments()
		{
			foreach( var segment in _segments )
			{
				// NOTE: future optimization: we could record the quadrant and the y/x or x/y ratio, and sort by (quadrant,
				// ratio), instead of calling atan2. See <https://github.com/mikolalysenko/compare-slope> for a
				// library that does this.

				segment.p1.angle = (float)Math.Atan2( segment.p1.position.Y - _origin.Y, segment.p1.position.X - _origin.X );
				segment.p2.angle = (float)Math.Atan2( segment.p2.position.Y - _origin.Y, segment.p2.position.X - _origin.X );

				// Map angle between -Pi and Pi
				var dAngle = segment.p2.angle - segment.p1.angle;
				if( dAngle <= -MathHelper.Pi )
					dAngle += MathHelper.TwoPi;

				if( dAngle > MathHelper.Pi )
					dAngle -= MathHelper.TwoPi;

				segment.p1.begin = ( dAngle > 0.0f );
				segment.p2.begin = !segment.p1.begin;
			}
		}


		/// <summary>
		/// Helper: do we know that segment a is in front of b? Implementation not anti-symmetric (that is to say,
		/// _segment_in_front_of(a, b) != (!_segment_in_front_of(b, a)). Also note that it only has to work in a restricted set of cases
		/// in the visibility algorithm; I don't think it handles all cases. See http://www.redblobgames.com/articles/visibility/segment-sorting.html
		/// </summary>
		/// <returns><c>true</c>, if in front of was segmented, <c>false</c> otherwise.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="relativeTo">Relative to.</param>
		bool isSegmentInFrontOf( Segment a, Segment b, Vector2 relativeTo )
		{
			// NOTE: we slightly shorten the segments so that intersections of the endpoints (common) don't count as intersections in this algorithm

			var a1 = isLeftOf( a.p2.position, a.p1.position, interpolate( b.p1.position, b.p2.position, 0.01f ) );
			var a2 = isLeftOf( a.p2.position, a.p1.position, interpolate( b.p2.position, b.p1.position, 0.01f ) );
			var a3 = isLeftOf( a.p2.position, a.p1.position, relativeTo );

			var b1 = isLeftOf( b.p2.position, b.p1.position, interpolate( a.p1.position, a.p2.position, 0.01f ) );
			var b2 = isLeftOf( b.p2.position, b.p1.position, interpolate( a.p2.position, a.p1.position, 0.01f ) );
			var b3 = isLeftOf( b.p2.position, b.p1.position, relativeTo );

			// NOTE: this algorithm is probably worthy of a short article
			// but for now, draw it on paper to see how it works. Consider
			// the line A1-A2. If both B1 and B2 are on one side and
			// relativeTo is on the other side, then A is in between the
			// viewer and B. We can do the same with B1-B2: if A1 and A2
			// are on one side, and relativeTo is on the other side, then
			// B is in between the viewer and A.
			if( b1 == b2 && b2 != b3 )
				return true;
			if( a1 == a2 && a2 == a3 )
				return true;
			if( a1 == a2 && a2 != a3 )
				return false;
			if( b1 == b2 && b2 == b3 )
				return false;

			// If A1 != A2 and B1 != B2 then we have an intersection.
			// Expose it for the GUI to show a message. A more robust
			// implementation would split segments at intersections so
			// that part of the segment is in front and part is behind.

			//demo_intersectionsDetected.push([a.p1, a.p2, b.p1, b.p2]);
			return false;

			// NOTE: previous implementation was a.d < b.d. That's simpler
			// but trouble when the segments are of dissimilar sizes. If
			// you're on a grid and the segments are similarly sized, then
			// using distance will be a simpler and faster implementation.
		}


		/// <summary>
		/// Computes the visibility polygon and returns the vertices of the triangle fan (minus the center vertex). Returned List is from the
		/// ListPool.
		/// </summary>        
		public List<Vector2> end()
		{
			var output = ListPool<Vector2>.obtain();
			updateSegments();
			_endpoints.Sort( _radialComparer );

			var currentAngle = 0f;

			// At the beginning of the sweep we want to know which
			// segments are active. The simplest way to do this is to make
			// a pass collecting the segments, and make another pass to
			// both collect and process them. However it would be more
			// efficient to go through all the segments, figure out which
			// ones intersect the initial sweep line, and then sort them.
			for( var pass = 0; pass < 2; pass++ )
			{
				foreach( var p in _endpoints )
				{
					var currentOld = _openSegments.Count == 0 ? null : _openSegments.First.Value;

					if( p.begin )
					{
						// Insert into the right place in the list
						var node = _openSegments.First;
						while( node != null && isSegmentInFrontOf( p.segment, node.Value, _origin ) )
							node = node.Next;

						if( node == null )
							_openSegments.AddLast( p.segment );
						else
							_openSegments.AddBefore( node, p.segment );
					}
					else
					{
						_openSegments.Remove( p.segment );
					}


					Segment currentNew = null;
					if( _openSegments.Count != 0 )
						currentNew = _openSegments.First.Value;

					if( currentOld != currentNew )
					{
						if( pass == 1 )
							addTriangle( output, currentAngle, p.angle, currentOld );
						currentAngle = p.angle;
					}
				}
			}

			_openSegments.Clear();
			clearOccluders();

			return output;
		}


		void addTriangle( List<Vector2> triangles, float angle1, float angle2, Segment segment )
		{
			var p1 = _origin;
			var p2 = new Vector2( _origin.X + (float)Math.Cos( angle1 ), _origin.Y + (float)Math.Sin( angle1 ) );
			var p3 = Vector2.Zero;
			var p4 = Vector2.Zero;

			if( segment != null )
			{
				// Stop the triangle at the intersecting segment
				p3.X = segment.p1.position.X;
				p3.Y = segment.p1.position.Y;
				p4.X = segment.p2.position.X;
				p4.Y = segment.p2.position.Y;
			}
			else
			{
				// Stop the triangle at a fixed distance; this probably is
				// not what we want, but it never gets used in the demo
				p3.X = _origin.X + Mathf.cos( angle1 ) * _radius * 2;
				p3.Y = _origin.Y + Mathf.sin( angle1 ) * _radius * 2;
				p4.X = _origin.X + Mathf.cos( angle2 ) * _radius * 2;
				p4.Y = _origin.Y + Mathf.sin( angle2 ) * _radius * 2;
			}

			var pBegin = lineLineIntersection( p3, p4, p1, p2 );

			p2.X = _origin.X + Mathf.cos( angle2 );
			p2.Y = _origin.Y + Mathf.sin( angle2 );

			var pEnd = lineLineIntersection( p3, p4, p1, p2 );

			triangles.Add( pBegin );
			triangles.Add( pEnd );
		}


		/// <summary>
		/// Computes the intersection point of the line p1-p2 with p3-p4
		/// </summary>        
		static Vector2 lineLineIntersection( Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4 )
		{
			// From http://paulbourke.net/geometry/lineline2d/
			var s = ( ( p4.X - p3.X ) * ( p1.Y - p3.Y ) - ( p4.Y - p3.Y ) * ( p1.X - p3.X ) )
				/ ( ( p4.Y - p3.Y ) * ( p2.X - p1.X ) - ( p4.X - p3.X ) * ( p2.Y - p1.Y ) );
			return new Vector2( p1.X + s * ( p2.X - p1.X ), p1.Y + s * ( p2.Y - p1.Y ) );
		}


		/// <summary>
		/// Returns if the point is 'left' of the line p1-p2
		/// </summary>        
		static bool isLeftOf( Vector2 p1, Vector2 p2, Vector2 point )
		{
			float cross = ( p2.X - p1.X ) * ( point.Y - p1.Y )
				- ( p2.Y - p1.Y ) * ( point.X - p1.X );

			return cross < 0;
		}


		/// <summary>
		/// Returns a slightly shortened version of the vector:
		/// p * (1 - f) + q * f
		/// </summary>        
		static Vector2 interpolate( Vector2 p, Vector2 q, float f )
		{
			return new Vector2( p.X * ( 1.0f - f ) + q.X * f, p.Y * ( 1.0f - f ) + q.Y * f );
		}

	}
}
