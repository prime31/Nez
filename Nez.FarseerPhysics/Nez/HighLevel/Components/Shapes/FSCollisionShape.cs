using FarseerPhysics.Dynamics;


namespace Nez.Farseer
{
	public abstract class FSCollisionShape : Component
	{
		internal FSFixtureDef _fixtureDef = new FSFixtureDef();
		protected Fixture _fixture;


		#region Configuration

		public FSCollisionShape SetFriction(float friction)
		{
			_fixtureDef.Friction = friction;
			if (_fixture != null)
			{
				_fixture.Friction = friction;

				var body = this.GetComponent<FSRigidBody>().Body;
				var contactEdge = body.ContactList;
				while (contactEdge != null)
				{
					var contact = contactEdge.Contact;
					if (contact.FixtureA == _fixture || contact.FixtureB == _fixture)
						contact.ResetFriction();
					contactEdge = contactEdge.Next;
				}
			}

			return this;
		}


		public FSCollisionShape SetRestitution(float restitution)
		{
			_fixtureDef.Restitution = restitution;
			if (_fixture != null)
			{
				_fixture.Restitution = restitution;

				var body = this.GetComponent<FSRigidBody>().Body;
				var contactEdge = body.ContactList;
				while (contactEdge != null)
				{
					var contact = contactEdge.Contact;
					if (contact.FixtureA == _fixture || contact.FixtureB == _fixture)
						contact.ResetRestitution();
					contactEdge = contactEdge.Next;
				}
			}

			return this;
		}


		public FSCollisionShape SetDensity(float density)
		{
			_fixtureDef.Density = density;
			if (_fixture != null)
				_fixture.Shape.Density = density;
			return this;
		}


		public FSCollisionShape SetIsSensor(bool isSensor)
		{
			_fixtureDef.IsSensor = isSensor;
			if (_fixture != null)
				_fixture.IsSensor = isSensor;
			return this;
		}


		public FSCollisionShape SetCollidesWith(Category collidesWith)
		{
			_fixtureDef.CollidesWith = collidesWith;
			if (_fixture != null)
				_fixture.CollidesWith = collidesWith;
			return this;
		}


		public FSCollisionShape SetCollisionCategories(Category collisionCategories)
		{
			_fixtureDef.CollisionCategories = collisionCategories;
			if (_fixture != null)
				_fixture.CollisionCategories = collisionCategories;
			return this;
		}


		public FSCollisionShape SetIgnoreCCDWith(Category ignoreCCDWith)
		{
			_fixtureDef.IgnoreCCDWith = ignoreCCDWith;
			if (_fixture != null)
				_fixture.IgnoreCCDWith = ignoreCCDWith;
			return this;
		}


		public FSCollisionShape SetCollisionGroup(short collisionGroup)
		{
			_fixtureDef.CollisionGroup = collisionGroup;
			if (_fixture != null)
				_fixture.CollisionGroup = collisionGroup;
			return this;
		}

		#endregion


		#region Component lifecycle

		public override void OnAddedToEntity()
		{
			CreateFixture();
		}


		public override void OnRemovedFromEntity()
		{
			DestroyFixture();
		}


		public override void OnEnabled()
		{
			CreateFixture();
		}


		public override void OnDisabled()
		{
			DestroyFixture();
		}

		#endregion


		/// <summary>
		/// wakes any contacting bodies. Useful when creating a fixture or changing something that won't trigger the bodies to wake themselves
		/// such as Circle.center.
		/// </summary>
		protected void WakeAnyContactingBodies()
		{
			var body = this.GetComponent<FSRigidBody>().Body;
			var contactEdge = body.ContactList;
			while (contactEdge != null)
			{
				var contact = contactEdge.Contact;
				if (contact.FixtureA == _fixture || contact.FixtureB == _fixture)
				{
					contact.FixtureA.Body.IsAwake = true;
					contact.FixtureB.Body.IsAwake = true;
				}

				contactEdge = contactEdge.Next;
			}
		}


		internal virtual void CreateFixture()
		{
			if (_fixture != null)
				return;

			var rigidBody = this.GetComponent<FSRigidBody>();
			if (rigidBody == null || rigidBody.Body == null)
				return;

			var body = rigidBody.Body;
			_fixtureDef.Shape.Density = _fixtureDef.Density;
			_fixture = body.CreateFixture(_fixtureDef.Shape, this);
			_fixture.Friction = _fixtureDef.Friction;
			_fixture.Restitution = _fixtureDef.Restitution;
			_fixture.IsSensor = _fixtureDef.IsSensor;
			_fixture.CollidesWith = _fixtureDef.CollidesWith;
			_fixture.CollisionCategories = _fixtureDef.CollisionCategories;
			_fixture.IgnoreCCDWith = _fixtureDef.IgnoreCCDWith;
			_fixture.CollisionGroup = _fixtureDef.CollisionGroup;
		}


		internal virtual void DestroyFixture()
		{
			if (_fixture == null)
				return;

			var rigidBody = this.GetComponent<FSRigidBody>();
			if (rigidBody == null || rigidBody.Body == null)
				return;

			rigidBody.Body.DestroyFixture(_fixture);
			_fixture = null;
		}
	}
}