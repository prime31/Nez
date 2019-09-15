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
	// p = attached point, m = mouse point
	// C = p - m
	// Cdot = v
	//      = v + cross(w, r)
	// J = [I r_skew]
	// Identity used:
	// w k % (rx i + ry j) = w * (-ry i + rx j)

	/// <summary>
	/// A mouse joint is used to make a point on a body track a
	/// specified world point. This is a soft constraint with a maximum
	/// force. This allows the constraint to stretch without
	/// applying huge forces.
	/// NOTE: this joint is not documented in the manual because it was
	/// developed to be used in the testbed. If you want to learn how to
	/// use the mouse joint, look at the testbed.
	/// </summary>
	public class FixedMouseJoint : Joint
	{
		#region Properties/Fields

		/// <summary>
		/// The local anchor point on BodyA
		/// </summary>
		public Vector2 LocalAnchorA;

		public override Vector2 WorldAnchorA
		{
			get => BodyA.GetWorldPoint(LocalAnchorA);
			set => LocalAnchorA = BodyA.GetLocalPoint(value);
		}

		public override Vector2 WorldAnchorB
		{
			get => _worldAnchor;
			set
			{
				WakeBodies();
				_worldAnchor = value;
			}
		}

		/// <summary>
		/// The maximum constraint force that can be exerted to move the candidate body. Usually you will express
		/// as some multiple of the weight (multiplier * mass * gravity).
		/// </summary>
		public float MaxForce
		{
			get => _maxForce;
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
				_maxForce = value;
			}
		}

		/// <summary>
		/// The response speed.
		/// </summary>
		public float Frequency
		{
			get => _frequency;
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
				_frequency = value;
			}
		}

		/// <summary>
		/// The damping ratio. 0 = no damping, 1 = critical damping.
		/// </summary>
		public float DampingRatio
		{
			get => _dampingRatio;
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0.0f);
				_dampingRatio = value;
			}
		}

		Vector2 _worldAnchor;
		float _frequency;
		float _dampingRatio;
		float _beta;

		// Solver shared
		Vector2 _impulse;
		float _maxForce;
		float _gamma;

		// Solver temp
		int _indexA;
		Vector2 _rA;
		Vector2 _localCenterA;
		float _invMassA;
		float _invIA;
		Mat22 _mass;
		Vector2 _C;

		#endregion


		/// <summary>
		/// This requires a world target point,
		/// tuning parameters, and the time step.
		/// </summary>
		/// <param name="body">The body.</param>
		/// <param name="worldAnchor">The target.</param>
		public FixedMouseJoint(Body body, Vector2 worldAnchor) : base(body)
		{
			JointType = JointType.FixedMouse;
			Frequency = 5.0f;
			DampingRatio = 0.7f;
			MaxForce = 1000 * body.Mass;

			Debug.Assert(worldAnchor.IsValid());

			_worldAnchor = worldAnchor;
			LocalAnchorA = MathUtils.MulT(BodyA._xf, worldAnchor);
		}

		public override Vector2 GetReactionForce(float invDt)
		{
			return invDt * _impulse;
		}

		public override float GetReactionTorque(float invDt)
		{
			return invDt * 0.0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			_indexA = BodyA.IslandIndex;
			_localCenterA = BodyA._sweep.LocalCenter;
			_invMassA = BodyA._invMass;
			_invIA = BodyA._invI;

			var cA = data.Positions[_indexA].C;
			var aA = data.Positions[_indexA].A;
			var vA = data.Velocities[_indexA].V;
			var wA = data.Velocities[_indexA].W;

			var qA = new Rot(aA);

			float mass = BodyA.Mass;

			// Frequency
			float omega = 2.0f * Settings.Pi * Frequency;

			// Damping coefficient
			float d = 2.0f * mass * DampingRatio * omega;

			// Spring stiffness
			float k = mass * (omega * omega);

			// magic formulas
			// gamma has units of inverse mass.
			// beta has units of inverse time.
			float h = data.Step.Dt;

			Debug.Assert(d + h * k > Settings.Epsilon, "damping is less than Epsilon. Does the body have mass?");

			_gamma = h * (d + h * k);
			if (_gamma != 0.0f)
				_gamma = 1.0f / _gamma;

			_beta = h * k * _gamma;

			// Compute the effective mass matrix.
			_rA = MathUtils.Mul(qA, LocalAnchorA - _localCenterA);

			// K    = [(1/m1 + 1/m2) * eye(2) - skew(r1) * invI1 * skew(r1) - skew(r2) * invI2 * skew(r2)]
			//      = [1/m1+1/m2     0    ] + invI1 * [r1.Y*r1.Y -r1.X*r1.Y] + invI2 * [r1.Y*r1.Y -r1.X*r1.Y]
			//        [    0     1/m1+1/m2]           [-r1.X*r1.Y r1.X*r1.X]           [-r1.X*r1.Y r1.X*r1.X]
			var K = new Mat22();
			K.Ex.X = _invMassA + _invIA * _rA.Y * _rA.Y + _gamma;
			K.Ex.Y = -_invIA * _rA.X * _rA.Y;
			K.Ey.X = K.Ex.Y;
			K.Ey.Y = _invMassA + _invIA * _rA.X * _rA.X + _gamma;

			_mass = K.Inverse;

			_C = cA + _rA - _worldAnchor;
			_C *= _beta;

			// Cheat with some damping
			wA *= 0.98f;

			if (Settings.EnableWarmstarting)
			{
				_impulse *= data.Step.DtRatio;
				vA += _invMassA * _impulse;
				wA += _invIA * MathUtils.Cross(_rA, _impulse);
			}
			else
			{
				_impulse = Vector2.Zero;
			}

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			var vA = data.Velocities[_indexA].V;
			var wA = data.Velocities[_indexA].W;

			// Cdot = v + cross(w, r)
			var Cdot = vA + MathUtils.Cross(wA, _rA);
			var impulse = MathUtils.Mul(ref _mass, -(Cdot + _C + _gamma * _impulse));

			var oldImpulse = _impulse;
			_impulse += impulse;
			float maxImpulse = data.Step.Dt * MaxForce;
			if (_impulse.LengthSquared() > maxImpulse * maxImpulse)
			{
				_impulse *= maxImpulse / _impulse.Length();
			}

			impulse = _impulse - oldImpulse;

			vA += _invMassA * impulse;
			wA += _invIA * MathUtils.Cross(_rA, impulse);

			data.Velocities[_indexA].V = vA;
			data.Velocities[_indexA].W = wA;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}