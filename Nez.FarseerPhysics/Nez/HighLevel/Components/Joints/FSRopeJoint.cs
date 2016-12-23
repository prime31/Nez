using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSRopeJoint : FSJoint
	{
		FSRopeJointDef _jointDef = new FSRopeJointDef();


		#region Configuration

		public FSRopeJoint setOwnerBodyAnchor( Vector2 ownerBodyAnchor )
		{
			_jointDef.ownerBodyAnchor = ownerBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSRopeJoint setOtherBodyAnchor( Vector2 otherBodyAnchor )
		{
			_jointDef.otherBodyAnchor = otherBodyAnchor;
			recreateJoint();
			return this;
		}


		public FSRopeJoint setMaxLength( float maxLength )
		{
			_jointDef.maxLength = maxLength;
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
