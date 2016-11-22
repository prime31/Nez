namespace Nez.Farseer
{
	public class FSAngleJoint : FSJoint
	{
		FSAngleJointDef _jointDef = new FSAngleJointDef();



		#region Configuration

		public FSAngleJoint setMaxImpulse( float maxImpulse )
		{
			_jointDef.maxImpulse = maxImpulse;
			recreateJoint();
			return this;
		}


		public FSAngleJoint setBiasFactor( float biasFactor )
		{
			_jointDef.biasFactor = biasFactor;
			recreateJoint();
			return this;
		}


		public FSAngleJoint setSoftness( float softness )
		{
			_jointDef.softness = softness;
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
