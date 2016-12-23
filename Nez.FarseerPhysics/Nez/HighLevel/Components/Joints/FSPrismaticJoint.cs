using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSPrismaticJoint : FSJoint
	{
		FSPrismaticJointDef _jointDef = new FSPrismaticJointDef();


		#region Configuration

		public FSPrismaticJoint setOwnerBodyAnchor( Vector2 ownerBodyAnchor )
		{
			_jointDef.ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
		{
			_jointDef.otherBodyAnchor = otherBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setAxis( Vector2 axis )
		{
			_jointDef.axis = axis;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setLimitEnabled( bool limitEnabled )
		{
			_jointDef.limitEnabled = limitEnabled;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setLowerLimit( float lowerLimit )
		{
			_jointDef.lowerLimit = lowerLimit;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setUpperLimit( float upperLimit )
		{
			_jointDef.upperLimit = upperLimit;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setMotorEnabled( bool motorEnabled )
		{
			_jointDef.motorEnabled = motorEnabled;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setMotorSpeed( float motorSpeed )
		{
			_jointDef.motorSpeed = motorSpeed;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setMaxMotorForce( float maxMotorForce )
		{
			_jointDef.maxMotorForce = maxMotorForce;
			recreateJoint();
			return this;
		}


		public FSPrismaticJoint setMotorImpulse( float motorImpulse )
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
