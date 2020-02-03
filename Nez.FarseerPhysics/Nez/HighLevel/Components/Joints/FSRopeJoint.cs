using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSRopeJoint : FSJoint
	{
		FSRopeJointDef _jointDef = new FSRopeJointDef();


		#region Configuration

		public FSRopeJoint SetOwnerBodyAnchor(Vector2 ownerBodyAnchor)
		{
			_jointDef.OwnerBodyAnchor = ownerBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSRopeJoint SetOtherBodyAnchor(Vector2 otherBodyAnchor)
		{
			_jointDef.OtherBodyAnchor = otherBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSRopeJoint SetMaxLength(float maxLength)
		{
			_jointDef.MaxLength = maxLength;
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