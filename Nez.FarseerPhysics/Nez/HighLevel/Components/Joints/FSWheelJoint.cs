using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSWheelJoint : FSJoint
	{
		FSWheelJointDef _jointDef = new FSWheelJointDef();


		#region Configuration

		public FSWheelJoint SetAnchor(Vector2 anchor)
		{
			_jointDef.Anchor = anchor;
			RecreateJoint();
			return this;
		}


		public FSWheelJoint SetAxis(Vector2 axis)
		{
			_jointDef.Axis = axis;
			RecreateJoint();
			return this;
		}


		public FSWheelJoint SetMotorEnabled(bool motorEnabled)
		{
			_jointDef.MotorEnabled = motorEnabled;
			RecreateJoint();
			return this;
		}


		public FSWheelJoint SetMotorSpeed(float motorSpeed)
		{
			_jointDef.MotorSpeed = motorSpeed;
			RecreateJoint();
			return this;
		}


		public FSWheelJoint SetMaxMotorTorque(float maxMotorTorque)
		{
			_jointDef.MaxMotorTorque = maxMotorTorque;
			RecreateJoint();
			return this;
		}


		public FSWheelJoint SetFrequency(float frequency)
		{
			_jointDef.Frequency = frequency;
			RecreateJoint();
			return this;
		}


		public FSWheelJoint SetDampingRatio(float damping)
		{
			_jointDef.DampingRatio = damping;
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