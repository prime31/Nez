using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public abstract class FSJointDef
	{
		public Body bodyA;
		public Body bodyB;
		public bool collideConnected;

		abstract public Joint createJoint();
	}


	public class FSDistanceJointDef : FSJointDef
	{
		public Vector2 ownerBodyAnchor;
		public Vector2 otherBodyAnchor;
		public float frequency;
		public float dampingRatio;

		public override Joint createJoint()
		{
			var joint = new DistanceJoint( bodyA, bodyB, ownerBodyAnchor * FSConvert.displayToSim, otherBodyAnchor * FSConvert.displayToSim );
			joint.collideConnected = collideConnected;
			joint.frequency = frequency;
			joint.dampingRatio = dampingRatio;
			return joint;
		}
	}


	public class FSFrictionJointDef : FSJointDef
	{
		public Vector2 anchor;
		public float maxForce;
		public float maxTorque;

		public override Joint createJoint()
		{
			var joint = new FrictionJoint( bodyA, bodyB, anchor );
			joint.collideConnected = collideConnected;
			joint.maxForce = maxForce;
			joint.maxTorque = maxTorque;
			return joint;
		}
	}


	public class FSWeldJointDef : FSJointDef
	{
		public Vector2 ownerBodyAnchor;
		public Vector2 otherBodyAnchor;
		public float dampingRatio;
		public float frequencyHz;

		public override Joint createJoint()
		{
			var joint = new WeldJoint( bodyA, bodyB, ownerBodyAnchor * FSConvert.displayToSim, otherBodyAnchor * FSConvert.displayToSim );
			joint.collideConnected = collideConnected;
			joint.dampingRatio = dampingRatio;
			joint.frequencyHz = frequencyHz;
			return joint;
		}
	}


	public class FSAngleJointDef : FSJointDef
	{
		public float maxImpulse = float.MaxValue;
		public float biasFactor = 0.2f;
		public float softness;

		public override Joint createJoint()
		{
			var joint = new AngleJoint( bodyA, bodyB );
			joint.collideConnected = collideConnected;
			joint.maxImpulse = maxImpulse;
			joint.biasFactor = biasFactor;
			joint.softness = softness;
			return joint;
		}
	}


	public class FSRevoluteJointDef : FSJointDef
	{
		public Vector2 ownerBodyAnchor;
		public Vector2 otherBodyAnchor;
		public bool limitEnabled;
		public float lowerLimit;
		public float upperLimit;
		public bool motorEnabled;
		public float motorSpeed;
		public float maxMotorTorque;
		public float motorImpulse;

		public override Joint createJoint()
		{
			var joint = new RevoluteJoint( bodyA, bodyB, ownerBodyAnchor * FSConvert.displayToSim, otherBodyAnchor * FSConvert.displayToSim );
			joint.collideConnected = collideConnected;
			joint.limitEnabled = limitEnabled;
			joint.lowerLimit = lowerLimit;
			joint.upperLimit = upperLimit;
			joint.motorEnabled = motorEnabled;
			joint.motorSpeed = motorSpeed;
			joint.maxMotorTorque = maxMotorTorque;
			joint.motorImpulse = motorImpulse;
			return joint;
		}
	}


	public class FSPrismaticJointDef : FSJointDef
	{
		public Vector2 ownerBodyAnchor;
		public Vector2 otherBodyAnchor;
		public Vector2 axis = Vector2.UnitY;
		public bool limitEnabled;
		public float lowerLimit;
		public float upperLimit;
		public bool motorEnabled;
		public float motorSpeed;
		public float maxMotorForce = 1;
		public float motorImpulse;

		public override Joint createJoint()
		{
			var joint = new PrismaticJoint( bodyA, bodyB, ownerBodyAnchor * FSConvert.displayToSim, otherBodyAnchor * FSConvert.displayToSim );
			joint.collideConnected = collideConnected;
			joint.axis = axis;
			joint.limitEnabled = limitEnabled;
			joint.lowerLimit = lowerLimit;
			joint.upperLimit = upperLimit;
			joint.motorEnabled = motorEnabled;
			joint.motorSpeed = motorSpeed;
			joint.maxMotorForce = maxMotorForce;
			joint.motorImpulse = motorImpulse;
			return joint;
		}
	}


	public class FSRopeJointDef : FSJointDef
	{
		public Vector2 ownerBodyAnchor;
		public Vector2 otherBodyAnchor;
		public float maxLength;

		public override Joint createJoint()
		{
			var joint = new RopeJoint( bodyA, bodyB, ownerBodyAnchor * FSConvert.displayToSim, otherBodyAnchor * FSConvert.displayToSim );
			joint.collideConnected = collideConnected;
			joint.maxLength = maxLength * FSConvert.displayToSim;
			return joint;
		}
	}


	public class FSMotorJointDef : FSJointDef
	{
		public Vector2 linearOffset;
		public float maxForce = 1;
		public float maxTorque = 1;
		public float angularOffset;

		public override Joint createJoint()
		{
			var joint = new MotorJoint( bodyA, bodyB );
			joint.collideConnected = collideConnected;
			joint.linearOffset = linearOffset * FSConvert.displayToSim;
			joint.maxForce = maxForce;
			joint.maxTorque = maxTorque;
			joint.angularOffset = angularOffset;
			return joint;
		}
	}


	public class FSWheelJointDef : FSJointDef
	{
		public Vector2 anchor;
		public Vector2 axis = Vector2.UnitY;
		public bool motorEnabled;
		public float motorSpeed;
		public float maxMotorTorque;
		public float frequency;
		public float dampingRatio;

		public override Joint createJoint()
		{
			var joint = new WheelJoint( bodyA, bodyB, anchor * FSConvert.displayToSim, axis );
			joint.collideConnected = collideConnected;
			joint.axis = axis;
			joint.motorEnabled = motorEnabled;
			joint.motorSpeed = motorSpeed;
			joint.maxMotorTorque = maxMotorTorque;
			joint.frequency = frequency;
			joint.dampingRatio = dampingRatio;
			return joint;
		}
	}


	public class FSPulleyJointDef : FSJointDef
	{
		public Vector2 ownerBodyAnchor;
		public Vector2 otherBodyAnchor;
		public Vector2 ownerBodyGroundAnchor;
		public Vector2 otherBodyGroundAnchor;
		public float ratio;

		public override Joint createJoint()
		{
			var joint = new PulleyJoint( bodyA, bodyB, ownerBodyAnchor * FSConvert.displayToSim, otherBodyGroundAnchor * FSConvert.displayToSim,
										ownerBodyGroundAnchor * FSConvert.displayToSim, otherBodyGroundAnchor * FSConvert.displayToSim, ratio );
			joint.collideConnected = collideConnected;
			return joint;
		}
	}


	public class FSGearJointDef : FSJointDef
	{
		public Joint ownerJoint;
		public Joint otherJoint;
		public float ratio = 1;

		public override Joint createJoint()
		{
			var joint = new GearJoint( bodyA, bodyB, ownerJoint, otherJoint, ratio );
			joint.collideConnected = collideConnected;
			return joint;
		}
	}

}
