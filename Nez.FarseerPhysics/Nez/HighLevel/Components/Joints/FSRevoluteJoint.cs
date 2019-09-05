using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSRevoluteJoint : FSJoint
	{
		FSRevoluteJointDef _jointDef = new FSRevoluteJointDef();


		#region Configuration

		public FSRevoluteJoint SetOwnerBodyAnchor(Vector2 ownerBodyAnchor)
		{
			_jointDef.OwnerBodyAnchor = ownerBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetOtherBodyAnchor(Vector2 otherBodyAnchor)
		{
			_jointDef.OtherBodyAnchor = otherBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetLimitEnabled(bool limitEnabled)
		{
			_jointDef.LimitEnabled = limitEnabled;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetLowerLimit(float lowerLimit)
		{
			_jointDef.LowerLimit = lowerLimit;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetUpperLimit(float upperLimit)
		{
			_jointDef.UpperLimit = upperLimit;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetMotorEnabled(bool motorEnabled)
		{
			_jointDef.MotorEnabled = motorEnabled;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetMotorSpeed(float motorSpeed)
		{
			_jointDef.MotorSpeed = motorSpeed;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetMaxMotorTorque(float maxMotorTorque)
		{
			_jointDef.MaxMotorTorque = maxMotorTorque;
			RecreateJoint();
			return this;
		}


		public FSRevoluteJoint SetMotorImpulse(float motorImpulse)
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