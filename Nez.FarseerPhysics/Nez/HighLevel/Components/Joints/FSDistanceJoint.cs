using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSDistanceJoint : FSJoint
	{
		FSDistanceJointDef _jointDef = new FSDistanceJointDef();


		#region Configuration

		public FSDistanceJoint SetFrequency(float frequency)
		{
			_jointDef.Frequency = frequency;
			RecreateJoint();
			return this;
		}


		public FSDistanceJoint SetDampingRatio(float damping)
		{
			_jointDef.DampingRatio = damping;
			RecreateJoint();
			return this;
		}


		public FSDistanceJoint SetOwnerBodyAnchor(Vector2 ownerBodyAnchor)
		{
			_jointDef.OwnerBodyAnchor = ownerBodyAnchor;
			RecreateJoint();
			return this;
		}


		public FSDistanceJoint SetOtherBodyAnchor(Vector2 otherBodyAnchor)
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