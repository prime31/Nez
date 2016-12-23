using FarseerPhysics.Dynamics;


namespace Nez.Farseer
{
	public abstract class FSCollisionShape : Component
	{
		internal FSFixtureDef _fixtureDef = new FSFixtureDef();
		protected Fixture _fixture;


		#region Configuration

		public FSCollisionShape setFriction( float friction )
		{
			_fixtureDef.friction = friction;
			if( _fixture != null )
			{
				_fixture.friction = friction;

				var body = this.getComponent<FSRigidBody>().body;
				var contactEdge = body.contactList;
				while( contactEdge != null )
				{
					var contact = contactEdge.contact;
					if( contact.fixtureA == _fixture || contact.fixtureB == _fixture )
						contact.resetFriction();
					contactEdge = contactEdge.next;
				}
			}

			return this;
		}


		public FSCollisionShape setRestitution( float restitution )
		{
			_fixtureDef.restitution = restitution;
			if( _fixture != null )
			{
				_fixture.restitution = restitution;

				var body = this.getComponent<FSRigidBody>().body;
				var contactEdge = body.contactList;
				while( contactEdge != null )
				{
					var contact = contactEdge.contact;
					if( contact.fixtureA == _fixture || contact.fixtureB == _fixture )
						contact.resetRestitution();
					contactEdge = contactEdge.next;
				}
			}
			return this;
		}


		public FSCollisionShape setDensity( float density )
		{
			_fixtureDef.density = density;
			if( _fixture != null )
				_fixture.shape.density = density;
			return this;
		}


		public FSCollisionShape setIsSensor( bool isSensor )
		{
			_fixtureDef.isSensor = isSensor;
			if( _fixture != null )
				_fixture.isSensor = isSensor;
			return this;
		}


		public FSCollisionShape setCollidesWith( Category collidesWith )
		{
			_fixtureDef.collidesWith = collidesWith;
			if( _fixture != null )
				_fixture.collidesWith = collidesWith;
			return this;
		}


		public FSCollisionShape setCollisionCategories( Category collisionCategories )
		{
			_fixtureDef.collisionCategories = collisionCategories;
			if( _fixture != null )
				_fixture.collisionCategories = collisionCategories;
			return this;
		}


		public FSCollisionShape setIgnoreCCDWith( Category ignoreCCDWith )
		{
			_fixtureDef.ignoreCCDWith = ignoreCCDWith;
			if( _fixture != null )
				_fixture.ignoreCCDWith = ignoreCCDWith;
			return this;
		}


		public FSCollisionShape setCollisionGroup( short collisionGroup )
		{
			_fixtureDef.collisionGroup = collisionGroup;
			if( _fixture != null )
				_fixture.collisionGroup = collisionGroup;
			return this;
		}

		#endregion


		#region Component lifecycle

		public override void onAddedToEntity()
		{
			createFixture();
		}


		public override void onRemovedFromEntity()
		{
			destroyFixture();
		}


		public override void onEnabled()
		{
			createFixture();
		}


		public override void onDisabled()
		{
			destroyFixture();
		}

		#endregion


		/// <summary>
		/// wakes any contacting bodies. Useful when creating a fixture or changing something that won't trigger the bodies to wake themselves
		/// such as Circle.center.
		/// </summary>
		protected void wakeAnyContactingBodies()
		{
			var body = this.getComponent<FSRigidBody>().body;
			var contactEdge = body.contactList;
			while( contactEdge != null )
			{
				var contact = contactEdge.contact;
				if( contact.fixtureA == _fixture || contact.fixtureB == _fixture )
				{
					contact.fixtureA.body.isAwake = true;
					contact.fixtureB.body.isAwake = true;
				}
				contactEdge = contactEdge.next;
			}	
		}


		internal virtual void createFixture()
		{
			if( _fixture != null )
				return;

			var rigidBody = this.getComponent<FSRigidBody>();
			if( rigidBody == null || rigidBody.body == null )
				return;

			var body = rigidBody.body;
			_fixtureDef.shape.density = _fixtureDef.density;
			_fixture = body.createFixture( _fixtureDef.shape, this );
			_fixture.friction = _fixtureDef.friction;
			_fixture.restitution = _fixtureDef.restitution;
			_fixture.isSensor = _fixtureDef.isSensor;
			_fixture.collidesWith = _fixtureDef.collidesWith;
			_fixture.collisionCategories = _fixtureDef.collisionCategories;
			_fixture.ignoreCCDWith = _fixtureDef.ignoreCCDWith;
			_fixture.collisionGroup = _fixtureDef.collisionGroup;
		}


		internal virtual void destroyFixture()
		{
			if( _fixture == null )
				return;

			var rigidBody = this.getComponent<FSRigidBody>();
			if( rigidBody == null || rigidBody.body == null )
				return;

			rigidBody.body.destroyFixture( _fixture );
			_fixture = null;
		}

	}
}
