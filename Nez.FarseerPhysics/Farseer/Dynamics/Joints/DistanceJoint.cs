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
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Joints
{
	// 1-D rained system
	// m (v2 - v1) = lambda
	// v2 + (beta/h) * x1 + gamma * lambda = 0, gamma has units of inverse mass.
	// x2 = x1 + h * v2

	// 1-D mass-damper-spring system
	// m (v2 - v1) + h * d * v2 + h * k * 

	// C = norm(p2 - p1) - L
	// u = (p2 - p1) / norm(p2 - p1)
	// Cdot = dot(u, v2 + cross(w2, r2) - v1 - cross(w1, r1))
	// J = [-u -cross(r1, u) u cross(r2, u)]
	// K = J * invM * JT
	//   = invMass1 + invI1 * cross(r1, u)^2 + invMass2 + invI2 * cross(r2, u)^2

	/// <summary>
	/// A distance joint rains two points on two bodies
	/// to remain at a fixed distance from each other. You can view
	/// this as a massless, rigid rod.
	/// </summary>
	public class DistanceJoint : Joint
	{
		#region Properties/Fields

		/// <summary>
		/// The local anchor point relative to bodyA's origin.
		/// </summary>
		public Vector2 LocalAnchorA;

		/// <summary>
		/// The local anchor point relative to bodyB's origin.
		/// </summary>
		public Vector2 LocalAnchorB;

		public override sealed Vector2 WorldAnchorA
		{
			get => BodyA.GetWorldPoint(LocalAnchorA);
			set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
		}

		public override sealed Vector2 WorldAnchorB
		{
			get => BodyB.GetWorldPoint(LocalAnchorB);
			set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
		}

		/// <summary>
		/// The natural length between the anchor points.
		/// Manipulating the length can lead to non-physical behavior when the frequency is zero.
		/// </summary>
		public float Length;

		/// <summary>
		/// The mass-spring-damper frequency in Hertz. A value of 0
		/// disables softness.
		/// </summary>
		public float Frequency;

		/// <summary>
		/// The damping ratio. 0 = no damping, 1 = critical damping.
		/// </summary>
		public float DampingRatio;

		// Solver shared
		float _bias;
		float _gamma;
		float _impulse;

		// Solver temp
		int _indexA;
		int _indexB;
		Vector2 _u;
		Vector2 _rA;
		Vector2 _rB;
		Vector2 _localCenterA;
		Vector2 _localCenterB;
		float _invMassA;
		float _invMassB;
		float _invIA;
		float _invIB;
		float _mass;

		#endregion


		internal DistanceJoint()
		{
			JointType = JointType.Distance;
		}

		/// <summary>
		/// This requires defining an
		/// anchor point on both bodies and the non-zero length of the
		/// distance joint. If you don't supply a length, the local anchor points
		/// is used so that the initial configuration can violate the constraint
		/// slightly. This helps when saving and loading a game.
		/// Warning Do not use a zero or short length.
		/// </summary>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		/// <param name="anchorA">The first body anchor</param>
		/// <param name="anchorB">The second body anchor</param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public DistanceJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false)
			: base(bodyA, bodyB)
		{
			JointType = JointType.Distance;

			if (useWorldCoordinates)
			{
				LocalAnchorA = bodyA.GetLocalPoint(ref anchorA);
				LocalAnchorB = bodyB.GetLocalPoint(ref anchorB);
				Length = (anchorB - anchorA).Length();
			}
			else
			{
				LocalAnchorA = anchorA;
				LocalAnchorB = anchorB;
				Length = (base.BodyB.GetWorldPoint(ref anchorB) - base.BodyA.GetWorldPoint(ref anchorA)).Length();
			}
		}

		/// <summary>
		/// Get the reaction force given the inverse time step. Unit is N.
		/// </summary>
		/// <param name="invDt"></param>
		/// <returns></returns>
		public override Vector2 GetReactionForce(float invDt)
		{
			Vector2 F = (invDt * _impulse) * _u;
			return F;
		}

		/// <summary>
		/// Get the reaction torque given the inverse time step.
		/// Unit is N*m. This is always zero for a distance joint.
		/// </summary>
		/// <param name="invDt"></param>
		/// <returns></returns>
		public override float GetReactionTorque(float invDt)
		{
			return 0.0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			_indexA = BodyA.IslandIndex;
			_indexB = BodyB.IslandIndex;
			_localCenterA = BodyA._sweep.LocalCenter;
			_localCenterB = BodyB._sweep.LocalCenter;
			_invMassA = BodyA._invMass;
			_invMassB = BodyB._invMass;
			_invIA = BodyA._invI;
			_invIB = BodyB._invI;

			Vector2 cA = data.Positions[_indexA].C;
			float aA = data.Positions[_indexA].A;
			Vector2 vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;

			Vector2 cB = data.Positions[_indexB].C;
			float aB = data.Positions[_indexB].A;
			Vector2 vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;

			Rot qA = new Rot(aA), qB = new Rot(aB);

			_rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
			_rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
			_u = cB + _rB - cA - _rA;

			// Handle singularity.
			float length = _u.Length();
			if (length > Settings.LinearSlop)
			{
				_u *= 1.0f / length;
			}
			else
			{
				_u = Vector2.Zero;
			}

			float crAu = MathUtils.Cross(_rA, _u);
			float crBu = MathUtils.Cross(_rB, _u);
			float invMass = _invMassA + _invIA * crAu * crAu + _invMassB + _invIB * crBu * crBu;

			// Compute the effective mass matrix.
			_mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;

			if (Frequency > 0.0f)
			{
				float C = length - this.Length;

				// Frequency
				float omega = 2.0f * Settings.Pi * Frequency;

				// Damping coefficient
				float d = 2.0f * _mass * DampingRatio * omega;

				// Spring stiffness
				float k = _mass * omega * omega;

				// magic formulas
				float h = data.Step.Dt;
				_gamma = h * (d + h * k);
				_gamma = _gamma != 0.0f ? 1.0f / _gamma : 0.0f;
				_bias = C * h * k * _gamma;

				invMass += _gamma;
				_mass = invMass != 0.0f ? 1.0f / invMass : 0.0f;
			}
			else
			{
				_gamma = 0.0f;
				_bias = 0.0f;
			}

			if (Settings.EnableWarmstarting)
			{
				// Scale the impulse to support a variable time step.
				_impulse *= data.Step.DtRatio;

				Vector2 P = _impulse * _u;
				vA -= _invMassA * P;
				wA -= _invIA * MathUtils.Cross(_rA, P);
				vB += _invMassB * P;
				wB += _invIB * MathUtils.Cross(_rB, P);
			}
			else
			{
				_impulse = 0.0f;
			}

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			Vector2 vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;
			Vector2 vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;

			// Cdot = dot(u, v + cross(w, r))
			Vector2 vpA = vA + MathUtils.Cross(wA, _rA);
			Vector2 vpB = vB + MathUtils.Cross(wB, _rB);
			float Cdot = Vector2.Dot(_u, vpB - vpA);

			float impulse = -_mass * (Cdot + _bias + _gamma * _impulse);
			_impulse += impulse;

			Vector2 P = impulse * _u;
			vA -= _invMassA * P;
			wA -= _invIA * MathUtils.Cross(_rA, P);
			vB += _invMassB * P;
			wB += _invIB * MathUtils.Cross(_rB, P);

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			if (Frequency > 0.0f)
			{
				// There is no position correction for soft distance constraints.
				return true;
			}

			var cA = data.Positions[_indexA].C;
			var aA = data.Positions[_indexA].A;
			var cB = data.Positions[_indexB].C;
			var aB = data.Positions[_indexB].A;

			Rot qA = new Rot(aA), qB = new Rot(aB);

			var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
			var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);
			var u = cB + rB - cA - rA;

			var length = u.Length();
			Nez.Vector2Ext.Normalize(ref u);
			var C = length - this.Length;
			C = MathUtils.Clamp(C, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);

			var impulse = -_mass * C;
			var P = impulse * u;

			cA -= _invMassA * P;
			aA -= _invIA * MathUtils.Cross(rA, P);
			cB += _invMassB * P;
			aB += _invIB * MathUtils.Cross(rB, P);

			data.Positions[_indexA].C = cA;
			data.Positions[_indexA].A = aA;
			data.Positions[_indexB].C = cB;
			data.Positions[_indexB].A = aB;

			return Math.Abs(C) < Settings.LinearSlop;
		}
	}
}