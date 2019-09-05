namespace Nez.Farseer
{
	public class FSAngleJoint : FSJoint
	{
		FSAngleJointDef _jointDef = new FSAngleJointDef();


		#region Configuration

		public FSAngleJoint SetMaxImpulse(float maxImpulse)
		{
			_jointDef.MaxImpulse = maxImpulse;
			RecreateJoint();
			return this;
		}


		public FSAngleJoint SetBiasFactor(float biasFactor)
		{
			_jointDef.BiasFactor = biasFactor;
			RecreateJoint();
			return this;
		}


		public FSAngleJoint SetSoftness(float softness)
		{
			_jointDef.Softness = softness;
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