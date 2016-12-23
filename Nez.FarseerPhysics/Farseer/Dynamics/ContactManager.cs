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

using System.Collections.Generic;
using FarseerPhysics.Collision;
using FarseerPhysics.Dynamics.Contacts;


namespace FarseerPhysics.Dynamics
{
	public class ContactManager
	{
		public DynamicTreeBroadPhase broadPhase;

		public List<Contact> contactList = new List<Contact>( 128 );

#if USE_ACTIVE_CONTACT_SET
        /// <summary>
        /// The set of active contacts.
        /// </summary>
		public HashSet<Contact> ActiveContacts = new HashSet<Contact>();

        /// <summary>
        /// A temporary copy of active contacts that is used during updates so
		/// the hash set can have members added/removed during the update.
		/// This list is cleared after every update.
        /// </summary>
		List<Contact> ActiveList = new List<Contact>();
#endif

		/// <summary>
		/// Fires when a contact is created
		/// </summary>
		public BeginContactDelegate onBeginContact;

		/// <summary>
		/// The filter used by the contact manager.
		/// </summary>
		public CollisionFilterDelegate onContactFilter;

		/// <summary>
		/// Fires when a contact is deleted
		/// </summary>
		public EndContactDelegate onEndContact;

		/// <summary>
		/// Fires when the broadphase detects that two Fixtures are close to each other.
		/// </summary>
		public BroadphaseDelegate onBroadphaseCollision;

		/// <summary>
		/// Fires after the solver has run
		/// </summary>
		public PostSolveDelegate onPostSolve;

		/// <summary>
		/// Fires before the solver runs
		/// </summary>
		public PreSolveDelegate onPreSolve;


		internal ContactManager( DynamicTreeBroadPhase broadPhase )
		{
			this.broadPhase = broadPhase;
			onBroadphaseCollision = addPair;
		}

		// Broad-phase callback.
		void addPair( ref FixtureProxy proxyA, ref FixtureProxy proxyB )
		{
			var fixtureA = proxyA.fixture;
			var fixtureB = proxyB.fixture;

			var indexA = proxyA.childIndex;
			var indexB = proxyB.childIndex;

			var bodyA = fixtureA.body;
			var bodyB = fixtureB.body;

			// Are the fixtures on the same body?
			if( bodyA == bodyB )
				return;

			// Does a contact already exist?
			var edge = bodyB.contactList;
			while( edge != null )
			{
				if( edge.other == bodyA )
				{
					var fA = edge.contact.fixtureA;
					var fB = edge.contact.fixtureB;
					int iA = edge.contact.childIndexA;
					int iB = edge.contact.childIndexB;

					if( fA == fixtureA && fB == fixtureB && iA == indexA && iB == indexB )
					{
						// A contact already exists.
						return;
					}

					if( fA == fixtureB && fB == fixtureA && iA == indexB && iB == indexA )
					{
						// A contact already exists.
						return;
					}
				}

				edge = edge.next;
			}

			// Does a joint override collision? Is at least one body dynamic?
			if( bodyB.shouldCollide( bodyA ) == false )
				return;

			// Check default filter
			if( shouldCollide( fixtureA, fixtureB ) == false )
				return;

			// Check user filtering.
			if( onContactFilter != null && onContactFilter( fixtureA, fixtureB ) == false )
				return;

			// FPE feature: BeforeCollision delegate
			if( fixtureA.beforeCollision != null && fixtureA.beforeCollision( fixtureA, fixtureB ) == false )
				return;

			if( fixtureB.beforeCollision != null && fixtureB.beforeCollision( fixtureB, fixtureA ) == false )
				return;

			// Call the factory.
			var c = Contact.create( fixtureA, indexA, fixtureB, indexB );

			if( c == null )
				return;

			// Contact creation may swap fixtures.
			fixtureA = c.fixtureA;
			fixtureB = c.fixtureB;
			bodyA = fixtureA.body;
			bodyB = fixtureB.body;

			// Insert into the world.
			contactList.Add( c );

#if USE_ACTIVE_CONTACT_SET
			ActiveContacts.Add(c);
#endif
			// Connect to island graph.

			// Connect to body A
			c._nodeA.contact = c;
			c._nodeA.other = bodyB;

			c._nodeA.prev = null;
			c._nodeA.next = bodyA.contactList;
			if( bodyA.contactList != null )
				bodyA.contactList.prev = c._nodeA;

			bodyA.contactList = c._nodeA;

			// Connect to body B
			c._nodeB.contact = c;
			c._nodeB.other = bodyA;

			c._nodeB.prev = null;
			c._nodeB.next = bodyB.contactList;
			if( bodyB.contactList != null )
				bodyB.contactList.prev = c._nodeB;

			bodyB.contactList = c._nodeB;

			// Wake up the bodies
			if( fixtureA.isSensor == false && fixtureB.isSensor == false )
			{
				bodyA.isAwake = true;
				bodyB.isAwake = true;
			}
		}

		internal void findNewContacts()
		{
			broadPhase.updatePairs( onBroadphaseCollision );
		}

		internal void destroy( Contact contact )
		{
			var fixtureA = contact.fixtureA;
			var fixtureB = contact.fixtureB;
			var bodyA = fixtureA.body;
			var bodyB = fixtureB.body;

			if( contact.isTouching )
			{
				//Report the separation to both participants:
				if( fixtureA != null && fixtureA.onSeparation != null )
					fixtureA.onSeparation( fixtureA, fixtureB );

				//Reverse the order of the reported fixtures. The first fixture is always the one that the
				//user subscribed to.
				if( fixtureB != null && fixtureB.onSeparation != null )
					fixtureB.onSeparation( fixtureB, fixtureA );

				if( onEndContact != null )
					onEndContact( contact );
			}

			// Remove from the world.
			contactList.Remove( contact );

			// Remove from body 1
			if( contact._nodeA.prev != null )
				contact._nodeA.prev.next = contact._nodeA.next;

			if( contact._nodeA.next != null )
				contact._nodeA.next.prev = contact._nodeA.prev;

			if( contact._nodeA == bodyA.contactList )
				bodyA.contactList = contact._nodeA.next;

			// Remove from body 2
			if( contact._nodeB.prev != null )
				contact._nodeB.prev.next = contact._nodeB.next;

			if( contact._nodeB.next != null )
				contact._nodeB.next.prev = contact._nodeB.prev;

			if( contact._nodeB == bodyB.contactList )
				bodyB.contactList = contact._nodeB.next;

#if USE_ACTIVE_CONTACT_SET
			if (ActiveContacts.Contains(contact))
			{
				ActiveContacts.Remove(contact);
			}
#endif
			contact.destroy();
		}

		internal void collide()
		{
			// Update awake contacts.
#if USE_ACTIVE_CONTACT_SET
			ActiveList.AddRange(ActiveContacts);

			foreach (var c in ActiveList)
			{
#else
			for( var i = 0; i < contactList.Count; i++ )
			{
				var c = contactList[i];
#endif
				var fixtureA = c.fixtureA;
				var fixtureB = c.fixtureB;
				var indexA = c.childIndexA;
				var indexB = c.childIndexB;
				var bodyA = fixtureA.body;
				var bodyB = fixtureB.body;

				// Do no try to collide disabled bodies
				if( !bodyA.enabled || !bodyB.enabled )
					continue;

				// Is this contact flagged for filtering?
				if( c.filterFlag )
				{
					// Should these bodies collide?
					if( bodyB.shouldCollide( bodyA ) == false )
					{
						var cNuke = c;
						destroy( cNuke );
						continue;
					}

					// Check default filtering
					if( shouldCollide( fixtureA, fixtureB ) == false )
					{
						var cNuke = c;
						destroy( cNuke );
						continue;
					}

					// Check user filtering.
					if( onContactFilter != null && onContactFilter( fixtureA, fixtureB ) == false )
					{
						var cNuke = c;
						destroy( cNuke );
						continue;
					}

					// Clear the filtering flag.
					c.filterFlag = false;
				}

				var activeA = bodyA.isAwake && bodyA.bodyType != BodyType.Static;
				var activeB = bodyB.isAwake && bodyB.bodyType != BodyType.Static;

				// At least one body must be awake and it must be dynamic or kinematic.
				if( activeA == false && activeB == false )
				{
#if USE_ACTIVE_CONTACT_SET
					ActiveContacts.Remove(c);
#endif
					continue;
				}

				var proxyIdA = fixtureA.proxies[indexA].proxyId;
				var proxyIdB = fixtureB.proxies[indexB].proxyId;

				var overlap = broadPhase.testOverlap( proxyIdA, proxyIdB );

				// Here we destroy contacts that cease to overlap in the broad-phase.
				if( overlap == false )
				{
					var cNuke = c;
					destroy( cNuke );
					continue;
				}

				// The contact persists.
				c.update( this );
			}

#if USE_ACTIVE_CONTACT_SET
			ActiveList.Clear();
#endif
		}

		public static bool shouldCollide( Fixture fixtureA, Fixture fixtureB )
		{
			if( Settings.useFPECollisionCategories )
			{
				if( ( fixtureA.collisionGroup == fixtureB.collisionGroup ) && fixtureA.collisionGroup != 0 && fixtureB.collisionGroup != 0 )
					return false;

				if( ( ( fixtureA.collisionCategories & fixtureB.collidesWith ) == Category.None ) &
					( ( fixtureB.collisionCategories & fixtureA.collidesWith ) == Category.None ) )
					return false;

				if( fixtureA.isFixtureIgnored( fixtureB ) || fixtureB.isFixtureIgnored( fixtureA ) )
					return false;

				return true;
			}

			if( fixtureA.collisionGroup == fixtureB.collisionGroup && fixtureA.collisionGroup != 0 )
				return fixtureA.collisionGroup > 0;

			bool collide = ( fixtureA.collidesWith & fixtureB.collisionCategories ) != 0 &&
						   ( fixtureA.collisionCategories & fixtureB.collidesWith ) != 0;

			if( collide )
			{
				if( fixtureA.isFixtureIgnored( fixtureB ) || fixtureB.isFixtureIgnored( fixtureA ) )
					return false;
			}

			return collide;
		}

		internal void updateContacts( ContactEdge contactEdge, bool value )
		{
#if USE_ACTIVE_CONTACT_SET
			if(value)
			{
				while(contactEdge != null)
				{
					var c = contactEdge.Contact;
					if (!ActiveContacts.Contains(c))
					{
						ActiveContacts.Add(c);
					}
					contactEdge = contactEdge.Next;
				}
			}
			else
			{
				while (contactEdge != null)
				{
					var c = contactEdge.Contact;
					if (!contactEdge.Other.Awake)
					{
						if (ActiveContacts.Contains(c))
						{
							ActiveContacts.Remove(c);
						}
					}
					contactEdge = contactEdge.Next;
				}
			}
#endif
		}

#if USE_ACTIVE_CONTACT_SET
		internal void RemoveActiveContact(Contact contact)
		{
			if (ActiveContacts.Contains(contact))
				ActiveContacts.Remove(contact);
		}
#endif
	}
}