namespace Nez.Farseer
{
	public class FSGearJoint : FSJoint
	{
		FSGearJointDef _jointDef = new FSGearJointDef();
		FSJoint _ownerJoint;
		FSJoint _otherJoint;


		#region Configuration

		public FSGearJoint setOwnerJoint( FSJoint ownerJoint )
		{
			if( _ownerJoint != null )
				_ownerJoint._attachedJoint = null;

			_ownerJoint = ownerJoint;

			if( _ownerJoint != null )
				_ownerJoint._attachedJoint = this;
			
			recreateJoint();
			return this;
		}


		public FSGearJoint setOtherJoint( FSJoint otherJoint )
		{
			if( _otherJoint != null )
				_otherJoint._attachedJoint = null;
			
			_otherJoint = otherJoint;

			if( _otherJoint != null )
				_otherJoint._attachedJoint = this;
			
			recreateJoint();
			return this;
		}


		public FSGearJoint setRatio( float ratio )
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

			if( _ownerJoint == null || _otherJoint == null )
				return null;

			if( _ownerJoint._joint == null || _otherJoint._joint == null )
				return null;

			_jointDef.ownerJoint = _ownerJoint._joint;
			_jointDef.otherJoint = _otherJoint._joint;

			return _jointDef;
		}


	}
}
