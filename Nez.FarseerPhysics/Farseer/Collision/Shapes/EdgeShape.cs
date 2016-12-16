/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/

using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Collision.Shapes
{
	/// <summary>
	/// A line segment (edge) shape. These can be connected in chains or loops
	/// to other edge shapes.
	/// The connectivity information is used to ensure correct contact normals.
	/// </summary>
	public class EdgeShape : Shape
	{
		public override int childCount { get { return 1; } }

		/// <summary>
		/// Is true if the edge is connected to an adjacent vertex before vertex 1.
		/// </summary>
		public bool hasVertex0;

		/// <summary>
		/// Is true if the edge is connected to an adjacent vertex after vertex2.
		/// </summary>
		public bool hasVertex3;

		/// <summary>
		/// Optional adjacent vertices. These are used for smooth collision.
		/// </summary>
		public Vector2 vertex0;

		/// <summary>
		/// Optional adjacent vertices. These are used for smooth collision.
		/// </summary>
		public Vector2 vertex3;

		/// <summary>
		/// These are the edge vertices
		/// </summary>
		public Vector2 vertex1
		{
			get { return _vertex1; }
			set
			{
				_vertex1 = value;
				computeProperties();
			}
		}

		/// <summary>
		/// These are the edge vertices
		/// </summary>
		public Vector2 vertex2
		{
			get { return _vertex2; }
			set
			{
				_vertex2 = value;
				computeProperties();
			}
		}

		/// <summary>
		/// Edge start vertex
		/// </summary>
		internal Vector2 _vertex1;

		/// <summary>
		/// Edge end vertex
		/// </summary>
		internal Vector2 _vertex2;


		internal EdgeShape() : base( 0 )
		{
			shapeType = ShapeType.Edge;
			_radius = Settings.polygonRadius;
		}

		/// <summary>
		/// Create a new EdgeShape with the specified start and end.
		/// </summary>
		/// <param name="start">The start of the edge.</param>
		/// <param name="end">The end of the edge.</param>
		public EdgeShape( Vector2 start, Vector2 end ) : base( 0 )
		{
			shapeType = ShapeType.Edge;
			_radius = Settings.polygonRadius;
			Set( start, end );
		}

		/// <summary>
		/// Set this as an isolated edge.
		/// </summary>
		/// <param name="start">The start.</param>
		/// <param name="end">The end.</param>
		public void Set( Vector2 start, Vector2 end )
		{
			_vertex1 = start;
			_vertex2 = end;
			hasVertex0 = false;
			hasVertex3 = false;

			computeProperties();
		}

		public override bool testPoint( ref Transform transform, ref Vector2 point )
		{
			return false;
		}

		public override bool rayCast( out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex )
		{
			// p = p1 + t * d
			// v = v1 + s * e
			// p1 + t * d = v1 + s * e
			// s * e - t * d = p1 - v1

			output = new RayCastOutput();

			// Put the ray into the edge's frame of reference.
			var p1 = MathUtils.mulT( transform.q, input.point1 - transform.p );
			var p2 = MathUtils.mulT( transform.q, input.point2 - transform.p );
			var d = p2 - p1;

			var v1 = _vertex1;
			var v2 = _vertex2;
			var e = v2 - v1;
			var normal = new Vector2( e.Y, -e.X ); //TODO: Could possibly cache the normal.
			Nez.Vector2Ext.normalize( ref normal );

			// q = p1 + t * d
			// dot(normal, q - v1) = 0
			// dot(normal, p1 - v1) + t * dot(normal, d) = 0
			var numerator = Vector2.Dot( normal, v1 - p1 );
			var denominator = Vector2.Dot( normal, d );

			if( denominator == 0.0f )
				return false;

			float t = numerator / denominator;
			if( t < 0.0f || input.maxFraction < t )
				return false;

			var q = p1 + t * d;

			// q = v1 + s * r
			// s = dot(q - v1, r) / dot(r, r)
			var r = v2 - v1;
			var rr = Vector2.Dot( r, r );
			if( rr == 0.0f )
				return false;

			float s = Vector2.Dot( q - v1, r ) / rr;
			if( s < 0.0f || 1.0f < s )
				return false;

			output.fraction = t;
			if( numerator > 0.0f )
				output.normal = -normal;
			else
				output.normal = normal;
			
			return true;
		}

		public override void computeAABB( out AABB aabb, ref Transform transform, int childIndex )
		{
			var v1 = MathUtils.mul( ref transform, _vertex1 );
			var v2 = MathUtils.mul( ref transform, _vertex2 );

			var lower = Vector2.Min( v1, v2 );
			var upper = Vector2.Max( v1, v2 );

			var r = new Vector2( radius, radius );
			aabb.lowerBound = lower - r;
			aabb.upperBound = upper + r;
		}

		protected override void computeProperties()
		{
			massData.centroid = 0.5f * ( _vertex1 + _vertex2 );
		}

		public override float computeSubmergedArea( ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc )
		{
			sc = Vector2.Zero;
			return 0;
		}

		public bool CompareTo( EdgeShape shape )
		{
			return ( hasVertex0 == shape.hasVertex0 &&
					hasVertex3 == shape.hasVertex3 &&
					vertex0 == shape.vertex0 &&
					vertex1 == shape.vertex1 &&
					vertex2 == shape.vertex2 &&
					vertex3 == shape.vertex3 );
		}

		public override Shape clone()
		{
			var clone = new EdgeShape();
			clone.shapeType = shapeType;
			clone._radius = _radius;
			clone._density = _density;
			clone.hasVertex0 = hasVertex0;
			clone.hasVertex3 = hasVertex3;
			clone.vertex0 = vertex0;
			clone._vertex1 = _vertex1;
			clone._vertex2 = _vertex2;
			clone.vertex3 = vertex3;
			clone.massData = massData;
			return clone;
		}
	
	}
}