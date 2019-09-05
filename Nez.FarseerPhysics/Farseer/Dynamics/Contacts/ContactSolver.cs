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
		public Vector2[] LocalPoints = new Vector2[Settings.MaxManifoldPoints];
		public Vector2 LocalNormal;
		public Vector2 LocalPoint;
		public int IndexA;
		public int IndexB;
		public float InvMassA, InvMassB;
		public Vector2 LocalCenterA, LocalCenterB;
		public float InvIA, InvIB;
		public ManifoldType Type;
		public float RadiusA, RadiusB;
		public int PointCount;
	}

	public sealed class VelocityConstraintPoint
	{
		public Vector2 RA;
		public Vector2 RB;
		public float NormalImpulse;
		public float TangentImpulse;
		public float NormalMass;
		public float TangentMass;
		public float VelocityBias;
	}

	public sealed class ContactVelocityConstraint
	{
		public VelocityConstraintPoint[] Points = new VelocityConstraintPoint[Settings.MaxManifoldPoints];
		public Vector2 Normal;
		public Mat22 NormalMass;
		public Mat22 K;
		public int IndexA;
		public int IndexB;
		public float InvMassA, InvMassB;
		public float InvIA, InvIB;
		public float Friction;
		public float Restitution;
		public float TangentSpeed;
		public int PointCount;
		public int ContactIndex;

		public ContactVelocityConstraint()
		{
			for (int i = 0; i < Settings.MaxManifoldPoints; i++)
			{
				Points[i] = new VelocityConstraintPoint();
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

		public void Reset(TimeStep step, int count, Contact[] contacts, Position[] positions, Velocity[] velocities)
		{
			_step = step;
			_count = count;
			_positions = positions;
			_velocities = velocities;
			_contacts = contacts;

			// grow the array
			if (_velocityConstraints == null || _velocityConstraints.Length < count)
			{
				_velocityConstraints = new ContactVelocityConstraint[count * 2];
				_positionConstraints = new ContactPositionConstraint[count * 2];

				for (int i = 0; i < _velocityConstraints.Length; i++)
				{
					_velocityConstraints[i] = new ContactVelocityConstraint();
				}

				for (int i = 0; i < _positionConstraints.Length; i++)
				{
					_positionConstraints[i] = new ContactPositionConstraint();
				}
			}

			// Initialize position independent portions of the constraints.
			for (int i = 0; i < _count; ++i)
			{
				var contact = contacts[i];

				var fixtureA = contact.FixtureA;
				var fixtureB = contact.FixtureB;
				var shapeA = fixtureA.Shape;
				var shapeB = fixtureB.Shape;
				var radiusA = shapeA.Radius;
				var radiusB = shapeB.Radius;
				var bodyA = fixtureA.Body;
				var bodyB = fixtureB.Body;
				var manifold = contact.Manifold;

				var pointCount = manifold.PointCount;
				Debug.Assert(pointCount > 0);

				var vc = _velocityConstraints[i];
				vc.Friction = contact.Friction;
				vc.Restitution = contact.Restitution;
				vc.TangentSpeed = contact.TangentSpeed;
				vc.IndexA = bodyA.IslandIndex;
				vc.IndexB = bodyB.IslandIndex;
				vc.InvMassA = bodyA._invMass;
				vc.InvMassB = bodyB._invMass;
				vc.InvIA = bodyA._invI;
				vc.InvIB = bodyB._invI;
				vc.ContactIndex = i;
				vc.PointCount = pointCount;
				vc.K.SetZero();
				vc.NormalMass.SetZero();

				var pc = _positionConstraints[i];
				pc.IndexA = bodyA.IslandIndex;
				pc.IndexB = bodyB.IslandIndex;
				pc.InvMassA = bodyA._invMass;
				pc.InvMassB = bodyB._invMass;
				pc.LocalCenterA = bodyA._sweep.LocalCenter;
				pc.LocalCenterB = bodyB._sweep.LocalCenter;
				pc.InvIA = bodyA._invI;
				pc.InvIB = bodyB._invI;
				pc.LocalNormal = manifold.LocalNormal;
				pc.LocalPoint = manifold.LocalPoint;
				pc.PointCount = pointCount;
				pc.RadiusA = radiusA;
				pc.RadiusB = radiusB;
				pc.Type = manifold.Type;

				for (int j = 0; j < pointCount; ++j)
				{
					var cp = manifold.Points[j];
					var vcp = vc.Points[j];

					if (Settings.EnableWarmstarting)
					{
						vcp.NormalImpulse = _step.DtRatio * cp.NormalImpulse;
						vcp.TangentImpulse = _step.DtRatio * cp.TangentImpulse;
					}
					else
					{
						vcp.NormalImpulse = 0.0f;
						vcp.TangentImpulse = 0.0f;
					}

					vcp.RA = Vector2.Zero;
					vcp.RB = Vector2.Zero;
					vcp.NormalMass = 0.0f;
					vcp.TangentMass = 0.0f;
					vcp.VelocityBias = 0.0f;

					pc.LocalPoints[j] = cp.LocalPoint;
				}
			}
		}

		public void InitializeVelocityConstraints()
		{
			for (int i = 0; i < _count; ++i)
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];
				ContactPositionConstraint pc = _positionConstraints[i];

				float radiusA = pc.RadiusA;
				float radiusB = pc.RadiusB;
				Manifold manifold = _contacts[vc.ContactIndex].Manifold;

				int indexA = vc.IndexA;
				int indexB = vc.IndexB;

				float mA = vc.InvMassA;
				float mB = vc.InvMassB;
				float iA = vc.InvIA;
				float iB = vc.InvIB;
				Vector2 localCenterA = pc.LocalCenterA;
				Vector2 localCenterB = pc.LocalCenterB;

				Vector2 cA = _positions[indexA].C;
				float aA = _positions[indexA].A;
				Vector2 vA = _velocities[indexA].V;
				float wA = _velocities[indexA].W;

				Vector2 cB = _positions[indexB].C;
				float aB = _positions[indexB].A;
				Vector2 vB = _velocities[indexB].V;
				float wB = _velocities[indexB].W;

				Debug.Assert(manifold.PointCount > 0);

				Transform xfA = new Transform();
				Transform xfB = new Transform();
				xfA.Q.Set(aA);
				xfB.Q.Set(aB);
				xfA.P = cA - MathUtils.Mul(xfA.Q, localCenterA);
				xfB.P = cB - MathUtils.Mul(xfB.Q, localCenterB);

				Vector2 normal;
				FixedArray2<Vector2> points;
				WorldManifold.Initialize(ref manifold, ref xfA, radiusA, ref xfB, radiusB, out normal, out points);

				vc.Normal = normal;

				int pointCount = vc.PointCount;
				for (int j = 0; j < pointCount; ++j)
				{
					VelocityConstraintPoint vcp = vc.Points[j];

					vcp.RA = points[j] - cA;
					vcp.RB = points[j] - cB;

					float rnA = MathUtils.Cross(vcp.RA, vc.Normal);
					float rnB = MathUtils.Cross(vcp.RB, vc.Normal);

					float kNormal = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

					vcp.NormalMass = kNormal > 0.0f ? 1.0f / kNormal : 0.0f;

					Vector2 tangent = MathUtils.Cross(vc.Normal, 1.0f);

					float rtA = MathUtils.Cross(vcp.RA, tangent);
					float rtB = MathUtils.Cross(vcp.RB, tangent);

					float kTangent = mA + mB + iA * rtA * rtA + iB * rtB * rtB;

					vcp.TangentMass = kTangent > 0.0f ? 1.0f / kTangent : 0.0f;

					// Setup a velocity bias for restitution.
					vcp.VelocityBias = 0.0f;
					float vRel = Vector2.Dot(vc.Normal,
						vB + MathUtils.Cross(wB, vcp.RB) - vA - MathUtils.Cross(wA, vcp.RA));
					if (vRel < -Settings.VelocityThreshold)
					{
						vcp.VelocityBias = -vc.Restitution * vRel;
					}
				}

				// If we have two points, then prepare the block solver.
				if (vc.PointCount == 2)
				{
					VelocityConstraintPoint vcp1 = vc.Points[0];
					VelocityConstraintPoint vcp2 = vc.Points[1];

					float rn1A = MathUtils.Cross(vcp1.RA, vc.Normal);
					float rn1B = MathUtils.Cross(vcp1.RB, vc.Normal);
					float rn2A = MathUtils.Cross(vcp2.RA, vc.Normal);
					float rn2B = MathUtils.Cross(vcp2.RB, vc.Normal);

					float k11 = mA + mB + iA * rn1A * rn1A + iB * rn1B * rn1B;
					float k22 = mA + mB + iA * rn2A * rn2A + iB * rn2B * rn2B;
					float k12 = mA + mB + iA * rn1A * rn2A + iB * rn1B * rn2B;

					// Ensure a reasonable condition number.
					const float k_maxConditionNumber = 1000.0f;
					if (k11 * k11 < k_maxConditionNumber * (k11 * k22 - k12 * k12))
					{
						// K is safe to invert.
						vc.K.Ex = new Vector2(k11, k12);
						vc.K.Ey = new Vector2(k12, k22);
						vc.NormalMass = vc.K.Inverse;
					}
					else
					{
						// The constraints are redundant, just use one.
						// TODO_ERIN use deepest?
						vc.PointCount = 1;
					}
				}
			}
		}

		public void WarmStart()
		{
			// Warm start.
			for (int i = 0; i < _count; ++i)
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];

				int indexA = vc.IndexA;
				int indexB = vc.IndexB;
				float mA = vc.InvMassA;
				float iA = vc.InvIA;
				float mB = vc.InvMassB;
				float iB = vc.InvIB;
				int pointCount = vc.PointCount;

				Vector2 vA = _velocities[indexA].V;
				float wA = _velocities[indexA].W;
				Vector2 vB = _velocities[indexB].V;
				float wB = _velocities[indexB].W;

				Vector2 normal = vc.Normal;
				Vector2 tangent = MathUtils.Cross(normal, 1.0f);

				for (int j = 0; j < pointCount; ++j)
				{
					VelocityConstraintPoint vcp = vc.Points[j];
					Vector2 P = vcp.NormalImpulse * normal + vcp.TangentImpulse * tangent;
					wA -= iA * MathUtils.Cross(vcp.RA, P);
					vA -= mA * P;
					wB += iB * MathUtils.Cross(vcp.RB, P);
					vB += mB * P;
				}

				_velocities[indexA].V = vA;
				_velocities[indexA].W = wA;
				_velocities[indexB].V = vB;
				_velocities[indexB].W = wB;
			}
		}

		public void SolveVelocityConstraints()
		{
			for (int i = 0; i < _count; ++i)
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];

				int indexA = vc.IndexA;
				int indexB = vc.IndexB;
				float mA = vc.InvMassA;
				float iA = vc.InvIA;
				float mB = vc.InvMassB;
				float iB = vc.InvIB;
				int pointCount = vc.PointCount;

				Vector2 vA = _velocities[indexA].V;
				float wA = _velocities[indexA].W;
				Vector2 vB = _velocities[indexB].V;
				float wB = _velocities[indexB].W;

				Vector2 normal = vc.Normal;
				Vector2 tangent = MathUtils.Cross(normal, 1.0f);
				float friction = vc.Friction;

				Debug.Assert(pointCount == 1 || pointCount == 2);

				// Solve tangent constraints first because non-penetration is more important
				// than friction.
				for (int j = 0; j < pointCount; ++j)
				{
					VelocityConstraintPoint vcp = vc.Points[j];

					// Relative velocity at contact
					Vector2 dv = vB + MathUtils.Cross(wB, vcp.RB) - vA - MathUtils.Cross(wA, vcp.RA);

					// Compute tangent force
					float vt = Vector2.Dot(dv, tangent) - vc.TangentSpeed;
					float lambda = vcp.TangentMass * (-vt);

					// b2Clamp the accumulated force
					float maxFriction = friction * vcp.NormalImpulse;
					float newImpulse = MathUtils.Clamp(vcp.TangentImpulse + lambda, -maxFriction, maxFriction);
					lambda = newImpulse - vcp.TangentImpulse;
					vcp.TangentImpulse = newImpulse;

					// Apply contact impulse
					Vector2 P = lambda * tangent;

					vA -= mA * P;
					wA -= iA * MathUtils.Cross(vcp.RA, P);

					vB += mB * P;
					wB += iB * MathUtils.Cross(vcp.RB, P);
				}

				// Solve normal constraints
				if (vc.PointCount == 1)
				{
					VelocityConstraintPoint vcp = vc.Points[0];

					// Relative velocity at contact
					Vector2 dv = vB + MathUtils.Cross(wB, vcp.RB) - vA - MathUtils.Cross(wA, vcp.RA);

					// Compute normal impulse
					float vn = Vector2.Dot(dv, normal);
					float lambda = -vcp.NormalMass * (vn - vcp.VelocityBias);

					// b2Clamp the accumulated impulse
					float newImpulse = Math.Max(vcp.NormalImpulse + lambda, 0.0f);
					lambda = newImpulse - vcp.NormalImpulse;
					vcp.NormalImpulse = newImpulse;

					// Apply contact impulse
					Vector2 P = lambda * normal;
					vA -= mA * P;
					wA -= iA * MathUtils.Cross(vcp.RA, P);

					vB += mB * P;
					wB += iB * MathUtils.Cross(vcp.RB, P);
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

					VelocityConstraintPoint cp1 = vc.Points[0];
					VelocityConstraintPoint cp2 = vc.Points[1];

					Vector2 a = new Vector2(cp1.NormalImpulse, cp2.NormalImpulse);
					Debug.Assert(a.X >= 0.0f && a.Y >= 0.0f);

					// Relative velocity at contact
					Vector2 dv1 = vB + MathUtils.Cross(wB, cp1.RB) - vA - MathUtils.Cross(wA, cp1.RA);
					Vector2 dv2 = vB + MathUtils.Cross(wB, cp2.RB) - vA - MathUtils.Cross(wA, cp2.RA);

					// Compute normal velocity
					float vn1 = Vector2.Dot(dv1, normal);
					float vn2 = Vector2.Dot(dv2, normal);

					Vector2 b = new Vector2();
					b.X = vn1 - cp1.VelocityBias;
					b.Y = vn2 - cp2.VelocityBias;

					// Compute b'
					b -= MathUtils.Mul(ref vc.K, a);

#if B2_DEBUG_SOLVER
					const float k_errorTol = 1e-3f;
					//B2_NOT_USED(k_errorTol);
#endif

					for (;;)
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
						Vector2 x = -MathUtils.Mul(ref vc.NormalMass, b);

						if (x.X >= 0.0f && x.Y >= 0.0f)
						{
							// Get the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * (P1 + P2);
							wA -= iA * (MathUtils.Cross(cp1.RA, P1) + MathUtils.Cross(cp2.RA, P2));

							vB += mB * (P1 + P2);
							wB += iB * (MathUtils.Cross(cp1.RB, P1) + MathUtils.Cross(cp2.RB, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

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
						x.X = -cp1.NormalMass * b.X;
						x.Y = 0.0f;
						vn1 = 0.0f;
						vn2 = vc.K.Ex.Y * x.X + b.Y;

						if (x.X >= 0.0f && vn2 >= 0.0f)
						{
							// Get the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * (P1 + P2);
							wA -= iA * (MathUtils.Cross(cp1.RA, P1) + MathUtils.Cross(cp2.RA, P2));

							vB += mB * (P1 + P2);
							wB += iB * (MathUtils.Cross(cp1.RB, P1) + MathUtils.Cross(cp2.RB, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

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
						x.Y = -cp2.NormalMass * b.Y;
						vn1 = vc.K.Ey.X * x.Y + b.X;
						vn2 = 0.0f;

						if (x.Y >= 0.0f && vn1 >= 0.0f)
						{
							// Resubstitute for the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * (P1 + P2);
							wA -= iA * (MathUtils.Cross(cp1.RA, P1) + MathUtils.Cross(cp2.RA, P2));

							vB += mB * (P1 + P2);
							wB += iB * (MathUtils.Cross(cp1.RB, P1) + MathUtils.Cross(cp2.RB, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

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

						if (vn1 >= 0.0f && vn2 >= 0.0f)
						{
							// Resubstitute for the incremental impulse
							Vector2 d = x - a;

							// Apply incremental impulse
							Vector2 P1 = d.X * normal;
							Vector2 P2 = d.Y * normal;
							vA -= mA * (P1 + P2);
							wA -= iA * (MathUtils.Cross(cp1.RA, P1) + MathUtils.Cross(cp2.RA, P2));

							vB += mB * (P1 + P2);
							wB += iB * (MathUtils.Cross(cp1.RB, P1) + MathUtils.Cross(cp2.RB, P2));

							// Accumulate
							cp1.NormalImpulse = x.X;
							cp2.NormalImpulse = x.Y;

							break;
						}

						// No solution, give up. This is hit sometimes, but it doesn't seem to matter.
						break;
					}
				}

				_velocities[indexA].V = vA;
				_velocities[indexA].W = wA;
				_velocities[indexB].V = vB;
				_velocities[indexB].W = wB;
			}
		}

		public void StoreImpulses()
		{
			for (int i = 0; i < _count; ++i)
			{
				ContactVelocityConstraint vc = _velocityConstraints[i];
				Manifold manifold = _contacts[vc.ContactIndex].Manifold;

				for (int j = 0; j < vc.PointCount; ++j)
				{
					ManifoldPoint point = manifold.Points[j];
					point.NormalImpulse = vc.Points[j].NormalImpulse;
					point.TangentImpulse = vc.Points[j].TangentImpulse;
					manifold.Points[j] = point;
				}

				_contacts[vc.ContactIndex].Manifold = manifold;
			}
		}

		public bool SolvePositionConstraints()
		{
			float minSeparation = 0.0f;

			for (int i = 0; i < _count; ++i)
			{
				ContactPositionConstraint pc = _positionConstraints[i];

				int indexA = pc.IndexA;
				int indexB = pc.IndexB;
				Vector2 localCenterA = pc.LocalCenterA;
				float mA = pc.InvMassA;
				float iA = pc.InvIA;
				Vector2 localCenterB = pc.LocalCenterB;
				float mB = pc.InvMassB;
				float iB = pc.InvIB;
				int pointCount = pc.PointCount;

				Vector2 cA = _positions[indexA].C;
				float aA = _positions[indexA].A;

				Vector2 cB = _positions[indexB].C;
				float aB = _positions[indexB].A;

				// Solve normal constraints
				for (int j = 0; j < pointCount; ++j)
				{
					Transform xfA = new Transform();
					Transform xfB = new Transform();
					xfA.Q.Set(aA);
					xfB.Q.Set(aB);
					xfA.P = cA - MathUtils.Mul(xfA.Q, localCenterA);
					xfB.P = cB - MathUtils.Mul(xfB.Q, localCenterB);

					Vector2 normal;
					Vector2 point;
					float separation;

					PositionSolverManifold.Initialize(pc, xfA, xfB, j, out normal, out point, out separation);

					Vector2 rA = point - cA;
					Vector2 rB = point - cB;

					// Track max constraint error.
					minSeparation = Math.Min(minSeparation, separation);

					// Prevent large corrections and allow slop.
					float C = MathUtils.Clamp(Settings.Baumgarte * (separation + Settings.LinearSlop),
						-Settings.MaxLinearCorrection, 0.0f);

					// Compute the effective mass.
					float rnA = MathUtils.Cross(rA, normal);
					float rnB = MathUtils.Cross(rB, normal);
					float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

					// Compute normal impulse
					float impulse = K > 0.0f ? -C / K : 0.0f;

					Vector2 P = impulse * normal;

					cA -= mA * P;
					aA -= iA * MathUtils.Cross(rA, P);

					cB += mB * P;
					aB += iB * MathUtils.Cross(rB, P);
				}

				_positions[indexA].C = cA;
				_positions[indexA].A = aA;

				_positions[indexB].C = cB;
				_positions[indexB].A = aB;
			}

			// We can't expect minSpeparation >= -b2_linearSlop because we don't
			// push the separation above -b2_linearSlop.
			return minSeparation >= -3.0f * Settings.LinearSlop;
		}

		// Sequential position solver for position constraints.
		public bool SolveTOIPositionConstraints(int toiIndexA, int toiIndexB)
		{
			float minSeparation = 0.0f;

			for (int i = 0; i < _count; ++i)
			{
				ContactPositionConstraint pc = _positionConstraints[i];

				int indexA = pc.IndexA;
				int indexB = pc.IndexB;
				Vector2 localCenterA = pc.LocalCenterA;
				Vector2 localCenterB = pc.LocalCenterB;
				int pointCount = pc.PointCount;

				float mA = 0.0f;
				float iA = 0.0f;
				if (indexA == toiIndexA || indexA == toiIndexB)
				{
					mA = pc.InvMassA;
					iA = pc.InvIA;
				}

				float mB = 0.0f;
				float iB = 0.0f;
				if (indexB == toiIndexA || indexB == toiIndexB)
				{
					mB = pc.InvMassB;
					iB = pc.InvIB;
				}

				Vector2 cA = _positions[indexA].C;
				float aA = _positions[indexA].A;

				Vector2 cB = _positions[indexB].C;
				float aB = _positions[indexB].A;

				// Solve normal constraints
				for (int j = 0; j < pointCount; ++j)
				{
					Transform xfA = new Transform();
					Transform xfB = new Transform();
					xfA.Q.Set(aA);
					xfB.Q.Set(aB);
					xfA.P = cA - MathUtils.Mul(xfA.Q, localCenterA);
					xfB.P = cB - MathUtils.Mul(xfB.Q, localCenterB);

					Vector2 normal;
					Vector2 point;
					float separation;

					PositionSolverManifold.Initialize(pc, xfA, xfB, j, out normal, out point, out separation);

					Vector2 rA = point - cA;
					Vector2 rB = point - cB;

					// Track max constraint error.
					minSeparation = Math.Min(minSeparation, separation);

					// Prevent large corrections and allow slop.
					float C = MathUtils.Clamp(Settings.Baumgarte * (separation + Settings.LinearSlop),
						-Settings.MaxLinearCorrection, 0.0f);

					// Compute the effective mass.
					float rnA = MathUtils.Cross(rA, normal);
					float rnB = MathUtils.Cross(rB, normal);
					float K = mA + mB + iA * rnA * rnA + iB * rnB * rnB;

					// Compute normal impulse
					float impulse = K > 0.0f ? -C / K : 0.0f;

					Vector2 P = impulse * normal;

					cA -= mA * P;
					aA -= iA * MathUtils.Cross(rA, P);

					cB += mB * P;
					aB += iB * MathUtils.Cross(rB, P);
				}

				_positions[indexA].C = cA;
				_positions[indexA].A = aA;

				_positions[indexB].C = cB;
				_positions[indexB].A = aB;
			}

			// We can't expect minSpeparation >= -b2_linearSlop because we don't
			// push the separation above -b2_linearSlop.
			return minSeparation >= -1.5f * Settings.LinearSlop;
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
			public static void Initialize(ref Manifold manifold, ref Transform xfA, float radiusA, ref Transform xfB,
			                              float radiusB, out Vector2 normal, out FixedArray2<Vector2> points)
			{
				normal = Vector2.Zero;
				points = new FixedArray2<Vector2>();

				if (manifold.PointCount == 0)
					return;

				switch (manifold.Type)
				{
					case ManifoldType.Circles:
					{
						normal = new Vector2(1.0f, 0.0f);
						var pointA = MathUtils.Mul(ref xfA, manifold.LocalPoint);
						var pointB = MathUtils.Mul(ref xfB, manifold.Points[0].LocalPoint);
						if (Vector2.DistanceSquared(pointA, pointB) > Settings.Epsilon * Settings.Epsilon)
						{
							normal = pointB - pointA;
							Nez.Vector2Ext.Normalize(ref normal);
						}

						var cA = pointA + radiusA * normal;
						var cB = pointB - radiusB * normal;
						points[0] = 0.5f * (cA + cB);
						break;
					}


					case ManifoldType.FaceA:
					{
						normal = MathUtils.Mul(xfA.Q, manifold.LocalNormal);
						var planePoint = MathUtils.Mul(ref xfA, manifold.LocalPoint);

						for (int i = 0; i < manifold.PointCount; ++i)
						{
							var clipPoint = MathUtils.Mul(ref xfB, manifold.Points[i].LocalPoint);
							var cA = clipPoint + (radiusA - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
							var cB = clipPoint - radiusB * normal;
							points[i] = 0.5f * (cA + cB);
						}

						break;
					}

					case ManifoldType.FaceB:
					{
						normal = MathUtils.Mul(xfB.Q, manifold.LocalNormal);
						var planePoint = MathUtils.Mul(ref xfB, manifold.LocalPoint);

						for (int i = 0; i < manifold.PointCount; ++i)
						{
							var clipPoint = MathUtils.Mul(ref xfA, manifold.Points[i].LocalPoint);
							var cB = clipPoint + (radiusB - Vector2.Dot(clipPoint - planePoint, normal)) * normal;
							var cA = clipPoint - radiusA * normal;
							points[i] = 0.5f * (cA + cB);
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
			public static void Initialize(ContactPositionConstraint pc, Transform xfA, Transform xfB, int index,
			                              out Vector2 normal, out Vector2 point, out float separation)
			{
				Debug.Assert(pc.PointCount > 0);

				switch (pc.Type)
				{
					case ManifoldType.Circles:
					{
						var pointA = MathUtils.Mul(ref xfA, pc.LocalPoint);
						var pointB = MathUtils.Mul(ref xfB, pc.LocalPoints[0]);
						normal = pointB - pointA;
						Nez.Vector2Ext.Normalize(ref normal);
						point = 0.5f * (pointA + pointB);
						separation = Vector2.Dot(pointB - pointA, normal) - pc.RadiusA - pc.RadiusB;
						break;
					}

					case ManifoldType.FaceA:
					{
						normal = MathUtils.Mul(xfA.Q, pc.LocalNormal);
						var planePoint = MathUtils.Mul(ref xfA, pc.LocalPoint);

						var clipPoint = MathUtils.Mul(ref xfB, pc.LocalPoints[index]);
						separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.RadiusA - pc.RadiusB;
						point = clipPoint;
						break;
					}

					case ManifoldType.FaceB:
					{
						normal = MathUtils.Mul(xfB.Q, pc.LocalNormal);
						var planePoint = MathUtils.Mul(ref xfB, pc.LocalPoint);

						var clipPoint = MathUtils.Mul(ref xfA, pc.LocalPoints[index]);
						separation = Vector2.Dot(clipPoint - planePoint, normal) - pc.RadiusA - pc.RadiusB;
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