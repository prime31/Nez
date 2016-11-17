using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSWeldJoint : FSJoint
	{
		FSWeldJointDef _jointDef = new FSWeldJointDef();


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
			_jointDef.ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSWeldJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
		{
			_jointDef.otherBodyAnchor = otherBodyAnchor;
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
