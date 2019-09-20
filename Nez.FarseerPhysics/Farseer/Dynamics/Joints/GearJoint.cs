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

using System.Diagnostics;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Joints
{
	// Gear Joint:
	// C0 = (coordinate1 + ratio * coordinate2)_initial
	// C = (coordinate1 + ratio * coordinate2) - C0 = 0
	// J = [J1 ratio * J2]
	// K = J * invM * JT
	//   = J1 * invM1 * J1T + ratio * ratio * J2 * invM2 * J2T
	//
	// Revolute:
	// coordinate = rotation
	// Cdot = angularVelocity
	// J = [0 0 1]
	// K = J * invM * JT = invI
	//
	// Prismatic:
	// coordinate = dot(p - pg, ug)
	// Cdot = dot(v + cross(w, r), ug)
	// J = [ug cross(r, ug)]
	// K = J * invM * JT = invMass + invI * cross(r, ug)^2

	/// <summary>
	/// A gear joint is used to connect two joints together.
	/// Either joint can be a revolute or prismatic joint.
	/// You specify a gear ratio to bind the motions together:
	/// <![CDATA[coordinate1 + ratio * coordinate2 = ant]]>
	/// The ratio can be negative or positive. If one joint is a revolute joint
	/// and the other joint is a prismatic joint, then the ratio will have units
	/// of length or units of 1/length.
	///
	/// Warning: You have to manually destroy the gear joint if jointA or jointB is destroyed.
	/// </summary>
	public class GearJoint : Joint
	{
		#region Properties/Fields

		public override Vector2 WorldAnchorA
		{
			get => _bodyA.GetWorldPoint(_localAnchorA);
			set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
		}

		public override Vector2 WorldAnchorB
		{
			get => _bodyB.GetWorldPoint(_localAnchorB);
			set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
		}

		/// <summary>
		/// The gear ratio.
		/// </summary>
		public float Ratio
		{
			get => _ratio;
			set
			{
				Debug.Assert(MathUtils.IsValid(value));
				_ratio = value;
			}
		}

		/// <summary>
		/// The first revolute/prismatic joint attached to the gear joint.
		/// </summary>
		public Joint JointA { get; private set; }

		/// <summary>
		/// The second revolute/prismatic joint attached to the gear joint.
		/// </summary>
		public Joint JointB { get; private set; }

		JointType _typeA;
		JointType _typeB;

		Body _bodyA;
		Body _bodyB;
		Body _bodyC;
		Body _bodyD;

		// Solver shared
		Vector2 _localAnchorA;
		Vector2 _localAnchorB;
		Vector2 _localAnchorC;
		Vector2 _localAnchorD;

		Vector2 _localAxisC;
		Vector2 _localAxisD;

		float _referenceAngleA;
		float _referenceAngleB;

		float _constant;
		float _ratio;

		float _impulse;

		// Solver temp
		int _indexA, _indexB, _indexC, _indexD;
		Vector2 _lcA, _lcB, _lcC, _lcD;
		float _mA, _mB, _mC, _mD;
		float _iA, _iB, _iC, _iD;
		Vector2 _JvAC, _JvBD;
		float _JwA, _JwB, _JwC, _JwD;
		float _mass;

		#endregion


		/// <summary>
		/// Requires two existing revolute or prismatic joints (any combination will work).
		/// The provided joints must attach a dynamic body to a static body.
		/// </summary>
		/// <param name="jointA">The first joint.</param>
		/// <param name="jointB">The second joint.</param>
		/// <param name="ratio">The ratio.</param>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		public GearJoint(Body bodyA, Body bodyB, Joint jointA, Joint jointB, float ratio = 1f)
		{
			JointType = JointType.Gear;
			base.BodyA = bodyA;
			base.BodyB = bodyB;
			this.JointA = jointA;
			this.JointB = jointB;
			this.Ratio = ratio;

			_typeA = jointA.JointType;
			_typeB = jointB.JointType;

			//Debug.Assert(_typeA == JointType.Revolute || _typeA == JointType.Prismatic );
			//Debug.Assert(_typeB == JointType.Revolute || _typeB == JointType.Prismatic );

			float coordinateA, coordinateB;

			// TODO_ERIN there might be some problem with the joint edges in b2Joint.

			_bodyC = jointA.BodyA;
			_bodyA = jointA.BodyB;

			// Get geometry of joint1
			Transform xfA = _bodyA._xf;
			float aA = _bodyA._sweep.A;
			Transform xfC = _bodyC._xf;
			float aC = _bodyC._sweep.A;

			if (_typeA == JointType.Revolute)
			{
				RevoluteJoint revolute = (RevoluteJoint) jointA;
				_localAnchorC = revolute.LocalAnchorA;
				_localAnchorA = revolute.LocalAnchorB;
				_referenceAngleA = revolute.ReferenceAngle;
				_localAxisC = Vector2.Zero;

				coordinateA = aA - aC - _referenceAngleA;
			}
			else
			{
				PrismaticJoint prismatic = (PrismaticJoint) jointA;
				_localAnchorC = prismatic.LocalAnchorA;
				_localAnchorA = prismatic.LocalAnchorB;
				_referenceAngleA = prismatic.ReferenceAngle;
				_localAxisC = prismatic.LocalXAxis;

				Vector2 pC = _localAnchorC;
				Vector2 pA = MathUtils.MulT(xfC.Q, MathUtils.Mul(xfA.Q, _localAnchorA) + (xfA.P - xfC.P));
				coordinateA = Vector2.Dot(pA - pC, _localAxisC);
			}

			_bodyD = jointB.BodyA;
			_bodyB = jointB.BodyB;

			// Get geometry of joint2
			Transform xfB = _bodyB._xf;
			float aB = _bodyB._sweep.A;
			Transform xfD = _bodyD._xf;
			float aD = _bodyD._sweep.A;

			if (_typeB == JointType.Revolute)
			{
				RevoluteJoint revolute = (RevoluteJoint) jointB;
				_localAnchorD = revolute.LocalAnchorA;
				_localAnchorB = revolute.LocalAnchorB;
				_referenceAngleB = revolute.ReferenceAngle;
				_localAxisD = Vector2.Zero;

				coordinateB = aB - aD - _referenceAngleB;
			}
			else
			{
				PrismaticJoint prismatic = (PrismaticJoint) jointB;
				_localAnchorD = prismatic.LocalAnchorA;
				_localAnchorB = prismatic.LocalAnchorB;
				_referenceAngleB = prismatic.ReferenceAngle;
				_localAxisD = prismatic.LocalXAxis;

				Vector2 pD = _localAnchorD;
				Vector2 pB = MathUtils.MulT(xfD.Q, MathUtils.Mul(xfB.Q, _localAnchorB) + (xfB.P - xfD.P));
				coordinateB = Vector2.Dot(pB - pD, _localAxisD);
			}

			_ratio = ratio;
			_constant = coordinateA + _ratio * coordinateB;
			_impulse = 0.0f;
		}

		public override Vector2 GetReactionForce(float invDt)
		{
			Vector2 P = _impulse * _JvAC;
			return invDt * P;
		}

		public override float GetReactionTorque(float invDt)
		{
			float L = _impulse * _JwA;
			return invDt * L;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			_indexA = _bodyA.IslandIndex;
			_indexB = _bodyB.IslandIndex;
			_indexC = _bodyC.IslandIndex;
			_indexD = _bodyD.IslandIndex;
			_lcA = _bodyA._sweep.LocalCenter;
			_lcB = _bodyB._sweep.LocalCenter;
			_lcC = _bodyC._sweep.LocalCenter;
			_lcD = _bodyD._sweep.LocalCenter;
			_mA = _bodyA._invMass;
			_mB = _bodyB._invMass;
			_mC = _bodyC._invMass;
			_mD = _bodyD._invMass;
			_iA = _bodyA._invI;
			_iB = _bodyB._invI;
			_iC = _bodyC._invI;
			_iD = _bodyD._invI;

			float aA = data.Positions[_indexA].A;
			Vector2 vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;

			float aB = data.Positions[_indexB].A;
			Vector2 vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;

			float aC = data.Positions[_indexC].A;
			Vector2 vC = data.Velocities[_indexC].V;
			float wC = data.Velocities[_indexC].W;

			float aD = data.Positions[_indexD].A;
			Vector2 vD = data.Velocities[_indexD].V;
			float wD = data.Velocities[_indexD].W;

			Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

			_mass = 0.0f;

			if (_typeA == JointType.Revolute)
			{
				_JvAC = Vector2.Zero;
				_JwA = 1.0f;
				_JwC = 1.0f;
				_mass += _iA + _iC;
			}
			else
			{
				Vector2 u = MathUtils.Mul(qC, _localAxisC);
				Vector2 rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
				Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
				_JvAC = u;
				_JwC = MathUtils.Cross(rC, u);
				_JwA = MathUtils.Cross(rA, u);
				_mass += _mC + _mA + _iC * _JwC * _JwC + _iA * _JwA * _JwA;
			}

			if (_typeB == JointType.Revolute)
			{
				_JvBD = Vector2.Zero;
				_JwB = _ratio;
				_JwD = _ratio;
				_mass += _ratio * _ratio * (_iB + _iD);
			}
			else
			{
				Vector2 u = MathUtils.Mul(qD, _localAxisD);
				Vector2 rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
				Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
				_JvBD = _ratio * u;
				_JwD = _ratio * MathUtils.Cross(rD, u);
				_JwB = _ratio * MathUtils.Cross(rB, u);
				_mass += _ratio * _ratio * (_mD + _mB) + _iD * _JwD * _JwD + _iB * _JwB * _JwB;
			}

			// Compute effective mass.
			_mass = _mass > 0.0f ? 1.0f / _mass : 0.0f;

			if (Settings.EnableWarmstarting)
			{
				vA += (_mA * _impulse) * _JvAC;
				wA += _iA * _impulse * _JwA;
				vB += (_mB * _impulse) * _JvBD;
				wB += _iB * _impulse * _JwB;
				vC -= (_mC * _impulse) * _JvAC;
				wC -= _iC * _impulse * _JwC;
				vD -= (_mD * _impulse) * _JvBD;
				wD -= _iD * _impulse * _JwD;
			}
			else
			{
				_impulse = 0.0f;
			}

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
			data.Velocities[_indexC].V = vC;
			data.Velocities[_indexC].W = wC;
			data.Velocities[_indexD].V = vD;
			data.Velocities[_indexD].W = wD;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			Vector2 vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;
			Vector2 vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;
			Vector2 vC = data.Velocities[_indexC].V;
			float wC = data.Velocities[_indexC].W;
			Vector2 vD = data.Velocities[_indexD].V;
			float wD = data.Velocities[_indexD].W;

			float Cdot = Vector2.Dot(_JvAC, vA - vC) + Vector2.Dot(_JvBD, vB - vD);
			Cdot += (_JwA * wA - _JwC * wC) + (_JwB * wB - _JwD * wD);

			float impulse = -_mass * Cdot;
			_impulse += impulse;

			vA += (_mA * impulse) * _JvAC;
			wA += _iA * impulse * _JwA;
			vB += (_mB * impulse) * _JvBD;
			wB += _iB * impulse * _JwB;
			vC -= (_mC * impulse) * _JvAC;
			wC -= _iC * impulse * _JwC;
			vD -= (_mD * impulse) * _JvBD;
			wD -= _iD * impulse * _JwD;

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
			data.Velocities[_indexC].V = vC;
			data.Velocities[_indexC].W = wC;
			data.Velocities[_indexD].V = vD;
			data.Velocities[_indexD].W = wD;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			Vector2 cA = data.Positions[_indexA].C;
			float aA = data.Positions[_indexA].A;
			Vector2 cB = data.Positions[_indexB].C;
			float aB = data.Positions[_indexB].A;
			Vector2 cC = data.Positions[_indexC].C;
			float aC = data.Positions[_indexC].A;
			Vector2 cD = data.Positions[_indexD].C;
			float aD = data.Positions[_indexD].A;

			Rot qA = new Rot(aA), qB = new Rot(aB), qC = new Rot(aC), qD = new Rot(aD);

			const float linearError = 0.0f;

			float coordinateA, coordinateB;

			Vector2 JvAC, JvBD;
			float JwA, JwB, JwC, JwD;
			float mass = 0.0f;

			if (_typeA == JointType.Revolute)
			{
				JvAC = Vector2.Zero;
				JwA = 1.0f;
				JwC = 1.0f;
				mass += _iA + _iC;

				coordinateA = aA - aC - _referenceAngleA;
			}
			else
			{
				Vector2 u = MathUtils.Mul(qC, _localAxisC);
				Vector2 rC = MathUtils.Mul(qC, _localAnchorC - _lcC);
				Vector2 rA = MathUtils.Mul(qA, _localAnchorA - _lcA);
				JvAC = u;
				JwC = MathUtils.Cross(rC, u);
				JwA = MathUtils.Cross(rA, u);
				mass += _mC + _mA + _iC * JwC * JwC + _iA * JwA * JwA;

				Vector2 pC = _localAnchorC - _lcC;
				Vector2 pA = MathUtils.MulT(qC, rA + (cA - cC));
				coordinateA = Vector2.Dot(pA - pC, _localAxisC);
			}

			if (_typeB == JointType.Revolute)
			{
				JvBD = Vector2.Zero;
				JwB = _ratio;
				JwD = _ratio;
				mass += _ratio * _ratio * (_iB + _iD);

				coordinateB = aB - aD - _referenceAngleB;
			}
			else
			{
				Vector2 u = MathUtils.Mul(qD, _localAxisD);
				Vector2 rD = MathUtils.Mul(qD, _localAnchorD - _lcD);
				Vector2 rB = MathUtils.Mul(qB, _localAnchorB - _lcB);
				JvBD = _ratio * u;
				JwD = _ratio * MathUtils.Cross(rD, u);
				JwB = _ratio * MathUtils.Cross(rB, u);
				mass += _ratio * _ratio * (_mD + _mB) + _iD * JwD * JwD + _iB * JwB * JwB;

				Vector2 pD = _localAnchorD - _lcD;
				Vector2 pB = MathUtils.MulT(qD, rB + (cB - cD));
				coordinateB = Vector2.Dot(pB - pD, _localAxisD);
			}

			float C = (coordinateA + _ratio * coordinateB) - _constant;

			float impulse = 0.0f;
			if (mass > 0.0f)
			{
				impulse = -C / mass;
			}

			cA += _mA * impulse * JvAC;
			aA += _iA * impulse * JwA;
			cB += _mB * impulse * JvBD;
			aB += _iB * impulse * JwB;
			cC -= _mC * impulse * JvAC;
			aC -= _iC * impulse * JwC;
			cD -= _mD * impulse * JvBD;
			aD -= _iD * impulse * JwD;

			data.Positions[_indexA].C = cA;
			data.Positions[_indexA].A = aA;
			data.Positions[_indexB].C = cB;
			data.Positions[_indexB].A = aB;
			data.Positions[_indexC].C = cC;
			data.Positions[_indexC].A = aC;
			data.Positions[_indexD].C = cD;
			data.Positions[_indexD].A = aD;

			// TODO_ERIN not implemented
			return linearError < Settings.LinearSlop;
		}
	}
}