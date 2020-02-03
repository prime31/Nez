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
		public Contact Contact;

		/// <summary>
		/// The next contact edge in the body's contact list
		/// </summary>
		public ContactEdge Next;

		/// <summary>
		/// Provides quick access to the other body attached.
		/// </summary>
		public Body Other;

		/// <summary>
		/// The previous contact edge in the body's contact list
		/// </summary>
		public ContactEdge Prev;
	}

	/// <summary>
	/// The class manages contact between two shapes. A contact exists for each overlapping
	/// AABB in the broad-phase (except if filtered). Therefore a contact object may exist
	/// that has no contact points.
	/// </summary>
	public class Contact
	{
		#region Properties/Fields

		public Fixture FixtureA;
		public Fixture FixtureB;
		public float Friction;
		public float Restitution;

		/// <summary>
		/// Get the contact manifold. Do not modify the manifold unless you understand the internals of Box2D.
		/// </summary>
		public Manifold Manifold;

		/// Get or set the desired tangent speed for a conveyor belt behavior. In meters per second.
		public float TangentSpeed;

		/// <summary>
		/// Enable/disable this contact. This can be used inside the pre-solve contact listener. The contact is only disabled for the current
		/// time step (or sub-step in continuous collisions).
		/// NOTE: If you are setting Enabled to a constant true or false, use the explicit Enable() or Disable() functions instead to 
		/// save the CPU from doing a branch operation.
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// Get the child primitive index for fixture A.
		/// </summary>
		/// <value>The child index A.</value>
		public int ChildIndexA { get; internal set; }

		/// <summary>
		/// Get the child primitive index for fixture B.
		/// </summary>
		/// <value>The child index B.</value>
		public int ChildIndexB { get; internal set; }

		/// <summary>
		/// Determines whether this contact is touching.
		/// </summary>
		/// <returns>
		/// 	<c>true</c> if this instance is touching; otherwise, <c>false</c>.
		/// </returns>
		public bool IsTouching { get; set; }

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


		Contact(Fixture fA, int indexA, Fixture fB, int indexB)
		{
			Reset(fA, indexA, fB, indexB);
		}

		public void ResetRestitution()
		{
			Restitution = Settings.MixRestitution(FixtureA.Restitution, FixtureB.Restitution);
		}

		public void ResetFriction()
		{
			Friction = Settings.MixFriction(FixtureA.Friction, FixtureB.Friction);
		}

		/// <summary>
		/// Gets the world manifold.
		/// </summary>
		public void GetWorldManifold(out Vector2 normal, out FixedArray2<Vector2> points)
		{
			var bodyA = FixtureA.Body;
			var bodyB = FixtureB.Body;
			var shapeA = FixtureA.Shape;
			var shapeB = FixtureB.Shape;

			ContactSolver.WorldManifold.Initialize(ref Manifold, ref bodyA._xf, shapeA.Radius, ref bodyB._xf,
				shapeB.Radius, out normal, out points);
		}

		void Reset(Fixture fA, int indexA, Fixture fB, int indexB)
		{
			Enabled = true;
			IsTouching = false;
			islandFlag = false;
			filterFlag = false;
			toiFlag = false;

			FixtureA = fA;
			FixtureB = fB;

			ChildIndexA = indexA;
			ChildIndexB = indexB;

			Manifold.PointCount = 0;

			_nodeA.Contact = null;
			_nodeA.Prev = null;
			_nodeA.Next = null;
			_nodeA.Other = null;

			_nodeB.Contact = null;
			_nodeB.Prev = null;
			_nodeB.Next = null;
			_nodeB.Other = null;

			_toiCount = 0;

			//FPE: We only set the friction and restitution if we are not destroying the contact
			if (FixtureA != null && FixtureB != null)
			{
				Friction = Settings.MixFriction(FixtureA.Friction, FixtureB.Friction);
				Restitution = Settings.MixRestitution(FixtureA.Restitution, FixtureB.Restitution);
			}

			TangentSpeed = 0;
		}

		/// <summary>
		/// Update the contact manifold and touching status.
		/// Note: do not assume the fixture AABBs are overlapping or are valid.
		/// </summary>
		/// <param name="contactManager">The contact manager.</param>
		internal void Update(ContactManager contactManager)
		{
			var bodyA = FixtureA.Body;
			var bodyB = FixtureB.Body;

			if (FixtureA == null || FixtureB == null)
				return;

			var oldManifold = Manifold;

			// Re-enable this contact.
			Enabled = true;

			bool touching;
			var wasTouching = IsTouching;
			var sensor = FixtureA.IsSensor || FixtureB.IsSensor;

			// Is this contact a sensor?
			if (sensor)
			{
				var shapeA = FixtureA.Shape;
				var shapeB = FixtureB.Shape;
				touching = Collision.Collision.TestOverlap(shapeA, ChildIndexA, shapeB, ChildIndexB, ref bodyA._xf,
					ref bodyB._xf);

				// Sensors don't generate manifolds.
				Manifold.PointCount = 0;
			}
			else
			{
				Evaluate(ref Manifold, ref bodyA._xf, ref bodyB._xf);
				touching = Manifold.PointCount > 0;

				// Match old contact ids to new contact ids and copy the
				// stored impulses to warm start the solver.
				for (int i = 0; i < Manifold.PointCount; ++i)
				{
					var mp2 = Manifold.Points[i];
					mp2.NormalImpulse = 0.0f;
					mp2.TangentImpulse = 0.0f;
					var id2 = mp2.Id;

					for (int j = 0; j < oldManifold.PointCount; ++j)
					{
						var mp1 = oldManifold.Points[j];

						if (mp1.Id.Key == id2.Key)
						{
							mp2.NormalImpulse = mp1.NormalImpulse;
							mp2.TangentImpulse = mp1.TangentImpulse;
							break;
						}
					}

					Manifold.Points[i] = mp2;
				}

				if (touching != wasTouching)
				{
					bodyA.IsAwake = true;
					bodyB.IsAwake = true;
				}
			}

			IsTouching = touching;
			if (wasTouching == false)
			{
				if (touching)
				{
					if (Settings.AllCollisionCallbacksAgree)
					{
						bool enabledA = true, enabledB = true;

						// Report the collision to both participants. Track which ones returned true so we can
						// later call OnSeparation if the contact is disabled for a different reason.
						if (FixtureA.OnCollision != null)
							foreach (OnCollisionEventHandler handler in FixtureA.OnCollision.GetInvocationList())
								enabledA = handler(FixtureA, FixtureB, this) && enabledA;

						// Reverse the order of the reported fixtures. The first fixture is always the one that the
						// user subscribed to.
						if (FixtureB.OnCollision != null)
							foreach (OnCollisionEventHandler handler in FixtureB.OnCollision.GetInvocationList())
								enabledB = handler(FixtureB, FixtureA, this) && enabledB;

						Enabled = enabledA && enabledB;

						// BeginContact can also return false and disable the contact
						if (enabledA && enabledB && contactManager.OnBeginContact != null)
							Enabled = contactManager.OnBeginContact(this);
					}
					else
					{
						// Report the collision to both participants:
						if (FixtureA.OnCollision != null)
							foreach (OnCollisionEventHandler handler in FixtureA.OnCollision.GetInvocationList())
								Enabled = handler(FixtureA, FixtureB, this);

						//Reverse the order of the reported fixtures. The first fixture is always the one that the
						//user subscribed to.
						if (FixtureB.OnCollision != null)
							foreach (OnCollisionEventHandler handler in FixtureB.OnCollision.GetInvocationList())
								Enabled = handler(FixtureB, FixtureA, this);

						//BeginContact can also return false and disable the contact
						if (contactManager.OnBeginContact != null)
							Enabled = contactManager.OnBeginContact(this);
					}

					// If the user disabled the contact (needed to exclude it in TOI solver) at any point by
					// any of the callbacks, we need to mark it as not touching and call any separation
					// callbacks for fixtures that didn't explicitly disable the collision.
					if (!Enabled)
						IsTouching = false;
				}
			}
			else
			{
				if (touching == false)
				{
					// Report the separation to both participants:
					if (FixtureA != null && FixtureA.OnSeparation != null)
						FixtureA.OnSeparation(FixtureA, FixtureB);

					//Reverse the order of the reported fixtures. The first fixture is always the one that the
					//user subscribed to.
					if (FixtureB != null && FixtureB.OnSeparation != null)
						FixtureB.OnSeparation(FixtureB, FixtureA);

					if (contactManager.OnEndContact != null)
						contactManager.OnEndContact(this);
				}
			}

			if (sensor)
				return;

			if (contactManager.OnPreSolve != null)
				contactManager.OnPreSolve(this, ref oldManifold);
		}

		/// <summary>
		/// Evaluate this contact with your own manifold and transforms.   
		/// </summary>
		/// <param name="manifold">The manifold.</param>
		/// <param name="transformA">The first transform.</param>
		/// <param name="transformB">The second transform.</param>
		void Evaluate(ref Manifold manifold, ref Transform transformA, ref Transform transformB)
		{
			switch (_type)
			{
				case ContactType.Polygon:
					Collision.Collision.CollidePolygons(ref manifold, (PolygonShape) FixtureA.Shape, ref transformA,
						(PolygonShape) FixtureB.Shape, ref transformB);
					break;
				case ContactType.PolygonAndCircle:
					Collision.Collision.CollidePolygonAndCircle(ref manifold, (PolygonShape) FixtureA.Shape,
						ref transformA, (CircleShape) FixtureB.Shape, ref transformB);
					break;
				case ContactType.EdgeAndCircle:
					Collision.Collision.CollideEdgeAndCircle(ref manifold, (EdgeShape) FixtureA.Shape, ref transformA,
						(CircleShape) FixtureB.Shape, ref transformB);
					break;
				case ContactType.EdgeAndPolygon:
					Collision.Collision.CollideEdgeAndPolygon(ref manifold, (EdgeShape) FixtureA.Shape, ref transformA,
						(PolygonShape) FixtureB.Shape, ref transformB);
					break;
				case ContactType.ChainAndCircle:
					var chain = (ChainShape) FixtureA.Shape;
					chain.GetChildEdge(_edge, ChildIndexA);
					Collision.Collision.CollideEdgeAndCircle(ref manifold, _edge, ref transformA,
						(CircleShape) FixtureB.Shape, ref transformB);
					break;
				case ContactType.ChainAndPolygon:
					var loop2 = (ChainShape) FixtureA.Shape;
					loop2.GetChildEdge(_edge, ChildIndexA);
					Collision.Collision.CollideEdgeAndPolygon(ref manifold, _edge, ref transformA,
						(PolygonShape) FixtureB.Shape, ref transformB);
					break;
				case ContactType.Circle:
					Collision.Collision.CollideCircles(ref manifold, (CircleShape) FixtureA.Shape, ref transformA,
						(CircleShape) FixtureB.Shape, ref transformB);
					break;
			}
		}

		internal static Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
		{
			var type1 = fixtureA.Shape.ShapeType;
			var type2 = fixtureB.Shape.ShapeType;

			Debug.Assert(ShapeType.Unknown < type1 && type1 < ShapeType.TypeCount);
			Debug.Assert(ShapeType.Unknown < type2 && type2 < ShapeType.TypeCount);

			Contact c;
			var pool = fixtureA.Body._world._contactPool;
			if (pool.Count > 0)
			{
				c = pool.Dequeue();
				if ((type1 >= type2 || (type1 == ShapeType.Edge && type2 == ShapeType.Polygon)) &&
				    !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
					c.Reset(fixtureA, indexA, fixtureB, indexB);
				else
					c.Reset(fixtureB, indexB, fixtureA, indexA);
			}
			else
			{
				// Edge+Polygon is non-symetrical due to the way Erin handles collision type registration.
				if ((type1 >= type2 || (type1 == ShapeType.Edge && type2 == ShapeType.Polygon)) &&
				    !(type2 == ShapeType.Edge && type1 == ShapeType.Polygon))
					c = new Contact(fixtureA, indexA, fixtureB, indexB);
				else
					c = new Contact(fixtureB, indexB, fixtureA, indexA);
			}

			c._type = _contactRegisters[(int) type1, (int) type2];

			return c;
		}

		internal void Destroy()
		{
#if USE_ACTIVE_CONTACT_SET
            FixtureA.Body.World.ContactManager.RemoveActiveContact(this);
#endif
			FixtureA.Body._world._contactPool.Enqueue(this);

			if (Manifold.PointCount > 0 && FixtureA.IsSensor == false && FixtureB.IsSensor == false)
			{
				FixtureA.Body.IsAwake = true;
				FixtureB.Body.IsAwake = true;
			}

			Reset(null, 0, null, 0);
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