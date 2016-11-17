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
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Contacts
{
	/// <summary>
	/// A contact edge is used to connect bodies and contacts together
	/// in a contact graph where each body is a node and each contact
	/// is an edge. A contact edge belongs to a doubly linked list
	/// maintained in each attached body. Each contact has two contact
	/// nodes, one for each attached body.
	/// </summary>
	public sealed class ContactEdge
	{
		/// <summary>
		/// The contact
		/// </summary>
		public Contact contact;

		/// <summary>
		/// The next contact edge in the body's contact list
		/// </summary>
		public ContactEdge next;

		/// <summary>
		/// Provides quick access to the other body attached.
		/// </summary>
		public Body other;

		/// <summary>
		/// The previous contact edge in the body's contact list
		/// </summary>
		public ContactEdge prev;
	}

	/// <summary>
	/// The class manages contact between two shapes. A contact exists for each overlapping
	/// AABB in the broad-phase (except if filtered). Therefore a contact object may exist
	/// that has no contact points.
	/// </summary>
	public class Contact
	{
		#region Properties/Fields

		public Fixture fixtureA;
		public Fixture fixtureB;
		public float friction;
		public float restitution;

		/// <summary>
		/// Get the contact manifold. Do not modify the manifold unless you understand the internals of Box2D.
		/// </summary>
		public Manifold manifold;

		/// Get or set the desired tangent speed for a conveyor belt behavior. In meters per second.
		public float tangentSpeed;

		/// <summary>
		/// Enable/disable this contact. This can be used inside the pre-solve contact listener. The contact is only disabled for the current
		/// time step (or sub-step in continuous collisions).
		/// NOTE: If you are setting Enabled to a constant true or false, use the explicit Enable() or Disable() functions instead to 
		/// save the CPU from doing a branch operation.
		/// </summary>
		public bool enabled;

		/// <summary>
		/// Get the child primitive index for fixture A.
		/// </summary>
		/// <value>The child index A.</value>
		public int childIndexA { get; internal set; }

		/// <summary>
		/// Get the child primitive index for fixture B.
		/// </summary>
		/// <value>The child index B.</value>
		public int childIndexB { get; internal set; }

		/// <summary>
		/// Determines whether this contact is touching.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is touching; otherwise, <c>false</c>.
		/// </returns>
		public bool isTouching { get; set; }

		internal bool islandFlag;
		internal bool toiFlag;
		internal bool filterFlag;

		ContactType _type;

		static EdgeShape _edge = new EdgeShape();

		static ContactType[,] _contactRegisters = new[,]
													   {
														   {
															   ContactType.Circle,
															   ContactType.EdgeAndCircle,
															   ContactType.PolygonAndCircle,
															   ContactType.ChainAndCircle,
														   },
														   {
															   ContactType.EdgeAndCircle,
															   ContactType.NotSupported,
                                                               // 1,1 is invalid (no ContactType.Edge)
                                                               ContactType.EdgeAndPolygon,
															   ContactType.NotSupported,
                                                               // 1,3 is invalid (no ContactType.EdgeAndLoop)
                                                           },
														   {
															   ContactType.PolygonAndCircle,
															   ContactType.EdgeAndPolygon,
															   ContactType.Polygon,
															   ContactType.ChainAndPolygon,
														   },
														   {
															   ContactType.ChainAndCircle,
															   ContactType.NotSupported,
                                                               // 3,1 is invalid (no ContactType.EdgeAndLoop)
                                                               ContactType.ChainAndPolygon,
															   ContactType.NotSupported,
                                                               // 3,3 is invalid (no ContactType.Loop)
                                                           },
													   };
		// Nodes for connecting bodies.
		internal ContactEdge _nodeA = new ContactEdge();
		internal ContactEdge _nodeB = new ContactEdge();
		internal int _toiCount;
		internal float _toi;

		#endregion


		Contact( Fixture fA, int indexA, Fixture fB, int indexB )
		{
			reset( fA, indexA, fB, indexB );
		}

		public void resetRestitution()
		{
			restitution = Settings.mixRestitution( fixtureA.restitution, fixtureB.restitution );
		}

		public void resetFriction()
		{
			friction = Settings.mixFriction( fixtureA.friction, fixtureB.friction );
		}

		/// <summary>
		/// Gets the world manifold.
		/// </summary>
		public void getWorldManifold( out Vector2 normal, out FixedArray2<Vector2> points )
		{
			var bodyA = fixtureA.body;
			var bodyB = fixtureB.body;
			var shapeA = fixtureA.shape;
			var shapeB = fixtureB.shape;

			ContactSolver.WorldManifold.initialize( ref manifold, ref bodyA._xf, shapeA.radius, ref bodyB._xf, shapeB.radius, out normal, out points );
		}

		void reset( Fixture fA, int indexA, Fixture fB, int indexB )
		{
			enabled = true;
			isTouching = false;
			islandFlag = false;
			filterFlag = false;
			toiFlag = false;

			fixtureA = fA;
			fixtureB = fB;

			childIndexA = indexA;
			childIndexB = indexB;

			manifold.pointCount = 0;

			_nodeA.contact = null;
			_nodeA.prev = null;
			_nodeA.next = null;
			_nodeA.other = null;

			_nodeB.contact = null;
			_nodeB.prev = null;
			_nodeB.next = null;
			_nodeB.other = null;

			_toiCount = 0;

			//FPE: We only set the friction and restitution if we are not destroying the contact
			if( fixtureA != null && fixtureB != null )
			{
				friction = Settings.mixFriction( fixtureA.friction, fixtureB.friction );
				restitution = Settings.mixRestitution( fixtureA.restitution, fixtureB.restitution );
			}

			tangentSpeed = 0;
		}

		/// <summary>
		/// Update the contact manifold and touching status.
		/// Note: do not assume the fixture AABBs are overlapping or are valid.
		/// </summary>
		/// <param name="contactManager">The contact manager.</param>
		internal void update( ContactManager contactManager )
		{
			var bodyA = fixtureA.body;
			var bodyB = fixtureB.body;

			if( fixtureA == null || fixtureB == null )
				return;

			var oldManifold = manifold;

			// Re-enable this contact.
			enabled = true;

			bool touching;
			var wasTouching = isTouching;
			var sensor = fixtureA.isSensor || fixtureB.isSensor;

			// Is this contact a sensor?
			if( sensor )
			{
				var shapeA = fixtureA.shape;
				var shapeB = fixtureB.shape;
				touching = Collision.Collision.testOverlap( shapeA, childIndexA, shapeB, childIndexB, ref bodyA._xf, ref bodyB._xf );

				// Sensors don't generate manifolds.
				manifold.pointCount = 0;
			}
			else
			{
				evaluate( ref manifold, ref bodyA._xf, ref bodyB._xf );
				touching = manifold.pointCount > 0;

				// Match old contact ids to new contact ids and copy the
				// stored impulses to warm start the solver.
				for( int i = 0; i < manifold.pointCount; ++i )
				{
					var mp2 = manifold.points[i];
					mp2.normalImpulse = 0.0f;
					mp2.tangentImpulse = 0.0f;
					var id2 = mp2.id;

					for( int j = 0; j < oldManifold.pointCount; ++j )
					{
						var mp1 = oldManifold.points[j];

						if( mp1.id.key == id2.key )
						{
							mp2.normalImpulse = mp1.normalImpulse;
							mp2.tangentImpulse = mp1.tangentImpulse;
							break;
						}
					}

					manifold.points[i] = mp2;
				}

				if( touching != wasTouching )
				{
					bodyA.isAwake = true;
					bodyB.isAwake = true;
				}
			}

			isTouching = touching;
			if( wasTouching == false )
			{
				if( touching )
				{
					if( Settings.allCollisionCallbacksAgree )
					{
						bool enabledA = true, enabledB = true;

						// Report the collision to both participants. Track which ones returned true so we can
						// later call OnSeparation if the contact is disabled for a different reason.
						if( fixtureA.onCollision != null )
							foreach( OnCollisionEventHandler handler in fixtureA.onCollision.GetInvocationList() )
								enabledA = handler( fixtureA, fixtureB, this ) && enabledA;

						// Reverse the order of the reported fixtures. The first fixture is always the one that the
						// user subscribed to.
						if( fixtureB.onCollision != null )
							foreach( OnCollisionEventHandler handler in fixtureB.onCollision.GetInvocationList() )
								enabledB = handler( fixtureB, fixtureA, this ) && enabledB;

						enabled = enabledA && enabledB;

						// BeginContact can also return false and disable the contact
						if( enabledA && enabledB && contactManager.onBeginContact != null )
							enabled = contactManager.onBeginContact( this );
					}
					else
					{
						// Report the collision to both participants:
						if( fixtureA.onCollision != null )
							foreach( OnCollisionEventHandler handler in fixtureA.onCollision.GetInvocationList() )
								enabled = handler( fixtureA, fixtureB, this );

						//Reverse the order of the reported fixtures. The first fixture is always the one that the
						//user subscribed to.
						if( fixtureB.onCollision != null )
							foreach( OnCollisionEventHandler handler in fixtureB.onCollision.GetInvocationList() )
								enabled = handler( fixtureB, fixtureA, this );

						//BeginContact can also return false and disable the contact
						if( contactManager.onBeginContact != null )
							enabled = contactManager.onBeginContact( this );
					}

					// If the user disabled the contact (needed to exclude it in TOI solver) at any point by
					// any of the callbacks, we need to mark it as not touching and call any separation
					// callbacks for fixtures that didn't explicitly disable the collision.
					if( !enabled )
						isTouching = false;
				}
			}
			else
			{
				if( touching == false )
				{
					// Report the separation to both participants:
					if( fixtureA != null && fixtureA.onSeparation != null )
						fixtureA.onSeparation( fixtureA, fixtureB );

					//Reverse the order of the reported fixtures. The first fixture is always the one that the
					//user subscribed to.
					if( fixtureB != null && fixtureB.onSeparation != null )
						fixtureB.onSeparation( fixtureB, fixtureA );

					if( contactManager.onEndContact != null )
						contactManager.onEndContact( this );
				}
			}

			if( sensor )
				return;

			if( contactManager.onPreSolve != null )
				contactManager.onPreSolve( this, ref oldManifold );
		}

		/// <summary>
		/// Evaluate this contact with your own manifold and transforms.   
		/// </summary>
		/// <param name="manifold">The manifold.</param>
		/// <param name="transformA">The first transform.</param>
		/// <param name="transformB">The second transform.</param>
		void evaluate( ref Manifold manifold, ref Transform transformA, ref Transform transformB )
		{
			switch( _type )
			{
				case ContactType.Polygon:
					Collision.Collision.collidePolygons( ref manifold, (PolygonShape)fixtureA.shape, ref transformA, (PolygonShape)fixtureB.shape, ref transformB );
					break;
				case ContactType.PolygonAndCircle:
					Collision.Collision.collidePolygonAndCircle( ref manifold, (PolygonShape)fixtureA.shape, ref transformA, (CircleShape)fixtureB.shape, ref transformB );
					break;
				case ContactType.EdgeAndCircle:
					Collision.Collision.collideEdgeAndCircle( ref manifold, (EdgeShape)fixtureA.shape, ref transformA, (CircleShape)fixtureB.shape, ref transformB );
					break;
				case ContactType.EdgeAndPolygon:
					Collision.Collision.collideEdgeAndPolygon( ref manifold, (EdgeShape)fixtureA.shape, ref transformA, (PolygonShape)fixtureB.shape, ref transformB );
					break;
				case ContactType.ChainAndCircle:
					var chain = (ChainShape)fixtureA.shape;
					chain.getChildEdge( _edge, childIndexA );
					Collision.Collision.collideEdgeAndCircle( ref manifold, _edge, ref transformA, (CircleShape)fixtureB.shape, ref transformB );
					break;
				case ContactType.ChainAndPolygon:
					var loop2 = (ChainShape)fixtureA.shape;
					loop2.getChildEdge( _edge, childIndexA );
					Collision.Collision.collideEdgeAndPolygon( ref manifold, _edge, ref transformA, (PolygonShape)fixtureB.shape, ref transformB );
					break;
				case ContactType.Circle:
					Collision.Collision.collideCircles( ref manifold, (CircleShape)fixtureA.shape, ref transformA, (CircleShape)fixtureB.shape, ref transformB );
					break;
			}
		}

		internal static Contact create( Fixture fixtureA, int indexA, Fixture fixtureB, int indexB )
		{
			var type1 = fixtureA.shape.shapeType;
			var type2 = fixtureB.shape.shapeType;

			Debug.Assert( ShapeType.Unknown < type1 && type1 < ShapeType.TypeCount );
			Debug.Assert( ShapeType.Unknown < type2 && type2 < ShapeType.TypeCount );

			Contact c;
			var pool = fixtureA.body._world._contactPool;
			if( pool.Count > 0 )
			{
				c = pool.Dequeue();
				if( ( type1 >= type2 || ( type1 == ShapeType.Edge && type2 == ShapeType.Polygon ) ) && !( type2 == ShapeType.Edge && type1 == ShapeType.Polygon ) )
					c.reset( fixtureA, indexA, fixtureB, indexB );
				else
					c.reset( fixtureB, indexB, fixtureA, indexA );
			}
			else
			{
				// Edge+Polygon is non-symetrical due to the way Erin handles collision type registration.
				if( ( type1 >= type2 || ( type1 == ShapeType.Edge && type2 == ShapeType.Polygon ) ) && !( type2 == ShapeType.Edge && type1 == ShapeType.Polygon ) )
					c = new Contact( fixtureA, indexA, fixtureB, indexB );
				else
					c = new Contact( fixtureB, indexB, fixtureA, indexA );
			}

			c._type = _contactRegisters[(int)type1, (int)type2];

			return c;
		}

		internal void destroy()
		{
#if USE_ACTIVE_CONTACT_SET
            FixtureA.Body.World.ContactManager.RemoveActiveContact(this);
#endif
			fixtureA.body._world._contactPool.Enqueue( this );

			if( manifold.pointCount > 0 && fixtureA.isSensor == false && fixtureB.isSensor == false )
			{
				fixtureA.body.isAwake = true;
				fixtureB.body.isAwake = true;
			}

			reset( null, 0, null, 0 );
		}


		#region Nested type: ContactType

		public enum ContactType
		{
			NotSupported,
			Polygon,
			PolygonAndCircle,
			Circle,
			EdgeAndPolygon,
			EdgeAndCircle,
			ChainAndPolygon,
			ChainAndCircle,
		}

		#endregion
	
	}
}