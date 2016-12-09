using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Factories
{
	/// <summary>
	/// An easy to use factory for creating bodies
	/// </summary>
	public static class FixtureFactory
	{
		public static Fixture attachEdge( Vector2 start, Vector2 end, Body body, object userData = null )
		{
			var edgeShape = new EdgeShape( start, end );
			return body.createFixture( edgeShape, userData );
		}

		public static Fixture attachChainShape( Vertices vertices, Body body, object userData = null )
		{
			var shape = new ChainShape( vertices );
			return body.createFixture( shape, userData );
		}

		public static Fixture attachLoopShape( Vertices vertices, Body body, object userData = null )
		{
			var shape = new ChainShape( vertices, true );
			return body.createFixture( shape, userData );
		}

		public static Fixture attachRectangle( float width, float height, float density, Vector2 offset, Body body, object userData = null )
		{
			var rectangleVertices = PolygonTools.createRectangle( width / 2, height / 2 );
			rectangleVertices.translate( ref offset );
			var rectangleShape = new PolygonShape( rectangleVertices, density );
			return body.createFixture( rectangleShape, userData );
		}

		public static List<Fixture> attachRoundedRectangle( float width, float height, float xRadius, float yRadius, int segments, float density, Body body, object userData = null )
		{
			var verts = PolygonTools.createRoundedRectangle( width, height, xRadius, yRadius, segments );

			//There are too many vertices in the capsule. We decompose it.
			if( verts.Count >= Settings.maxPolygonVertices )
			{
				var vertList = Triangulate.convexPartition( verts, TriangulationAlgorithm.Earclip );
				return attachCompoundPolygon( vertList, density, body, userData );
			}

			var fixtures = new List<Fixture>( 1 );
			fixtures.Add( attachPolygon( verts, density, body, userData ) );
			return fixtures;
		}

		public static Fixture attachCircle( float radius, float density, Body body, object userData = null )
		{
			if( radius <= 0 )
				throw new ArgumentOutOfRangeException( nameof( radius ), "Radius must be more than 0 meters" );

			var circleShape = new CircleShape( radius, density );
			return body.createFixture( circleShape, userData );
		}

		public static Fixture attachCircle( float radius, float density, Body body, Vector2 offset, object userData = null )
		{
			if( radius <= 0 )
				throw new ArgumentOutOfRangeException( nameof( radius ), "Radius must be more than 0 meters" );

			var circleShape = new CircleShape( radius, density );
			circleShape.position = offset;
			return body.createFixture( circleShape, userData );
		}

		public static Fixture attachPolygon( Vertices vertices, float density, Body body, object userData = null )
		{
			if( vertices.Count <= 1 )
				throw new ArgumentOutOfRangeException( nameof( vertices ), "Too few points to be a polygon" );

			var polygon = new PolygonShape( vertices, density );
			return body.createFixture( polygon, userData );
		}

		public static Fixture attachEllipse( float xRadius, float yRadius, int edges, float density, Body body, object userData = null )
		{
			if( xRadius <= 0 )
				throw new ArgumentOutOfRangeException( nameof( xRadius ), "X-radius must be more than 0" );

			if( yRadius <= 0 )
				throw new ArgumentOutOfRangeException( nameof( yRadius ), "Y-radius must be more than 0" );

			var ellipseVertices = PolygonTools.createEllipse( xRadius, yRadius, edges );
			var polygonShape = new PolygonShape( ellipseVertices, density );
			return body.createFixture( polygonShape, userData );
		}

		public static List<Fixture> attachCompoundPolygon( List<Vertices> list, float density, Body body, object userData = null )
		{
			var res = new List<Fixture>( list.Count );

			// Then we create several fixtures using the body
			foreach( var vertices in list )
			{
				if( vertices.Count == 2 )
				{
					var shape = new EdgeShape( vertices[0], vertices[1] );
					res.Add( body.createFixture( shape, userData ) );
				}
				else
				{
					var shape = new PolygonShape( vertices, density );
					res.Add( body.createFixture( shape, userData ) );
				}
			}

			return res;
		}

		public static Fixture attachLineArc( float radians, int sides, float radius, bool closed, Body body )
		{
			var arc = PolygonTools.createArc( radians, sides, radius );
			arc.rotate( ( MathHelper.Pi - radians ) / 2 );
			return closed ? attachLoopShape( arc, body ) : attachChainShape( arc, body );
		}

		public static List<Fixture> attachSolidArc( float density, float radians, int sides, float radius, Body body )
		{
			var arc = PolygonTools.createArc( radians, sides, radius );
			arc.rotate( ( MathHelper.Pi - radians ) / 2 );

			// Close the arc
			arc.Add( arc[0] );

			var triangles = Triangulate.convexPartition( arc, TriangulationAlgorithm.Earclip );

			return attachCompoundPolygon( triangles, density, body );
		}
	
	}
}