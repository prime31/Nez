using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Common.PolygonManipulation
{
	public static class CuttingTools
	{
		//Cutting a shape into two is based on the work of Daid and his prototype BoxCutter: http://www.box2d.org/forum/viewtopic.php?f=3&t=1473

		/// <summary>
		/// Split a fixture into 2 vertice collections using the given entry and exit-point.
		/// </summary>
		/// <param name="fixture">The Fixture to split</param>
		/// <param name="entryPoint">The entry point - The start point</param>
		/// <param name="exitPoint">The exit point - The end point</param>
		/// <param name="first">The first collection of vertexes</param>
		/// <param name="second">The second collection of vertexes</param>
		public static void splitShape( Fixture fixture, Vector2 entryPoint, Vector2 exitPoint, out Vertices first, out Vertices second )
		{
			Vector2 localEntryPoint = fixture.body.getLocalPoint( ref entryPoint );
			Vector2 localExitPoint = fixture.body.getLocalPoint( ref exitPoint );

			PolygonShape shape = fixture.shape as PolygonShape;

			//We can only cut polygons at the moment
			if( shape == null )
			{
				first = new Vertices();
				second = new Vertices();
				return;
			}

			//Offset the entry and exit points if they are too close to the vertices
			foreach( Vector2 vertex in shape.vertices )
			{
				if( vertex.Equals( localEntryPoint ) )
					localEntryPoint -= new Vector2( 0, Settings.epsilon );

				if( vertex.Equals( localExitPoint ) )
					localExitPoint += new Vector2( 0, Settings.epsilon );
			}

			Vertices vertices = new Vertices( shape.vertices );
			Vertices[] newPolygon = new Vertices[2];

			for( int i = 0; i < newPolygon.Length; i++ )
			{
				newPolygon[i] = new Vertices( vertices.Count );
			}

			int[] cutAdded = { -1, -1 };
			int last = -1;
			for( int i = 0; i < vertices.Count; i++ )
			{
				int n;
				//Find out if this vertex is on the old or new shape.
				if( Vector2.Dot( MathUtils.cross( localExitPoint - localEntryPoint, 1 ), vertices[i] - localEntryPoint ) > Settings.epsilon )
					n = 0;
				else
					n = 1;

				if( last != n )
				{
					//If we switch from one shape to the other add the cut vertices.
					if( last == 0 )
					{
						Debug.Assert( cutAdded[0] == -1 );
						cutAdded[0] = newPolygon[last].Count;
						newPolygon[last].Add( localExitPoint );
						newPolygon[last].Add( localEntryPoint );
					}
					if( last == 1 )
					{
						Debug.Assert( cutAdded[last] == -1 );
						cutAdded[last] = newPolygon[last].Count;
						newPolygon[last].Add( localEntryPoint );
						newPolygon[last].Add( localExitPoint );
					}
				}

				newPolygon[n].Add( vertices[i] );
				last = n;
			}

			//Add the cut in case it has not been added yet.
			if( cutAdded[0] == -1 )
			{
				cutAdded[0] = newPolygon[0].Count;
				newPolygon[0].Add( localExitPoint );
				newPolygon[0].Add( localEntryPoint );
			}
			if( cutAdded[1] == -1 )
			{
				cutAdded[1] = newPolygon[1].Count;
				newPolygon[1].Add( localEntryPoint );
				newPolygon[1].Add( localExitPoint );
			}

			for( int n = 0; n < 2; n++ )
			{
				Vector2 offset;
				if( cutAdded[n] > 0 )
					offset = ( newPolygon[n][cutAdded[n] - 1] - newPolygon[n][cutAdded[n]] );
				else
					offset = ( newPolygon[n][newPolygon[n].Count - 1] - newPolygon[n][0] );
				Nez.Vector2Ext.normalize( ref offset );

				if( !offset.isValid() )
					offset = Vector2.One;

				newPolygon[n][cutAdded[n]] += Settings.epsilon * offset;

				if( cutAdded[n] < newPolygon[n].Count - 2 )
					offset = ( newPolygon[n][cutAdded[n] + 2] - newPolygon[n][cutAdded[n] + 1] );
				else
					offset = ( newPolygon[n][0] - newPolygon[n][newPolygon[n].Count - 1] );
				Nez.Vector2Ext.normalize( ref offset );

				if( !offset.isValid() )
					offset = Vector2.One;

				newPolygon[n][cutAdded[n] + 1] += Settings.epsilon * offset;
			}

			first = newPolygon[0];
			second = newPolygon[1];
		}


		/// <summary>
		/// This is a high-level function to cuts fixtures inside the given world, using the start and end points.
		/// Note: We don't support cutting when the start or end is inside a shape.
		/// </summary>
		/// <param name="world">The world.</param>
		/// <param name="start">The startpoint.</param>
		/// <param name="end">The endpoint.</param>
		/// <returns>True if the cut was performed.</returns>
		public static bool cut( World world, Vector2 start, Vector2 end )
		{
			var fixtures = new List<Fixture>();
			var entryPoints = new List<Vector2>();
			var exitPoints = new List<Vector2>();

			//We don't support cutting when the start or end is inside a shape.
			if( world.testPoint( start ) != null || world.testPoint( end ) != null )
				return false;

			//Get the entry points
			world.rayCast( ( f, p, n, fr ) =>
							   {
								   fixtures.Add( f );
								   entryPoints.Add( p );
								   return 1;
							   }, start, end );

			//Reverse the ray to get the exitpoints
			world.rayCast( ( f, p, n, fr ) =>
							   {
								   exitPoints.Add( p );
								   return 1;
							   }, end, start );

			//We only have a single point. We need at least 2
			if( entryPoints.Count + exitPoints.Count < 2 )
				return false;

			for( int i = 0; i < fixtures.Count; i++ )
			{
				// can't cut circles or edges yet !
				if( fixtures[i].shape.shapeType != ShapeType.Polygon )
					continue;

				if( fixtures[i].body.bodyType != BodyType.Static )
				{
					//Split the shape up into two shapes
					Vertices first;
					Vertices second;
					splitShape( fixtures[i], entryPoints[i], exitPoints[i], out first, out second );

					//Delete the original shape and create two new. Retain the properties of the body.
					if( first.checkPolygon() == PolygonError.NoError )
					{
						Body firstFixture = BodyFactory.createPolygon( world, first, fixtures[i].shape.density, fixtures[i].body.position );
						firstFixture.rotation = fixtures[i].body.rotation;
						firstFixture.linearVelocity = fixtures[i].body.linearVelocity;
						firstFixture.angularVelocity = fixtures[i].body.angularVelocity;
						firstFixture.bodyType = BodyType.Dynamic;
					}

					if( second.checkPolygon() == PolygonError.NoError )
					{
						Body secondFixture = BodyFactory.createPolygon( world, second, fixtures[i].shape.density, fixtures[i].body.position );
						secondFixture.rotation = fixtures[i].body.rotation;
						secondFixture.linearVelocity = fixtures[i].body.linearVelocity;
						secondFixture.angularVelocity = fixtures[i].body.angularVelocity;
						secondFixture.bodyType = BodyType.Dynamic;
					}

					world.removeBody( fixtures[i].body );
				}
			}

			return true;
		}
	
	}
}