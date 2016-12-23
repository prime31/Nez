using System.Collections.Generic;
using FarseerPhysics.Common;
using FarseerPhysics.Common.Decomposition;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using FarseerPhysics.Factories;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public static class BodyExt
	{
		#region Fixtures/Shapes

		public static Fixture attachEdge( this Body body, Vector2 start, Vector2 end )
		{
			return FixtureFactory.attachEdge( FSConvert.displayToSim * start, FSConvert.displayToSim * end, body );
		}


		public static Fixture attachChainShape( this Body body, List<Vector2> vertices )
		{
			for( var i = 0; i < vertices.Count; i++ )
				vertices[i] = FSConvert.toSimUnits( vertices[i] );

			return FixtureFactory.attachChainShape( new Vertices( vertices ), body, null );
		}


		public static Fixture attachLoopShape( this Body body, List<Vector2> vertices )
		{
			for( var i = 0; i < vertices.Count; i++ )
				vertices[i] = FSConvert.toSimUnits( vertices[i] );

			return FixtureFactory.attachLoopShape( new Vertices( vertices ), body, null );
		}


		public static Fixture attachCircle( this Body body, float radius, float density, Vector2 offset = default( Vector2 ) )
		{
			return FixtureFactory.attachCircle( FSConvert.displayToSim * radius, density, body, offset * FSConvert.displayToSim, null );
		}


		public static Fixture attachRectangle( this Body body, float width, float height, float density, Vector2 offset = default( Vector2 ) )
		{
			return FixtureFactory.attachRectangle( FSConvert.displayToSim * width, FSConvert.displayToSim * height, density, FSConvert.displayToSim * offset, body );
		}


		public static List<Fixture> attachRoundedRectangle( this Body body, float width, float height, float xRadius, float yRadius, int segments, float density, Vector2 position = new Vector2() )
		{
			width *= FSConvert.displayToSim;
			height *= FSConvert.displayToSim;
			xRadius *= FSConvert.displayToSim;
			yRadius *= FSConvert.displayToSim;
			position *= FSConvert.displayToSim;

			return FixtureFactory.attachRoundedRectangle( width, height, xRadius, yRadius, segments, density, body );
		}


		public static Fixture attachPolygon( this Body body, List<Vector2> vertices, float density )
		{
			for( var i = 0; i < vertices.Count; i++ )
				vertices[i] = FSConvert.displayToSim * vertices[i];

			return FixtureFactory.attachPolygon( new Vertices( vertices ), density, body );
		}


		public static Fixture attachEllipse( this Body body, float xRadius, float yRadius, int edges, float density )
		{
			return FixtureFactory.attachEllipse( FSConvert.displayToSim * xRadius, FSConvert.displayToSim * yRadius, edges, density, body );
		}


		public static List<Fixture> attachCompoundPolygon( this Body body, List<Vertices> list, float density )
		{
			for( var i = 0; i < list.Count; i++ )
			{
				var vertices = list[i];
				for( var j = 0; j < vertices.Count; j++ )
					vertices[j] = FSConvert.displayToSim * vertices[j];

			}

			return FixtureFactory.attachCompoundPolygon( list, density, body );
		}


		public static Fixture attachLineArc( this Body body, float radians, int sides, float radius, bool closed )
		{
			return FixtureFactory.attachLineArc( radians, sides, FSConvert.displayToSim * radius, closed, body );
		}


		public static List<Fixture> attachSolidArc( this Body body, float density, float radians, int sides, float radius )
		{
			return FixtureFactory.attachSolidArc( density, radians, sides, FSConvert.displayToSim * radius, body );
		}


		public static List<Fixture> attachGear( this Body body, float radius, int numberOfTeeth, float tipPercentage, float toothHeight, float density )
		{
			var gearPolygon = PolygonTools.createGear( FSConvert.displayToSim * radius, numberOfTeeth, tipPercentage, FSConvert.displayToSim * toothHeight );

			// Gears can in some cases be convex
			if( !gearPolygon.isConvex() )
			{
				//Decompose the gear:
				var list = Triangulate.convexPartition( gearPolygon, TriangulationAlgorithm.Earclip );
				return body.attachCompoundPolygon( list, density );
			}

			var fixtures = new List<Fixture>();
			fixtures.Add( body.attachPolygon( gearPolygon, density ) );
			return fixtures;
		}


		public static void attachCapsule( this Body body, float height, float endRadius, float density )
		{
			// Create the middle rectangle
			body.attachRectangle( endRadius, height / 2, density );

			// create the two circles
			body.attachCircle( endRadius, density, new Vector2( 0, height / 2 ) );
			body.attachCircle( endRadius, density, new Vector2( 0, -height / 2 ) );
		}

		#endregion


		#region Joints

		public static MotorJoint createMotorJoint( this Body body, Body bodyB, bool useWorldCoordinates = false )
		{
			return JointFactory.createMotorJoint( body.world, body, bodyB, useWorldCoordinates );
		}


		public static RevoluteJoint createRevoluteJoint( this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
		{
			return JointFactory.createRevoluteJoint( body.world, body, bodyB, FSConvert.displayToSim * anchorA, FSConvert.displayToSim * anchorB, useWorldCoordinates );
		}


		public static RevoluteJoint createRevoluteJoint( this Body body, Body bodyB, Vector2 anchor )
		{
			return JointFactory.createRevoluteJoint( body.world, body, bodyB, FSConvert.displayToSim * anchor );
		}


		public static RopeJoint createRopeJoint( this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
		{
			return JointFactory.createRopeJoint( body.world, body, bodyB, FSConvert.displayToSim * anchorA, FSConvert.displayToSim * anchorB, useWorldCoordinates );
		}


		public static WeldJoint createWeldJoint( this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
		{
			return JointFactory.createWeldJoint( body.world, body, bodyB, FSConvert.displayToSim * anchorA, FSConvert.displayToSim * anchorB, useWorldCoordinates );
		}


		public static PrismaticJoint createPrismaticJoint( this Body body, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false )
		{
			return JointFactory.createPrismaticJoint( body.world, body, bodyB, FSConvert.displayToSim * anchor, axis, useWorldCoordinates );
		}


		public static WheelJoint createWheelJoint( this Body body, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false )
		{
			return JointFactory.createWheelJoint( body.world, body, bodyB, FSConvert.displayToSim * anchor, axis, useWorldCoordinates );
		}


		public static WheelJoint createWheelJoint( this Body body, Body bodyB, Vector2 axis )
		{
			return createWheelJoint( body, bodyB, Vector2.Zero, axis );
		}


		public static AngleJoint createAngleJoint( this Body body, Body bodyB )
		{
			return JointFactory.createAngleJoint( body.world, body, bodyB );
		}


		public static DistanceJoint createDistanceJoint( this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
		{
			return JointFactory.createDistanceJoint( body.world, body, bodyB, FSConvert.displayToSim * anchorA, FSConvert.displayToSim * anchorB, useWorldCoordinates );
		}


		public static DistanceJoint createDistanceJoint( this Body body, Body bodyB )
		{
			return createDistanceJoint( body, bodyB, Vector2.Zero, Vector2.Zero );
		}


		public static FrictionJoint createFrictionJoint( this Body body, Body bodyB, Vector2 anchor, bool useWorldCoordinates = false )
		{
			return JointFactory.createFrictionJoint( body.world, body, bodyB, FSConvert.displayToSim * anchor, useWorldCoordinates );
		}


		public static FrictionJoint createFrictionJoint( this Body body, Body bodyB )
		{
			return createFrictionJoint( body, bodyB, Vector2.Zero );
		}


		public static GearJoint createGearJoint( this Body body, Body bodyB, Joint jointA, Joint jointB, float ratio )
		{
			return JointFactory.createGearJoint( body.world, body, bodyB, jointA, jointB, ratio );
		}


		public static PulleyJoint createPulleyJoint( this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB, Vector2 worldAnchorA, Vector2 worldAnchorB, float ratio, bool useWorldCoordinates = false )
		{
			return JointFactory.createPulleyJoint( body.world, body, bodyB, FSConvert.displayToSim * anchorA, FSConvert.displayToSim * anchorB, FSConvert.displayToSim * worldAnchorA, FSConvert.displayToSim * worldAnchorB, ratio, useWorldCoordinates );
		}


		public static FixedMouseJoint createFixedMouseJoint( this Body body, Vector2 worldAnchor )
		{
			return JointFactory.createFixedMouseJoint( body.world, body, FSConvert.displayToSim * worldAnchor );
		}

		#endregion

	}
}
