using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Nez.Shadows
{
	/// <summary>
	/// Class which computes a mesh that represents which regions are visibile from the origin point given a set of occluders
	/// </summary>
	public class VisibilityComputer
	{
		/// <summary>
		/// The origin, or position of the observer
		/// </summary>
		public Vector2 origin { get { return _origin; } }

		/// <summary>
		/// The maxiumum view distance
		/// </summary>
		public float radius { get { return _radius; } }
		float _radius;

		// These represent the map and light location:
		List<EndPoint> _endpoints;
		List<Segment> _segments;
		Vector2 _origin;
		EndPointComparer _radialComparer;


		public VisibilityComputer( Vector2 origin, float radius )
		{
			_segments = new List<Segment>();
			_endpoints = new List<EndPoint>();
			_radialComparer = new EndPointComparer();

			this._origin = origin;
			this._radius = radius;            
			loadBoundaries();
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

			var corners = new Vector2[4];
			for( var i = 0; i < 4; i++ )
			{
				corners[i] = new Vector2(
					(float)Math.Cos( rotation + i * Math.PI * 0.5 ) * radius + x,
					(float)Math.Sin( rotation + i * Math.PI * 0.5 ) * radius + y
				);
			}

			addSegment( corners[0], corners[1] );
			addSegment( corners[1], corners[2] );
			addSegment( corners[2], corners[3] );
			addSegment( corners[3], corners[0] );
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


		// Processess segments so that we can sort them later
		void updateSegments()
		{            
			foreach( var segment in _segments )
			{                               
				// NOTE: future optimization: we could record the quadrant
				// and the y/x or x/y ratio, and sort by (quadrant,
				// ratio), instead of calling atan2. See
				// <https://github.com/mikolalysenko/compare-slope> for a
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
       

		// Helper: do we know that segment a is in front of b?
		// Implementation not anti-symmetric (that is to say,
		// _segment_in_front_of(a, b) != (!_segment_in_front_of(b, a)).
		// Also note that it only has to work in a restricted set of cases
		// in the visibility algorithm; I don't think it handles all
		// cases. See http://www.redblobgames.com/articles/visibility/segment-sorting.html
		bool segmentInFrontOf( Segment a, Segment b, Vector2 relativeTo )
		{
			// NOTE: we slightly shorten the segments so that
			// intersections of the endpoints (common) don't count as
			// intersections in this algorithm                        

			var a1 = leftOf( a.p2.position, a.p1.position, interpolate( b.p1.position, b.p2.position, 0.01f ) );
			var a2 = leftOf( a.p2.position, a.p1.position, interpolate( b.p2.position, b.p1.position, 0.01f ) );
			var a3 = leftOf( a.p2.position, a.p1.position, relativeTo );

			var b1 = leftOf( b.p2.position, b.p1.position, interpolate( a.p1.position, a.p2.position, 0.01f ) );
			var b2 = leftOf( b.p2.position, b.p1.position, interpolate( a.p2.position, a.p1.position, 0.01f ) );
			var b3 = leftOf( b.p2.position, b.p1.position, relativeTo );                        

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
		/// Computes the visibility polygon and returns the vertices
		/// of the triangle fan (minus the center vertex)
		/// </summary>        
		public List<Vector2> computeVisibilityPolygon()
		{
			var output = new List<Vector2>();
			var open = new LinkedList<Segment>();

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
				foreach( EndPoint p in _endpoints )
				{
					Segment currentOld = open.Count == 0 ? null : open.First.Value;

					if( p.begin )
					{
						// Insert into the right place in the list
						var node = open.First;
						while( node != null && segmentInFrontOf( p.segment, node.Value, _origin ) )
						{
							node = node.Next;
						}

						if( node == null )
						{
							open.AddLast( p.segment );
						}
						else
						{
							open.AddBefore( node, p.segment );
						}
					}
					else
					{
						open.Remove( p.segment );
					}


					Segment currentNew = null;
					if( open.Count != 0 )
					{                
						currentNew = open.First.Value;
					}
             
					if( currentOld != currentNew )
					{
						if( pass == 1 )
						{
							addTriangle( output, currentAngle, p.angle, currentOld );

						}
						currentAngle = p.angle;
					}
				}
			}

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
				p3.X = _origin.X + (float)Math.Cos( angle1 ) * _radius * 2;
				p3.Y = _origin.Y + (float)Math.Sin( angle1 ) * _radius * 2;
				p4.X = _origin.X + (float)Math.Cos( angle2 ) * _radius * 2;
				p4.Y = _origin.Y + (float)Math.Sin( angle2 ) * _radius * 2;
			}

			var pBegin = lineLineIntersection( p3, p4, p1, p2 );

			p2.X = _origin.X + (float)Math.Cos( angle2 );
			p2.Y = _origin.Y + (float)Math.Sin( angle2 );

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
		static bool leftOf( Vector2 p1, Vector2 p2, Vector2 point )
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
