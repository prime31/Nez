using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSMotorJoint : FSJoint
	{
		FSMotorJointDef _jointDef = new FSMotorJointDef();


		#region Configuration

		public FSMotorJoint setLinearOffset( Vector2 linearOffset )
		{
			_jointDef.linearOffset = linearOffset;
			recreateJoint();
			return this;
		}


		public FSMotorJoint setMaxForce( float maxForce )
		{
			_jointDef.maxForce = maxForce;
			recreateJoint();
			return this;
		}


		public FSMotorJoint setMaxTorque( float maxTorque )
		{
			_jointDef.maxTorque = maxTorque;
			recreateJoint();
			return this;
		}


		public FSMotorJoint setAngularOffset( float angularOffset )
		{
			_jointDef.angularOffset = angularOffset;
			recreateJoint();
			return this;
		}

		#endregion


		internal override FSJointDef getJointDef()
		{
			initializeJointDef( _jointDef );
			if( _jointDef.bodyA == null || _jointDef.bodyB == null )
				return null;

			return _jointDef;
		}
	}
}
