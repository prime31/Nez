using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSDistanceJoint : FSJoint
	{
		FSDistanceJointDef _jointDef = new FSDistanceJointDef();
		Vector2 _ownerBodyAnchor;
		Vector2 _otherBodyAnchor;


		#region Configuration

		public FSDistanceJoint setFrequency( float frequency )
		{
			_jointDef.frequency = frequency;
			recreateJoint();
			return this;
		}


		public FSDistanceJoint setDampingRatio( float damping )
		{
			_jointDef.dampingRatio = damping;
			recreateJoint();
			return this;
		}


		public FSDistanceJoint setOwnerBodyAnchor( Vector2 ownerBodyAnchor )
		{
			_ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSDistanceJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
		{
			_otherBodyAnchor = otherBodyAnchor;
			recreateJoint();
			return this;
		}

		#endregion


		protected override FSJointDef getJointDef()
		{
			initializeJointDef( _jointDef );
			if( _jointDef.bodyA == null || _jointDef.bodyB == null )
				return null;

			_jointDef.ownerBodyAnchor = FSConvert.displayToSim * _ownerBodyAnchor;
			_jointDef.otherBodyAnchor = FSConvert.displayToSim * _otherBodyAnchor;

			return _jointDef;
		}

	}
}
