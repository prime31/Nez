using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSPrismaticJoint : FSJoint
	{
		FSPrismaticJointDef _jointDef = new FSPrismaticJointDef();


		#region Configuration

		public FSPrismaticJoint SetOwnerBodyAnchor(Vector2 ownerBodyAnchor)
		{
			_jointDef.OwnerBodyAnchor = ownerBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetOtherBodyAnchor(Vector2 otherBodyAnchor)
		{
			_jointDef.OtherBodyAnchor = otherBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetAxis(Vector2 axis)
		{
			_jointDef.Axis = axis;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetLimitEnabled(bool limitEnabled)
		{
			_jointDef.LimitEnabled = limitEnabled;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetLowerLimit(float lowerLimit)
		{
			_jointDef.LowerLimit = lowerLimit;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetUpperLimit(float upperLimit)
		{
			_jointDef.UpperLimit = upperLimit;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetMotorEnabled(bool motorEnabled)
		{
			_jointDef.MotorEnabled = motorEnabled;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetMotorSpeed(float motorSpeed)
		{
			_jointDef.MotorSpeed = motorSpeed;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetMaxMotorForce(float maxMotorForce)
		{
			_jointDef.MaxMotorForce = maxMotorForce;
			RecreateJoint();
			return this;
		}


		public FSPrismaticJoint SetMotorImpulse(float motorImpulse)
		{
			_jointDef.MotorImpulse = motorImpulse;
			RecreateJoint();
			return this;
		}

		#endregion


		internal override FSJointDef GetJointDef()
		{
			InitializeJointDef(_jointDef);
			if (_jointDef.BodyA == null || _jointDef.BodyB == null)
				return null;

			return _jointDef;
		}
	}
}