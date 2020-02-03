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

using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Joints
{
	// Point-to-point constraint
	// Cdot = v2 - v1
	//      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
	// J = [-I -r1_skew I r2_skew ]
	// Identity used:
	// w k % (rx i + ry j) = w * (-ry i + rx j)

	// Angle constraint
	// Cdot = w2 - w1
	// J = [0 0 -1 0 0 1]
	// K = invI1 + invI2

	/// <summary>
	/// Friction joint. This is used for top-down friction.
	/// It provides 2D translational friction and angular friction.
	/// </summary>
	public class FrictionJoint : Joint
	{
		#region Properties/Fields

		/// <summary>
		/// The local anchor point on BodyA
		/// </summary>
		public Vector2 LocalAnchorA;

		/// <summary>
		/// The local anchor point on BodyB
		/// </summary>
		public Vector2 LocalAnchorB;

		public override Vector2 WorldAnchorA
		{
			get => BodyA.GetWorldPoint(LocalAnchorA);
			set => LocalAnchorA = BodyA.GetLocalPoint(value);
		}

		public override Vector2 WorldAnchorB
		{
			get => BodyB.GetWorldPoint(LocalAnchorB);
			set => LocalAnchorB = BodyB.GetLocalPoint(value);
		}

		/// <summary>
		/// The maximum friction force in N.
		/// </summary>
		public float MaxForce;

		/// <summary>
		/// The maximum friction torque in N-m.
		/// </summary>
		public float MaxTorque;

		// Solver shared
		Vector2 _linearImpulse;
		float _angularImpulse;

		// Solver temp
		int _indexA;
		int _indexB;
		Vector2 _rA;
		Vector2 _rB;
		Vector2 _localCenterA;
		Vector2 _localCenterB;
		float _invMassA;
		float _invMassB;
		float _invIA;
		float _invIB;
		float _angularMass;
		Mat22 _linearMass;

		#endregion


		internal FrictionJoint()
		{
			JointType = JointType.Friction;
		}

		/// <summary>
		/// Constructor for FrictionJoint.
		/// </summary>
		/// <param name="bodyA"></param>
		/// <param name="bodyB"></param>
		/// <param name="anchor"></param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public FrictionJoint(Body bodyA, Body bodyB, Vector2 anchor, bool useWorldCoordinates = false)
			: base(bodyA, bodyB)
		{
			JointType = JointType.Friction;

			if (useWorldCoordinates)
			{
				LocalAnchorA = base.BodyA.GetLocalPoint(anchor);
				LocalAnchorB = base.BodyB.GetLocalPoint(anchor);
			}
			else
			{
				LocalAnchorA = anchor;
				LocalAnchorB = anchor;
			}
		}

		public override Vector2 GetReactionForce(float invDt)
		{
			return invDt * _linearImpulse;
		}

		public override float GetReactionTorque(float invDt)
		{
			return invDt * _angularImpulse;
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

			float aA = data.Positions[_indexA].A;
			Vector2 vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;

			float aB = data.Positions[_indexB].A;
			Vector2 vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;

			Rot qA = new Rot(aA), qB = new Rot(aB);

			// Compute the effective mass matrix.
			_rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
			_rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

			// J = [-I -r1_skew I r2_skew]
			//     [ 0       -1 0       1]
			// r_skew = [-ry; rx]

			// Matlab
			// K = [ mA+r1y^2*iA+mB+r2y^2*iB,  -r1y*iA*r1x-r2y*iB*r2x,          -r1y*iA-r2y*iB]
			//     [  -r1y*iA*r1x-r2y*iB*r2x, mA+r1x^2*iA+mB+r2x^2*iB,           r1x*iA+r2x*iB]
			//     [          -r1y*iA-r2y*iB,           r1x*iA+r2x*iB,                   iA+iB]

			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			Mat22 K = new Mat22();
			K.Ex.X = mA + mB + iA * _rA.Y * _rA.Y + iB * _rB.Y * _rB.Y;
			K.Ex.Y = -iA * _rA.X * _rA.Y - iB * _rB.X * _rB.Y;
			K.Ey.X = K.Ex.Y;
			K.Ey.Y = mA + mB + iA * _rA.X * _rA.X + iB * _rB.X * _rB.X;

			_linearMass = K.Inverse;

			_angularMass = iA + iB;
			if (_angularMass > 0.0f)
			{
				_angularMass = 1.0f / _angularMass;
			}

			if (Settings.EnableWarmstarting)
			{
				// Scale impulses to support a variable time step.
				_linearImpulse *= data.Step.DtRatio;
				_angularImpulse *= data.Step.DtRatio;

				Vector2 P = new Vector2(_linearImpulse.X, _linearImpulse.Y);
				vA -= mA * P;
				wA -= iA * (MathUtils.Cross(_rA, P) + _angularImpulse);
				vB += mB * P;
				wB += iB * (MathUtils.Cross(_rB, P) + _angularImpulse);
			}
			else
			{
				_linearImpulse = Vector2.Zero;
				_angularImpulse = 0.0f;
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

			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			float h = data.Step.Dt;

			// Solve angular friction
			{
				float Cdot = wB - wA;
				float impulse = -_angularMass * Cdot;

				float oldImpulse = _angularImpulse;
				float maxImpulse = h * MaxTorque;
				_angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
				impulse = _angularImpulse - oldImpulse;

				wA -= iA * impulse;
				wB += iB * impulse;
			}

			// Solve linear friction
			{
				var Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);

				var impulse = -MathUtils.Mul(ref _linearMass, Cdot);
				var oldImpulse = _linearImpulse;
				_linearImpulse += impulse;

				var maxImpulse = h * MaxForce;
				if (_linearImpulse.LengthSquared() > maxImpulse * maxImpulse)
				{
					Nez.Vector2Ext.Normalize(ref _linearImpulse);
					_linearImpulse *= maxImpulse;
				}

				impulse = _linearImpulse - oldImpulse;

				vA -= mA * impulse;
				wA -= iA * MathUtils.Cross(_rA, impulse);

				vB += mB * impulse;
				wB += iB * MathUtils.Cross(_rB, impulse);
			}

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}