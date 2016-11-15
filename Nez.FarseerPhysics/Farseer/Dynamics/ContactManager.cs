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
		/// <summary>
		/// Fires when a contact is created
		/// </summary>
		public BeginContactDelegate BeginContact;

		public IBroadPhase BroadPhase;

		/// <summary>
		/// The filter used by the contact manager.
		/// </summary>
		public CollisionFilterDelegate ContactFilter;

		public List<Contact> ContactList = new List<Contact>( 128 );

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
		/// Fires when a contact is deleted
		/// </summary>
		public EndContactDelegate EndContact;

		/// <summary>
		/// Fires when the broadphase detects that two Fixtures are close to each other.
		/// </summary>
		public BroadphaseDelegate OnBroadphaseCollision;

		/// <summary>
		/// Fires after the solver has run
		/// </summary>
		public PostSolveDelegate PostSolve;

		/// <summary>
		/// Fires before the solver runs
		/// </summary>
		public PreSolveDelegate PreSolve;

		internal ContactManager( IBroadPhase broadPhase )
		{
			BroadPhase = broadPhase;
			OnBroadphaseCollision = AddPair;
		}

		// Broad-phase callback.
		void AddPair( ref FixtureProxy proxyA, ref FixtureProxy proxyB )
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
				if( edge.Other == bodyA )
				{
					Fixture fA = edge.Contact.FixtureA;
					Fixture fB = edge.Contact.FixtureB;
					int iA = edge.Contact.ChildIndexA;
					int iB = edge.Contact.ChildIndexB;

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

				edge = edge.Next;
			}

			// Does a joint override collision? Is at least one body dynamic?
			if( bodyB.ShouldCollide( bodyA ) == false )
				return;

			// Check default filter
			if( ShouldCollide( fixtureA, fixtureB ) == false )
				return;

			// Check user filtering.
			if( ContactFilter != null && ContactFilter( fixtureA, fixtureB ) == false )
				return;

			// FPE feature: BeforeCollision delegate
			if( fixtureA.beforeCollision != null && fixtureA.beforeCollision( fixtureA, fixtureB ) == false )
				return;

			if( fixtureB.beforeCollision != null && fixtureB.beforeCollision( fixtureB, fixtureA ) == false )
				return;

			// Call the factory.
			var c = Contact.Create( fixtureA, indexA, fixtureB, indexB );

			if( c == null )
				return;

			// Contact creation may swap fixtures.
			fixtureA = c.FixtureA;
			fixtureB = c.FixtureB;
			bodyA = fixtureA.body;
			bodyB = fixtureB.body;

			// Insert into the world.
			ContactList.Add( c );

#if USE_ACTIVE_CONTACT_SET
			ActiveContacts.Add(c);
#endif
			// Connect to island graph.

			// Connect to body A
			c._nodeA.Contact = c;
			c._nodeA.Other = bodyB;

			c._nodeA.Prev = null;
			c._nodeA.Next = bodyA.contactList;
			if( bodyA.contactList != null )
				bodyA.contactList.Prev = c._nodeA;

			bodyA.contactList = c._nodeA;

			// Connect to body B
			c._nodeB.Contact = c;
			c._nodeB.Other = bodyA;

			c._nodeB.Prev = null;
			c._nodeB.Next = bodyB.contactList;
			if( bodyB.contactList != null )
				bodyB.contactList.Prev = c._nodeB;

			bodyB.contactList = c._nodeB;

			// Wake up the bodies
			if( fixtureA.isSensor == false && fixtureB.isSensor == false )
			{
				bodyA.awake = true;
				bodyB.awake = true;
			}
		}

		internal void FindNewContacts()
		{
			BroadPhase.UpdatePairs( OnBroadphaseCollision );
		}

		internal void Destroy( Contact contact )
		{
			var fixtureA = contact.FixtureA;
			var fixtureB = contact.FixtureB;
			var bodyA = fixtureA.body;
			var bodyB = fixtureB.body;

			if( contact.IsTouching )
			{
				//Report the separation to both participants:
				if( fixtureA != null && fixtureA.onSeparation != null )
					fixtureA.onSeparation( fixtureA, fixtureB );

				//Reverse the order of the reported fixtures. The first fixture is always the one that the
				//user subscribed to.
				if( fixtureB != null && fixtureB.onSeparation != null )
					fixtureB.onSeparation( fixtureB, fixtureA );

				if( EndContact != null )
					EndContact( contact );
			}

			// Remove from the world.
			ContactList.Remove( contact );

			// Remove from body 1
			if( contact._nodeA.Prev != null )
				contact._nodeA.Prev.Next = contact._nodeA.Next;

			if( contact._nodeA.Next != null )
				contact._nodeA.Next.Prev = contact._nodeA.Prev;

			if( contact._nodeA == bodyA.contactList )
				bodyA.contactList = contact._nodeA.Next;

			// Remove from body 2
			if( contact._nodeB.Prev != null )
				contact._nodeB.Prev.Next = contact._nodeB.Next;

			if( contact._nodeB.Next != null )
				contact._nodeB.Next.Prev = contact._nodeB.Prev;

			if( contact._nodeB == bodyB.contactList )
				bodyB.contactList = contact._nodeB.Next;

#if USE_ACTIVE_CONTACT_SET
			if (ActiveContacts.Contains(contact))
			{
				ActiveContacts.Remove(contact);
			}
#endif
			contact.Destroy();
		}

		internal void Collide()
		{
			// Update awake contacts.
#if USE_ACTIVE_CONTACT_SET
			ActiveList.AddRange(ActiveContacts);

			foreach (var c in ActiveList)
			{
#else
			for( var i = 0; i < ContactList.Count; i++ )
			{
				var c = ContactList[i];
#endif
				var fixtureA = c.FixtureA;
				var fixtureB = c.FixtureB;
				var indexA = c.ChildIndexA;
				var indexB = c.ChildIndexB;
				var bodyA = fixtureA.body;
				var bodyB = fixtureB.body;

				// Do no try to collide disabled bodies
				if( !bodyA.enabled || !bodyB.enabled )
					continue;

				// Is this contact flagged for filtering?
				if( c.FilterFlag )
				{
					// Should these bodies collide?
					if( bodyB.ShouldCollide( bodyA ) == false )
					{
						var cNuke = c;
						Destroy( cNuke );
						continue;
					}

					// Check default filtering
					if( ShouldCollide( fixtureA, fixtureB ) == false )
					{
						var cNuke = c;
						Destroy( cNuke );
						continue;
					}

					// Check user filtering.
					if( ContactFilter != null && ContactFilter( fixtureA, fixtureB ) == false )
					{
						var cNuke = c;
						Destroy( cNuke );
						continue;
					}

					// Clear the filtering flag.
					c.FilterFlag = false;
				}

				var activeA = bodyA.awake && bodyA.bodyType != BodyType.Static;
				var activeB = bodyB.awake && bodyB.bodyType != BodyType.Static;

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

				var overlap = BroadPhase.TestOverlap( proxyIdA, proxyIdB );

				// Here we destroy contacts that cease to overlap in the broad-phase.
				if( overlap == false )
				{
					var cNuke = c;
					Destroy( cNuke );
					continue;
				}

				// The contact persists.
				c.Update( this );
			}

#if USE_ACTIVE_CONTACT_SET
			ActiveList.Clear();
#endif
		}

		public static bool ShouldCollide( Fixture fixtureA, Fixture fixtureB )
		{
			if( Settings.UseFPECollisionCategories )
			{
				if( ( fixtureA.collisionGroup == fixtureB.collisionGroup ) && fixtureA.collisionGroup != 0 && fixtureB.collisionGroup != 0 )
					return false;

				if( ( ( fixtureA.collisionCategories & fixtureB.collidesWith ) == Category.None ) &
					( ( fixtureB.collisionCategories & fixtureA.collidesWith ) == Category.None ) )
					return false;

				if( fixtureA.IsFixtureIgnored( fixtureB ) || fixtureB.IsFixtureIgnored( fixtureA ) )
					return false;

				return true;
			}

			if( fixtureA.collisionGroup == fixtureB.collisionGroup && fixtureA.collisionGroup != 0 )
				return fixtureA.collisionGroup > 0;

			bool collide = ( fixtureA.collidesWith & fixtureB.collisionCategories ) != 0 &&
						   ( fixtureA.collisionCategories & fixtureB.collidesWith ) != 0;

			if( collide )
			{
				if( fixtureA.IsFixtureIgnored( fixtureB ) || fixtureB.IsFixtureIgnored( fixtureA ) )
					return false;
			}

			return collide;
		}

		internal void UpdateContacts( ContactEdge contactEdge, bool value )
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