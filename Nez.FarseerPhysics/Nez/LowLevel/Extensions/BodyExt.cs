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

		public static Fixture AttachEdge(this Body body, Vector2 start, Vector2 end)
		{
			return FixtureFactory.AttachEdge(FSConvert.DisplayToSim * start, FSConvert.DisplayToSim * end, body);
		}


		public static Fixture AttachChainShape(this Body body, List<Vector2> vertices)
		{
			for (var i = 0; i < vertices.Count; i++)
				vertices[i] = FSConvert.ToSimUnits(vertices[i]);

			return FixtureFactory.AttachChainShape(new Vertices(vertices), body, null);
		}


		public static Fixture AttachLoopShape(this Body body, List<Vector2> vertices)
		{
			for (var i = 0; i < vertices.Count; i++)
				vertices[i] = FSConvert.ToSimUnits(vertices[i]);

			return FixtureFactory.AttachLoopShape(new Vertices(vertices), body, null);
		}


		public static Fixture AttachCircle(this Body body, float radius, float density,
		                                   Vector2 offset = default(Vector2))
		{
			return FixtureFactory.AttachCircle(FSConvert.DisplayToSim * radius, density, body,
				offset * FSConvert.DisplayToSim, null);
		}


		public static Fixture AttachRectangle(this Body body, float width, float height, float density,
		                                      Vector2 offset = default(Vector2))
		{
			return FixtureFactory.AttachRectangle(FSConvert.DisplayToSim * width, FSConvert.DisplayToSim * height,
				density, FSConvert.DisplayToSim * offset, body);
		}


		public static List<Fixture> AttachRoundedRectangle(this Body body, float width, float height, float xRadius,
		                                                   float yRadius, int segments, float density,
		                                                   Vector2 position = new Vector2())
		{
			width *= FSConvert.DisplayToSim;
			height *= FSConvert.DisplayToSim;
			xRadius *= FSConvert.DisplayToSim;
			yRadius *= FSConvert.DisplayToSim;
			position *= FSConvert.DisplayToSim;

			return FixtureFactory.AttachRoundedRectangle(width, height, xRadius, yRadius, segments, density, body);
		}


		public static Fixture AttachPolygon(this Body body, List<Vector2> vertices, float density)
		{
			for (var i = 0; i < vertices.Count; i++)
				vertices[i] = FSConvert.DisplayToSim * vertices[i];

			return FixtureFactory.AttachPolygon(new Vertices(vertices), density, body);
		}


		public static Fixture AttachEllipse(this Body body, float xRadius, float yRadius, int edges, float density)
		{
			return FixtureFactory.AttachEllipse(FSConvert.DisplayToSim * xRadius, FSConvert.DisplayToSim * yRadius,
				edges, density, body);
		}


		public static List<Fixture> AttachCompoundPolygon(this Body body, List<Vertices> list, float density)
		{
			for (var i = 0; i < list.Count; i++)
			{
				var vertices = list[i];
				for (var j = 0; j < vertices.Count; j++)
					vertices[j] = FSConvert.DisplayToSim * vertices[j];
			}

			return FixtureFactory.AttachCompoundPolygon(list, density, body);
		}


		public static Fixture AttachLineArc(this Body body, float radians, int sides, float radius, bool closed)
		{
			return FixtureFactory.AttachLineArc(radians, sides, FSConvert.DisplayToSim * radius, closed, body);
		}


		public static List<Fixture> AttachSolidArc(this Body body, float density, float radians, int sides,
		                                           float radius)
		{
			return FixtureFactory.AttachSolidArc(density, radians, sides, FSConvert.DisplayToSim * radius, body);
		}


		public static List<Fixture> AttachGear(this Body body, float radius, int numberOfTeeth, float tipPercentage,
		                                       float toothHeight, float density)
		{
			var gearPolygon = PolygonTools.CreateGear(FSConvert.DisplayToSim * radius, numberOfTeeth, tipPercentage,
				FSConvert.DisplayToSim * toothHeight);

			// Gears can in some cases be convex
			if (!gearPolygon.IsConvex())
			{
				//Decompose the gear:
				var list = Triangulate.ConvexPartition(gearPolygon, TriangulationAlgorithm.Earclip);
				return body.AttachCompoundPolygon(list, density);
			}

			var fixtures = new List<Fixture>();
			fixtures.Add(body.AttachPolygon(gearPolygon, density));
			return fixtures;
		}


		public static void AttachCapsule(this Body body, float height, float endRadius, float density)
		{
			// Create the middle rectangle
			body.AttachRectangle(endRadius, height / 2, density);

			// create the two circles
			body.AttachCircle(endRadius, density, new Vector2(0, height / 2));
			body.AttachCircle(endRadius, density, new Vector2(0, -height / 2));
		}

		#endregion


		#region Joints

		public static MotorJoint CreateMotorJoint(this Body body, Body bodyB, bool useWorldCoordinates = false)
		{
			return JointFactory.CreateMotorJoint(body.World, body, bodyB, useWorldCoordinates);
		}


		public static RevoluteJoint CreateRevoluteJoint(this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB,
		                                                bool useWorldCoordinates = false)
		{
			return JointFactory.CreateRevoluteJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchorA,
				FSConvert.DisplayToSim * anchorB, useWorldCoordinates);
		}


		public static RevoluteJoint CreateRevoluteJoint(this Body body, Body bodyB, Vector2 anchor)
		{
			return JointFactory.CreateRevoluteJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchor);
		}


		public static RopeJoint CreateRopeJoint(this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB,
		                                        bool useWorldCoordinates = false)
		{
			return JointFactory.CreateRopeJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchorA,
				FSConvert.DisplayToSim * anchorB, useWorldCoordinates);
		}


		public static WeldJoint CreateWeldJoint(this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB,
		                                        bool useWorldCoordinates = false)
		{
			return JointFactory.CreateWeldJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchorA,
				FSConvert.DisplayToSim * anchorB, useWorldCoordinates);
		}


		public static PrismaticJoint CreatePrismaticJoint(this Body body, Body bodyB, Vector2 anchor, Vector2 axis,
		                                                  bool useWorldCoordinates = false)
		{
			return JointFactory.CreatePrismaticJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchor, axis,
				useWorldCoordinates);
		}


		public static WheelJoint CreateWheelJoint(this Body body, Body bodyB, Vector2 anchor, Vector2 axis,
		                                          bool useWorldCoordinates = false)
		{
			return JointFactory.CreateWheelJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchor, axis,
				useWorldCoordinates);
		}


		public static WheelJoint CreateWheelJoint(this Body body, Body bodyB, Vector2 axis)
		{
			return CreateWheelJoint(body, bodyB, Vector2.Zero, axis);
		}


		public static AngleJoint CreateAngleJoint(this Body body, Body bodyB)
		{
			return JointFactory.CreateAngleJoint(body.World, body, bodyB);
		}


		public static DistanceJoint CreateDistanceJoint(this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB,
		                                                bool useWorldCoordinates = false)
		{
			return JointFactory.CreateDistanceJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchorA,
				FSConvert.DisplayToSim * anchorB, useWorldCoordinates);
		}


		public static DistanceJoint CreateDistanceJoint(this Body body, Body bodyB)
		{
			return CreateDistanceJoint(body, bodyB, Vector2.Zero, Vector2.Zero);
		}


		public static FrictionJoint CreateFrictionJoint(this Body body, Body bodyB, Vector2 anchor,
		                                                bool useWorldCoordinates = false)
		{
			return JointFactory.CreateFrictionJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchor,
				useWorldCoordinates);
		}


		public static FrictionJoint CreateFrictionJoint(this Body body, Body bodyB)
		{
			return CreateFrictionJoint(body, bodyB, Vector2.Zero);
		}


		public static GearJoint CreateGearJoint(this Body body, Body bodyB, Joint jointA, Joint jointB, float ratio)
		{
			return JointFactory.CreateGearJoint(body.World, body, bodyB, jointA, jointB, ratio);
		}


		public static PulleyJoint CreatePulleyJoint(this Body body, Body bodyB, Vector2 anchorA, Vector2 anchorB,
		                                            Vector2 worldAnchorA, Vector2 worldAnchorB, float ratio,
		                                            bool useWorldCoordinates = false)
		{
			return JointFactory.CreatePulleyJoint(body.World, body, bodyB, FSConvert.DisplayToSim * anchorA,
				FSConvert.DisplayToSim * anchorB, FSConvert.DisplayToSim * worldAnchorA,
				FSConvert.DisplayToSim * worldAnchorB, ratio, useWorldCoordinates);
		}


		public static FixedMouseJoint CreateFixedMouseJoint(this Body body, Vector2 worldAnchor)
		{
			return JointFactory.CreateFixedMouseJoint(body.World, body, FSConvert.DisplayToSim * worldAnchor);
		}

		#endregion
	}
}