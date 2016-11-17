using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSWeldJoint : FSJoint
	{
		FSWeldJointDef _jointDef = new FSWeldJointDef();
		Vector2 _ownerBodyAnchor;
		Vector2 _otherBodyAnchor;


		#region Configuration

		public FSWeldJoint setFrequencyHz( float frequency )
		{
			_jointDef.frequencyHz = frequency;
			recreateJoint();
			return this;
		}


		public FSWeldJoint setDampingRatio( float damping )
		{
			_jointDef.dampingRatio = damping;
			recreateJoint();
			return this;
		}


		public FSWeldJoint setOwnerBodyAnchor( Vector2 ownerBodyAnchor )
		{
			_ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSWeldJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
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
