using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Nez.PhysicsShapes;
using System.Runtime.CompilerServices;


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
		/// <summary>
		/// total number of lines that will be used when approximating a circle. Only a 180 degree hemisphere is needed so this will be the number
		/// of segments to approximate that hemisphere.
		/// </summary>
		public int LineCountForCircleApproximation = 10;

		float _radius;
		Vector2 _origin;
		bool _isSpotLight;
		float _spotStartAngle, _spotEndAngle;

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

		public VisibilityComputer(Vector2 origin, float radius) : this()
		{
			_origin = origin;
			_radius = radius;
		}

		/// <summary>
		/// adds a Collider as an occluder for the PolyLight
		/// </summary>
		/// <param name="collider">Collider.</param>
		public void AddColliderOccluder(Collider collider)
		{
			// special case for BoxColliders with no rotation
			if (collider is BoxCollider && collider.Rotation == 0)
			{
				AddSquareOccluder(collider.Bounds);
				return;
			}

			if (collider is PolygonCollider)
			{
				var poly = collider.Shape as Polygon;
				for (var i = 0; i < poly.Points.Length; i++)
				{
					var firstIndex = i - 1;
					if (i == 0)
						firstIndex += poly.Points.Length;
					AddLineOccluder(poly.Points[firstIndex] + poly.position, poly.Points[i] + poly.position);
				}
			}
			else if (collider is CircleCollider)
			{
				AddCircleOccluder(collider.AbsolutePosition, (collider as CircleCollider).Radius);
			}
		}

		/// <summary>
		/// Add a square shaped occluder
		/// </summary>        
		public void AddSquareOccluder(Vector2 position, float width, float rotation)
		{
			var x = position.X;
			var y = position.Y;

			// The distance to each corner is half of the width times sqrt(2)
			var radius = width * 0.5f * 1.41f;

			// Add Pi/4 to get the corners
			rotation += MathHelper.PiOver4;

			for (var i = 0; i < 4; i++)
			{
				_cornerCache[i] = new Vector2(
					(float) Math.Cos(rotation + i * Math.PI * 0.5) * radius + x,
					(float) Math.Sin(rotation + i * Math.PI * 0.5) * radius + y
				);
			}

			AddSegment(_cornerCache[0], _cornerCache[1]);
			AddSegment(_cornerCache[1], _cornerCache[2]);
			AddSegment(_cornerCache[2], _cornerCache[3]);
			AddSegment(_cornerCache[3], _cornerCache[0]);
		}

		/// <summary>
		/// Add a square shaped occluder
		/// </summary>        
		public void AddSquareOccluder(RectangleF bounds)
		{
			var tr = new Vector2(bounds.Right, bounds.Top);
			var bl = new Vector2(bounds.Left, bounds.Bottom);
			var br = new Vector2(bounds.Right, bounds.Bottom);

			AddSegment(bounds.Location, tr);
			AddSegment(tr, br);
			AddSegment(br, bl);
			AddSegment(bl, bounds.Location);
		}

		/// <summary>
		/// adds a circle shaped occluder
		/// </summary>
		/// <param name="position">Position.</param>
		/// <param name="radius">Radius.</param>
		public void AddCircleOccluder(Vector2 position, float radius)
		{
			var dirToCircle = position - _origin;
			var angle = Mathf.Atan2(dirToCircle.Y, dirToCircle.X);

			var stepSize = MathHelper.Pi / LineCountForCircleApproximation;
			var startAngle = angle + MathHelper.PiOver2;
			var lastPt = Mathf.AngleToVector(startAngle, radius) + position;
			for (var i = 1; i < LineCountForCircleApproximation; i++)
			{
				var nextPt = Mathf.AngleToVector(startAngle + i * stepSize, radius) + position;
				AddLineOccluder(lastPt, nextPt);
				lastPt = nextPt;
			}
		}

		/// <summary>
		/// Add a line shaped occluder
		/// </summary>        
		public void AddLineOccluder(Vector2 p1, Vector2 p2)
		{
			AddSegment(p1, p2);
		}

		// Add a segment, where the first point shows up in the
		// visualization but the second one does not. (Every endpoint is
		// part of two segments, but we want to only show them once.)
		void AddSegment(Vector2 p1, Vector2 p2)
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

			_segments.Add(segment);
			_endpoints.Add(endPoint1);
			_endpoints.Add(endPoint2);
		}

		/// <summary>
		/// Remove all occluders
		/// </summary>
		public void ClearOccluders()
		{
			_segments.Clear();
			_endpoints.Clear();
		}

		/// <summary>
		/// prepares the computer for calculating the current poly light
		/// </summary>
		/// <param name="origin">Origin.</param>
		/// <param name="radius">Radius.</param>
		public void Begin(Vector2 origin, float radius)
		{
			_origin = origin;
			_radius = radius;
			_isSpotLight = false;
		}

		/// <summary>
		/// Computes the visibility polygon and returns the vertices of the triangle fan (minus the center vertex). Returned List is from the
		/// ListPool.
		/// </summary>        
		public List<Vector2> End()
		{
			var output = ListPool<Vector2>.Obtain();
			UpdateSegments();
			_endpoints.Sort(_radialComparer);

			var currentAngle = 0f;

			// At the beginning of the sweep we want to know which segments are active. The simplest way to do this is to make
			// a pass collecting the segments, and make another pass to both collect and process them. However it would be more
			// efficient to go through all the segments, figure out which ones intersect the initial sweep line, and then sort them.
			for (var pass = 0; pass < 2; pass++)
			{
				foreach (var p in _endpoints)
				{
					var currentOld = _openSegments.Count == 0 ? null : _openSegments.First.Value;

					if (p.begin)
					{
						// Insert into the right place in the list
						var node = _openSegments.First;
						while (node != null && IsSegmentInFrontOf(p.segment, node.Value, _origin))
							node = node.Next;

						if (node == null)
							_openSegments.AddLast(p.segment);
						else
							_openSegments.AddBefore(node, p.segment);
					}
					else
					{
						_openSegments.Remove(p.segment);
					}


					Segment currentNew = null;
					if (_openSegments.Count != 0)
						currentNew = _openSegments.First.Value;

					if (currentOld != currentNew)
					{
						if (pass == 1)
						{
							if (!_isSpotLight || (Between(currentAngle, _spotStartAngle, _spotEndAngle) &&
							                      Between(p.angle, _spotStartAngle, _spotEndAngle)))
								AddTriangle(output, currentAngle, p.angle, currentOld);
						}

						currentAngle = p.angle;
					}
				}
			}

			_openSegments.Clear();
			ClearOccluders();

			// uncomment to draw squares at all the encounter points
			//for( var i = 0; i < output.Count; i++ )
			//	Debug.drawPixel( output[i], 10, Color.Orange );

			return output;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		static bool Between(float value, float min, float max)
		{
			//const float maxDistance = (float)Math.PI * 0.5f; // 90 degrees

			//return Math.Abs( MathHelper.WrapAngle( min - value ) ) < maxDistance
			//	       && Math.Abs( MathHelper.WrapAngle( max - value ) ) < maxDistance;


			//var normalisedMin = min > 0 ? min : 2 * Math.PI + min;
			//var normalisedMax = max > 0 ? max : 2 * Math.PI + max;
			//var normalisedTarget = value > 0 ? value : 2 * Math.PI + value;

			//return normalisedMin <= normalisedTarget && normalisedTarget <= normalisedMax;


			value = (360 + (value % 360)) % 360;
			min = (3600000 + min) % 360;
			max = (3600000 + max) % 360;

			if (min < max)
				return min <= value && value <= max;

			return min <= value || value <= max;


			//return value >= min && value <= max;
		}

		/// <summary>
		/// Helper function to construct segments along the outside perimiter in order to limit the radius of the light
		/// </summary>        
		public void LoadRectangleBoundaries()
		{
			//Top
			AddSegment(new Vector2(_origin.X - _radius, _origin.Y - _radius),
				new Vector2(_origin.X + _radius, _origin.Y - _radius));

			//Bottom
			AddSegment(new Vector2(_origin.X - _radius, _origin.Y + _radius),
				new Vector2(_origin.X + _radius, _origin.Y + _radius));

			//Left
			AddSegment(new Vector2(_origin.X - _radius, _origin.Y - _radius),
				new Vector2(_origin.X - _radius, _origin.Y + _radius));

			//Right
			AddSegment(new Vector2(_origin.X + _radius, _origin.Y - _radius),
				new Vector2(_origin.X + _radius, _origin.Y + _radius));
		}

		public void LoadSpotLightBoundaries(Vector2[] points)
		{
			_isSpotLight = true;

			// add the two outer edges of the polygon but lerp them a bit so they dont start at the origin
			var first = Vector2.Lerp(_origin, _origin + points[1], 0.1f);
			var second = Vector2.Lerp(_origin, _origin + points[points.Length - 1], 0.1f);
			AddSegment(first, _origin + points[1]);
			AddSegment(second, _origin + points[points.Length - 1]);

			LoadRectangleBoundaries();
		}

		/// <summary>
		/// Processes segments so that we can sort them later
		/// </summary>
		void UpdateSegments()
		{
			foreach (var segment in _segments)
			{
				// NOTE: future optimization: we could record the quadrant and the y/x or x/y ratio, and sort by (quadrant,
				// ratio), instead of calling atan2. See <https://github.com/mikolalysenko/compare-slope> for a
				// library that does this.

				segment.p1.angle = Mathf.Atan2(segment.p1.position.Y - _origin.Y, segment.p1.position.X - _origin.X);
				segment.p2.angle = Mathf.Atan2(segment.p2.position.Y - _origin.Y, segment.p2.position.X - _origin.X);

				// Map angle between -Pi and Pi
				var dAngle = segment.p2.angle - segment.p1.angle;
				if (dAngle <= -MathHelper.Pi)
					dAngle += MathHelper.TwoPi;

				if (dAngle > MathHelper.Pi)
					dAngle -= MathHelper.TwoPi;

				segment.p1.begin = (dAngle > 0.0f);
				segment.p2.begin = !segment.p1.begin;
			}

			// if we have a spot light we need to store the first two segments angles. These are the spot boundaries and we will use them to filter
			// any verts outside of them.
			if (_isSpotLight)
			{
				_spotStartAngle = _segments[0].p2.angle;
				_spotEndAngle = _segments[1].p2.angle;
			}
		}

		/// <summary>
		/// Helper: do we know that segment a is in front of b? Implementation not anti-symmetric (that is to say,
		/// isSegmentInFrontOf(a, b) != (!isSegmentInFrontOf(b, a)). Also note that it only has to work in a restricted set of cases
		/// in the visibility algorithm; I don't think it handles all cases. See http://www.redblobgames.com/articles/visibility/segment-sorting.html
		/// </summary>
		/// <returns><c>true</c>, if in front of was segmented, <c>false</c> otherwise.</returns>
		/// <param name="a">The alpha component.</param>
		/// <param name="b">The blue component.</param>
		/// <param name="relativeTo">Relative to.</param>
		bool IsSegmentInFrontOf(Segment a, Segment b, Vector2 relativeTo)
		{
			// NOTE: we slightly shorten the segments so that intersections of the endpoints (common) don't count as intersections in this algorithm
			var a1 = IsLeftOf(a.p2.position, a.p1.position, Interpolate(b.p1.position, b.p2.position, 0.01f));
			var a2 = IsLeftOf(a.p2.position, a.p1.position, Interpolate(b.p2.position, b.p1.position, 0.01f));
			var a3 = IsLeftOf(a.p2.position, a.p1.position, relativeTo);

			var b1 = IsLeftOf(b.p2.position, b.p1.position, Interpolate(a.p1.position, a.p2.position, 0.01f));
			var b2 = IsLeftOf(b.p2.position, b.p1.position, Interpolate(a.p2.position, a.p1.position, 0.01f));
			var b3 = IsLeftOf(b.p2.position, b.p1.position, relativeTo);

			// NOTE: this algorithm is probably worthy of a short article but for now, draw it on paper to see how it works. Consider
			// the line A1-A2. If both B1 and B2 are on one side and relativeTo is on the other side, then A is in between the
			// viewer and B. We can do the same with B1-B2: if A1 and A2 are on one side, and relativeTo is on the other side, then
			// B is in between the viewer and A.
			if (b1 == b2 && b2 != b3)
				return true;
			if (a1 == a2 && a2 == a3)
				return true;
			if (a1 == a2 && a2 != a3)
				return false;
			if (b1 == b2 && b2 == b3)
				return false;

			// If A1 != A2 and B1 != B2 then we have an intersection. A more robust implementation would split segments at intersections so that
			// part of the segment is in front and part is behind but we shouldnt have overlapping colliders anyway so it isnt too important.

			return false;

			// NOTE: previous implementation was a.d < b.d. That's simpler but trouble when the segments are of dissimilar sizes. If
			// you're on a grid and the segments are similarly sized, then using distance will be a simpler and faster implementation.
		}

		void AddTriangle(List<Vector2> triangles, float angle1, float angle2, Segment segment)
		{
			var p1 = _origin;
			var p2 = new Vector2(_origin.X + Mathf.Cos(angle1), _origin.Y + Mathf.Sin(angle1));
			var p3 = Vector2.Zero;
			var p4 = Vector2.Zero;

			if (segment != null)
			{
				// Stop the triangle at the intersecting segment
				p3.X = segment.p1.position.X;
				p3.Y = segment.p1.position.Y;
				p4.X = segment.p2.position.X;
				p4.Y = segment.p2.position.Y;
			}
			else
			{
				// Stop the triangle at a fixed distance
				p3.X = _origin.X + Mathf.Cos(angle1) * _radius * 2;
				p3.Y = _origin.Y + Mathf.Sin(angle1) * _radius * 2;
				p4.X = _origin.X + Mathf.Cos(angle2) * _radius * 2;
				p4.Y = _origin.Y + Mathf.Sin(angle2) * _radius * 2;
			}

			var pBegin = LineLineIntersection(p3, p4, p1, p2);

			p2.X = _origin.X + Mathf.Cos(angle2);
			p2.Y = _origin.Y + Mathf.Sin(angle2);

			var pEnd = LineLineIntersection(p3, p4, p1, p2);

			triangles.Add(pBegin);
			triangles.Add(pEnd);
		}

		/// <summary>
		/// Computes the intersection point of the line p1-p2 with p3-p4
		/// </summary>        
		static Vector2 LineLineIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
		{
			// From http://paulbourke.net/geometry/lineline2d/
			var s = ((p4.X - p3.X) * (p1.Y - p3.Y) - (p4.Y - p3.Y) * (p1.X - p3.X))
			        / ((p4.Y - p3.Y) * (p2.X - p1.X) - (p4.X - p3.X) * (p2.Y - p1.Y));
			return new Vector2(p1.X + s * (p2.X - p1.X), p1.Y + s * (p2.Y - p1.Y));
		}

		/// <summary>
		/// Returns if the point is 'left' of the line p1-p2
		/// </summary>        
		static bool IsLeftOf(Vector2 p1, Vector2 p2, Vector2 point)
		{
			float cross = (p2.X - p1.X) * (point.Y - p1.Y)
			              - (p2.Y - p1.Y) * (point.X - p1.X);

			return cross < 0;
		}

		/// <summary>
		/// Returns a slightly shortened version of the vector:
		/// p * (1 - f) + q * f
		/// </summary>        
		static Vector2 Interpolate(Vector2 p, Vector2 q, float f)
		{
			return new Vector2(p.X * (1.0f - f) + q.X * f, p.Y * (1.0f - f) + q.Y * f);
		}
	}
}