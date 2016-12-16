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

using System.Diagnostics;
using FarseerPhysics.Common;
using FarseerPhysics.Common.ConvexHull;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Collision.Shapes
{
	/// <summary>
	/// Represents a simple non-selfintersecting convex polygon.
	/// Create a convex hull from the given array of points.
	/// </summary>
	public class PolygonShape : Shape
	{
		/// <summary>
		/// Create a convex hull from the given array of local points.
		/// The number of vertices must be in the range [3, Settings.MaxPolygonVertices].
		/// Warning: the points may be re-ordered, even if they form a convex polygon
		/// Warning: collinear points are handled but not removed. Collinear points may lead to poor stacking behavior.
		/// </summary>
		public Vertices vertices
		{
			get { return _vertices; }
			set { setVerticesNoCopy( new Vertices( value ) ); }
		}

		public Vertices normals { get { return _normals; } }

		public override int childCount { get { return 1; } }

		Vertices _vertices;
		Vertices _normals;


		/// <summary>
		/// Initializes a new instance of the <see cref="PolygonShape"/> class.
		/// </summary>
		/// <param name="vertices">The vertices.</param>
		/// <param name="density">The density.</param>
		public PolygonShape( Vertices vertices, float density ) : base( density )
		{
			shapeType = ShapeType.Polygon;
			_radius = Settings.polygonRadius;

			this.vertices = vertices;
		}

		/// <summary>
		/// Create a new PolygonShape with the specified density.
		/// </summary>
		/// <param name="density">The density.</param>
		public PolygonShape( float density ) : base( density )
		{
			Debug.Assert( density >= 0f );

			shapeType = ShapeType.Polygon;
			_radius = Settings.polygonRadius;
			_vertices = new Vertices( Settings.maxPolygonVertices );
			_normals = new Vertices( Settings.maxPolygonVertices );
		}

		internal PolygonShape() : base( 0 )
		{
			shapeType = ShapeType.Polygon;
			_radius = Settings.polygonRadius;
			_vertices = new Vertices( Settings.maxPolygonVertices );
			_normals = new Vertices( Settings.maxPolygonVertices );
		}


		/// <summary>
		/// sets the vertices without copying over the data from verts to the local List.
		/// </summary>
		/// <param name="verts">Verts.</param>
		public void setVerticesNoCopy( Vertices verts )
		{
			Debug.Assert( verts.Count >= 3 && verts.Count <= Settings.maxPolygonVertices );
			_vertices = verts;

			if( Settings.useConvexHullPolygons )
			{
				// FPE note: This check is required as the GiftWrap algorithm early exits on triangles
				// So instead of giftwrapping a triangle, we just force it to be clock wise.
				if( _vertices.Count <= 3 )
					_vertices.forceCounterClockWise();
				else
					_vertices = GiftWrap.getConvexHull( _vertices );
			}

			if( _normals == null )
				_normals = new Vertices( _vertices.Count );
			else
				_normals.Clear();

			// Compute normals. Ensure the edges have non-zero length.
			for( var i = 0; i < _vertices.Count; ++i )
			{
				var next = i + 1 < _vertices.Count ? i + 1 : 0;
				var edge = _vertices[next] - _vertices[i];
				Debug.Assert( edge.LengthSquared() > Settings.epsilon * Settings.epsilon );

				// FPE optimization: Normals.Add(MathHelper.Cross(edge, 1.0f));
				var temp = new Vector2( edge.Y, -edge.X );
				Nez.Vector2Ext.normalize( ref temp );
				_normals.Add( temp );
			}

			// Compute the polygon mass data
			computeProperties();
		}

		protected override void computeProperties()
		{
			// Polygon mass, centroid, and inertia.
			// Let rho be the polygon density in mass per unit area.
			// Then:
			// mass = rho * int(dA)
			// centroid.X = (1/mass) * rho * int(x * dA)
			// centroid.Y = (1/mass) * rho * int(y * dA)
			// I = rho * int((x*x + y*y) * dA)
			//
			// We can compute these integrals by summing all the integrals
			// for each triangle of the polygon. To evaluate the integral
			// for a single triangle, we make a change of variables to
			// the (u,v) coordinates of the triangle:
			// x = x0 + e1x * u + e2x * v
			// y = y0 + e1y * u + e2y * v
			// where 0 <= u && 0 <= v && u + v <= 1.
			//
			// We integrate u from [0,1-v] and then v from [0,1].
			// We also need to use the Jacobian of the transformation:
			// D = cross(e1, e2)
			//
			// Simplification: triangle centroid = (1/3) * (p1 + p2 + p3)
			//
			// The rest of the derivation is handled by computer algebra.

			Debug.Assert( vertices.Count >= 3 );

			//FPE optimization: Early exit as polygons with 0 density does not have any properties.
			if( _density <= 0 )
				return;

			//FPE optimization: Consolidated the calculate centroid and mass code to a single method.
			var center = Vector2.Zero;
			var area = 0.0f;
			var I = 0.0f;

			// pRef is the reference point for forming triangles.
			// It's location doesn't change the result (except for rounding error).
			var s = Vector2.Zero;

			// This code would put the reference point inside the polygon.
			for( int i = 0; i < vertices.Count; ++i )
				s += vertices[i];
			s *= 1.0f / vertices.Count;

			const float k_inv3 = 1.0f / 3.0f;

			for( int i = 0; i < vertices.Count; ++i )
			{
				// Triangle vertices.
				Vector2 e1 = vertices[i] - s;
				Vector2 e2 = i + 1 < vertices.Count ? vertices[i + 1] - s : vertices[0] - s;

				var D = MathUtils.cross( e1, e2 );

				var triangleArea = 0.5f * D;
				area += triangleArea;

				// Area weighted centroid
				center += triangleArea * k_inv3 * ( e1 + e2 );

				float ex1 = e1.X, ey1 = e1.Y;
				float ex2 = e2.X, ey2 = e2.Y;

				var intx2 = ex1 * ex1 + ex2 * ex1 + ex2 * ex2;
				var inty2 = ey1 * ey1 + ey2 * ey1 + ey2 * ey2;

				I += ( 0.25f * k_inv3 * D ) * ( intx2 + inty2 );
			}

			//The area is too small for the engine to handle.
			Debug.Assert( area > Settings.epsilon );

			// We save the area
			massData.area = area;

			// Total mass
			massData.mass = _density * area;

			// Center of mass
			center *= 1.0f / area;
			massData.centroid = center + s;

			// Inertia tensor relative to the local origin (point s).
			massData.inertia = _density * I;

			// Shift to center of mass then to original body origin.
			massData.inertia += massData.mass * ( Vector2.Dot( massData.centroid, massData.centroid ) - Vector2.Dot( center, center ) );
		}

		public override bool testPoint( ref Transform transform, ref Vector2 point )
		{
			Vector2 pLocal = MathUtils.mulT( transform.q, point - transform.p );

			for( int i = 0; i < vertices.Count; ++i )
			{
				float dot = Vector2.Dot( normals[i], pLocal - vertices[i] );
				if( dot > 0.0f )
				{
					return false;
				}
			}

			return true;
		}

		public override bool rayCast( out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex )
		{
			output = new RayCastOutput();

			// Put the ray into the polygon's frame of reference.
			Vector2 p1 = MathUtils.mulT( transform.q, input.point1 - transform.p );
			Vector2 p2 = MathUtils.mulT( transform.q, input.point2 - transform.p );
			Vector2 d = p2 - p1;

			float lower = 0.0f, upper = input.maxFraction;

			int index = -1;

			for( int i = 0; i < vertices.Count; ++i )
			{
				// p = p1 + a * d
				// dot(normal, p - v) = 0
				// dot(normal, p1 - v) + a * dot(normal, d) = 0
				float numerator = Vector2.Dot( normals[i], vertices[i] - p1 );
				float denominator = Vector2.Dot( normals[i], d );

				if( denominator == 0.0f )
				{
					if( numerator < 0.0f )
					{
						return false;
					}
				}
				else
				{
					// Note: we want this predicate without division:
					// lower < numerator / denominator, where denominator < 0
					// Since denominator < 0, we have to flip the inequality:
					// lower < numerator / denominator <==> denominator * lower > numerator.
					if( denominator < 0.0f && numerator < lower * denominator )
					{
						// Increase lower.
						// The segment enters this half-space.
						lower = numerator / denominator;
						index = i;
					}
					else if( denominator > 0.0f && numerator < upper * denominator )
					{
						// Decrease upper.
						// The segment exits this half-space.
						upper = numerator / denominator;
					}
				}

				// The use of epsilon here causes the assert on lower to trip
				// in some cases. Apparently the use of epsilon was to make edge
				// shapes work, but now those are handled separately.
				//if (upper < lower - b2_epsilon)
				if( upper < lower )
				{
					return false;
				}
			}

			Debug.Assert( 0.0f <= lower && lower <= input.maxFraction );

			if( index >= 0 )
			{
				output.fraction = lower;
				output.normal = MathUtils.mul( transform.q, normals[index] );
				return true;
			}

			return false;
		}

		/// <summary>
		/// Given a transform, compute the associated axis aligned bounding box for a child shape.
		/// </summary>
		/// <param name="aabb">The aabb results.</param>
		/// <param name="transform">The world transform of the shape.</param>
		/// <param name="childIndex">The child shape index.</param>
		public override void computeAABB( out AABB aabb, ref Transform transform, int childIndex )
		{
			var lower = MathUtils.mul( ref transform, vertices[0] );
			var upper = lower;

			for( int i = 1; i < vertices.Count; ++i )
			{
				var v = MathUtils.mul( ref transform, vertices[i] );
				lower = Vector2.Min( lower, v );
				upper = Vector2.Max( upper, v );
			}

			var r = new Vector2( radius, radius );
			aabb.lowerBound = lower - r;
			aabb.upperBound = upper + r;
		}

		public override float computeSubmergedArea( ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc )
		{
			sc = Vector2.Zero;

			//Transform plane into shape co-ordinates
			var normalL = MathUtils.mulT( xf.q, normal );
			float offsetL = offset - Vector2.Dot( normal, xf.p );

			float[] depths = new float[Settings.maxPolygonVertices];
			int diveCount = 0;
			int intoIndex = -1;
			int outoIndex = -1;

			bool lastSubmerged = false;
			int i;
			for( i = 0; i < vertices.Count; i++ )
			{
				depths[i] = Vector2.Dot( normalL, vertices[i] ) - offsetL;
				bool isSubmerged = depths[i] < -Settings.epsilon;
				if( i > 0 )
				{
					if( isSubmerged )
					{
						if( !lastSubmerged )
						{
							intoIndex = i - 1;
							diveCount++;
						}
					}
					else
					{
						if( lastSubmerged )
						{
							outoIndex = i - 1;
							diveCount++;
						}
					}
				}
				lastSubmerged = isSubmerged;
			}
			switch( diveCount )
			{
				case 0:
					if( lastSubmerged )
					{
						//Completely submerged
						sc = MathUtils.mul( ref xf, massData.centroid );
						return massData.mass / density;
					}

					//Completely dry
					return 0;
				case 1:
					if( intoIndex == -1 )
					{
						intoIndex = vertices.Count - 1;
					}
					else
					{
						outoIndex = vertices.Count - 1;
					}
					break;
			}

			int intoIndex2 = ( intoIndex + 1 ) % vertices.Count;
			int outoIndex2 = ( outoIndex + 1 ) % vertices.Count;

			float intoLambda = ( 0 - depths[intoIndex] ) / ( depths[intoIndex2] - depths[intoIndex] );
			float outoLambda = ( 0 - depths[outoIndex] ) / ( depths[outoIndex2] - depths[outoIndex] );

			Vector2 intoVec = new Vector2( vertices[intoIndex].X * ( 1 - intoLambda ) + vertices[intoIndex2].X * intoLambda, vertices[intoIndex].Y * ( 1 - intoLambda ) + vertices[intoIndex2].Y * intoLambda );
			Vector2 outoVec = new Vector2( vertices[outoIndex].X * ( 1 - outoLambda ) + vertices[outoIndex2].X * outoLambda, vertices[outoIndex].Y * ( 1 - outoLambda ) + vertices[outoIndex2].Y * outoLambda );

			//Initialize accumulator
			float area = 0;
			Vector2 center = new Vector2( 0, 0 );
			Vector2 p2 = vertices[intoIndex2];

			const float k_inv3 = 1.0f / 3.0f;

			//An awkward loop from intoIndex2+1 to outIndex2
			i = intoIndex2;
			while( i != outoIndex2 )
			{
				i = ( i + 1 ) % vertices.Count;
				Vector2 p3;
				if( i == outoIndex2 )
					p3 = outoVec;
				else
					p3 = vertices[i];
				//Add the triangle formed by intoVec,p2,p3
				{
					Vector2 e1 = p2 - intoVec;
					Vector2 e2 = p3 - intoVec;

					float D = MathUtils.cross( e1, e2 );

					float triangleArea = 0.5f * D;

					area += triangleArea;

					// Area weighted centroid
					center += triangleArea * k_inv3 * ( intoVec + p2 + p3 );
				}

				p2 = p3;
			}

			//Normalize and transform centroid
			center *= 1.0f / area;

			sc = MathUtils.mul( ref xf, center );

			return area;
		}

		public bool CompareTo( PolygonShape shape )
		{
			if( vertices.Count != shape.vertices.Count )
				return false;

			for( int i = 0; i < vertices.Count; i++ )
			{
				if( vertices[i] != shape.vertices[i] )
					return false;
			}

			return ( radius == shape.radius && massData == shape.massData );
		}

		public override Shape clone()
		{
			var clone = new PolygonShape();
			clone.shapeType = shapeType;
			clone._radius = _radius;
			clone._density = _density;
			clone._vertices = new Vertices( _vertices );
			clone._normals = new Vertices( _normals );
			clone.massData = massData;
			return clone;
		}

	}
}