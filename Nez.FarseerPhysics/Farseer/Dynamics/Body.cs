/*
* Farseer Physics Engine:
* Copyright (c) 2012 Ian Qvist
* 
* Original source Box2D:
* Copyright (c) 2006-2011 Erin Catto http://www.box2d.org 
* 
* This software is provided 'as-is', without any express or implied 
* warranty.  In no event will the authors be held liable for any damages 
* arising from the use of this software. 
* Permission is granted to anyone to use this software for any purpose, 
* including commercial applications, and to alter it and redistribute it 
* freely, subject to the following restrictions: 
* 1. The origin of this software must not be misrepresented; you must not 
* claim that you wrote the original software. If you use this software 
* in a product, an acknowledgment in the product documentation would be 
* appreciated but is not required. 
* 2. Altered source versions must be plainly marked as such, and must not be 
* misrepresented as being the original software. 
* 3. This notice may not be removed or altered from any source distribution. 
*/
//#define USE_AWAKE_BODY_SET

using System;
using System.Collections.Generic;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Common.PhysicsLogic;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;
using Nez.Farseer;


namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// The body type.
	/// </summary>
	public enum BodyType
	{
		/// <summary>
		/// Zero velocity, may be manually moved. Note: even static bodies have mass.
		/// </summary>
		Static,

		/// <summary>
		/// Zero mass, non-zero velocity set by user, moved by solver
		/// </summary>
		Kinematic,

		/// <summary>
		/// Positive mass, non-zero velocity determined by forces, moved by solver
		/// </summary>
		Dynamic,
	}

	public class Body : IDisposable
	{
		#region Properties/Fields/Events

		public PhysicsLogicFilter physicsLogicFilter;
		public ControllerFilter controllerFilter;

		/// <summary>
		/// A unique id for this body.
		/// </summary>
		public int bodyId { get; private set; }

		public int islandIndex;

		/// <summary>
		/// Scale the gravity applied to this body.
		/// Defaults to 1. A value of 2 means double the gravity is applied to this body.
		/// </summary>
		public float gravityScale;

		public World world { get { return _world; } }

		/// <summary>
		/// Set the user data. Use this to store your application specific data.
		/// </summary>
		/// <value>The user data.</value>
		public object userData;

		/// <summary>
		/// Gets the total number revolutions the body has made.
		/// </summary>
		/// <value>The revolutions.</value>
		public float revolutions
		{
			get { return rotation / (float)Math.PI; }
		}

		/// <summary>
		/// Gets or sets the body type.
		/// Warning: Calling this mid-update might cause a crash.
		/// </summary>
		/// <value>The type of body.</value>
		public BodyType bodyType
		{
			get { return _bodyType; }
			set
			{
				if( _bodyType == value )
					return;

				_bodyType = value;

				resetMassData();

				if( _bodyType == BodyType.Static )
				{
					_linearVelocity = Vector2.Zero;
					_angularVelocity = 0.0f;
					_sweep.a0 = _sweep.a;
					_sweep.c0 = _sweep.c;
					synchronizeFixtures();
				}

				isAwake = true;

				_force = Vector2.Zero;
				_torque = 0.0f;

				// Delete the attached contacts.
				var ce = contactList;
				while( ce != null )
				{
					var ce0 = ce;
					ce = ce.next;
					_world.contactManager.destroy( ce0.contact );
				}

				contactList = null;

				// Touch the proxies so that new contacts will be created (when appropriate)
				var broadPhase = _world.contactManager.broadPhase;
				foreach( Fixture fixture in fixtureList )
				{
					var proxyCount = fixture.proxyCount;
					for( var j = 0; j < proxyCount; j++ )
						broadPhase.touchProxy( fixture.proxies[j].proxyId );
				}
			}
		}

		/// <summary>
		/// Get or sets the linear velocity of the center of mass.
		/// </summary>
		/// <value>The linear velocity.</value>
		public Vector2 linearVelocity
		{
			set
			{
				Debug.Assert( !float.IsNaN( value.X ) && !float.IsNaN( value.Y ) );

				if( _bodyType == BodyType.Static )
					return;

				if( Vector2.Dot( value, value ) > 0.0f )
					isAwake = true;

				_linearVelocity = value;
			}
			get { return _linearVelocity; }
		}

		/// <summary>
		/// Gets or sets the angular velocity. Radians/second.
		/// </summary>
		/// <value>The angular velocity.</value>
		public float angularVelocity
		{
			set
			{
				Debug.Assert( !float.IsNaN( value ) );

				if( _bodyType == BodyType.Static )
					return;

				if( value * value > 0.0f )
					isAwake = true;

				_angularVelocity = value;
			}
			get { return _angularVelocity; }
		}

		/// <summary>
		/// Gets or sets the linear damping.
		/// </summary>
		/// <value>The linear damping.</value>
		public float linearDamping
		{
			get { return _linearDamping; }
			set
			{
				Debug.Assert( !float.IsNaN( value ) );
				_linearDamping = value;
			}
		}

		/// <summary>
		/// Gets or sets the angular damping.
		/// </summary>
		/// <value>The angular damping.</value>
		public float angularDamping
		{
			get { return _angularDamping; }
			set
			{
				Debug.Assert( !float.IsNaN( value ) );
				_angularDamping = value;
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this body should be included in the CCD solver.
		/// </summary>
		/// <value><c>true</c> if this instance is included in CCD; otherwise, <c>false</c>.</value>
		public bool isBullet;

		/// <summary>
		/// You can disable sleeping on this body. If you disable sleeping, the
		/// body will be woken.
		/// </summary>
		/// <value><c>true</c> if sleeping is allowed; otherwise, <c>false</c>.</value>
		public bool isSleepingAllowed
		{
			set
			{
				if( !value )
					isAwake = true;

				_sleepingAllowed = value;
			}
			get { return _sleepingAllowed; }
		}

		/// <summary>
		/// Set the sleep state of the body. A sleeping body has very
		/// low CPU cost.
		/// </summary>
		/// <value><c>true</c> if awake; otherwise, <c>false</c>.</value>
		public bool isAwake
		{
			set
			{
				if( value )
				{
					if( !_awake )
					{
						_sleepTime = 0.0f;
						_world.contactManager.updateContacts( contactList, true );
#if USE_AWAKE_BODY_SET
						if (InWorld && !World.AwakeBodySet.Contains(this))
						{
							World.AwakeBodySet.Add(this);
						}
#endif
					}
				}
				else
				{
#if USE_AWAKE_BODY_SET
					// Check even for BodyType.Static because if this body had just been changed to Static it will have
					// set Awake = false in the process.
					if (InWorld && World.AwakeBodySet.Contains(this))
					{
						World.AwakeBodySet.Remove(this);
					}
#endif
					resetDynamics();
					_sleepTime = 0.0f;
					_world.contactManager.updateContacts( contactList, false );
				}

				_awake = value;
			}
			get { return _awake; }
		}

		/// <summary>
		/// Set the active state of the body. An inactive body is not simulated and cannot be collided with or woken up.
		/// If you pass a flag of true, all fixtures will be added to the broad-phase.
		/// If you pass a flag of false, all fixtures will be removed from the broad-phase and all contacts will be destroyed.
		/// Fixtures and joints are otherwise unaffected. You may continue to create/destroy fixtures and joints on inactive bodies.
		/// Fixtures on an inactive body are implicitly inactive and will not participate in collisions, ray-casts, or queries.
		/// Joints connected to an inactive body are implicitly inactive. An inactive body is still owned by a b2World object and remains
		/// in the body list.
		/// </summary>
		/// <value><c>true</c> if active; otherwise, <c>false</c>.</value>
		public bool enabled
		{
			set
			{
				if( value == _enabled )
					return;

				if( value )
				{
					// Create all proxies.
					var broadPhase = _world.contactManager.broadPhase;
					for( int i = 0; i < fixtureList.Count; i++ )
						fixtureList[i].createProxies( broadPhase, ref _xf );

					// Contacts are created the next time step.
				}
				else
				{
					// Destroy all proxies.
					var broadPhase = _world.contactManager.broadPhase;

					for( int i = 0; i < fixtureList.Count; i++ )
						fixtureList[i].destroyProxies( broadPhase );

					// Destroy the attached contacts.
					var ce = contactList;
					while( ce != null )
					{
						var ce0 = ce;
						ce = ce.next;
						_world.contactManager.destroy( ce0.contact );
					}
					contactList = null;
				}

				_enabled = value;
			}
			get { return _enabled; }
		}

		/// <summary>
		/// Set this body to have fixed rotation. This causes the mass
		/// to be reset.
		/// </summary>
		/// <value><c>true</c> if it has fixed rotation; otherwise, <c>false</c>.</value>
		public bool fixedRotation
		{
			set
			{
				if( _fixedRotation == value )
					return;

				_fixedRotation = value;

				_angularVelocity = 0f;
				resetMassData();
			}
			get { return _fixedRotation; }
		}

		/// <summary>
		/// Gets all the fixtures attached to this body.
		/// </summary>
		/// <value>The fixture list.</value>
		public List<Fixture> fixtureList { get; internal set; }

		/// <summary>
		/// Get the list of all joints attached to this body.
		/// </summary>
		/// <value>The joint list.</value>
		public JointEdge jointList { get; internal set; }

		/// <summary>
		/// Get the list of all contacts attached to this body.
		/// Warning: this list changes during the time step and you may
		/// miss some collisions if you don't use ContactListener.
		/// </summary>
		/// <value>The contact list.</value>
		public ContactEdge contactList { get; internal set; }

		/// <summary>
		/// Get the world body origin position.
		/// </summary>
		/// <returns>Return the world position of the body's origin.</returns>
		public Vector2 position
		{
			get { return _xf.p; }
			set
			{
				Debug.Assert( !float.IsNaN( value.X ) && !float.IsNaN( value.Y ) );
				setTransform( ref value, rotation );
			}
		}

		/// <summary>
		/// Get/set the world body origin position in display units.
		/// </summary>
		/// <returns>Return the world position of the body's origin.</returns>
		public Vector2 displayPosition
		{
			get { return _xf.p * FSConvert.simToDisplay; }
			set { position = value * FSConvert.displayToSim; }
		}

		/// <summary>
		/// Get the angle in radians.
		/// </summary>
		/// <returns>Return the current world rotation angle in radians.</returns>
		public float rotation
		{
			get { return _sweep.a; }
			set
			{
				Debug.Assert( !float.IsNaN( value ) );
				setTransform( ref _xf.p, value );
			}
		}

		/// <summary>
		/// Gets or sets a value indicating whether this body is static.
		/// </summary>
		/// <value><c>true</c> if this instance is static; otherwise, <c>false</c>.</value>
		public bool isStatic
		{
			get { return _bodyType == BodyType.Static; }
			set { bodyType = value ? BodyType.Static : BodyType.Dynamic; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this body is kinematic.
		/// </summary>
		/// <value><c>true</c> if this instance is kinematic; otherwise, <c>false</c>.</value>
		public bool isKinematic
		{
			get { return _bodyType == BodyType.Kinematic; }
			set { bodyType = value ? BodyType.Kinematic : BodyType.Dynamic; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this body is dynamic.
		/// </summary>
		/// <value><c>true</c> if this instance is dynamic; otherwise, <c>false</c>.</value>
		public bool isDynamic
		{
			get { return _bodyType == BodyType.Dynamic; }
			set { bodyType = value ? BodyType.Dynamic : BodyType.Kinematic; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this body ignores gravity.
		/// </summary>
		/// <value><c>true</c> if  it ignores gravity; otherwise, <c>false</c>.</value>
		public bool ignoreGravity;

		/// <summary>
		/// Get the world position of the center of mass.
		/// </summary>
		/// <value>The world position.</value>
		public Vector2 worldCenter { get { return _sweep.c; } }

		/// <summary>
		/// Get the local position of the center of mass.
		/// </summary>
		/// <value>The local position.</value>
		public Vector2 localCenter
		{
			get { return _sweep.localCenter; }
			set
			{
				if( _bodyType != BodyType.Dynamic )
					return;

				// Move center of mass.
				var oldCenter = _sweep.c;
				_sweep.localCenter = value;
				_sweep.c0 = _sweep.c = MathUtils.mul( ref _xf, ref _sweep.localCenter );

				// Update center of mass velocity.
				var a = _sweep.c - oldCenter;
				_linearVelocity += new Vector2( -_angularVelocity * a.Y, _angularVelocity * a.X );
			}
		}

		/// <summary>
		/// Gets or sets the mass. Usually in kilograms (kg).
		/// </summary>
		/// <value>The mass.</value>
		public float mass
		{
			get { return _mass; }
			set
			{
				Debug.Assert( !float.IsNaN( value ) );

				if( _bodyType != BodyType.Dynamic ) //Make an assert
					return;

				_mass = value;

				if( _mass <= 0.0f )
					_mass = 1.0f;

				_invMass = 1.0f / _mass;
			}
		}

		/// <summary>
		/// Get or set the rotational inertia of the body about the local origin. usually in kg-m^2.
		/// </summary>
		/// <value>The inertia.</value>
		public float inertia
		{
			get { return _inertia + mass * Vector2.Dot( _sweep.localCenter, _sweep.localCenter ); }
			set
			{
				Debug.Assert( !float.IsNaN( value ) );

				if( _bodyType != BodyType.Dynamic ) //Make an assert
					return;

				if( value > 0.0f && !_fixedRotation ) //Make an assert
				{
					_inertia = value - mass * Vector2.Dot( localCenter, localCenter );
					Debug.Assert( _inertia > 0.0f );
					_invI = 1.0f / _inertia;
				}
			}
		}

		public float restitution
		{
			get
			{
				float res = 0;

				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					res += f.restitution;
				}

				return fixtureList.Count > 0 ? res / fixtureList.Count : 0;
			}
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					f.restitution = value;
				}
			}
		}

		public float friction
		{
			get
			{
				float res = 0;

				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					res += f.friction;
				}

				return fixtureList.Count > 0 ? res / fixtureList.Count : 0;
			}
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					f.friction = value;
				}
			}
		}

		public Category collisionCategories
		{
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					f.collisionCategories = value;
				}
			}
		}

		public Category collidesWith
		{
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					f.collidesWith = value;
				}
			}
		}

		/// <summary>
		/// Body objects can define which categories of bodies they wish to ignore CCD with. 
		/// This allows certain bodies to be configured to ignore CCD with objects that
		/// aren't a penetration problem due to the way content has been prepared.
		/// This is compared against the other Body's fixture CollisionCategories within World.SolveTOI().
		/// </summary>
		public Category ignoreCCDWith
		{
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					var f = fixtureList[i];
					f.ignoreCCDWith = value;
				}
			}
		}

		public short collisionGroup
		{
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					Fixture f = fixtureList[i];
					f.collisionGroup = value;
				}
			}
		}

		public bool isSensor
		{
			set
			{
				for( int i = 0; i < fixtureList.Count; i++ )
				{
					Fixture f = fixtureList[i];
					f.isSensor = value;
				}
			}
		}

		public bool ignoreCCD;

		/// <summary>
		/// wires up the onCollision event for every fixture on the Body
		/// </summary>
		public event OnCollisionEventHandler onCollision
		{
			add
			{
				for( int i = 0; i < fixtureList.Count; i++ )
					fixtureList[i].onCollision += value;
			}
			remove
			{
				for( int i = 0; i < fixtureList.Count; i++ )
					fixtureList[i].onCollision -= value;
			}
		}

		/// <summary>
		/// wires up the onSeparation event for every fixture on the Body
		/// </summary>
		public event OnSeparationEventHandler onSeparation
		{
			add
			{
				for( int i = 0; i < fixtureList.Count; i++ )
					fixtureList[i].onSeparation += value;
			}
			remove
			{
				for( int i = 0; i < fixtureList.Count; i++ )
					fixtureList[i].onSeparation -= value;
			}
		}

		[ThreadStatic]
		static int _bodyIdCounter;

		float _angularDamping;
		BodyType _bodyType;
		float _inertia;
		float _linearDamping;
		float _mass;
		bool _sleepingAllowed;
		bool _awake;
		bool _fixedRotation;

		internal bool _enabled;
		internal float _angularVelocity;
		internal Vector2 _linearVelocity;
		internal Vector2 _force;
		internal float _invI;
		internal float _invMass;
		internal float _sleepTime;
		internal Sweep _sweep; // the swept motion for CCD
		internal float _torque;
		internal World _world;
		internal Transform _xf; // the body origin transform
		internal bool _island;

		#endregion


		public Body( World world, Vector2 position = new Vector2(), float rotation = 0, BodyType bodyType = BodyType.Static, object userdata = null )
		{
			fixtureList = new List<Fixture>();
			bodyId = _bodyIdCounter++;

			_world = world;
			_enabled = true;
			_awake = true;
			_sleepingAllowed = true;

			userData = userdata;
			gravityScale = 1.0f;
			this.bodyType = bodyType;

			_xf.q.Set( rotation );

			//FPE: optimization
			if( position != Vector2.Zero )
			{
				_xf.p = position;
				_sweep.c0 = _xf.p;
				_sweep.c = _xf.p;
			}

			//FPE: optimization
			if( rotation != 0 )
			{
				_sweep.a0 = rotation;
				_sweep.a = rotation;
			}

			world.addBody( this ); //FPE note: bodies can't live without a World
		}


		/// <summary>
		/// Resets the dynamics of this body.
		/// Sets torque, force and linear/angular velocity to 0
		/// </summary>
		public void resetDynamics()
		{
			_torque = 0;
			_angularVelocity = 0;
			_force = Vector2.Zero;
			_linearVelocity = Vector2.Zero;
		}

		/// <summary>
		/// Creates a fixture and attach it to this body.
		/// If the density is non-zero, this function automatically updates the mass of the body.
		/// Contacts are not created until the next time step.
		/// Warning: This function is locked during callbacks.
		/// </summary>
		/// <param name="shape">The shape.</param>
		/// <param name="userData">Application specific data</param>
		/// <returns></returns>
		public Fixture createFixture( Shape shape, object userData = null )
		{
			return new Fixture( this, shape, userData );
		}

		/// <summary>
		/// Destroy a fixture. This removes the fixture from the broad-phase and
		/// destroys all contacts associated with this fixture. This will
		/// automatically adjust the mass of the body if the body is dynamic and the
		/// fixture has positive density.
		/// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
		/// Warning: This function is locked during callbacks.
		/// </summary>
		/// <param name="fixture">The fixture to be removed.</param>
		public void destroyFixture( Fixture fixture )
		{
			Debug.Assert( fixture.body == this );

			// Remove the fixture from this body's singly linked list.
			Debug.Assert( fixtureList.Count > 0 );

			// You tried to remove a fixture that not present in the fixturelist.
			Debug.Assert( fixtureList.Contains( fixture ) );

			// Destroy any contacts associated with the fixture.
			ContactEdge edge = contactList;
			while( edge != null )
			{
				var c = edge.contact;
				edge = edge.next;

				var fixtureA = c.fixtureA;
				var fixtureB = c.fixtureB;

				if( fixture == fixtureA || fixture == fixtureB )
				{
					// This destroys the contact and removes it from
					// this body's contact list.
					_world.contactManager.destroy( c );
				}
			}

			if( _enabled )
			{
				var broadPhase = _world.contactManager.broadPhase;
				fixture.destroyProxies( broadPhase );
			}

			fixtureList.Remove( fixture );
			fixture.destroy();
			fixture.body = null;

			resetMassData();
		}

		/// <summary>
		/// Set the position of the body's origin and rotation.
		/// This breaks any contacts and wakes the other bodies.
		/// Manipulating a body's transform may cause non-physical behavior.
		/// </summary>
		/// <param name="position">The world position of the body's local origin.</param>
		/// <param name="rotation">The world rotation in radians.</param>
		public void setTransform( ref Vector2 position, float rotation )
		{
			setTransformIgnoreContacts( ref position, rotation );
			_world.contactManager.findNewContacts();
		}

		/// <summary>
		/// Set the position of the body's origin and rotation.
		/// This breaks any contacts and wakes the other bodies.
		/// Manipulating a body's transform may cause non-physical behavior.
		/// </summary>
		/// <param name="position">The world position of the body's local origin.</param>
		/// <param name="rotation">The world rotation in radians.</param>
		public void setTransform( Vector2 position, float rotation )
		{
			setTransform( ref position, rotation );
		}

		/// <summary>
		/// For teleporting a body without considering new contacts immediately.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="angle">The angle.</param>
		public void setTransformIgnoreContacts( ref Vector2 position, float angle )
		{
			_xf.q.Set( angle );
			_xf.p = position;

			_sweep.c = MathUtils.mul( ref _xf, _sweep.localCenter );
			_sweep.a = angle;

			_sweep.c0 = _sweep.c;
			_sweep.a0 = angle;

			var broadPhase = _world.contactManager.broadPhase;
			for( var i = 0; i < fixtureList.Count; i++ )
				fixtureList[i].synchronize( broadPhase, ref _xf, ref _xf );
		}

		/// <summary>
		/// Get the body transform for the body's origin.
		/// </summary>
		/// <param name="transform">The transform of the body's origin.</param>
		public void getTransform( out Transform transform )
		{
			transform = _xf;
		}

		/// <summary>
		/// This resets the mass properties to the sum of the mass properties of the fixtures.
		/// This normally does not need to be called unless you called SetMassData to override
		/// the mass and you later want to reset the mass.
		/// </summary>
		public void resetMassData()
		{
			// Compute mass data from shapes. Each shape has its own density.
			_mass = 0.0f;
			_invMass = 0.0f;
			_inertia = 0.0f;
			_invI = 0.0f;
			_sweep.localCenter = Vector2.Zero;

			// Kinematic bodies have zero mass.
			if( bodyType == BodyType.Kinematic )
			{
				_sweep.c0 = _xf.p;
				_sweep.c = _xf.p;
				_sweep.a0 = _sweep.a;
				return;
			}

			Debug.Assert( bodyType == BodyType.Dynamic || bodyType == BodyType.Static );

			// Accumulate mass over all fixtures.
			Vector2 localCenter = Vector2.Zero;
			foreach( Fixture f in fixtureList )
			{
				if( f.shape._density == 0 )
				{
					continue;
				}

				var massData = f.shape.massData;
				_mass += massData.mass;
				localCenter += massData.mass * massData.centroid;
				_inertia += massData.inertia;
			}

			//FPE: Static bodies only have mass, they don't have other properties. A little hacky tho...
			if( bodyType == BodyType.Static )
			{
				_sweep.c0 = _sweep.c = _xf.p;
				return;
			}

			// Compute center of mass.
			if( _mass > 0.0f )
			{
				_invMass = 1.0f / _mass;
				localCenter *= _invMass;
			}
			else
			{
				// Force all dynamic bodies to have a positive mass.
				_mass = 1.0f;
				_invMass = 1.0f;
			}

			if( _inertia > 0.0f && !_fixedRotation )
			{
				// Center the inertia about the center of mass.
				_inertia -= _mass * Vector2.Dot( localCenter, localCenter );

				Debug.Assert( _inertia > 0.0f );
				_invI = 1.0f / _inertia;
			}
			else
			{
				_inertia = 0.0f;
				_invI = 0.0f;
			}

			// Move center of mass.
			var oldCenter = _sweep.c;
			_sweep.localCenter = localCenter;
			_sweep.c0 = _sweep.c = MathUtils.mul( ref _xf, ref _sweep.localCenter );

			// Update center of mass velocity.
			var a = _sweep.c - oldCenter;
			_linearVelocity += new Vector2( -_angularVelocity * a.Y, _angularVelocity * a.X );
		}


		#region Forces

		/// <summary>
		/// Apply a force at a world point. If the force is not
		/// applied at the center of mass, it will generate a torque and
		/// affect the angular velocity. This wakes up the body.
		/// </summary>
		/// <param name="force">The world force vector, usually in Newtons (N).</param>
		/// <param name="point">The world position of the point of application.</param>
		public void applyForce( Vector2 force, Vector2 point )
		{
			applyForce( ref force, ref point );
		}

		/// <summary>
		/// Applies a force at the center of mass.
		/// </summary>
		/// <param name="force">The force.</param>
		public void applyForce( ref Vector2 force )
		{
			applyForce( ref force, ref _xf.p );
		}

		/// <summary>
		/// Applies a force at the center of mass.
		/// </summary>
		/// <param name="force">The force.</param>
		public void applyForce( Vector2 force )
		{
			applyForce( ref force, ref _xf.p );
		}

		/// <summary>
		/// Apply a force at a world point. If the force is not
		/// applied at the center of mass, it will generate a torque and
		/// affect the angular velocity. This wakes up the body.
		/// </summary>
		/// <param name="force">The world force vector, usually in Newtons (N).</param>
		/// <param name="point">The world position of the point of application.</param>
		public void applyForce( ref Vector2 force, ref Vector2 point )
		{
			Debug.Assert( !float.IsNaN( force.X ) );
			Debug.Assert( !float.IsNaN( force.Y ) );
			Debug.Assert( !float.IsNaN( point.X ) );
			Debug.Assert( !float.IsNaN( point.Y ) );

			if( _bodyType == BodyType.Dynamic )
			{
				if( isAwake == false )
					isAwake = true;

				_force += force;
				_torque += ( point.X - _sweep.c.X ) * force.Y - ( point.Y - _sweep.c.Y ) * force.X;
			}
		}

		/// <summary>
		/// Apply a torque. This affects the angular velocity
		/// without affecting the linear velocity of the center of mass.
		/// This wakes up the body.
		/// </summary>
		/// <param name="torque">The torque about the z-axis (out of the screen), usually in N-m.</param>
		public void applyTorque( float torque )
		{
			Debug.Assert( !float.IsNaN( torque ) );

			if( _bodyType == BodyType.Dynamic )
			{
				if( isAwake == false )
					isAwake = true;

				_torque += torque;
			}
		}

		/// <summary>
		/// Apply an impulse at a point. This immediately modifies the velocity.
		/// This wakes up the body.
		/// </summary>
		/// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
		public void applyLinearImpulse( Vector2 impulse )
		{
			applyLinearImpulse( ref impulse );
		}

		/// <summary>
		/// Apply an impulse at a point. This immediately modifies the velocity.
		/// It also modifies the angular velocity if the point of application
		/// is not at the center of mass.
		/// This wakes up the body.
		/// </summary>
		/// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
		/// <param name="point">The world position of the point of application.</param>
		public void applyLinearImpulse( Vector2 impulse, Vector2 point )
		{
			applyLinearImpulse( ref impulse, ref point );
		}

		/// <summary>
		/// Apply an impulse at a point. This immediately modifies the velocity.
		/// This wakes up the body.
		/// </summary>
		/// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
		public void applyLinearImpulse( ref Vector2 impulse )
		{
			if( _bodyType != BodyType.Dynamic )
			{
				return;
			}
			if( isAwake == false )
			{
				isAwake = true;
			}
			_linearVelocity += _invMass * impulse;
		}

		/// <summary>
		/// Apply an impulse at a point. This immediately modifies the velocity.
		/// It also modifies the angular velocity if the point of application
		/// is not at the center of mass.
		/// This wakes up the body.
		/// </summary>
		/// <param name="impulse">The world impulse vector, usually in N-seconds or kg-m/s.</param>
		/// <param name="point">The world position of the point of application.</param>
		public void applyLinearImpulse( ref Vector2 impulse, ref Vector2 point )
		{
			if( _bodyType != BodyType.Dynamic )
				return;

			if( isAwake == false )
				isAwake = true;

			_linearVelocity += _invMass * impulse;
			_angularVelocity += _invI * ( ( point.X - _sweep.c.X ) * impulse.Y - ( point.Y - _sweep.c.Y ) * impulse.X );
		}

		/// <summary>
		/// Apply an angular impulse.
		/// </summary>
		/// <param name="impulse">The angular impulse in units of kg*m*m/s.</param>
		public void applyAngularImpulse( float impulse )
		{
			if( _bodyType != BodyType.Dynamic )
			{
				return;
			}

			if( isAwake == false )
			{
				isAwake = true;
			}

			_angularVelocity += _invI * impulse;
		}

		#endregion


		#region Conversions to/from local/world

		/// <summary>
		/// Get the world coordinates of a point given the local coordinates.
		/// </summary>
		/// <param name="localPoint">A point on the body measured relative the the body's origin.</param>
		/// <returns>The same point expressed in world coordinates.</returns>
		public Vector2 getWorldPoint( ref Vector2 localPoint )
		{
			return MathUtils.mul( ref _xf, ref localPoint );
		}

		/// <summary>
		/// Get the world coordinates of a point given the local coordinates.
		/// </summary>
		/// <param name="localPoint">A point on the body measured relative the the body's origin.</param>
		/// <returns>The same point expressed in world coordinates.</returns>
		public Vector2 getWorldPoint( Vector2 localPoint )
		{
			return getWorldPoint( ref localPoint );
		}

		/// <summary>
		/// Get the world coordinates of a vector given the local coordinates.
		/// Note that the vector only takes the rotation into account, not the position.
		/// </summary>
		/// <param name="localVector">A vector fixed in the body.</param>
		/// <returns>The same vector expressed in world coordinates.</returns>
		public Vector2 getWorldVector( ref Vector2 localVector )
		{
			return MathUtils.mul( _xf.q, localVector );
		}

		/// <summary>
		/// Get the world coordinates of a vector given the local coordinates.
		/// </summary>
		/// <param name="localVector">A vector fixed in the body.</param>
		/// <returns>The same vector expressed in world coordinates.</returns>
		public Vector2 getWorldVector( Vector2 localVector )
		{
			return getWorldVector( ref localVector );
		}

		/// <summary>
		/// Gets a local point relative to the body's origin given a world point.
		/// Note that the vector only takes the rotation into account, not the position.
		/// </summary>
		/// <param name="worldPoint">A point in world coordinates.</param>
		/// <returns>The corresponding local point relative to the body's origin.</returns>
		public Vector2 getLocalPoint( ref Vector2 worldPoint )
		{
			return MathUtils.mulT( ref _xf, worldPoint );
		}

		/// <summary>
		/// Gets a local point relative to the body's origin given a world point.
		/// </summary>
		/// <param name="worldPoint">A point in world coordinates.</param>
		/// <returns>The corresponding local point relative to the body's origin.</returns>
		public Vector2 getLocalPoint( Vector2 worldPoint )
		{
			return getLocalPoint( ref worldPoint );
		}

		/// <summary>
		/// Gets a local vector given a world vector.
		/// Note that the vector only takes the rotation into account, not the position.
		/// </summary>
		/// <param name="worldVector">A vector in world coordinates.</param>
		/// <returns>The corresponding local vector.</returns>
		public Vector2 getLocalVector( ref Vector2 worldVector )
		{
			return MathUtils.mulT( _xf.q, worldVector );
		}

		/// <summary>
		/// Gets a local vector given a world vector.
		/// Note that the vector only takes the rotation into account, not the position.
		/// </summary>
		/// <param name="worldVector">A vector in world coordinates.</param>
		/// <returns>The corresponding local vector.</returns>
		public Vector2 getLocalVector( Vector2 worldVector )
		{
			return getLocalVector( ref worldVector );
		}

		/// <summary>
		/// Get the world linear velocity of a world point attached to this body.
		/// </summary>
		/// <param name="worldPoint">A point in world coordinates.</param>
		/// <returns>The world velocity of a point.</returns>
		public Vector2 getLinearVelocityFromWorldPoint( Vector2 worldPoint )
		{
			return getLinearVelocityFromWorldPoint( ref worldPoint );
		}

		/// <summary>
		/// Get the world linear velocity of a world point attached to this body.
		/// </summary>
		/// <param name="worldPoint">A point in world coordinates.</param>
		/// <returns>The world velocity of a point.</returns>
		public Vector2 getLinearVelocityFromWorldPoint( ref Vector2 worldPoint )
		{
			return _linearVelocity +
				   new Vector2( -_angularVelocity * ( worldPoint.Y - _sweep.c.Y ),
							   _angularVelocity * ( worldPoint.X - _sweep.c.X ) );
		}

		/// <summary>
		/// Get the world velocity of a local point.
		/// </summary>
		/// <param name="localPoint">A point in local coordinates.</param>
		/// <returns>The world velocity of a point.</returns>
		public Vector2 getLinearVelocityFromLocalPoint( Vector2 localPoint )
		{
			return getLinearVelocityFromLocalPoint( ref localPoint );
		}

		/// <summary>
		/// Get the world velocity of a local point.
		/// </summary>
		/// <param name="localPoint">A point in local coordinates.</param>
		/// <returns>The world velocity of a point.</returns>
		public Vector2 getLinearVelocityFromLocalPoint( ref Vector2 localPoint )
		{
			return getLinearVelocityFromWorldPoint( getWorldPoint( ref localPoint ) );
		}

		#endregion


		internal void synchronizeFixtures()
		{
			var xf1 = new Transform();
			xf1.q.Set( _sweep.a0 );
			xf1.p = _sweep.c0 - MathUtils.mul( xf1.q, _sweep.localCenter );

			var broadPhase = _world.contactManager.broadPhase;
			for( int i = 0; i < fixtureList.Count; i++ )
				fixtureList[i].synchronize( broadPhase, ref xf1, ref _xf );
		}

		internal void synchronizeTransform()
		{
			_xf.q.Set( _sweep.a );
			_xf.p = _sweep.c - MathUtils.mul( _xf.q, _sweep.localCenter );
		}

		/// <summary>
		/// This is used to prevent connected bodies from colliding.
		/// It may lie, depending on the collideConnected flag.
		/// </summary>
		/// <param name="other">The other body.</param>
		/// <returns></returns>
		internal bool shouldCollide( Body other )
		{
			// At least one body should be dynamic.
			if( _bodyType != BodyType.Dynamic && other._bodyType != BodyType.Dynamic )
				return false;

			// Does a joint prevent collision?
			for( JointEdge jn = jointList; jn != null; jn = jn.next )
			{
				if( jn.other == other )
				{
					if( jn.joint.collideConnected == false )
						return false;
				}
			}

			return true;
		}

		internal void advance( float alpha )
		{
			// Advance to the new safe time. This doesn't sync the broad-phase.
			_sweep.advance( alpha );
			_sweep.c = _sweep.c0;
			_sweep.a = _sweep.a0;
			_xf.q.Set( _sweep.a );
			_xf.p = _sweep.c - MathUtils.mul( _xf.q, _sweep.localCenter );
		}

		public void ignoreCollisionWith( Body other )
		{
			for( int i = 0; i < fixtureList.Count; i++ )
			{
				for( int j = 0; j < other.fixtureList.Count; j++ )
				{
					fixtureList[i].ignoreCollisionWith( other.fixtureList[j] );
				}
			}
		}

		public void restoreCollisionWith( Body other )
		{
			for( int i = 0; i < fixtureList.Count; i++ )
			{
				for( int j = 0; j < other.fixtureList.Count; j++ )
				{
					fixtureList[i].restoreCollisionWith( other.fixtureList[j] );
				}
			}
		}


		#region IDisposable Members

		public bool IsDisposed { get; set; }

		public void Dispose()
		{
			if( !IsDisposed )
			{
				_world.removeBody( this );
				IsDisposed = true;
				GC.SuppressFinalize( this );
			}
		}

		#endregion


		/// <summary>
		/// Makes a clone of the body. Fixtures and therefore shapes are not included.
		/// Use DeepClone() to clone the body, as well as fixtures and shapes.
		/// </summary>
		/// <param name="world"></param>
		/// <returns></returns>
		public Body clone( World world = null )
		{
			var body = new Body( world ?? _world, position, rotation );
			body._bodyType = _bodyType;
			body._linearVelocity = _linearVelocity;
			body._angularVelocity = _angularVelocity;
			body.gravityScale = gravityScale;
			body.userData = userData;
			body._enabled = _enabled;
			body._fixedRotation = _fixedRotation;
			body._sleepingAllowed = _sleepingAllowed;
			body._linearDamping = _linearDamping;
			body._angularDamping = _angularDamping;
			body._awake = _awake;
			body.isBullet = isBullet;
			body.ignoreCCD = ignoreCCD;
			body.ignoreGravity = ignoreGravity;
			body._torque = _torque;

			return body;
		}

		/// <summary>
		/// Clones the body and all attached fixtures and shapes. Simply said, it makes a complete copy of the body.
		/// </summary>
		/// <param name="world"></param>
		/// <returns></returns>
		public Body deepClone( World world = null )
		{
			Body body = clone( world ?? _world );

			int count = fixtureList.Count; //Make a copy of the count. Otherwise it causes an infinite loop.
			for( int i = 0; i < count; i++ )
			{
				fixtureList[i].cloneOnto( body );
			}

			return body;
		}
	
	}
}