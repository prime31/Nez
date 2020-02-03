namespace Nez.Farseer
{
	public class FSGearJoint : FSJoint
	{
		FSGearJointDef _jointDef = new FSGearJointDef();
		FSJoint _ownerJoint;
		FSJoint _otherJoint;


		#region Configuration

		public FSGearJoint SetOwnerJoint(FSJoint ownerJoint)
		{
			if (_ownerJoint != null)
				_ownerJoint._attachedJoint = null;

			_ownerJoint = ownerJoint;

			if (_ownerJoint != null)
				_ownerJoint._attachedJoint = this;

			RecreateJoint();
			return this;
		}


		public FSGearJoint SetOtherJoint(FSJoint otherJoint)
		{
			if (_otherJoint != null)
				_otherJoint._attachedJoint = null;

			_otherJoint = otherJoint;

			if (_otherJoint != null)
				_otherJoint._attachedJoint = this;

			RecreateJoint();
			return this;
		}


		public FSGearJoint SetRatio(float ratio)
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

			if (_ownerJoint == null || _otherJoint == null)
				return null;

			if (_ownerJoint._joint == null || _otherJoint._joint == null)
				return null;

			_jointDef.OwnerJoint = _ownerJoint._joint;
			_jointDef.OtherJoint = _otherJoint._joint;

			return _jointDef;
		}
	}
}