using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSRevoluteJoint : FSJoint
	{
		FSRevoluteJointDef _jointDef = new FSRevoluteJointDef();


		#region Configuration

		public FSRevoluteJoint setOwnerBodyAnchor( Vector2 ownerBodyAnchor )
		{
			_jointDef.ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
		{
			_jointDef.otherBodyAnchor = otherBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setLimitEnabled( bool limitEnabled )
		{
			_jointDef.limitEnabled = limitEnabled;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setLowerLimit( float lowerLimit )
		{
			_jointDef.lowerLimit = lowerLimit;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setUpperLimit( float upperLimit )
		{
			_jointDef.upperLimit = upperLimit;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setMotorEnabled( bool motorEnabled )
		{
			_jointDef.motorEnabled = motorEnabled;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setMotorSpeed( float motorSpeed )
		{
			_jointDef.motorSpeed = motorSpeed;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setMaxMotorTorque( float maxMotorTorque )
		{
			_jointDef.maxMotorTorque = maxMotorTorque;
			recreateJoint();
			return this;
		}


		public FSRevoluteJoint setMotorImpulse( float motorImpulse )
		{
			_jointDef.motorImpulse = motorImpulse;
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
