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

		public FSJoint SetOtherBody(FSRigidBody otherBody)
		{
			_otherBody = otherBody;
			RecreateJoint();
			return this;
		}


		public FSJoint SetCollideConnected(bool collideConnected)
		{
			_collideConnected = collideConnected;
			RecreateJoint();
			return this;
		}

		#endregion


		#region Component lifecycle

		public override void OnAddedToEntity()
		{
			_ownerBody = this.GetComponent<FSRigidBody>();
			Insist.IsNotNull(_ownerBody, "Joint added to an Entity with no RigidBody!");
			CreateJoint();
		}


		public override void OnRemovedFromEntity()
		{
			DestroyJoint();
		}


		public override void OnEnabled()
		{
			CreateJoint();

			// HACK: if we still dont have a Joint after onEnabled is called delay the call to createJoint one frame. This will allow the otherBody
			// to be initialized and have the Body created.
			if (_joint == null)
				Core.Schedule(0, this, t => (t.Context as FSJoint).CreateJoint());
		}


		public override void OnDisabled()
		{
			DestroyJoint();
		}

		#endregion


		internal void InitializeJointDef(FSJointDef jointDef)
		{
			jointDef.BodyA = _ownerBody?.Body;
			jointDef.BodyB = _otherBody?.Body;
			jointDef.CollideConnected = _collideConnected;
		}


		internal abstract FSJointDef GetJointDef();


		protected void RecreateJoint()
		{
			if (_attachedJoint != null)
				_attachedJoint.DestroyJoint();

			DestroyJoint();
			CreateJoint();

			if (_attachedJoint != null)
				_attachedJoint.CreateJoint();
		}


		internal void CreateJoint()
		{
			if (_joint != null)
				return;

			var jointDef = GetJointDef();
			if (jointDef == null)
				return;

			_joint = jointDef.CreateJoint();
			jointDef.BodyA.World.AddJoint(_joint);
		}


		internal void DestroyJoint()
		{
			if (_joint == null)
				return;

			if (_ownerBody != null)
				_ownerBody._joints.Remove(this);

			if (_otherBody != null)
				_otherBody._joints.Remove(this);

			_joint.BodyA.World.RemoveJoint(_joint);
			_joint = null;
		}
	}
}