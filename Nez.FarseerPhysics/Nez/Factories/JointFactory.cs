using FarseerPhysics;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public static partial class Farseer
	{
		/// <summary>
		/// exactly the same as FarseerPhysics.Factories.JointFactory except all units are in display/Nez space as opposed to simulation space
		/// </summary>
		public static class JointFactory
		{
			public static MotorJoint createMotorJoint( World world, Body bodyA, Body bodyB, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateMotorJoint( world, bodyA, bodyB, useWorldCoordinates );
			}


			public static RevoluteJoint createRevoluteJoint( World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateRevoluteJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchorA, ConvertUnits.displayToSim * anchorB, useWorldCoordinates );
			}


			public static RevoluteJoint createRevoluteJoint( World world, Body bodyA, Body bodyB, Vector2 anchor )
			{
				return FarseerPhysics.Factories.JointFactory.CreateRevoluteJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchor );
			}


			public static RopeJoint createRopeJoint( World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateRopeJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchorA, ConvertUnits.displayToSim * anchorB, useWorldCoordinates );
			}


			public static WeldJoint createWeldJoint( World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateWeldJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchorA, ConvertUnits.displayToSim * anchorB, useWorldCoordinates );
			}


			public static PrismaticJoint createPrismaticJoint( World world, Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreatePrismaticJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchor, axis, useWorldCoordinates );
			}


			public static WheelJoint createWheelJoint( World world, Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateWheelJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchor, axis, useWorldCoordinates );
			}


			public static WheelJoint createWheelJoint( World world, Body bodyA, Body bodyB, Vector2 axis )
			{
				return createWheelJoint( world, bodyA, bodyB, Vector2.Zero, axis );
			}


			public static AngleJoint createAngleJoint( World world, Body bodyA, Body bodyB )
			{
				return FarseerPhysics.Factories.JointFactory.CreateAngleJoint( world, bodyA, bodyB );
			}


			public static DistanceJoint createDistanceJoint( World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateDistanceJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchorA, ConvertUnits.displayToSim * anchorB, useWorldCoordinates );
			}


			public static DistanceJoint createDistanceJoint( World world, Body bodyA, Body bodyB )
			{
				return createDistanceJoint( world, bodyA, bodyB, Vector2.Zero, Vector2.Zero );
			}


			public static FrictionJoint createFrictionJoint( World world, Body bodyA, Body bodyB, Vector2 anchor, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreateFrictionJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchor, useWorldCoordinates );
			}


			public static FrictionJoint createFrictionJoint( World world, Body bodyA, Body bodyB )
			{
				return createFrictionJoint( world, bodyA, bodyB, Vector2.Zero );
			}


			public static GearJoint createGearJoint( World world, Body bodyA, Body bodyB, Joint jointA, Joint jointB, float ratio )
			{
				return FarseerPhysics.Factories.JointFactory.CreateGearJoint( world, bodyA, bodyB, jointA, jointB, ratio );
			}


			public static PulleyJoint createPulleyJoint( World world, Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, Vector2 worldAnchorA, Vector2 worldAnchorB, float ratio, bool useWorldCoordinates = false )
			{
				return FarseerPhysics.Factories.JointFactory.CreatePulleyJoint( world, bodyA, bodyB, ConvertUnits.displayToSim * anchorA, ConvertUnits.toSimUnits( anchorB ), ConvertUnits.toSimUnits( worldAnchorA ), ConvertUnits.toSimUnits( worldAnchorB ), ratio, useWorldCoordinates );
			}


			public static FixedMouseJoint createFixedMouseJoint( World world, Body body, Vector2 worldAnchor )
			{
				return FarseerPhysics.Factories.JointFactory.CreateFixedMouseJoint( world, body, ConvertUnits.displayToSim * worldAnchor );
			}

		}
	}
}
