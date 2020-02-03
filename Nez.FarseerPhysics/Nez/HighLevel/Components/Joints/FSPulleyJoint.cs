using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSPulleyJoint : FSJoint
	{
		FSPulleyJointDef _jointDef = new FSPulleyJointDef();


		#region Configuration

		public FSPulleyJoint SetOwnerBodyAnchor(Vector2 ownerBodyAnchor)
		{
			_jointDef.OwnerBodyAnchor = ownerBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSPulleyJoint SetOtherBodyAnchor(Vector2 otherBodyAnchor)
		{
			_jointDef.OtherBodyAnchor = otherBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSPulleyJoint SetOwnerBodyGroundAnchor(Vector2 ownerBodyGroundAnchor)
		{
			_jointDef.OwnerBodyGroundAnchor = ownerBodyGroundAnchor;
			RecreateJoint();
			return this;
		}


		public FSPulleyJoint SetOtherBodyGroundAnchor(Vector2 otherBodyGroundAnchor)
		{
			_jointDef.OtherBodyGroundAnchor = otherBodyGroundAnchor;
			RecreateJoint();
			return this;
		}


		public FSPulleyJoint SetRatio(float ratio)
		{
			_jointDef.Ratio = ratio;
			RecreateJoint();
			return this;
		}

		#endregion


		internal override FSJointDef GetJointDef()
		{
			InitializeJointDef(_jointDef);
			if (_jointDef.BodyA == null || _jointDef.BodyB == null)
				return null;

			return _jointDef;
		}
	}
}