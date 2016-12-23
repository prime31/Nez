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
//#define USE_ACTIVE_CONTACT_SET
//#define USE_AWAKE_BODY_SET
//#define USE_ISLAND_SET
//#define OPTIMIZE_TOI
//#define USE_IGNORE_CCD_CATEGORIES

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Controllers;
using FarseerPhysics.Dynamics.Contacts;
using FarseerPhysics.Dynamics.Joints;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics
{
	/// <summary>
	/// The world class manages all physics entities, dynamic simulation,
	/// and asynchronous queries.
	/// </summary>
	public class World
	{
		#region Properties/Fields

		public List<Controller> controllerList;
		public List<BreakableBody> breakableBodyList;
		public float updateTime;
		public float continuousPhysicsTime;
		public float controllersUpdateTime;
		public float addRemoveTime;
		public float newContactsTime;
		public float contactsUpdateTime;
		public float solveUpdateTime;

		/// <summary>
		/// Get the number of broad-phase proxies.
		/// </summary>
		/// <value>The proxy count.</value>
		public int proxyCount { get { return contactManager.broadPhase.proxyCount; } }

		/// <summary>
		/// Change the global gravity vector.
		/// </summary>
		/// <value>The gravity.</value>
		public Vector2 gravity;

		/// <summary>
		/// Get the contact manager for testing.
		/// </summary>
		/// <value>The contact manager.</value>
		public ContactManager contactManager;

		/// <summary>
		/// Get the world body list.
		/// </summary>
		/// <value>Thehead of the world body list.</value>
		public List<Body> bodyList;

#if USE_AWAKE_BODY_SET
        public HashSet<Body> AwakeBodySet;
        List<Body> AwakeBodyList;
#endif
#if USE_ISLAND_SET
        HashSet<Body> IslandSet;
#endif
#if OPTIMIZE_TOI
        HashSet<Body> TOISet;
#endif

		/// <summary>
		/// Get the world joint list. 
		/// </summary>
		/// <value>The joint list.</value>
		public List<Joint> jointList;

		/// <summary>
		/// Get the world contact list. With the returned contact, use Contact.GetNext to get
		/// the next contact in the world list. A null contact indicates the end of the list.
		/// </summary>
		/// <value>The head of the world contact list.</value>
		public List<Contact> contactList { get { return contactManager.contactList; } }

		/// <summary>
		/// If false, the whole simulation stops. It still processes added and removed geometries.
		/// </summary>
		public bool enabled;

		public Island island;


		CircleShape _tempOverlapCircle = new CircleShape();

		#endregion


		#region Events

		/// <summary>
		/// Fires whenever a body has been added
		/// </summary>
		public BodyDelegate onBodyAdded;

		/// <summary>
		/// Fires whenever a body has been removed
		/// </summary>
		public BodyDelegate onBodyRemoved;

		/// <summary>
		/// Fires whenever a fixture has been added
		/// </summary>
		public FixtureDelegate onFixtureAdded;

		/// <summary>
		/// Fires whenever a fixture has been removed
		/// </summary>
		public FixtureDelegate onFixtureRemoved;

		/// <summary>
		/// Fires whenever a joint has been added
		/// </summary>
		public JointDelegate onJointAdded;

		/// <summary>
		/// Fires whenever a joint has been removed
		/// </summary>
		public JointDelegate onJointRemoved;

		/// <summary>
		/// Fires every time a controller is added to the World.
		/// </summary>
		public ControllerDelegate onControllerAdded;

		/// <summary>
		/// Fires every time a controlelr is removed form the World.
		/// </summary>
		public ControllerDelegate onControllerRemoved;

		#endregion


		#region Internal Fields

		float _invDt0;
		Body[] _stack = new Body[64];
		bool _stepComplete;
		HashSet<Body> _bodyAddList = new HashSet<Body>();
		HashSet<Body> _bodyRemoveList = new HashSet<Body>();
		HashSet<Joint> _jointAddList = new HashSet<Joint>();
		HashSet<Joint> _jointRemoveList = new HashSet<Joint>();

		Func<Fixture, bool> _queryAABBCallback;
		Func<int, bool> _queryAABBCallbackWrapper;
		Func<Fixture, Vector2, Vector2, float, float> _rayCastCallback;
		Func<RayCastInput, int, float> _rayCastCallbackWrapper;

		TOIInput _input = new TOIInput();
		Fixture _myFixture;
		Vector2 _point1;
		Vector2 _point2;
		List<Fixture> _testPointAllFixtures;
		Stopwatch _watch = new Stopwatch();

		internal Queue<Contact> _contactPool = new Queue<Contact>( 256 );
		internal bool _worldHasNewFixture;

		#endregion


		/// <summary>
		/// Initializes a new instance of the <see cref="World"/> class.
		/// </summary>
		public World( Vector2 gravity )
		{
			island = new Island();
			enabled = true;
			controllerList = new List<Controller>();
			breakableBodyList = new List<BreakableBody>();
			bodyList = new List<Body>( 32 );
			jointList = new List<Joint>( 32 );

#if USE_AWAKE_BODY_SET
            AwakeBodySet = new HashSet<Body>();
            AwakeBodyList = new List<Body>(32);
#endif
#if USE_ISLAND_SET
            IslandSet = new HashSet<Body>();
#endif
#if OPTIMIZE_TOI
            TOISet = new HashSet<Body>();
#endif

			_queryAABBCallbackWrapper = queryAABBCallbackWrapper;
			_rayCastCallbackWrapper = rayCastCallbackWrapper;

			contactManager = new ContactManager( new DynamicTreeBroadPhase() );
			this.gravity = gravity;
		}


		#region List Change Processing

		/// <summary>
		/// All adds and removes are cached by the World duing a World step.
		/// To process the changes before the world updates again, call this method.
		/// </summary>
		public void processChanges()
		{
			processAddedBodies();
			processAddedJoints();

			processRemovedBodies();
			processRemovedJoints();
#if DEBUG && USE_AWAKE_BODY_SET
            foreach (var b in AwakeBodySet)
            {
                Debug.Assert(BodyList.Contains(b));
            }
#endif
		}

		void processRemovedJoints()
		{
			if( _jointRemoveList.Count > 0 )
			{
				foreach( Joint joint in _jointRemoveList )
				{
					bool collideConnected = joint.collideConnected;

					// Remove from the world list.
					jointList.Remove( joint );

					// Disconnect from island graph.
					Body bodyA = joint.bodyA;
					Body bodyB = joint.bodyB;

					// Wake up connected bodies.
					bodyA.isAwake = true;

					// WIP David
					if( !joint.isFixedType() )
					{
						bodyB.isAwake = true;
					}

					// Remove from body 1.
					if( joint.edgeA.prev != null )
					{
						joint.edgeA.prev.next = joint.edgeA.next;
					}

					if( joint.edgeA.next != null )
					{
						joint.edgeA.next.prev = joint.edgeA.prev;
					}

					if( joint.edgeA == bodyA.jointList )
					{
						bodyA.jointList = joint.edgeA.next;
					}

					joint.edgeA.prev = null;
					joint.edgeA.next = null;

					// WIP David
					if( !joint.isFixedType() )
					{
						// Remove from body 2
						if( joint.edgeB.prev != null )
						{
							joint.edgeB.prev.next = joint.edgeB.next;
						}

						if( joint.edgeB.next != null )
						{
							joint.edgeB.next.prev = joint.edgeB.prev;
						}

						if( joint.edgeB == bodyB.jointList )
						{
							bodyB.jointList = joint.edgeB.next;
						}

						joint.edgeB.prev = null;
						joint.edgeB.next = null;
					}

					// WIP David
					if( !joint.isFixedType() )
					{
						// If the joint prevents collisions, then flag any contacts for filtering.
						if( collideConnected == false )
						{
							ContactEdge edge = bodyB.contactList;
							while( edge != null )
							{
								if( edge.other == bodyA )
								{
									// Flag the contact for filtering at the next time step (where either
									// body is awake).
									edge.contact.filterFlag = true;
								}

								edge = edge.next;
							}
						}
					}

					if( onJointRemoved != null )
					{
						onJointRemoved( joint );
					}
				}

				_jointRemoveList.Clear();
			}
		}

		void processAddedJoints()
		{
			if( _jointAddList.Count > 0 )
			{
				foreach( Joint joint in _jointAddList )
				{
					// Connect to the world list.
					jointList.Add( joint );

					// Connect to the bodies' doubly linked lists.
					joint.edgeA.joint = joint;
					joint.edgeA.other = joint.bodyB;
					joint.edgeA.prev = null;
					joint.edgeA.next = joint.bodyA.jointList;

					if( joint.bodyA.jointList != null )
						joint.bodyA.jointList.prev = joint.edgeA;

					joint.bodyA.jointList = joint.edgeA;

					// WIP David
					if( !joint.isFixedType() )
					{
						joint.edgeB.joint = joint;
						joint.edgeB.other = joint.bodyA;
						joint.edgeB.prev = null;
						joint.edgeB.next = joint.bodyB.jointList;

						if( joint.bodyB.jointList != null )
							joint.bodyB.jointList.prev = joint.edgeB;

						joint.bodyB.jointList = joint.edgeB;

						Body bodyA = joint.bodyA;
						Body bodyB = joint.bodyB;

						// If the joint prevents collisions, then flag any contacts for filtering.
						if( joint.collideConnected == false )
						{
							ContactEdge edge = bodyB.contactList;
							while( edge != null )
							{
								if( edge.other == bodyA )
								{
									// Flag the contact for filtering at the next time step (where either
									// body is awake).
									edge.contact.filterFlag = true;
								}

								edge = edge.next;
							}
						}
					}

					if( onJointAdded != null )
						onJointAdded( joint );

					// Note: creating a joint doesn't wake the bodies.
				}

				_jointAddList.Clear();
			}
		}

		void processAddedBodies()
		{
			if( _bodyAddList.Count > 0 )
			{
				foreach( Body body in _bodyAddList )
				{
#if USE_AWAKE_BODY_SET
                    Debug.Assert(!body.IsDisposed);
                    if (body.Awake)
                    {
                        if (!AwakeBodySet.Contains(body))
                            AwakeBodySet.Add(body);
                    }
                    else
                    {
                        if (AwakeBodySet.Contains(body))
                            AwakeBodySet.Remove(body);
                    }
#endif
					// Add to world list.
					bodyList.Add( body );

					if( onBodyAdded != null )
						onBodyAdded( body );
				}

				_bodyAddList.Clear();
			}
		}

		void processRemovedBodies()
		{
			if( _bodyRemoveList.Count > 0 )
			{
				foreach( Body body in _bodyRemoveList )
				{
					Debug.Assert( bodyList.Count > 0 );

					// You tried to remove a body that is not contained in the BodyList.
					// Are you removing the body more than once?
					Debug.Assert( bodyList.Contains( body ) );

#if USE_AWAKE_BODY_SET
                    Debug.Assert(!AwakeBodySet.Contains(body));
#endif
					// Delete the attached joints.
					JointEdge je = body.jointList;
					while( je != null )
					{
						JointEdge je0 = je;
						je = je.next;

						removeJoint( je0.joint, false );
					}
					body.jointList = null;

					// Delete the attached contacts.
					ContactEdge ce = body.contactList;
					while( ce != null )
					{
						ContactEdge ce0 = ce;
						ce = ce.next;
						contactManager.destroy( ce0.contact );
					}
					body.contactList = null;

					// Delete the attached fixtures. This destroys broad-phase proxies.
					for( int i = 0; i < body.fixtureList.Count; i++ )
					{
						body.fixtureList[i].destroyProxies( contactManager.broadPhase );
						body.fixtureList[i].destroy();
					}

					body.fixtureList = null;

					// Remove world body list.
					bodyList.Remove( body );

					if( onBodyRemoved != null )
						onBodyRemoved( body );

#if USE_AWAKE_BODY_SET
                    Debug.Assert(!AwakeBodySet.Contains(body));
#endif
				}

				_bodyRemoveList.Clear();
			}
		}

		#endregion


		void solve( ref TimeStep timeStep )
		{
			// Size the island for the worst case.
			island.reset( bodyList.Count,
						 contactManager.contactList.Count,
						 jointList.Count,
						 contactManager );

			// Clear all the island flags.
#if USE_ISLAND_SET
            Debug.Assert(IslandSet.Count == 0);
#else
			foreach( Body b in bodyList )
			{
				b._island = false;
			}
#endif

#if USE_ACTIVE_CONTACT_SET
            foreach (var c in ContactManager.ActiveContacts)
            {
                c.Flags &= ~ContactFlags.Island;
            }
#else
			foreach( Contact c in contactManager.contactList )
			{
				c.islandFlag = false;
			}
#endif
			foreach( Joint j in jointList )
			{
				j.islandFlag = false;
			}

			// Build and simulate all awake islands.
			var stackSize = bodyList.Count;
			if( stackSize > _stack.Length )
				_stack = new Body[Math.Max( _stack.Length * 2, stackSize )];

#if USE_AWAKE_BODY_SET

            // If AwakeBodyList is empty, the Island code will not have a chance
            // to update the diagnostics timer so reset the timer here. 
            Island.JointUpdateTime = 0;
      
            Debug.Assert(AwakeBodyList.Count == 0);
            AwakeBodyList.AddRange(AwakeBodySet);

            foreach (var seed in AwakeBodyList)
            {
#else
			for( int index = bodyList.Count - 1; index >= 0; index-- )
			{
				Body seed = bodyList[index];
#endif
				if( seed._island )
					continue;

				if( seed.isAwake == false || seed.enabled == false )
					continue;

				// The seed can be dynamic or kinematic.
				if( seed.bodyType == BodyType.Static )
					continue;

				// Reset island and stack.
				island.clear();
				int stackCount = 0;
				_stack[stackCount++] = seed;

#if USE_ISLAND_SET
            if (!IslandSet.Contains(body))
                IslandSet.Add(body);
#endif
				seed._island = true;

				// Perform a depth first search (DFS) on the constraint graph.
				while( stackCount > 0 )
				{
					// Grab the next body off the stack and add it to the island.
					var b = _stack[--stackCount];
					Debug.Assert( b.enabled );
					island.add( b );

					// Make sure the body is awake.
					b.isAwake = true;

					// To keep islands as small as possible, we don't
					// propagate islands across static bodies.
					if( b.bodyType == BodyType.Static )
						continue;

					// Search all contacts connected to this body.
					for( ContactEdge ce = b.contactList; ce != null; ce = ce.next )
					{
						Contact contact = ce.contact;

						// Has this contact already been added to an island?
						if( contact.islandFlag )
							continue;

						// Is this contact solid and touching?
						if( ce.contact.enabled == false || ce.contact.isTouching == false )
							continue;

						// Skip sensors.
						var sensorA = contact.fixtureA.isSensor;
						var sensorB = contact.fixtureB.isSensor;
						if( sensorA || sensorB )
							continue;

						island.add( contact );
						contact.islandFlag = true;

						Body other = ce.other;

						// Was the other body already added to this island?
						if( other._island )
							continue;

						Debug.Assert( stackCount < stackSize );
						_stack[stackCount++] = other;

#if USE_ISLAND_SET
                        if (!IslandSet.Contains(body))
                            IslandSet.Add(body);
#endif
						other._island = true;
					}

					// Search all joints connect to this body.
					for( JointEdge je = b.jointList; je != null; je = je.next )
					{
						if( je.joint.islandFlag )
							continue;

						var other = je.other;

						// WIP David
						//Enter here when it's a non-fixed joint. Non-fixed joints have a other body.
						if( other != null )
						{
							// Don't simulate joints connected to inactive bodies.
							if( other.enabled == false )
								continue;

							island.add( je.joint );
							je.joint.islandFlag = true;

							if( other._island )
								continue;

							Debug.Assert( stackCount < stackSize );
							_stack[stackCount++] = other;
#if USE_ISLAND_SET
                            if (!IslandSet.Contains(body))
                                IslandSet.Add(body);
#endif
							other._island = true;
						}
						else
						{
							island.add( je.joint );
							je.joint.islandFlag = true;
						}
					}
				}

				island.solve( ref timeStep, ref gravity );

				// Post solve cleanup.
				for( int i = 0; i < island.BodyCount; ++i )
				{
					// Allow static bodies to participate in other islands.
					var b = island.Bodies[i];
					if( b.bodyType == BodyType.Static )
						b._island = false;
				}
			}

			// Synchronize fixtures, check for out of range bodies.
#if USE_ISLAND_SET
            foreach (var b in IslandSet)
#else
			foreach( Body b in bodyList )
#endif
			{
				// If a body was not in an island then it did not move.
				if( !b._island )
					continue;
#if USE_ISLAND_SET
                Debug.Assert(b.BodyType != BodyType.Static);
#else
				if( b.bodyType == BodyType.Static )
					continue;
#endif

				// Update fixtures (for broad-phase).
				b.synchronizeFixtures();
			}
#if OPTIMIZE_TOI
            foreach (var b in IslandSet)
            {
                if (!TOISet.Contains(b))
                {
                    TOISet.Add(b);
                }
            }
#endif
#if USE_ISLAND_SET
            IslandSet.Clear();
#endif

			// Look for new contacts.
			contactManager.findNewContacts();

#if USE_AWAKE_BODY_SET
            AwakeBodyList.Clear();
#endif
		}

		void solveTOI( ref TimeStep timeStep )
		{
			island.reset( 2 * Settings.maxTOIContacts, Settings.maxTOIContacts, 0, contactManager );

#if OPTIMIZE_TOI
            bool wasStepComplete = _stepComplete;
#endif
			if( _stepComplete )
			{
#if OPTIMIZE_TOI
                foreach (var b in TOISet)
                {
                    b.Flags &= ~BodyFlags.Island;
                    b.Sweep.Alpha0 = 0.0f;
                }
#else
				for( int i = 0; i < bodyList.Count; i++ )
				{
					bodyList[i]._island = false;
					bodyList[i]._sweep.alpha0 = 0.0f;
				}
#endif
#if USE_ACTIVE_CONTACT_SET
                foreach (var c in ContactManager.ActiveContacts)
                {
#else
				for( int i = 0; i < contactManager.contactList.Count; i++ )
				{
					Contact c = contactManager.contactList[i];
#endif
					// Invalidate TOI
					c.islandFlag = false;
					c.toiFlag = false;
					c._toiCount = 0;
					c._toi = 1.0f;
				}
			}

			// Find TOI events and solve them.
			for( ;;)
			{
				// Find the first TOI.
				Contact minContact = null;
				float minAlpha = 1.0f;

#if USE_ACTIVE_CONTACT_SET
                foreach (var c in ContactManager.ActiveContacts)
                {
#else
				for( int i = 0; i < contactManager.contactList.Count; i++ )
				{
					Contact c = contactManager.contactList[i];
#endif

					// Is this contact disabled?
					if( c.enabled == false )
						continue;

					// Prevent excessive sub-stepping.
					if( c._toiCount > Settings.maxSubSteps )
						continue;

					float alpha;
					if( c.toiFlag )
					{
						// This contact has a valid cached TOI.
						alpha = c._toi;
					}
					else
					{
						Fixture fA = c.fixtureA;
						Fixture fB = c.fixtureB;

						// Is there a sensor?
						if( fA.isSensor || fB.isSensor )
							continue;

						Body bA = fA.body;
						Body bB = fB.body;

						BodyType typeA = bA.bodyType;
						BodyType typeB = bB.bodyType;
						Debug.Assert( typeA == BodyType.Dynamic || typeB == BodyType.Dynamic );

						bool activeA = bA.isAwake && typeA != BodyType.Static;
						bool activeB = bB.isAwake && typeB != BodyType.Static;

						// Is at least one body active (awake and dynamic or kinematic)?
						if( activeA == false && activeB == false )
							continue;

						bool collideA = ( bA.isBullet || typeA != BodyType.Dynamic ) && ( ( fA.ignoreCCDWith & fB.collisionCategories ) == 0 ) && !bA.ignoreCCD;
						bool collideB = ( bB.isBullet || typeB != BodyType.Dynamic ) && ( ( fB.ignoreCCDWith & fA.collisionCategories ) == 0 ) && !bB.ignoreCCD;

						// Are these two non-bullet dynamic bodies?
						if( collideA == false && collideB == false )
							continue;

#if OPTIMIZE_TOI
                        if (_stepComplete)
                        {
                            if (!TOISet.Contains(bA))
                            {
                                TOISet.Add(bA);
                                bA.Flags &= ~BodyFlags.Island;
                                bA.Sweep.Alpha0 = 0.0f;
                            }
                            if (!TOISet.Contains(bB))
                            {
                                TOISet.Add(bB);
                                bB.Flags &= ~BodyFlags.Island;
                                bB.Sweep.Alpha0 = 0.0f;
                            }
                        }
#endif
						// Compute the TOI for this contact.
						// Put the sweeps onto the same time interval.
						float alpha0 = bA._sweep.alpha0;

						if( bA._sweep.alpha0 < bB._sweep.alpha0 )
						{
							alpha0 = bB._sweep.alpha0;
							bA._sweep.advance( alpha0 );
						}
						else if( bB._sweep.alpha0 < bA._sweep.alpha0 )
						{
							alpha0 = bA._sweep.alpha0;
							bB._sweep.advance( alpha0 );
						}

						Debug.Assert( alpha0 < 1.0f );

						// Compute the time of impact in interval [0, minTOI]
						_input.proxyA.set( fA.shape, c.childIndexA );
						_input.proxyB.set( fB.shape, c.childIndexB );
						_input.sweepA = bA._sweep;
						_input.sweepB = bB._sweep;
						_input.tMax = 1.0f;

						TOIOutput output;
						TimeOfImpact.calculateTimeOfImpact( out output, _input );

						// Beta is the fraction of the remaining portion of the .
						float beta = output.t;
						if( output.state == TOIOutputState.Touching )
							alpha = Math.Min( alpha0 + ( 1.0f - alpha0 ) * beta, 1.0f );
						else
							alpha = 1.0f;

						c._toi = alpha;
						c.toiFlag = true;
					}

					if( alpha < minAlpha )
					{
						// This is the minimum TOI found so far.
						minContact = c;
						minAlpha = alpha;
					}
				}

				if( minContact == null || 1.0f - 10.0f * Settings.epsilon < minAlpha )
				{
					// No more TOI events. Done!
					_stepComplete = true;
					break;
				}

				// Advance the bodies to the TOI.
				Fixture fA1 = minContact.fixtureA;
				Fixture fB1 = minContact.fixtureB;
				Body bA0 = fA1.body;
				Body bB0 = fB1.body;

				Sweep backup1 = bA0._sweep;
				Sweep backup2 = bB0._sweep;

				bA0.advance( minAlpha );
				bB0.advance( minAlpha );

				// The TOI contact likely has some new contact points.
				minContact.update( contactManager );
				minContact.toiFlag = false;
				++minContact._toiCount;

				// Is the contact solid?
				if( minContact.enabled == false || minContact.isTouching == false )
				{
					// Restore the sweeps.
					minContact.enabled = false;
					bA0._sweep = backup1;
					bB0._sweep = backup2;
					bA0.synchronizeTransform();
					bB0.synchronizeTransform();
					continue;
				}

				bA0.isAwake = true;
				bB0.isAwake = true;

				// Build the island
				island.clear();
				island.add( bA0 );
				island.add( bB0 );
				island.add( minContact );

				bA0._island = true;
				bB0._island = true;
				minContact.islandFlag = true;

				// Get contacts on bodyA and bodyB.
				Body[] bodies = { bA0, bB0 };
				for( int i = 0; i < 2; ++i )
				{
					var body = bodies[i];
					if( body.bodyType == BodyType.Dynamic )
					{
						for( ContactEdge ce = body.contactList; ce != null; ce = ce.next )
						{
							Contact contact = ce.contact;

							if( island.BodyCount == island.BodyCapacity )
								break;

							if( island.ContactCount == island.ContactCapacity )
								break;

							// Has this contact already been added to the island?
							if( contact.islandFlag )
								continue;

							// Only add static, kinematic, or bullet bodies.
							Body other = ce.other;
							if( other.bodyType == BodyType.Dynamic &&
								body.isBullet == false && other.isBullet == false )
								continue;

							// Skip sensors.
							if( contact.fixtureA.isSensor || contact.fixtureB.isSensor )
								continue;

							// Tentatively advance the body to the TOI.
							Sweep backup = other._sweep;
							if( !other._island )
								other.advance( minAlpha );

							// Update the contact points
							contact.update( contactManager );

							// Was the contact disabled by the user?
							if( contact.enabled == false )
							{
								other._sweep = backup;
								other.synchronizeTransform();
								continue;
							}

							// Are there contact points?
							if( contact.isTouching == false )
							{
								other._sweep = backup;
								other.synchronizeTransform();
								continue;
							}

							// Add the contact to the island
							contact.islandFlag = true;
							island.add( contact );

							// Has the other body already been added to the island?
							if( other._island )
								continue;

							// Add the other body to the island.
							other._island = true;

							if( other.bodyType != BodyType.Static )
								other.isAwake = true;
#if OPTIMIZE_TOI
                            if (_stepComplete)
                            {
                                if (!TOISet.Contains(other))
                                {
                                    TOISet.Add(other);
                                    other.Sweep.Alpha0 = 0.0f;
                                }
                            }
#endif
							island.add( other );
						}
					}
				}

				TimeStep subStep;
				subStep.dt = ( 1.0f - minAlpha ) * timeStep.dt;
				subStep.inv_dt = 1.0f / subStep.dt;
				subStep.dtRatio = 1.0f;
				island.solveTOI( ref subStep, bA0.islandIndex, bB0.islandIndex );

				// Reset island flags and synchronize broad-phase proxies.
				for( int i = 0; i < island.BodyCount; ++i )
				{
					Body body = island.Bodies[i];
					body._island = false;

					if( body.bodyType != BodyType.Dynamic )
						continue;

					body.synchronizeFixtures();

					// Invalidate all contact TOIs on this displaced body.
					for( ContactEdge ce = body.contactList; ce != null; ce = ce.next )
					{
						ce.contact.toiFlag = false;
						ce.contact.islandFlag = false;
					}
				}

				// Commit fixture proxy movements to the broad-phase so that new contacts are created.
				// Also, some contacts can be destroyed.
				contactManager.findNewContacts();

				#pragma warning disable CS0162
				if( Settings.enableSubStepping )
				{
					_stepComplete = false;
					break;
				}
			}
#if OPTIMIZE_TOI
            if (wasStepComplete)
            {
                TOISet.Clear();
            }
#endif
		}


		#region Add/Remove Body/Joint/Controller/BreakableBody

		/// <summary>
		/// Add a rigid body.
		/// </summary>
		/// <returns></returns>
		internal void addBody( Body body )
		{
			Debug.Assert( !_bodyAddList.Contains( body ), "You are adding the same body more than once." );

			if( !_bodyAddList.Contains( body ) )
				_bodyAddList.Add( body );
		}

		/// <summary>
		/// Destroy a rigid body.
		/// Warning: This automatically deletes all associated shapes and joints.
		/// </summary>
		/// <param name="body">The body.</param>
		public void removeBody( Body body )
		{
			Debug.Assert( !_bodyRemoveList.Contains( body ), "The body is already marked for removal. You are removing the body more than once." );

			if( !_bodyRemoveList.Contains( body ) )
				_bodyRemoveList.Add( body );

#if USE_AWAKE_BODY_SET
            if (AwakeBodySet.Contains(body))
            {
                AwakeBodySet.Remove(body);
            }
#endif
		}

		/// <summary>
		/// Create a joint to constrain bodies together. This may cause the connected bodies to cease colliding.
		/// </summary>
		/// <param name="joint">The joint.</param>
		public void addJoint( Joint joint )
		{
			Debug.Assert( !_jointAddList.Contains( joint ), "You are adding the same joint more than once." );

			if( !_jointAddList.Contains( joint ) )
				_jointAddList.Add( joint );
		}

		void removeJoint( Joint joint, bool doCheck )
		{
			if( doCheck )
			{
				Debug.Assert( !_jointRemoveList.Contains( joint ),
							 "The joint is already marked for removal. You are removing the joint more than once." );
			}

			if( !_jointRemoveList.Contains( joint ) )
				_jointRemoveList.Add( joint );
		}

		/// <summary>
		/// Destroy a joint. This may cause the connected bodies to begin colliding.
		/// </summary>
		/// <param name="joint">The joint.</param>
		public void removeJoint( Joint joint )
		{
			removeJoint( joint, true );
		}

		public void addController( Controller controller )
		{
			Debug.Assert( !controllerList.Contains( controller ), "You are adding the same controller more than once." );

			controller.world = this;
			controllerList.Add( controller );

			if( onControllerAdded != null )
				onControllerAdded( controller );
		}

		public void removeController( Controller controller )
		{
			Debug.Assert( controllerList.Contains( controller ),
						 "You are removing a controller that is not in the simulation." );

			if( controllerList.Contains( controller ) )
			{
				controllerList.Remove( controller );

				if( onControllerRemoved != null )
					onControllerRemoved( controller );
			}
		}

		public void addBreakableBody( BreakableBody breakableBody )
		{
			breakableBodyList.Add( breakableBody );
		}

		public void removeBreakableBody( BreakableBody breakableBody )
		{
			//The breakable body list does not contain the body you tried to remove.
			Debug.Assert( breakableBodyList.Contains( breakableBody ) );

			breakableBodyList.Remove( breakableBody );
		}

		#endregion


		/// <summary>
		/// Take a time step. This performs collision detection, integration,
		/// and consraint solution.
		/// </summary>
		/// <param name="dt">The amount of time to simulate, this should not vary.</param>
		public void step( float dt )
		{
			if( !enabled )
				return;

			if( Settings.enableDiagnostics )
				_watch.Start();

			processChanges();

			if( Settings.enableDiagnostics )
				addRemoveTime = _watch.ElapsedTicks;

			// If new fixtures were added, we need to find the new contacts.
			if( _worldHasNewFixture )
			{
				contactManager.findNewContacts();
				_worldHasNewFixture = false;
			}

			if( Settings.enableDiagnostics )
				newContactsTime = _watch.ElapsedTicks - addRemoveTime;

			// FPE only: moved position and velocity iterations into Settings.cs
			TimeStep step;
			step.inv_dt = dt > 0.0f ? 1.0f / dt : 0.0f;
			step.dt = dt;
			step.dtRatio = _invDt0 * dt;

			// Update controllers
			for( var i = 0; i < controllerList.Count; i++ )
				controllerList[i].update( dt );

			if( Settings.enableDiagnostics )
				controllersUpdateTime = _watch.ElapsedTicks - ( addRemoveTime + newContactsTime );

			// Update contacts. This is where some contacts are destroyed.
			contactManager.collide();

			if( Settings.enableDiagnostics )
				contactsUpdateTime = _watch.ElapsedTicks - ( addRemoveTime + newContactsTime + controllersUpdateTime );

			// Integrate velocities, solve velocity raints, and integrate positions.
			solve( ref step );

			if( Settings.enableDiagnostics )
				solveUpdateTime = _watch.ElapsedTicks - ( addRemoveTime + newContactsTime + controllersUpdateTime + contactsUpdateTime );

			// Handle TOI events.
			if( Settings.continuousPhysics )
			{
				solveTOI( ref step );
			}

			if( Settings.enableDiagnostics )
				continuousPhysicsTime = _watch.ElapsedTicks - ( addRemoveTime + newContactsTime + controllersUpdateTime + contactsUpdateTime + solveUpdateTime );

			if( Settings.autoClearForces )
				clearForces();

			for( var i = 0; i < breakableBodyList.Count; i++ )
				breakableBodyList[i].update();

			_invDt0 = step.inv_dt;

			if( Settings.enableDiagnostics )
			{
				_watch.Stop();
				updateTime = _watch.ElapsedTicks;
				_watch.Reset();
			}
		}

		/// <summary>
		/// Call this after you are done with time steps to clear the forces. You normally
		/// call this after each call to Step, unless you are performing sub-steps. By default,
		/// forces will be automatically cleared, so you don't need to call this function.
		/// </summary>
		public void clearForces()
		{
			for( int i = 0; i < bodyList.Count; i++ )
			{
				var body = bodyList[i];
				body._force = Vector2.Zero;
				body._torque = 0.0f;
			}
		}


		#region World queries

		/// <summary>
		/// returns via fixtures all the Fixtures that overlap a circle
		/// </summary>
		/// <param name="center">Center.</param>
		/// <param name="radius">Radius.</param>
		/// <param name="fixtures">Fixtures.</param>
		public void queryCircle( Vector2 center, float radius, List<Fixture> fixtures )
		{
			// prep the CircleShape
			_tempOverlapCircle.radius = radius;

			var circleTransform = new Transform();
			circleTransform.p = center;

			// create an AABB for our query
			AABB aabb;
			var d = new Vector2( radius, radius );
			aabb.lowerBound = center - d;
			aabb.upperBound = center + d;

			// fetch all the Fixtures the AABB overlaps
			contactManager.broadPhase.query( ref aabb, fixtures );

			Transform transformB;

			// loop through and remove any Fixtures that arent overlapping the CircleShape
			for( var i = fixtures.Count - 1; i >= 0; i-- )
			{
				fixtures[i].body.getTransform( out transformB );
				if( !Nez.Farseer.FSCollisions.testOverlap( _tempOverlapCircle, fixtures[i].shape, ref circleTransform, ref transformB ) )
					fixtures.RemoveAt( i );
			}
		}

		/// <summary>
		/// Query the world for all fixtures that potentially overlap the provided AABB.
		/// 
		/// Inside the callback:
		/// Return true: Continues the query
		/// Return false: Terminate the query
		/// </summary>
		/// <param name="callback">A user implemented callback class.</param>
		/// <param name="aabb">The aabb query box.</param>
		public void queryAABB( Func<Fixture, bool> callback, ref AABB aabb )
		{
			_queryAABBCallback = callback;
			contactManager.broadPhase.query( _queryAABBCallbackWrapper, ref aabb );
			_queryAABBCallback = null;
		}

		/// <summary>
		/// Query the world for all fixtures that potentially overlap the provided AABB.
		/// Use the overload with a callback for filtering and better performance.
		/// </summary>
		/// <param name="aabb">The aabb query box.</param>
		/// <returns>A list of fixtures that were in the affected area.</returns>
		public List<Fixture> queryAABB( ref AABB aabb )
		{
			var affected = new List<Fixture>();

			queryAABB( fixture =>
			{
				affected.Add( fixture );
				return true;
			}, ref aabb );

			return affected;
		}

		/// <summary>
		/// Query an AABB for overlapping proxies
		/// </summary>
		/// <param name="aabb">Aabb.</param>
		/// <param name="fixtures">Fixtures.</param>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void queryAABB( ref AABB aabb, List<Fixture> fixtures )
		{
			contactManager.broadPhase.query( ref aabb, fixtures );
		}

		bool queryAABBCallbackWrapper( int proxyId )
		{
			var proxy = contactManager.broadPhase.getProxy( proxyId );
			return _queryAABBCallback( proxy.fixture );
		}

		/// <summary>
		/// Ray-cast the world for all fixtures in the path of the ray. Your callback
		/// controls whether you get the closest point, any point, or n-points.
		/// The ray-cast ignores shapes that contain the starting point.
		/// 
		/// Inside the callback:
		/// return -1: ignore this fixture and continue
		/// return 0: terminate the ray cast
		/// return fraction: clip the ray to this point
		/// return 1: don't clip the ray and continue
		/// </summary>
		/// <param name="callback">A user implemented callback class.</param>
		/// <param name="point1">The ray starting point.</param>
		/// <param name="point2">The ray ending point.</param>
		public void rayCast( Func<Fixture, Vector2, Vector2, float, float> callback, Vector2 point1, Vector2 point2 )
		{
			var input = new RayCastInput();
			input.maxFraction = 1.0f;
			input.point1 = point1;
			input.point2 = point2;

			_rayCastCallback = callback;
			contactManager.broadPhase.rayCast( _rayCastCallbackWrapper, ref input );
			_rayCastCallback = null;
		}

		public List<Fixture> rayCast( Vector2 point1, Vector2 point2 )
		{
			var affected = new List<Fixture>();

			rayCast( ( f, p, n, fr ) =>
			 {
				 affected.Add( f );
				 return 1;
			 }, point1, point2 );

			return affected;
		}

		float rayCastCallbackWrapper( RayCastInput rayCastInput, int proxyId )
		{
			var proxy = contactManager.broadPhase.getProxy( proxyId );
			var fixture = proxy.fixture;
			int index = proxy.childIndex;
			RayCastOutput output;
			bool hit = fixture.rayCast( out output, ref rayCastInput, index );

			if( hit )
			{
				var fraction = output.fraction;
				var point = ( 1.0f - fraction ) * rayCastInput.point1 + fraction * rayCastInput.point2;
				return _rayCastCallback( fixture, point, output.normal, fraction );
			}

			return rayCastInput.maxFraction;
		}

		public Fixture testPoint( Vector2 point )
		{
			AABB aabb;
			var d = new Vector2( Settings.epsilon, Settings.epsilon );
			aabb.lowerBound = point - d;
			aabb.upperBound = point + d;

			_myFixture = null;
			_point1 = point;

			// Query the world for overlapping shapes.
			queryAABB( testPointCallback, ref aabb );

			return _myFixture;
		}

		bool testPointCallback( Fixture fixture )
		{
			var inside = fixture.testPoint( ref _point1 );
			if( inside )
			{
				_myFixture = fixture;
				return false;
			}

			// Continue the query.
			return true;
		}

		/// <summary>
		/// Returns a list of fixtures that are at the specified point.
		/// </summary>
		/// <param name="point">The point.</param>
		/// <returns></returns>
		public List<Fixture> testPointAll( Vector2 point )
		{
			AABB aabb;
			var d = new Vector2( Settings.epsilon, Settings.epsilon );
			aabb.lowerBound = point - d;
			aabb.upperBound = point + d;

			_point2 = point;
			_testPointAllFixtures = new List<Fixture>();

			// Query the world for overlapping shapes.
			queryAABB( testPointAllCallback, ref aabb );

			return _testPointAllFixtures;
		}

		bool testPointAllCallback( Fixture fixture )
		{
			var inside = fixture.testPoint( ref _point2 );
			if( inside )
				_testPointAllFixtures.Add( fixture );

			// Continue the query.
			return true;
		}

		#endregion


		/// <summary>
		/// Shift the world origin. Useful for large worlds. The body shift formula is: position -= newOrigin
		/// Warning: Calling this method mid-update might cause a crash.
		/// </summary>
		/// <param name="newOrigin">the new origin with respect to the old origin</param>
		public void shiftOrigin( Vector2 newOrigin )
		{
			foreach( Body b in bodyList )
			{
				b._xf.p -= newOrigin;
				b._sweep.c0 -= newOrigin;
				b._sweep.c -= newOrigin;
			}

			foreach( Joint joint in jointList )
			{
				//joint.ShiftOrigin(newOrigin); //TODO: uncomment
			}

			contactManager.broadPhase.shiftOrigin( newOrigin );
		}

		public void clear()
		{
			processChanges();

			for( int i = bodyList.Count - 1; i >= 0; i-- )
				removeBody( bodyList[i] );

			for( int i = controllerList.Count - 1; i >= 0; i-- )
				removeController( controllerList[i] );

			for( int i = breakableBodyList.Count - 1; i >= 0; i-- )
				removeBreakableBody( breakableBodyList[i] );

			processChanges();
		}

	}
}