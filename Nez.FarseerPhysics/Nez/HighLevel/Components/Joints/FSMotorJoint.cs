using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSMotorJoint : FSJoint
	{
		FSMotorJointDef _jointDef = new FSMotorJointDef();


		#region Configuration

		public FSMotorJoint SetLinearOffset(Vector2 linearOffset)
		{
			_jointDef.LinearOffset = linearOffset;
			RecreateJoint();
			return this;
		}


		public FSMotorJoint SetMaxForce(float maxForce)
		{
			_jointDef.MaxForce = maxForce;
			RecreateJoint();
			return this;
		}


		public FSMotorJoint SetMaxTorque(float maxTorque)
		{
			_jointDef.MaxTorque = maxTorque;
			RecreateJoint();
			return this;
		}


		public FSMotorJoint SetAngularOffset(float angularOffset)
		{
			_jointDef.AngularOffset = angularOffset;
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