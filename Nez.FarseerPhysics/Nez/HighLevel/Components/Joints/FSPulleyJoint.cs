using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSPulleyJoint : FSJoint
	{
		FSPulleyJointDef _jointDef = new FSPulleyJointDef();


		#region Configuration

		public FSPulleyJoint setOwnerBodyAnchor( Vector2 ownerBodyAnchor )
		{
			_jointDef.ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSPulleyJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
		{
			_jointDef.otherBodyAnchor = otherBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSPulleyJoint setOwnerBodyGroundAnchor( Vector2 ownerBodyGroundAnchor )
		{
			_jointDef.ownerBodyGroundAnchor = ownerBodyGroundAnchor;
			recreateJoint();
			return this;
		}


		public FSPulleyJoint setOtherBodyGroundAnchor( Vector2 otherBodyGroundAnchor )
		{
			_jointDef.otherBodyGroundAnchor = otherBodyGroundAnchor;
			recreateJoint();
			return this;
		}


		public FSPulleyJoint setRatio( float ratio )
		{
			_jointDef.ratio = ratio;
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
