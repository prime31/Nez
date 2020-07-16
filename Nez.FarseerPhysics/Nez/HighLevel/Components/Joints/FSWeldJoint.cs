using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSWeldJoint : FSJoint
	{
		FSWeldJointDef _jointDef = new FSWeldJointDef();


		#region Configuration

		public FSWeldJoint SetFrequencyHz(float frequency)
		{
			_jointDef.FrequencyHz = frequency;
			RecreateJoint();
			return this;
		}


		public FSWeldJoint SetDampingRatio(float damping)
		{
			_jointDef.DampingRatio = damping;
			RecreateJoint();
			return this;
		}


		public FSWeldJoint SetOwnerBodyAnchor(Vector2 ownerBodyAnchor)
		{
			_jointDef.OwnerBodyAnchor = ownerBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSWeldJoint SetOtherBodyAnchor(Vector2 otherBodyAnchor)
		{
			_jointDef.OtherBodyAnchor = otherBodyAnchor;
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