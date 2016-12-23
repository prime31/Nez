using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSWheelJoint : FSJoint
	{
		FSWheelJointDef _jointDef = new FSWheelJointDef();


		#region Configuration

		public FSWheelJoint setAnchor( Vector2 anchor )
		{
			_jointDef.anchor = anchor;
			recreateJoint();
			return this;
		}


		public FSWheelJoint setAxis( Vector2 axis )
		{
			_jointDef.axis = axis;
			recreateJoint();
			return this;
		}


		public FSWheelJoint setMotorEnabled( bool motorEnabled )
		{
			_jointDef.motorEnabled = motorEnabled;
			recreateJoint();
			return this;
		}


		public FSWheelJoint setMotorSpeed( float motorSpeed )
		{
			_jointDef.motorSpeed = motorSpeed;
			recreateJoint();
			return this;
		}


		public FSWheelJoint setMaxMotorTorque( float maxMotorTorque )
		{
			_jointDef.maxMotorTorque = maxMotorTorque;
			recreateJoint();
			return this;
		}


		public FSWheelJoint setFrequency( float frequency )
		{
			_jointDef.frequency = frequency;
			recreateJoint();
			return this;
		}


		public FSWheelJoint setDampingRatio( float damping )
		{
			_jointDef.dampingRatio = damping;
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
