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
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Joints
{
	// Point-to-point constraint
	// C = p2 - p1
	// Cdot = v2 - v1
	//      = v2 + cross(w2, r2) - v1 - cross(w1, r1)
	// J = [-I -r1_skew I r2_skew ]
	// Identity used:
	// w k % (rx i + ry j) = w * (-ry i + rx j)

	// Angle constraint
	// C = angle2 - angle1 - referenceAngle
	// Cdot = w2 - w1
	// J = [0 0 -1 0 0 1]
	// K = invI1 + invI2

	/// <summary>
	/// A weld joint essentially glues two bodies together. A weld joint may distort somewhat because the island constraint solver is approximate.
	/// 
	/// The joint is soft constraint based, which means the two bodies will move relative to each other, when a force is applied. To combine two bodies
	/// in a rigid fashion, combine the fixtures to a single body instead.
	/// </summary>
	public class WeldJoint : Joint
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
		/// The bodyB angle minus bodyA angle in the reference state (radians).
		/// </summary>
		public float ReferenceAngle;

		/// <summary>
		/// The frequency of the joint. A higher frequency means a stiffer joint, but
		/// a too high value can cause the joint to oscillate.
		/// Default is 0, which means the joint does no spring calculations.
		/// </summary>
		public float FrequencyHz;

		/// <summary>
		/// The damping on the joint. The damping is only used when
		/// the joint has a frequency (> 0). A higher value means more damping.
		/// </summary>
		public float DampingRatio;

		// Solver shared
		Vector3 _impulse;
		float _gamma;
		float _bias;

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
		Mat33 _mass;

		#endregion


		internal WeldJoint()
		{
			JointType = JointType.Weld;
		}

		/// <summary>
		/// You need to specify an anchor point where they are attached.
		/// The position of the anchor point is important for computing the reaction torque.
		/// </summary>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		/// <param name="anchorA">The first body anchor.</param>
		/// <param name="anchorB">The second body anchor.</param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public WeldJoint(Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, bool useWorldCoordinates = false)
			: base(bodyA, bodyB)
		{
			JointType = JointType.Weld;

			if (useWorldCoordinates)
			{
				LocalAnchorA = bodyA.GetLocalPoint(anchorA);
				LocalAnchorB = bodyB.GetLocalPoint(anchorB);
			}
			else
			{
				LocalAnchorA = anchorA;
				LocalAnchorB = anchorB;
			}

			ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
		}

		public override Vector2 GetReactionForce(float invDt)
		{
			return invDt * new Vector2(_impulse.X, _impulse.Y);
		}

		public override float GetReactionTorque(float invDt)
		{
			return invDt * _impulse.Z;
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

			Mat33 K = new Mat33();
			K.Ex.X = mA + mB + _rA.Y * _rA.Y * iA + _rB.Y * _rB.Y * iB;
			K.Ey.X = -_rA.Y * _rA.X * iA - _rB.Y * _rB.X * iB;
			K.Ez.X = -_rA.Y * iA - _rB.Y * iB;
			K.Ex.Y = K.Ey.X;
			K.Ey.Y = mA + mB + _rA.X * _rA.X * iA + _rB.X * _rB.X * iB;
			K.Ez.Y = _rA.X * iA + _rB.X * iB;
			K.Ex.Z = K.Ez.X;
			K.Ey.Z = K.Ez.Y;
			K.Ez.Z = iA + iB;

			if (FrequencyHz > 0.0f)
			{
				K.GetInverse22(ref _mass);

				float invM = iA + iB;
				float m = invM > 0.0f ? 1.0f / invM : 0.0f;

				float C = aB - aA - ReferenceAngle;

				// Frequency
				float omega = 2.0f * Settings.Pi * FrequencyHz;

				// Damping coefficient
				float d = 2.0f * m * DampingRatio * omega;

				// Spring stiffness
				float k = m * omega * omega;

				// magic formulas
				float h = data.Step.Dt;
				_gamma = h * (d + h * k);
				_gamma = _gamma != 0.0f ? 1.0f / _gamma : 0.0f;
				_bias = C * h * k * _gamma;

				invM += _gamma;
				_mass.Ez.Z = invM != 0.0f ? 1.0f / invM : 0.0f;
			}
			else
			{
				K.GetSymInverse33(ref _mass);
				_gamma = 0.0f;
				_bias = 0.0f;
			}

			if (Settings.EnableWarmstarting)
			{
				// Scale impulses to support a variable time step.
				_impulse *= data.Step.DtRatio;

				Vector2 P = new Vector2(_impulse.X, _impulse.Y);

				vA -= mA * P;
				wA -= iA * (MathUtils.Cross(_rA, P) + _impulse.Z);

				vB += mB * P;
				wB += iB * (MathUtils.Cross(_rB, P) + _impulse.Z);
			}
			else
			{
				_impulse = Vector3.Zero;
			}

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			var vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;
			var vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;

			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			if (FrequencyHz > 0.0f)
			{
				float Cdot2 = wB - wA;

				float impulse2 = -_mass.Ez.Z * (Cdot2 + _bias + _gamma * _impulse.Z);
				_impulse.Z += impulse2;

				wA -= iA * impulse2;
				wB += iB * impulse2;

				var Cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);

				var impulse1 = -MathUtils.Mul22(_mass, Cdot1);
				_impulse.X += impulse1.X;
				_impulse.Y += impulse1.Y;

				var P = impulse1;

				vA -= mA * P;
				wA -= iA * MathUtils.Cross(_rA, P);

				vB += mB * P;
				wB += iB * MathUtils.Cross(_rB, P);
			}
			else
			{
				var Cdot1 = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA);
				float Cdot2 = wB - wA;
				var Cdot = new Vector3(Cdot1.X, Cdot1.Y, Cdot2);

				var impulse = -MathUtils.Mul(_mass, Cdot);
				_impulse += impulse;

				var P = new Vector2(impulse.X, impulse.Y);

				vA -= mA * P;
				wA -= iA * (MathUtils.Cross(_rA, P) + impulse.Z);

				vB += mB * P;
				wB += iB * (MathUtils.Cross(_rB, P) + impulse.Z);
			}

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
			data.Velocities[_indexB].V = vB;
			data.Velocities[_indexB].W = wB;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			var cA = data.Positions[_indexA].C;
			float aA = data.Positions[_indexA].A;
			var cB = data.Positions[_indexB].C;
			float aB = data.Positions[_indexB].A;

			Rot qA = new Rot(aA), qB = new Rot(aB);

			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			var rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);
			var rB = MathUtils.Mul(qB, LocalAnchorB - _localCenterB);

			float positionError, angularError;

			var K = new Mat33();
			K.Ex.X = mA + mB + rA.Y * rA.Y * iA + rB.Y * rB.Y * iB;
			K.Ey.X = -rA.Y * rA.X * iA - rB.Y * rB.X * iB;
			K.Ez.X = -rA.Y * iA - rB.Y * iB;
			K.Ex.Y = K.Ey.X;
			K.Ey.Y = mA + mB + rA.X * rA.X * iA + rB.X * rB.X * iB;
			K.Ez.Y = rA.X * iA + rB.X * iB;
			K.Ex.Z = K.Ez.X;
			K.Ey.Z = K.Ez.Y;
			K.Ez.Z = iA + iB;

			if (FrequencyHz > 0.0f)
			{
				Vector2 C1 = cB + rB - cA - rA;

				positionError = C1.Length();
				angularError = 0.0f;

				Vector2 P = -K.Solve22(C1);

				cA -= mA * P;
				aA -= iA * MathUtils.Cross(rA, P);

				cB += mB * P;
				aB += iB * MathUtils.Cross(rB, P);
			}
			else
			{
				Vector2 C1 = cB + rB - cA - rA;
				float C2 = aB - aA - ReferenceAngle;

				positionError = C1.Length();
				angularError = Math.Abs(C2);

				Vector3 C = new Vector3(C1.X, C1.Y, C2);

				Vector3 impulse = -K.Solve33(C);
				Vector2 P = new Vector2(impulse.X, impulse.Y);

				cA -= mA * P;
				aA -= iA * (MathUtils.Cross(rA, P) + impulse.Z);

				cB += mB * P;
				aB += iB * (MathUtils.Cross(rB, P) + impulse.Z);
			}

			data.Positions[_indexA].C = cA;
			data.Positions[_indexA].A = aA;
			data.Positions[_indexB].C = cB;
			data.Positions[_indexB].A = aB;

			return positionError <= Settings.LinearSlop && angularError <= Settings.AngularSlop;
		}
	}
}