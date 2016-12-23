using System.Collections.Generic;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSRigidBody : Component, IUpdatable
	{
		public Body body;

		FSBodyDef _bodyDef = new FSBodyDef();
		bool _ignoreTransformChanges;
		internal List<FSJoint> _joints = new List<FSJoint>();


		#region Configuration

		public FSRigidBody setBodyType( BodyType bodyType )
		{
			if( body != null )
				body.bodyType = bodyType;
			else
				_bodyDef.bodyType = bodyType;
			return this;
		}


		public FSRigidBody setLinearVelocity( Vector2 linearVelocity )
		{
			if( body != null )
				body.linearVelocity = linearVelocity;
			else
				_bodyDef.linearVelocity = linearVelocity;
			return this;
		}


		public FSRigidBody setAngularVelocity( float angularVelocity )
		{
			if( body != null )
				body.angularVelocity = angularVelocity;
			else
				_bodyDef.angularVelocity = angularVelocity;
			return this;
		}


		public FSRigidBody setLinearDamping( float linearDamping )
		{
			if( body != null )
				body.linearDamping = linearDamping;
			else
				_bodyDef.linearDamping = linearDamping;
			return this;
		}


		public FSRigidBody setAngularDamping( float angularDamping )
		{
			if( body != null )
				body.angularDamping = angularDamping;
			else
				_bodyDef.angularDamping = angularDamping;
			return this;
		}


		public FSRigidBody setIsBullet( bool isBullet )
		{
			if( body != null )
				body.isBullet = isBullet;
			else
				_bodyDef.isBullet = isBullet;
			return this;
		}


		public FSRigidBody setIsSleepingAllowed( bool isSleepingAllowed )
		{
			if( body != null )
				body.isSleepingAllowed = isSleepingAllowed;
			else
				_bodyDef.isSleepingAllowed = isSleepingAllowed;
			return this;
		}


		public FSRigidBody setIsAwake( bool isAwake )
		{
			if( body != null )
				body.isAwake = isAwake;
			else
				_bodyDef.isAwake = isAwake;
			return this;
		}


		public FSRigidBody setFixedRotation( bool fixedRotation )
		{
			if( body != null )
				body.fixedRotation = fixedRotation;
			else
				_bodyDef.fixedRotation = fixedRotation;
			return this;
		}


		public FSRigidBody setIgnoreGravity( bool ignoreGravity )
		{
			if( body != null )
				body.ignoreGravity = ignoreGravity;
			else
				_bodyDef.ignoreGravity = ignoreGravity;
			return this;
		}


		public FSRigidBody setGravityScale( float gravityScale )
		{
			if( body != null )
				body.gravityScale = gravityScale;
			else
				_bodyDef.gravityScale = gravityScale;
			return this;
		}


		public FSRigidBody setMass( float mass )
		{
			if( body != null )
				body.mass = mass;
			else
				_bodyDef.mass = mass;
			return this;
		}


		public FSRigidBody setInertia( float inertia )
		{
			if( body != null )
				body.inertia = inertia;
			else
				_bodyDef.inertia = inertia;
			return this;
		}

		#endregion


		#region Component lifecycle

		public override void initialize()
		{
			createBody();
		}


		public override void onAddedToEntity()
		{
			createBody();
		}


		public override void onRemovedFromEntity()
		{
			destroyBody();
		}


		public override void onEnabled()
		{
			if( body != null )
				body.enabled = true;
		}


		public override void onDisabled()
		{
			if( body != null )
				body.enabled = false;
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			if( _ignoreTransformChanges || body == null )
				return;

			if( comp == Transform.Component.Position )
				body.position = transform.position * FSConvert.displayToSim;
			else if( comp == Transform.Component.Rotation )
				body.rotation = transform.rotation;
		}

		#endregion


		void IUpdatable.update()
		{
			if( body == null || !body.isAwake )
				return;

			_ignoreTransformChanges = true;
			transform.position = FSConvert.simToDisplay * body.position;
			transform.rotation = body.rotation;
			_ignoreTransformChanges = false;
		}


		void createBody()
		{
			if( body != null )
				return;
			
			var world = entity.scene.getOrCreateSceneComponent<FSWorld>();
			body = new Body( world, transform.position * FSConvert.displayToSim, transform.rotation, _bodyDef.bodyType, this );
			body.linearVelocity = _bodyDef.linearVelocity;
			body.angularVelocity = _bodyDef.angularVelocity;
			body.linearDamping = _bodyDef.linearDamping;
			body.angularDamping = _bodyDef.angularDamping;

			body.isBullet = _bodyDef.isBullet;
			body.isSleepingAllowed = _bodyDef.isSleepingAllowed;
			body.isAwake = _bodyDef.isAwake;
			body.enabled = enabled;
			body.fixedRotation = _bodyDef.fixedRotation;
			body.ignoreGravity = _bodyDef.ignoreGravity;
			body.mass = _bodyDef.mass;
			body.inertia = _bodyDef.inertia;

			var collisionShapes = entity.getComponents<FSCollisionShape>();
			for( var i = 0; i < collisionShapes.Count; i++ )
				collisionShapes[i].createFixture();
			ListPool<FSCollisionShape>.free( collisionShapes );

			for( var i = 0; i < _joints.Count; i++ )
				_joints[i].createJoint();
		}


		void destroyBody()
		{
			for( var i = 0; i < _joints.Count; i++ )
				_joints[i].destroyJoint();
			_joints.Clear();

			var collisionShapes = entity.getComponents<FSCollisionShape>();
			for( var i = 0; i < collisionShapes.Count; i++ )
				collisionShapes[i].destroyFixture();
			ListPool<FSCollisionShape>.free( collisionShapes );

			body.world.removeBody( body );
			body = null;
		}

	}
}
