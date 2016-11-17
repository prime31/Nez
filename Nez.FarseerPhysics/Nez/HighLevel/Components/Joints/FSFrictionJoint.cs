using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSFrictionJoint : FSJoint
	{
		FSFrictionJointDef _jointDef = new FSFrictionJointDef();
		Vector2 _anchor;


		#region Configuration

		public FSFrictionJoint setAnchor( Vector2 anchor )
		{
			_anchor = anchor;
			recreateJoint();
			return this;
		}


		public FSFrictionJoint setMaxForce( float maxForce )
		{
			_jointDef.maxForce = maxForce;
			recreateJoint();
			return this;
		}


		public FSFrictionJoint setMaxTorque( float maxTorque )
		{
			_jointDef.maxTorque = maxTorque;
			recreateJoint();
			return this;
		}

		#endregion


		internal override FSJointDef getJointDef()
		{
			initializeJointDef( _jointDef );
			if( _jointDef.bodyA == null || _jointDef.bodyB == null )
				return null;

			_jointDef.anchor = FSConvert.displayToSim * _anchor;

			return _jointDef;
		}

	}
}
