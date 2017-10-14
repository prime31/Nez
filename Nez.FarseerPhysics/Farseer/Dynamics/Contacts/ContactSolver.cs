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

using System;
using System.Diagnostics;
using FarseerPhysics.Collision;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Contacts
{
	public sealed class ContactPositionConstraint
	{
		public Vector2[] localPoints = new Vector2[Settings.maxManifoldPoints];
		public Vector2 localNormal;
		public Vector2 localPoint;
		public int indexA;
		public int indexB;
		public float invMassA, invMassB;
		public Vector2 localCenterA, localCenterB;
		public float invIA, invIB;
		public ManifoldType type;
		public float radiusA, radiusB;
		public int pointCount;
	}

	public sealed class VelocityConstraintPoint
	{
		public Vector2 rA;
		public Vector2 rB;
		public float normalImpulse;
		public float tangentImpulse;
		public float normalMass;
		public float tangentMass;
		public float velocityBias;
	}

	public sealed class ContactVelocityConstraint
	{
		public VelocityConstraintPoint[] points = new VelocityConstraintPoint[Settings.maxManifoldPoints];
		public Vector2 normal;
		public Mat22 normalMass;
		public Mat22 K;
		public int indexA;
		public int indexB;
		public float invMassA, invMassB;
		public float invIA, invIB;
		public float friction;
		public float restitution;
		public float tangentSpeed;
		public int pointCount;
		public int contactIndex;

		public ContactVelocityConstraint()
		{
			for( int i = 0; i < Settings.maxManifoldPoints; i++ )
			{
				points[i] = new VelocityConstraintPoint();
			}
		}
	}

	public class ContactSolver
	{
		public TimeStep _step;
		public Position[] _positions;
		public Velocity[] _velocities;
		public ContactPositionConstraint[] _positionConstraints;
		public ContactVelocityConstraint[] _velocityConstraints;
		public Contact[] _contacts;
		public int _count;

		public void reset( TimeStep step, int count, Contact[] contacts, Position[] positions, Velocity[] velocities )
		{
			_step = step;
			_count = count;
			_positions = positions;
			_velocities = velocities;
			_contacts = contacts;

			// grow the array
			if( _velocityConstraints == null || _velocityConstraints.Length < count )
			{
				_velocityConstraints = new ContactVelocityConstraint[count * 2];
				_positionConstraints = new ContactPositionConstraint[count * 2];

				for( int i = 0; i < _velocityConstraints.Length; i++ )
				{
					_velocityConstraints[i] = new ContactVelocityConstraint();
				}

				for( int i = 0; i < _positionConstraints.Length; i++ )
				{
					_positionConstraints[i] = new ContactPositionConstraint();
				}
			}

			// Initialize position independent portions of the constraints.
			for( int i = 0; i < _count; ++i )
			{
				var contact = contacts[i];

				var fixtureA = contact.fixtureA;
				var fixtureB = contact.fixtureB;
				var shapeA = fixtureA.shape;
				var shapeB = fixtureB.shape;
				var radiusA = shapeA.radius;
				var radiusB = shapeB.radius;
				var bodyA = fixtureA.body;
				var bodyB = fixtureB.body;
				var manifold = contact.manifold;

				var pointCount = manifold.pointCount;
				Debug.Assert( pointCount > 0 );

				var vc = _velocityConstraints[i];
				vc.friction = contact.friction;
				vc.restitution = contact.restitution;
				vc.tangentSpeed = contact.tangentSpeed;
				vc.indexA = bodyA.islandIndex;
				vc.indexB = bodyB.islandIndex;
				vc.invMassA = bodyA._invMass;
				vc.invMassB = bodyB._invMass;
				vc.invIA = bodyA._invI;
				vc.invIB = bodyB._invI;
				vc.contactIndex = i;
				vc.pointCount = pointCount;
				vc.K.SetZero();
				vc.normalMass.SetZero();

				var pc = _positionConstraints[i];
				pc.indexA = bodyA.islandIndex;
				pc.indexB = bodyB.islandIndex;
				pc.invMassA = bodyA._invMass;
				pc.invMassB = bodyB._invMass;
				pc.localCenterA = bodyA._sweep.localCenter;
				pc.localCenterB = bodyB._sweep.localCenter;
				pc.invIA = bodyA._invI;
				pc.invIB = bodyB._invI;
				pc.localNormal = manifold.localNormal;
				pc.localPoint = manifold.localPoint;
				pc.pointCount = pointCount;
				pc.radiusA = radiusA;
				pc.radiusB = radiusB;
				pc.type = manifold.type;

				for( int j = 0; j < pointCount; ++j )
				{
					var cp = manifold.points[j];
					var vcp = vc.points[j];

					if( Settings.enableWarmstarting )
					{
						vcp.normalImpulse = _step.dtRatio * cp.normalImpulse;
						vcp.tangentImpulse = _step.dtRatio * cp.tangentImpulse;
					}
					else
					{
						vcp.normalImpulse = 0.0f;
						vcp.tangentImpulse = 0.0f;
					}

					vcp.rA = Vector2.Zero;
					vcp.rB = Vector2.Zero;
					vcp.normalMass = 0.0f;
					vcp.tangentMass = 0.0f;
					vcp.velocityBias = 0.0f;

					pc.localPoints[j] = cp.localPoint;
				}
			}
		}

		public void initializeVelocityConstraints()
		{
			for( int i = 0; i < _count; ++i )
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];
				ContactPositionConstraint pc = _positionConstraints[i];

				float radiusA = pc.radiusA;
				float radiusB = pc.radiusB;
				Manifold manifold = _contacts[vc.contactIndex].manifold;

				int indexA = vc.indexA;
				int indexB = vc.indexB;

				float mA = vc.invMassA;
				float mB = vc.invMassB;
				float iA = vc.invIA;
				float iB = vc.invIB;
				Vector2 localCenterA = pc.localCenterA;
				Vector2 localCenterB = pc.localCenterB;

				Vector2 cA = _positions[indexA].c;
				float aA = _positions[indexA].a;
				Vector2 vA = _velocities[indexA].v;
				float wA = _velocities[indexA].w;

				Vector2 cB = _positions[indexB].c;
				float aB = _positions[indexB].a;
				Vector2 vB = _velocities[indexB].v;
				float wB = _velocities[indexB].w;

				Debug.Assert( manifold.pointCount > 0 );

				Transform xfA = new Transform();
				Transform xfB = new Transform();
				xfA.q.Set( aA );
				xfB.q.Set( aB );
				xfA.p = cA - MathUtils.mul( xfA.q, localCenterA );
				xfB.p = cB - MathUtils.mul( xfB.q, localCenterB );

				Vector2 normal;
				FixedArray2<Vector2> points;
				WorldManifold.initialize( ref manifold, ref xfA, radiusA, ref xfB, radiusB, out normal, out points );

				vc.normal = normal;

				int pointCount = vc.pointCount;
				for( int j = 0; j < pointCount; ++j )
				{
					VelocityConstraintPoint vcp = vc.points[j];

					vcp.rA = points[j] - cA;
					vcp.rB = points[j] - cB;

					float rnA = MathUtils.cross( vcp.rA, vc.normal );
					float rnB = MathUtils.cross( vcp.rB, vc.normal );

					float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

					vcp.normalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;

					Vector2 tangent = MathUtils.cross( vc.normal, 1.0f );

					float rtA = MathUtils.cross( vcp.rA, tangent );
					float rtB = MathUtils.cross( vcp.rB, tangent );

					float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

					vcp.tangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

					// Setup a velocity bias for restitution.
					vcp.velocityBias = 0.0f;
					float vRel = Vector2.Dot( vc.normal, vB + MathUtils.cross( wB, vcp.rB ) - vA - MathUtils.cross( wA, vcp.rA ) );
					if( vRel < -Settings.velocityThreshold )
					{
						vcp.velocityBias = -vc.restitution * vRel;
					}
				}

				// If we have two points, then prepare the block solver.
				if( vc.pointCount == 2 )
				{
					VelocityConstraintPoint vcp1 = vc.points[0];
					VelocityConstraintPoint vcp2 = vc.points[1];

					float rn1A = MathUtils.cross( vcp1.rA, vc.normal );
					float rn1B = MathUtils.cross( vcp1.rB, vc.normal );
					float rn2A = MathUtils.cross( vcp2.rA, vc.normal );
					float rn2B = MathUtils.cross( vcp2.rB, vc.normal );

					float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
					float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
					float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

					// Ensure a reasonable condition number.
					const float k_maxConditionNumber = 1000.0f;
					if( k11 * k11 < k_maxConditionNumber * ( k11 * k22 - k12 * k12 ) )
					{
						// K is safe to invert.
						vc.K.ex = new Vector2( k11, k12 );
						vc.K.ey = new Vector2( k12, k22 );
						vc.normalMass = vc.K.Inverse;
					}
					else
					{
						// The constraints are redundant, just use one.
						// TODO_ERIN use deepest?
						vc.pointCount = 1;
					}
				}
			}
		}

		public void warmStart()
		{
			// Warm start.
			for( int i = 0; i < _count; ++i )
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];

				int indexA = vc.indexA;
				int indexB = vc.indexB;
				float mA = vc.invMassA;
				float iA = vc.invIA;
				float mB = vc.invMassB;
				float iB = vc.invIB;
				int pointCount = vc.pointCount;

				Vector2 vA = _velocities[indexA].v;
				float wA = _velocities[indexA].w;
				Vector2 vB = _velocities[indexB].v;
				float wB = _velocities[indexB].w;

				Vector2 normal = vc.normal;
				Vector2 tangent = MathUtils.cross( normal, 1.0f );

				for( int j = 0; j < pointCount; ++j )
				{
					VelocityConstraintPoint vcp = vc.points[j];
					Vector2 P = vcp.normalImpulse * normal + vcp.tangentImpulse * tangent;
					wA -= iA * MathUtils.cross( vcp.rA, P );
					vA -= mA * P;
					wB += iB * MathUtils.cross( vcp.rB, P );
					vB += mB * P;
				}

				_velocities[indexA].v = vA;
				_velocities[indexA].w = wA;
				_velocities[indexB].v = vB;
				_velocities[indexB].w = wB;
			}
		}

		public void solveVelocityConstraints()
		{
			for( int i = 0; i < _count; ++i )
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];

				int indexA = vc.indexA;
				int indexB = vc.indexB;
				float mA = vc.invMassA;
				float iA = vc.invIA;
				float mB = vc.invMassB;
				float iB = vc.invIB;
				int pointCount = vc.pointCount;

				Vector2 vA = _velocities[indexA].v;
				float wA = _velocities[indexA].w;
				Vector2 vB = _velocities[indexB].v;
				float wB = _velocities[indexB].w;

				Vector2 normal = vc.normal;
				Vector2 tangent = MathUtils.cross( normal, 1.0f );
				float friction = vc.friction;

				Debug.Assert( pointCount == 1 || pointCount == 2 );

				// Solve tangent constraints first because non-penetration is more important
				// than friction.
				for( int j = 0; j < pointCount; ++j )
				{
					VelocityConstraintPoint vcp = vc.points[j];

					// Relative velocity at contact
					Vector2 dv = vB + MathUtils.cross( wB, vcp.rB ) - vA - MathUtils.cross( wA, vcp.rA );

					// Compute tangent force
					float vt = Vector2.Dot( dv, tangent ) - vc.tangentSpeed;
					float lambda = vcp.tangentMass * ( -vt );

					// b2Clamp the accumulated force
					float maxFriction = friction * vcp.normalImpulse;
					float newImpulse = MathUtils.clamp( vcp.tangentImpulse + lambda, -maxFriction, maxFriction );
					lambda = newImpulse - vcp.tangentImpulse;
					vcp.tangentImpulse = newImpulse;

					// Apply contact impulse
					Vector2 P = lambda * tangent;

					vA -= mA * P;
					wA -= iA * MathUtils.cross( vcp.rA, P );

					vB += mB * P;
					wB += iB * MathUtils.cross( vcp.rB, P );
				}

				// Solve normal constraints
				if( vc.pointCount == 1 )
				{
					VelocityConstraintPoint vcp = vc.points[0];

					// Relative velocity at contact
					Vector2 dv = vB + MathUtils.cross( wB, vcp.rB ) - vA - MathUtils.cross( wA, vcp.rA );

					// Compute normal impulse
					float vn = Vector2.Dot( dv, normal );
					float lambda = -vcp.normalMass * ( vn - vcp.velocityBias );

					// b2Clamp the accumulated impulse
					float newImpulse = Math.Max( vcp.normalImpulse + lambda, 0.0f );
					lambda = newImpulse - vcp.normalImpulse;
					vcp.normalImpulse = newImpulse;

					// Apply contact impulse
					Vector2 P = lambda * normal;
					vA -= mA * P;
					wA -= iA * MathUtils.cross( vcp.rA, P );

					vB += mB * P;
					wB += iB * MathUtils.cross( vcp.rB, P );
				}
				else
				{
					// Block solver developed in collaboration with Dirk Gregorius (back in 01/07 on Box2D_Lite).
					// Build the mini LCP for this contact patch
					//
					// vn = A * x + b, vn >= 0, , vn >= 0, x >= 0 and vn_i * x_i = 0 with i = 1..2
					//
					// A = J * W * JT and J = ( -n, -r1 x n, n, r2 x n )
					// b = vn0 - velocityBias
					//
					// The system is solved using the "Total enumeration method" (s. Murty). The complementary constraint vn_i * x_i
					// implies that we must have in any solution either vn_i = 0 or x_i = 0. So for the 2D contact problem the cases
					// vn1 = 0 and vn2 = 0, x1 = 0 and x2 = 0, x1 = 0 and vn2 = 0, x2 = 0 and vn1 = 0 need to be tested. The first valid
					// solution that satisfies the problem is chosen.
					// 
					// In order to account of the accumulated impulse 'a' (because of the iterative nature of the solver which only requires
					// that the accumulated impulse is clamped and not the incremental impulse) we change the impulse variable (x_i).
					//
					// Substitute:
					// 
					// x = a + d
					// 
					// a := old total impulse
					// x := new total impulse
					// d := incremental impulse 
					//
					// For the current iteration we extend the formula for the incremental impulse
					// to compute the new total impulse:
					//
					// vn = A * d + b
					//    = A * (x - a) + b
					//    = A * x + b - A * a
					//    = A * x + b'
					// b' = b - A * a;

					VelocityConstraintPoint cp1 = vc.points[0];
					VelocityConstraintPoint cp2 = vc.points[1];

					Vector2 a = new Vector2( cp1.normalImpulse, cp2.normalImpulse );
					Debug.Assert( a.X >= 0.0f && a.Y >= 0.0f );

					// Relative velocity at contact
					Vector2 dv1 = vB + MathUtils.cross( wB, cp1.rB ) - vA - MathUtils.cross( wA, cp1.rA );
					Vector2 dv2 = vB + MathUtils.cross( wB, cp2.rB ) - vA - MathUtils.cross( wA, cp2.rA );

					// Compute normal velocity
					float vn1 = Vector2.Dot( dv1, normal );
					float vn2 = Vector2.Dot( dv2, normal );

					Vector2 b = new Vector2();
					b.X = vn1 - cp1.velocityBias;
					b.Y = vn2 - cp2.velocityBias;

					// Compute b'
					b -= MathUtils.mul( ref vc.K, a );

#if B2_DEBUG_SOLVER
					const float k_errorTol = 1e-3f;
					//B2_NOT_USED(k_errorTol);
#endif

					for( ;; )
					{
						//
						// Case 1: vn = 0
						//
						// 0 = A * x + b'
						//
						// Solve for x:
						//
						// x = - inv(A) * b'
						//
						Vector2 x = -MathUtils.mul( ref vc.normalMass, b );

						if( x.X >= 0.0f && x.Y >= 0.0f )
						{
							// Get the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * ( P1 + P2 );
							wA -= iA * ( MathUtils.cross( cp1.rA, P1 ) + MathUtils.cross( cp2.rA, P2 ) );

							vB += mB * ( P1 + P2 );
							wB += iB * ( MathUtils.cross( cp1.rB, P1 ) + MathUtils.cross( cp2.rB, P2 ) );

							// Accumulate
							cp1.normalImpulse = x.X;
							cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER
					// Postconditions
					dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);
					dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

					// Compute normal velocity
					vn1 = Vector2.Dot(dv1, normal);
					vn2 = Vector2.Dot(dv2, normal);

					b2Assert(b2Abs(vn1 - cp1.velocityBias) < k_errorTol);
					b2Assert(b2Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
							break;
						}

						//
						// Case 2: vn1 = 0 and x2 = 0
						//
						//   0 = a11 * x1 + a12 * 0 + b1' 
						// vn2 = a21 * x1 + a22 * 0 + b2'
						//
						x.X = -cp1.normalMass * b.X;
						x.Y = 0.0f;
						vn1 = 0.0f;
						vn2 = vc.K.ex.Y * x.X + b.Y;

						if( x.X >= 0.0f && vn2 >= 0.0f )
						{
							// Get the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * ( P1 + P2 );
							wA -= iA * ( MathUtils.cross( cp1.rA, P1 ) + MathUtils.cross( cp2.rA, P2 ) );

							vB += mB * ( P1 + P2 );
							wB += iB * ( MathUtils.cross( cp1.rB, P1 ) + MathUtils.cross( cp2.rB, P2 ) );

							// Accumulate
							cp1.normalImpulse = x.X;
							cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER
					// Postconditions
					dv1 = vB + MathUtils.Cross(wB, cp1.rB) - vA - MathUtils.Cross(wA, cp1.rA);

					// Compute normal velocity
					vn1 = Vector2.Dot(dv1, normal);

					b2Assert(b2Abs(vn1 - cp1.velocityBias) < k_errorTol);
#endif
							break;
						}


						//
						// Case 3: vn2 = 0 and x1 = 0
						//
						// vn1 = a11 * 0 + a12 * x2 + b1' 
						//   0 = a21 * 0 + a22 * x2 + b2'
						//
						x.X = 0.0f;
						x.Y = -cp2.normalMass * b.Y;
						vn1 = vc.K.ey.X * x.Y + b.X;
						vn2 = 0.0f;

						if( x.Y >= 0.0f && vn1 >= 0.0f )
						{
							// Resubstitute for the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * ( P1 + P2 );
							wA -= iA * ( MathUtils.cross( cp1.rA, P1 ) + MathUtils.cross( cp2.rA, P2 ) );

							vB += mB * ( P1 + P2 );
							wB += iB * ( MathUtils.cross( cp1.rB, P1 ) + MathUtils.cross( cp2.rB, P2 ) );

							// Accumulate
							cp1.normalImpulse = x.X;
							cp2.normalImpulse = x.Y;

#if B2_DEBUG_SOLVER
					// Postconditions
					dv2 = vB + MathUtils.Cross(wB, cp2.rB) - vA - MathUtils.Cross(wA, cp2.rA);

					// Compute normal velocity
					vn2 = Vector2.Dot(dv2, normal);

					b2Assert(b2Abs(vn2 - cp2.velocityBias) < k_errorTol);
#endif
							break;
						}

						//
						// Case 4: x1 = 0 and x2 = 0
						// 
						// vn1 = b1
						// vn2 = b2;
						x.X = 0.0f;
						x.Y = 0.0f;
						vn1 = b.X;
						vn2 = b.Y;

						if( vn1 >= 0.0f && vn2 >= 0.0f )
						{
							// Resubstitute for the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * ( P1 + P2 );
							wA -= iA * ( MathUtils.cross( cp1.rA, P1 ) + MathUtils.cross( cp2.rA, P2 ) );

							vB += mB * ( P1 + P2 );
							wB += iB * ( MathUtils.cross( cp1.rB, P1 ) + MathUtils.cross( cp2.rB, P2 ) );

							// Accumulate
							cp1.normalImpulse = x.X;
							cp2.normalImpulse = x.Y;

							break;
						}

						// No solution, give up. This is hit sometimes, but it doesn't seem to matter.
						break;
					}
				}

				_velocities[indexA].v = vA;
				_velocities[indexA].w = wA;
				_velocities[indexB].v = vB;
				_velocities[indexB].w = wB;
			}
		}

		public void storeImpulses()
		{
			for( int i = 0; i < _count; ++i )
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];
				Manifold manifold = _contacts[vc.contactIndex].manifold;

				for( int j = 0; j < vc.pointCount; ++j )
				{
					ManifoldPoint point = manifold.points[j];
					point.normalImpulse = vc.points[j].normalImpulse;
					point.tangentImpulse = vc.points[j].tangentImpulse;
					manifold.points[j] = point;
				}

				_contacts[vc.contactIndex].manifold = manifold;
			}
		}

		public bool solvePositionConstraints()
		{
			float minSeparation = 0.0f;

			for( int i = 0; i < _count; ++i )
			{
				ContactPositionConstraint pc = _positionConstraints[i];

				int indexA = pc.indexA;
				int indexB = pc.indexB;
				Vector2 localCenterA = pc.localCenterA;
				float mA = pc.invMassA;
				float iA = pc.invIA;
				Vector2 localCenterB = pc.localCenterB;
				float mB = pc.invMassB;
				float iB = pc.invIB;
				int pointCount = pc.pointCount;

				Vector2 cA = _positions[indexA].c;
				float aA = _positions[indexA].a;

				Vector2 cB = _positions[indexB].c;
				float aB = _positions[indexB].a;

				// Solve normal constraints
				for( int j = 0; j < pointCount; ++j )
				{
					Transform xfA = new Transform();
					Transform xfB = new Transform();
					xfA.q.Set( aA );
					xfB.q.Set( aB );
					xfA.p = cA - MathUtils.mul( xfA.q, localCenterA );
					xfB.p = cB - MathUtils.mul( xfB.q, localCenterB );

					Vector2 normal;
					Vector2 point;
					float separation;

					PositionSolverManifold.initialize( pc, xfA, xfB, j, out normal, out point, out separation );

					Vector2 rA = point - cA;
					Vector2 rB = point - cB;

					// Track max constraint error.
					minSeparation = Math.Min( minSeparation, separation );

					// Prevent large corrections and allow slop.
					float C = MathUtils.clamp( Settings.baumgarte * ( separation + Settings.linearSlop ), -Settings.maxLinearCorrection, 0.0f );

					// Compute the effective mass.
					float rnA = MathUtils.cross( rA, normal );
					float rnB = MathUtils.cross( rB, normal );
					float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

					// Compute normal impulse
					float impulse = K > 0.0f ? -C / K : 0.0f;

					Vector2 P = impulse * normal;

					cA -= mA * P;
					aA -= iA * MathUtils.cross( rA, P );

					cB += mB * P;
					aB += iB * MathUtils.cross( rB, P );
				}

				_positions[indexA].c = cA;
				_positions[indexA].a = aA;

				_positions[indexB].c = cB;
				_positions[indexB].a = aB;
			}

			// We can't expect minSpeparation >= -b2_linearSlop because we don't
			// push the separation above -b2_linearSlop.
			return minSeparation >= -3.0f * Settings.linearSlop;
		}

		// Sequential position solver for position constraints.
		public bool solveTOIPositionConstraints( int toiIndexA, int toiIndexB )
		{
			float minSeparation = 0.0f;

			for( int i = 0; i < _count; ++i )
			{
				ContactPositionConstraint pc = _positionConstraints[i];

				int indexA = pc.indexA;
				int indexB = pc.indexB;
				Vector2 localCenterA = pc.localCenterA;
				Vector2 localCenterB = pc.localCenterB;
				int pointCount = pc.pointCount;

				float mA = 0.0f;
				float iA = 0.0f;
				if( indexA == toiIndexA || indexA == toiIndexB )
				{
					mA = pc.invMassA;
					iA = pc.invIA;
				}

				float mB = 0.0f;
				float iB = 0.0f;
				if( indexB == toiIndexA || indexB == toiIndexB )
				{
					mB = pc.invMassB;
					iB = pc.invIB;
				}

				Vector2 cA = _positions[indexA].c;
				float aA = _positions[indexA].a;

				Vector2 cB = _positions[indexB].c;
				float aB = _positions[indexB].a;

				// Solve normal constraints
				for( int j = 0; j < pointCount; ++j )
				{
					Transform xfA = new Transform();
					Transform xfB = new Transform();
					xfA.q.Set( aA );
					xfB.q.Set( aB );
					xfA.p = cA - MathUtils.mul( xfA.q, localCenterA );
					xfB.p = cB - MathUtils.mul( xfB.q, localCenterB );

					Vector2 normal;
					Vector2 point;
					float separation;

					PositionSolverManifold.initialize( pc, xfA, xfB, j, out normal, out point, out separation );

					Vector2 rA = point - cA;
					Vector2 rB = point - cB;

					// Track max constraint error.
					minSeparation = Math.Min( minSeparation, separation );

					// Prevent large corrections and allow slop.
					float C = MathUtils.clamp( Settings.baumgarte * ( separation + Settings.linearSlop ), -Settings.maxLinearCorrection, 0.0f );

					// Compute the effective mass.
					float rnA = MathUtils.cross( rA, normal );
					float rnB = MathUtils.cross( rB, normal );
					float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

					// Compute normal impulse
					float impulse = K > 0.0f ? -C / K : 0.0f;

					Vector2 P = impulse * normal;

					cA -= mA * P;
					aA -= iA * MathUtils.cross( rA, P );

					cB += mB * P;
					aB += iB * MathUtils.cross( rB, P );
				}

				_positions[indexA].c = cA;
				_positions[indexA].a = aA;

				_positions[indexB].c = cB;
				_positions[indexB].a = aB;
			}

			// We can't expect minSpeparation >= -b2_linearSlop because we don't
			// push the separation above -b2_linearSlop.
			return minSeparation >= -1.5f * Settings.linearSlop;
		}


		public static class WorldManifold
		{
			/// <summary>
			/// Evaluate the manifold with supplied transforms. This assumes
			/// modest motion from the original state. This does not change the
			/// point count, impulses, etc. The radii must come from the Shapes
			/// that generated the manifold.
			/// </summary>
			/// <param name="manifold">The manifold.</param>
			/// <param name="xfA">The transform for A.</param>
			/// <param name="radiusA">The radius for A.</param>
			/// <param name="xfB">The transform for B.</param>
			/// <param name="radiusB">The radius for B.</param>
			/// <param name="normal">World vector pointing from A to B</param>
			/// <param name="points">Torld contact point (point of intersection).</param>
			public static void initialize( ref Manifold manifold, ref Transform xfA, float radiusA, ref Transform xfB, float radiusB, out Vector2 normal, out FixedArray2<Vector2> points )
			{
				normal = Vector2.Zero;
				points = new FixedArray2<Vector2>();

				if( manifold.pointCount == 0 )
					return;

				switch( manifold.type )
				{
					case ManifoldType.Circles:
						{
							normal = new Vector2( 1.0f, 0.0f );
							var pointA = MathUtils.mul( ref xfA, manifold.localPoint );
							var pointB = MathUtils.mul( ref xfB, manifold.points[0].localPoint );
							if( Vector2.DistanceSquared( pointA, pointB ) > Settings.epsilon * Settings.epsilon )
							{
								normal = pointB - pointA;
								Nez.Vector2Ext.normalize( ref normal );
							}

							var cA = pointA + radiusA * normal;
							var cB = pointB - radiusB * normal;
							points[0] = 0.5f * ( cA + cB );
							break;
						}


					case ManifoldType.FaceA:
						{
							normal = MathUtils.mul( xfA.q, manifold.localNormal );
							var planePoint = MathUtils.mul( ref xfA, manifold.localPoint );

							for( int i = 0; i < manifold.pointCount; ++i )
							{
								var clipPoint = MathUtils.mul( ref xfB, manifold.points[i].localPoint );
								var cA = clipPoint + ( radiusA - Vector2.Dot( clipPoint - planePoint, normal ) ) * normal;
								var cB = clipPoint - radiusB * normal;
								points[i] = 0.5f * ( cA + cB );
							}
							break;
						}

					case ManifoldType.FaceB:
						{
							normal = MathUtils.mul( xfB.q, manifold.localNormal );
							var planePoint = MathUtils.mul( ref xfB, manifold.localPoint );

							for( int i = 0; i < manifold.pointCount; ++i )
							{
								var clipPoint = MathUtils.mul( ref xfA, manifold.points[i].localPoint );
								var cB = clipPoint + ( radiusB - Vector2.Dot( clipPoint - planePoint, normal ) ) * normal;
								var cA = clipPoint - radiusA * normal;
								points[i] = 0.5f * ( cA + cB );
							}

							// Ensure normal points from A to B.
							normal = -normal;
							break;
						}
				}
			}
		}


		static class PositionSolverManifold
		{
			public static void initialize( ContactPositionConstraint pc, Transform xfA, Transform xfB, int index, out Vector2 normal, out Vector2 point, out float separation )
			{
				Debug.Assert( pc.pointCount > 0 );

				switch( pc.type )
				{
					case ManifoldType.Circles:
					{
						var pointA = MathUtils.mul( ref xfA, pc.localPoint );
						var pointB = MathUtils.mul( ref xfB, pc.localPoints[0] );
						normal = pointB - pointA;
						Nez.Vector2Ext.normalize( ref normal );
						point = 0.5f * ( pointA + pointB );
						separation = Vector2.Dot( pointB - pointA, normal ) - pc.radiusA - pc.radiusB;
						break;
					}

					case ManifoldType.FaceA:
					{
						normal = MathUtils.mul( xfA.q, pc.localNormal );
						var planePoint = MathUtils.mul( ref xfA, pc.localPoint );

						var clipPoint = MathUtils.mul( ref xfB, pc.localPoints[index] );
						separation = Vector2.Dot( clipPoint - planePoint, normal ) - pc.radiusA - pc.radiusB;
						point = clipPoint;
						break;
					}

					case ManifoldType.FaceB:
					{
						normal = MathUtils.mul( xfB.q, pc.localNormal );
						var planePoint = MathUtils.mul( ref xfB, pc.localPoint );

						var clipPoint = MathUtils.mul( ref xfA, pc.localPoints[index] );
						separation = Vector2.Dot( clipPoint - planePoint, normal ) - pc.radiusA - pc.radiusB;
						point = clipPoint;

						// Ensure normal points from A to B
						normal = -normal;
						break;
					}

					default:
						normal = Vector2.Zero;
						point = Vector2.Zero;
						separation = 0;
						break;

				}
			}
		}

	}
}