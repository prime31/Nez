using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Factories
{
	public static class BodyFactory
	{
		public static Body createBody( World world, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			return new Body( world, position, rotation, bodyType, userData );
		}

		public static Body createEdge( World world, Vector2 start, Vector2 end, object userData = null )
		{
			var body = createBody( world );
			FixtureFactory.attachEdge( start, end, body, userData );
			return body;
		}

		public static Body createChainShape( World world, Vertices vertices, Vector2 position = new Vector2(), object userData = null )
		{
			var body = createBody( world, position );
			FixtureFactory.attachChainShape( vertices, body, userData );
			return body;
		}

		public static Body createLoopShape( World world, Vertices vertices, Vector2 position = new Vector2(), object userData = null )
		{
			var body = createBody( world, position );
			FixtureFactory.attachLoopShape( vertices, body, userData );
			return body;
		}

		public static Body createRectangle( World world, float width, float height, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			if( width <= 0 )
				throw new ArgumentOutOfRangeException( nameof( width ), "Width must be more than 0 meters" );

			if( height <= 0 )
				throw new ArgumentOutOfRangeException( nameof( height ), "Height must be more than 0 meters" );

			var newBody = createBody( world, position, rotation, bodyType );
			newBody.userData = userData;

			var rectangleVertices = PolygonTools.createRectangle( width / 2, height / 2 );
			var rectangleShape = new PolygonShape( rectangleVertices, density );
			newBody.createFixture( rectangleShape );

			return newBody;
		}

		public static Body createCircle( World world, float radius, float density, Vector2 position = new Vector2(), BodyType bodyType = BodyType.Static, object userData = null )
		{
			var body = createBody( world, position, 0, bodyType );
			FixtureFactory.attachCircle( radius, density, body, userData );
			return body;
		}

		public static Body createEllipse( World world, float xRadius, float yRadius, int edges, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			var body = createBody( world, position, rotation, bodyType );
			FixtureFactory.attachEllipse( xRadius, yRadius, edges, density, body, userData );
			return body;
		}

		public static Body createPolygon( World world, Vertices vertices, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			var body = createBody( world, position, rotation, bodyType );
			FixtureFactory.attachPolygon( vertices, density, body, userData );
			return body;
		}

		public static Body createCompoundPolygon( World world, List<Vertices> list, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			//We create a single body
			var polygonBody = createBody( world, position, rotation, bodyType );
			FixtureFactory.attachCompoundPolygon( list, density, polygonBody, userData );
			return polygonBody;
		}

		public static Body createGear( World world, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			var gearPolygon = PolygonTools.createGear( radius, numberOfTeeth, tipPercentage, toothHeight );

			//Gears can in some cases be convex
			if( !gearPolygon.isConvex() )
			{
				//Decompose the gear:
				var list = Triangulate.convexPartition( gearPolygon, TriangulationAlgorithm.Earclip );
				return createCompoundPolygon( world, list, density, position, rotation, bodyType, userData );
			}

			return createPolygon( world, gearPolygon, density, position, rotation, bodyType, userData );
		}

		public static Body createCapsule( World world, float height, float topRadius, int topEdges, float bottomRadius, int bottomEdges, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			var verts = PolygonTools.createCapsule( height, topRadius, topEdges, bottomRadius, bottomEdges );

			//There are too many vertices in the capsule. We decompose it.
			if( verts.Count >= Settings.maxPolygonVertices )
			{
				var vertList = Triangulate.convexPartition( verts, TriangulationAlgorithm.Earclip );
				return createCompoundPolygon( world, vertList, density, position, rotation, bodyType, userData );
			}

			return createPolygon( world, verts, density, position, rotation, bodyType, userData );
		}

		public static Body createCapsule( World world, float height, float endRadius, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			//Create the middle rectangle
			var rectangle = PolygonTools.createRectangle( endRadius, height / 2 );

			var list = new List<Vertices>();
			list.Add( rectangle );
			var body = createCompoundPolygon( world, list, density, position, rotation, bodyType, userData );

			//Create the two circles
			FixtureFactory.attachCircle( endRadius, density, body, new Vector2( 0, height / 2 ) );
			FixtureFactory.attachCircle( endRadius, density, body, new Vector2( 0, -( height / 2 ) ) );

			return body;
		}

		public static Body createRoundedRectangle( World world, float width, float height, float xRadius, float yRadius, int segments, float density, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userData = null )
		{
			var verts = PolygonTools.createRoundedRectangle( width, height, xRadius, yRadius, segments );

			//There are too many vertices in the rect. We decompose it.
			if( verts.Count >= Settings.maxPolygonVertices )
			{
				var vertList = Triangulate.convexPartition( verts, TriangulationAlgorithm.Earclip );
				return createCompoundPolygon( world, vertList, density, position, rotation, bodyType, userData );
			}

			return createPolygon( world, verts, density, position, rotation, bodyType, userData );
		}

		public static Body createLineArc( World world, float radians, int sides, float radius, bool closed = false, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static )
		{
			var body = createBody( world, position, rotation, bodyType );
			FixtureFactory.attachLineArc( radians, sides, radius, closed, body );
			return body;
		}

		public static Body createSolidArc( World world, float density, float radians, int sides, float radius, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static )
		{
			var body = createBody( world, position, rotation, bodyType );
			FixtureFactory.attachSolidArc( density, radians, sides, radius, body );

			return body;
		}

		public static BreakableBody createBreakableBody( World world, Vertices vertices, float density, Vector2 position = new Vector2(), float rotation = 0 )
		{
			//TODO: Implement a Voronoi diagram algorithm to split up the vertices
			var triangles = Triangulate.convexPartition( vertices, TriangulationAlgorithm.Earclip );

			var breakableBody = new BreakableBody( world, triangles, density, position, rotation );
			breakableBody.mainBody.position = position;
			world.addBreakableBody( breakableBody );
			return breakableBody;
		}

		public static BreakableBody createBreakableBody( World world, IEnumerable<Shape> shapes, Vector2 position = new Vector2(), float rotation = 0 )
		{
			var breakableBody = new BreakableBody( world, shapes, position, rotation );
			breakableBody.mainBody.position = position;
			world.addBreakableBody( breakableBody );
			return breakableBody;
		}


	}
}