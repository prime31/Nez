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
	/// <summary>
	/// A motor joint is used to control the relative motion
	/// between two bodies. A typical usage is to control the movement
	/// of a dynamic body with respect to the ground.
	/// </summary>
	public class MotorJoint : Joint
	{
		#region Properties/Fields

		public override Vector2 WorldAnchorA
		{
			get => BodyA.Position;
			set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
		}

		public override Vector2 WorldAnchorB
		{
			get => BodyB.Position;
			set => Debug.Assert(false, "You can't set the world anchor on this joint type.");
		}

		/// <summary>
		/// The maximum amount of force that can be applied to BodyA
		/// </summary>
		public float MaxForce
		{
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
				_maxForce = value;
			}
			get => _maxForce;
		}

		/// <summary>
		/// The maximum amount of torque that can be applied to BodyA
		/// </summary>
		public float MaxTorque
		{
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
				_maxTorque = value;
			}
			get => _maxTorque;
		}

		/// <summary>
		/// The linear (translation) offset.
		/// </summary>
		public Vector2 LinearOffset
		{
			set
			{
				if (_linearOffset.X != value.X || _linearOffset.Y != value.Y)
				{
					WakeBodies();
					_linearOffset = value;
				}
			}
			get => _linearOffset;
		}

		/// <summary>
		/// Get or set the angular offset.
		/// </summary>
		public float AngularOffset
		{
			set
			{
				if (_angularOffset != value)
				{
					WakeBodies();
					_angularOffset = value;
				}
			}
			get => _angularOffset;
		}

		// FPE note: Used for serialization.
		internal float correctionFactor;

		// Solver shared
		Vector2 _linearOffset;
		float _angularOffset;
		Vector2 _linearImpulse;
		float _angularImpulse;
		float _maxForce;
		float _maxTorque;

		// Solver temp
		int _indexA;
		int _indexB;
		Vector2 _rA;
		Vector2 _rB;
		Vector2 _localCenterA;
		Vector2 _localCenterB;
		Vector2 _linearError;
		float _angularError;
		float _invMassA;
		float _invMassB;
		float _invIA;
		float _invIB;
		Mat22 _linearMass;
		float _angularMass;

		#endregion


		internal MotorJoint()
		{
			JointType = JointType.Motor;
		}

		/// <summary>
		/// Constructor for MotorJoint.
		/// </summary>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public MotorJoint(Body bodyA, Body bodyB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			JointType = JointType.Motor;

			Vector2 xB = base.BodyB.Position;

			if (useWorldCoordinates)
				_linearOffset = base.BodyA.GetLocalPoint(xB);
			else
				_linearOffset = xB;

			//Defaults
			_angularOffset = 0.0f;
			_maxForce = 1.0f;
			_maxTorque = 1.0f;
			correctionFactor = 0.3f;

			_angularOffset = base.BodyB.Rotation - base.BodyA.Rotation;
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

			Vector2 cA = data.Positions[_indexA].C;
			float aA = data.Positions[_indexA].A;
			Vector2 vA = data.Velocities[_indexA].V;
			float wA = data.Velocities[_indexA].W;

			Vector2 cB = data.Positions[_indexB].C;
			float aB = data.Positions[_indexB].A;
			Vector2 vB = data.Velocities[_indexB].V;
			float wB = data.Velocities[_indexB].W;

			Rot qA = new Rot(aA);
			Rot qB = new Rot(aB);

			// Compute the effective mass matrix.
			_rA = MathUtils.Mul(qA, -_localCenterA);
			_rB = MathUtils.Mul(qB, -_localCenterB);

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

			_linearError = cB + _rB - cA - _rA - MathUtils.Mul(qA, _linearOffset);
			_angularError = aB - aA - _angularOffset;

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
			float inv_h = data.Step.Inv_dt;

			// Solve angular friction
			{
				float Cdot = wB - wA + inv_h * correctionFactor * _angularError;
				float impulse = -_angularMass * Cdot;

				float oldImpulse = _angularImpulse;
				float maxImpulse = h * _maxTorque;
				_angularImpulse = MathUtils.Clamp(_angularImpulse + impulse, -maxImpulse, maxImpulse);
				impulse = _angularImpulse - oldImpulse;

				wA -= iA * impulse;
				wB += iB * impulse;
			}

			// Solve linear friction
			{
				var Cdot = vB + MathUtils.Cross(wB, _rB) - vA - MathUtils.Cross(wA, _rA) +
				           inv_h * correctionFactor * _linearError;

				var impulse = -MathUtils.Mul(ref _linearMass, ref Cdot);
				var oldImpulse = _linearImpulse;
				_linearImpulse += impulse;

				var maxImpulse = h * _maxForce;
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