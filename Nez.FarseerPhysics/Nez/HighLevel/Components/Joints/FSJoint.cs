using FarseerPhysics.Dynamics.Joints;


namespace Nez.Farseer
{
	public abstract class FSJoint : Component
	{
		internal Joint _joint;
		FSRigidBody _ownerBody;
		FSRigidBody _otherBody;
		internal FSJoint _attachedJoint;
		bool _collideConnected;


		#region Configuration

		public FSJoint setOtherBody( FSRigidBody otherBody )
		{
			_otherBody = otherBody;
			recreateJoint();
			return this;
		}


		public FSJoint setCollideConnected( bool collideConnected )
		{
			_collideConnected = collideConnected;
			recreateJoint();
			return this;
		}

		#endregion


		#region Component lifecycle

		public override void onAddedToEntity()
		{
			_ownerBody = this.getComponent<FSRigidBody>();
			Insist.isNotNull( _ownerBody, "Joint added to an Entity with no RigidBody!" );
			createJoint();
		}


		public override void onRemovedFromEntity()
		{
			destroyJoint();
		}


		public override void onEnabled()
		{
			createJoint();

			// HACK: if we still dont have a Joint after onEnabled is called delay the call to createJoint one frame. This will allow the otherBody
			// to be initialized and have the Body created.
			if( _joint == null )
				Core.schedule( 0, this, t => ( t.context as FSJoint ).createJoint() );
		}


		public override void onDisabled()
		{
			destroyJoint();
		}

		#endregion


		internal void initializeJointDef( FSJointDef jointDef )
		{
			jointDef.bodyA = _ownerBody?.body;
			jointDef.bodyB = _otherBody?.body;
			jointDef.collideConnected = _collideConnected;
		}


		internal abstract FSJointDef getJointDef();


		protected void recreateJoint()
		{
			if( _attachedJoint != null )
				_attachedJoint.destroyJoint();
			
			destroyJoint();
			createJoint();

			if( _attachedJoint != null )
				_attachedJoint.createJoint();
		}


		internal void createJoint()
		{
			if( _joint != null )
				return;

			var jointDef = getJointDef();
			if( jointDef == null )
				return;

			_joint = jointDef.createJoint();
			jointDef.bodyA.world.addJoint( _joint );
		}


		internal void destroyJoint()
		{
			if( _joint == null )
				return;

			if( _ownerBody != null )
				_ownerBody._joints.Remove( this );

			if( _otherBody != null )
				_otherBody._joints.Remove( this );

			_joint.bodyA.world.removeJoint( _joint );
			_joint = null;
		}

	}
}
