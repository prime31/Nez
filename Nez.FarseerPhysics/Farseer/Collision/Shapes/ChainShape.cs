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
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Collision.Shapes
{
	/// <summary>
	/// A chain shape is a free form sequence of line segments.
	/// The chain has two-sided collision, so you can use inside and outside collision.
	/// Therefore, you may use any winding order.
	/// Connectivity information is used to create smooth collisions.
	/// WARNING: The chain will not collide properly if there are self-intersections.
	/// </summary>
	public class ChainShape : Shape
	{
		/// <summary>
		/// The vertices. These are not owned/freed by the chain Shape.
		/// </summary>
		public Vertices vertices;

		public override int childCount
		{
			// edge count = vertex count - 1
			get { return vertices.Count - 1; }
		}

		/// <summary>
		/// Establish connectivity to a vertex that precedes the first vertex.
		/// Don't call this for loops.
		/// </summary>
		public Vector2 prevVertex
		{
			get { return _prevVertex; }
			set
			{
				_prevVertex = value;
				_hasPrevVertex = true;
			}
		}

		/// <summary>
		/// Establish connectivity to a vertex that follows the last vertex.
		/// Don't call this for loops.
		/// </summary>
		public Vector2 nextVertex
		{
			get { return _nextVertex; }
			set
			{
				_nextVertex = value;
				_hasNextVertex = true;
			}
		}

		Vector2 _prevVertex, _nextVertex;
		bool _hasPrevVertex, _hasNextVertex;
		static EdgeShape _edgeShape = new EdgeShape();


		/// <summary>
		/// Constructor for ChainShape. By default have 0 in density.
		/// </summary>
		public ChainShape() : base( 0 )
		{
			shapeType = ShapeType.Chain;
			_radius = Settings.polygonRadius;
		}

		/// <summary>
		/// Create a new chainshape from the vertices.
		/// </summary>
		/// <param name="vertices">The vertices to use. Must contain 2 or more vertices.</param>
		/// <param name="createLoop">Set to true to create a closed loop. It connects the first vertice to the last, and automatically adjusts connectivity to create smooth collisions along the chain.</param>
		public ChainShape( Vertices vertices, bool createLoop = false ) : base( 0 )
		{
			shapeType = ShapeType.Chain;
			_radius = Settings.polygonRadius;

			setVertices( vertices, createLoop );
		}

		/// <summary>
		/// This method has been optimized to reduce garbage.
		/// </summary>
		/// <param name="edge">The cached edge to set properties on.</param>
		/// <param name="index">The index.</param>
		internal void getChildEdge( EdgeShape edge, int index )
		{
			Debug.Assert( 0 <= index && index < vertices.Count - 1 );
			Debug.Assert( edge != null );

			edge.shapeType = ShapeType.Edge;
			edge._radius = _radius;

			edge.vertex1 = vertices[index + 0];
			edge.vertex2 = vertices[index + 1];

			if( index > 0 )
			{
				edge.vertex0 = vertices[index - 1];
				edge.hasVertex0 = true;
			}
			else
			{
				edge.vertex0 = _prevVertex;
				edge.hasVertex0 = _hasPrevVertex;
			}

			if( index < vertices.Count - 2 )
			{
				edge.vertex3 = vertices[index + 2];
				edge.hasVertex3 = true;
			}
			else
			{
				edge.vertex3 = _nextVertex;
				edge.hasVertex3 = _hasNextVertex;
			}
		}

		/// <summary>
		/// Get a child edge.
		/// </summary>
		/// <param name="index">The index.</param>
		public EdgeShape getChildEdge( int index )
		{
			var edgeShape = new EdgeShape();
			getChildEdge( edgeShape, index );
			return edgeShape;
		}

		public void setVertices( Vertices vertices, bool createLoop = false )
		{
			Debug.Assert( vertices != null && vertices.Count >= 3 );
			Debug.Assert( vertices[0] != vertices[vertices.Count - 1] ); // FPE. See http://www.box2d.org/forum/viewtopic.php?f=4&t=7973&p=35363

			for( int i = 1; i < vertices.Count; ++i )
			{
				var v1 = vertices[i - 1];
				var v2 = vertices[i];

				// If the code crashes here, it means your vertices are too close together.
				Debug.Assert( Vector2.DistanceSquared( v1, v2 ) > Settings.linearSlop * Settings.linearSlop );
			}

			this.vertices = vertices;

			if( createLoop )
			{
				this.vertices.Add( vertices[0] );
				prevVertex = this.vertices[this.vertices.Count - 2]; // FPE: We use the properties instead of the private fields here.
				nextVertex = this.vertices[1]; // FPE: We use the properties instead of the private fields here.
			}
		}

		public override bool testPoint( ref Transform transform, ref Vector2 point )
		{
			return false;
		}

		public override bool rayCast( out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex )
		{
			Debug.Assert( childIndex < vertices.Count );

			int i1 = childIndex;
			int i2 = childIndex + 1;
			if( i2 == vertices.Count )
				i2 = 0;

			_edgeShape.vertex1 = vertices[i1];
			_edgeShape.vertex2 = vertices[i2];

			return _edgeShape.rayCast( out output, ref input, ref transform, 0 );
		}

		public override void computeAABB( out AABB aabb, ref Transform transform, int childIndex )
		{
			Debug.Assert( childIndex < vertices.Count );

			int i1 = childIndex;
			int i2 = childIndex + 1;
			if( i2 == vertices.Count )
				i2 = 0;

			var v1 = MathUtils.mul( ref transform, vertices[i1] );
			var v2 = MathUtils.mul( ref transform, vertices[i2] );

			aabb.lowerBound = Vector2.Min( v1, v2 );
			aabb.upperBound = Vector2.Max( v1, v2 );
		}

		protected override void computeProperties()
		{
			//Does nothing. Chain shapes don't have properties.
		}

		public override float computeSubmergedArea( ref Vector2 normal, float offset, ref Transform xf, out Vector2 sc )
		{
			sc = Vector2.Zero;
			return 0;
		}

		/// <summary>
		/// Compare the chain to another chain
		/// </summary>
		/// <param name="shape">The other chain</param>
		/// <returns>True if the two chain shapes are the same</returns>
		public bool CompareTo( ChainShape shape )
		{
			if( vertices.Count != shape.vertices.Count )
				return false;

			for( int i = 0; i < vertices.Count; i++ )
			{
				if( vertices[i] != shape.vertices[i] )
					return false;
			}

			return prevVertex == shape.prevVertex && nextVertex == shape.nextVertex;
		}

		public override Shape clone()
		{
			var clone = new ChainShape();
			clone.shapeType = shapeType;
			clone._density = _density;
			clone._radius = _radius;
			clone.prevVertex = _prevVertex;
			clone.nextVertex = _nextVertex;
			clone._hasNextVertex = _hasNextVertex;
			clone._hasPrevVertex = _hasPrevVertex;
			clone.vertices = new Vertices( vertices );
			clone.massData = massData;
			return clone;
		}
	
	}
}