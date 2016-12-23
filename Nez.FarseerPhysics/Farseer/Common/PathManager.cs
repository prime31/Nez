using System;
using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Common
{
	/// <summary>
	/// An easy to use manager for creating paths.
	/// </summary>
	public static class PathManager
	{
		public enum LinkType
		{
			Revolute,
			Slider
		}

		//Contributed by Matthew Bettcher

		/// <summary>
		/// Convert a path into a set of edges and attaches them to the specified body.
		/// Note: use only for static edges.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="body">The body.</param>
		/// <param name="subdivisions">The subdivisions.</param>
		public static void convertPathToEdges( Path path, Body body, int subdivisions )
		{
			var verts = path.getVertices( subdivisions );
			if( path.isClosed )
			{
				var chain = new ChainShape( verts, true );
				body.createFixture( chain );
			}
			else
			{
				for( int i = 1; i < verts.Count; i++ )
				{
					body.createFixture( new EdgeShape( verts[i], verts[i - 1] ) );
				}
			}
		}

		/// <summary>
		/// Convert a closed path into a polygon.
		/// Convex decomposition is automatically performed.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="body">The body.</param>
		/// <param name="density">The density.</param>
		/// <param name="subdivisions">The subdivisions.</param>
		public static void convertPathToPolygon( Path path, Body body, float density, int subdivisions )
		{
			if( !path.isClosed )
				throw new Exception( "The path must be closed to convert to a polygon." );

			var verts = path.getVertices( subdivisions );
			var decomposedVerts = Triangulate.convexPartition( new Vertices( verts ), TriangulationAlgorithm.Bayazit );

			foreach( Vertices item in decomposedVerts )
			{
				body.createFixture( new PolygonShape( item, density ) );
			}
		}

		/// <summary>
		/// Duplicates the given Body along the given path for approximatly the given copies.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="path">The path.</param>
		/// <param name="shapes">The shapes.</param>
		/// <param name="type">The type.</param>
		/// <param name="copies">The copies.</param>
		/// <param name="userData"></param>
		/// <returns></returns>
		public static List<Body> evenlyDistributeShapesAlongPath( World world, Path path, IEnumerable<Shape> shapes, BodyType type, int copies, object userData = null )
		{
			List<Vector3> centers = path.subdivideEvenly( copies );
			List<Body> bodyList = new List<Body>();

			for( int i = 0; i < centers.Count; i++ )
			{
				Body b = new Body( world );

				// copy the type from original body
				b.bodyType = type;
				b.position = new Vector2( centers[i].X, centers[i].Y );
				b.rotation = centers[i].Z;
				b.userData = userData;

				foreach( Shape shape in shapes )
				{
					b.createFixture( shape );
				}

				bodyList.Add( b );
			}

			return bodyList;
		}


		/// <summary>
		/// Duplicates the given Body along the given path for approximatly the given copies.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="path">The path.</param>
		/// <param name="shape">The shape.</param>
		/// <param name="type">The type.</param>
		/// <param name="copies">The copies.</param>
		/// <param name="userData">The user data.</param>
		/// <returns></returns>
		public static List<Body> evenlyDistributeShapesAlongPath( World world, Path path, Shape shape, BodyType type,
																 int copies, object userData )
		{
			var shapes = new List<Shape>( 1 );
			shapes.Add( shape );

			return evenlyDistributeShapesAlongPath( world, path, shapes, type, copies, userData );
		}

		public static List<Body> evenlyDistributeShapesAlongPath( World world, Path path, Shape shape, BodyType type, int copies )
		{
			return evenlyDistributeShapesAlongPath( world, path, shape, type, copies, null );
		}

		/// <summary>
		/// Moves the given body along the defined path.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="body">The body.</param>
		/// <param name="time">The time.</param>
		/// <param name="strength">The strength.</param>
		/// <param name="timeStep">The time step.</param>
		public static void moveBodyOnPath( Path path, Body body, float time, float strength, float timeStep )
		{
			var destination = path.getPosition( time );
			var positionDelta = body.position - destination;
			var velocity = ( positionDelta / timeStep ) * strength;

			body.linearVelocity = -velocity;
		}

		/// <summary>
		/// Attaches the bodies with revolute joints.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="bodies">The bodies.</param>
		/// <param name="localAnchorA">The local anchor A.</param>
		/// <param name="localAnchorB">The local anchor B.</param>
		/// <param name="connectFirstAndLast">if set to <c>true</c> [connect first and last].</param>
		/// <param name="collideConnected">if set to <c>true</c> [collide connected].</param>
		public static List<RevoluteJoint> attachBodiesWithRevoluteJoint( World world, List<Body> bodies, Vector2 localAnchorA, Vector2 localAnchorB, bool connectFirstAndLast, bool collideConnected )
		{
			var joints = new List<RevoluteJoint>( bodies.Count + 1 );

			for( int i = 1; i < bodies.Count; i++ )
			{
				RevoluteJoint joint = new RevoluteJoint( bodies[i], bodies[i - 1], localAnchorA, localAnchorB );
				joint.collideConnected = collideConnected;
				world.addJoint( joint );
				joints.Add( joint );
			}

			if( connectFirstAndLast )
			{
				RevoluteJoint lastjoint = new RevoluteJoint( bodies[0], bodies[bodies.Count - 1], localAnchorA, localAnchorB );
				lastjoint.collideConnected = collideConnected;
				world.addJoint( lastjoint );
				joints.Add( lastjoint );
			}

			return joints;
		}
	
	}
}