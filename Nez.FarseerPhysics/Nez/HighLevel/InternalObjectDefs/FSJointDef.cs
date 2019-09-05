using FarseerPhysics.Dynamics;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	internal abstract class FSJointDef
	{
		public Body BodyA;
		public Body BodyB;
		public bool CollideConnected;

		abstract public Joint CreateJoint();
	}


	internal class FSDistanceJointDef : FSJointDef
	{
		public Vector2 OwnerBodyAnchor;
		public Vector2 OtherBodyAnchor;
		public float Frequency;
		public float DampingRatio;

		public override Joint CreateJoint()
		{
			var joint = new DistanceJoint(BodyA, BodyB, OwnerBodyAnchor * FSConvert.DisplayToSim,
				OtherBodyAnchor * FSConvert.DisplayToSim);
			joint.CollideConnected = CollideConnected;
			joint.Frequency = Frequency;
			joint.DampingRatio = DampingRatio;
			return joint;
		}
	}


	internal class FSFrictionJointDef : FSJointDef
	{
		public Vector2 Anchor;
		public float MaxForce;
		public float MaxTorque;

		public override Joint CreateJoint()
		{
			var joint = new FrictionJoint(BodyA, BodyB, Anchor);
			joint.CollideConnected = CollideConnected;
			joint.MaxForce = MaxForce;
			joint.MaxTorque = MaxTorque;
			return joint;
		}
	}


	internal class FSWeldJointDef : FSJointDef
	{
		public Vector2 OwnerBodyAnchor;
		public Vector2 OtherBodyAnchor;
		public float DampingRatio;
		public float FrequencyHz;

		public override Joint CreateJoint()
		{
			var joint = new WeldJoint(BodyA, BodyB, OwnerBodyAnchor * FSConvert.DisplayToSim,
				OtherBodyAnchor * FSConvert.DisplayToSim);
			joint.CollideConnected = CollideConnected;
			joint.DampingRatio = DampingRatio;
			joint.FrequencyHz = FrequencyHz;
			return joint;
		}
	}


	internal class FSAngleJointDef : FSJointDef
	{
		public float MaxImpulse = float.MaxValue;
		public float BiasFactor = 0.2f;
		public float Softness;

		public override Joint CreateJoint()
		{
			var joint = new AngleJoint(BodyA, BodyB);
			joint.CollideConnected = CollideConnected;
			joint.MaxImpulse = MaxImpulse;
			joint.BiasFactor = BiasFactor;
			joint.Softness = Softness;
			return joint;
		}
	}


	internal class FSRevoluteJointDef : FSJointDef
	{
		public Vector2 OwnerBodyAnchor;
		public Vector2 OtherBodyAnchor;
		public bool LimitEnabled;
		public float LowerLimit;
		public float UpperLimit;
		public bool MotorEnabled;
		public float MotorSpeed;
		public float MaxMotorTorque;
		public float MotorImpulse;

		public override Joint CreateJoint()
		{
			var joint = new RevoluteJoint(BodyA, BodyB, OwnerBodyAnchor * FSConvert.DisplayToSim,
				OtherBodyAnchor * FSConvert.DisplayToSim);
			joint.CollideConnected = CollideConnected;
			joint.LimitEnabled = LimitEnabled;
			joint.LowerLimit = LowerLimit;
			joint.UpperLimit = UpperLimit;
			joint.MotorEnabled = MotorEnabled;
			joint.MotorSpeed = MotorSpeed;
			joint.MaxMotorTorque = MaxMotorTorque;
			joint.MotorImpulse = MotorImpulse;
			return joint;
		}
	}


	internal class FSPrismaticJointDef : FSJointDef
	{
		public Vector2 OwnerBodyAnchor;
		public Vector2 OtherBodyAnchor;
		public Vector2 Axis = Vector2.UnitY;
		public bool LimitEnabled;
		public float LowerLimit;
		public float UpperLimit;
		public bool MotorEnabled;
		public float MotorSpeed = 0.7f;
		public float MaxMotorForce = 2;
		public float MotorImpulse;

		public override Joint CreateJoint()
		{
			var joint = new PrismaticJoint(BodyA, BodyB, OwnerBodyAnchor * FSConvert.DisplayToSim,
				OtherBodyAnchor * FSConvert.DisplayToSim);
			joint.CollideConnected = CollideConnected;
			joint.Axis = Axis;
			joint.LimitEnabled = LimitEnabled;
			joint.LowerLimit = LowerLimit;
			joint.UpperLimit = UpperLimit;
			joint.MotorEnabled = MotorEnabled;
			joint.MotorSpeed = MotorSpeed;
			joint.MaxMotorForce = MaxMotorForce;
			joint.MotorImpulse = MotorImpulse;
			return joint;
		}
	}


	internal class FSRopeJointDef : FSJointDef
	{
		public Vector2 OwnerBodyAnchor;
		public Vector2 OtherBodyAnchor;
		public float MaxLength;

		public override Joint CreateJoint()
		{
			var joint = new RopeJoint(BodyA, BodyB, OwnerBodyAnchor * FSConvert.DisplayToSim,
				OtherBodyAnchor * FSConvert.DisplayToSim);
			joint.CollideConnected = CollideConnected;
			joint.MaxLength = MaxLength * FSConvert.DisplayToSim;
			return joint;
		}
	}


	internal class FSMotorJointDef : FSJointDef
	{
		public Vector2 LinearOffset;
		public float MaxForce = 1;
		public float MaxTorque = 1;
		public float AngularOffset;

		public override Joint CreateJoint()
		{
			var joint = new MotorJoint(BodyA, BodyB);
			joint.CollideConnected = CollideConnected;
			joint.LinearOffset = LinearOffset * FSConvert.DisplayToSim;
			joint.MaxForce = MaxForce;
			joint.MaxTorque = MaxTorque;
			joint.AngularOffset = AngularOffset;
			return joint;
		}
	}


	internal class FSWheelJointDef : FSJointDef
	{
		public Vector2 Anchor;
		public Vector2 Axis = Vector2.UnitY;
		public bool MotorEnabled;
		public float MotorSpeed;
		public float MaxMotorTorque;
		public float Frequency = 2;
		public float DampingRatio = 0.7f;

		public override Joint CreateJoint()
		{
			var joint = new WheelJoint(BodyA, BodyB, Anchor * FSConvert.DisplayToSim, Axis);
			joint.CollideConnected = CollideConnected;
			joint.Axis = Axis;
			joint.MotorEnabled = MotorEnabled;
			joint.MotorSpeed = MotorSpeed;
			joint.MaxMotorTorque = MaxMotorTorque;
			joint.Frequency = Frequency;
			joint.DampingRatio = DampingRatio;
			return joint;
		}
	}


	internal class FSPulleyJointDef : FSJointDef
	{
		public Vector2 OwnerBodyAnchor;
		public Vector2 OtherBodyAnchor;
		public Vector2 OwnerBodyGroundAnchor;
		public Vector2 OtherBodyGroundAnchor;
		public float Ratio;

		public override Joint CreateJoint()
		{
			var joint = new PulleyJoint(BodyA, BodyB, OwnerBodyAnchor * FSConvert.DisplayToSim,
				OtherBodyGroundAnchor * FSConvert.DisplayToSim,
				OwnerBodyGroundAnchor * FSConvert.DisplayToSim, OtherBodyGroundAnchor * FSConvert.DisplayToSim, Ratio);
			joint.CollideConnected = CollideConnected;
			return joint;
		}
	}


	internal class FSGearJointDef : FSJointDef
	{
		public Joint OwnerJoint;
		public Joint OtherJoint;
		public float Ratio = 1;

		public override Joint CreateJoint()
		{
			var joint = new GearJoint(BodyA, BodyB, OwnerJoint, OtherJoint, Ratio);
			joint.CollideConnected = CollideConnected;
			return joint;
		}
	}


	internal class FSMouseJointDef : FSJointDef
	{
		public Vector2 WorldAnchor;
		public float MaxForce;
		public float Frequency = 5;
		public float DampingRatio = 0.7f;

		public override Joint CreateJoint()
		{
			var joint = new FixedMouseJoint(BodyA, WorldAnchor * FSConvert.DisplayToSim);
			joint.CollideConnected = CollideConnected;

			// conditionally set the maxForce
			if (MaxForce > 0)
				joint.MaxForce = MaxForce;

			joint.Frequency = Frequency;
			joint.DampingRatio = DampingRatio;
			return joint;
		}
	}
}