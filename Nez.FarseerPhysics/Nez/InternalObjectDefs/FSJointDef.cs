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
			var joint = new DistanceJoint( bodyA, bodyB, ownerBodyAnchor, otherBodyAnchor );
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
			var joint = new WeldJoint( bodyA, bodyB, ownerBodyAnchor, otherBodyAnchor );
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

}
