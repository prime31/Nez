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
	// Pulley:
	// length1 = norm(p1 - s1)
	// length2 = norm(p2 - s2)
	// C0 = (length1 + ratio * length2)_initial
	// C = C0 - (length1 + ratio * length2)
	// u1 = (p1 - s1) / norm(p1 - s1)
	// u2 = (p2 - s2) / norm(p2 - s2)
	// Cdot = -dot(u1, v1 + cross(w1, r1)) - ratio * dot(u2, v2 + cross(w2, r2))
	// J = -[u1 cross(r1, u1) ratio * u2  ratio * cross(r2, u2)]
	// K = J * invM * JT
	//   = invMass1 + invI1 * cross(r1, u1)^2 + ratio^2 * (invMass2 + invI2 * cross(r2, u2)^2)

	/// <summary>
	/// The pulley joint is connected to two bodies and two fixed world points.
	/// The pulley supports a ratio such that:
	/// <![CDATA[length1 + ratio * length2 <= constant]]>
	/// Yes, the force transmitted is scaled by the ratio.
	/// 
	/// Warning: the pulley joint can get a bit squirrelly by itself. They often
	/// work better when combined with prismatic joints. You should also cover the
	/// the anchor points with static shapes to prevent one side from going to zero length.
	/// </summary>
	public class PulleyJoint : Joint
	{
		#region Properites/Fields

		/// <summary>
		/// The local anchor point on BodyA
		/// </summary>
		public Vector2 localAnchorA;

		/// <summary>
		/// The local anchor point on BodyB
		/// </summary>
		public Vector2 localAnchorB;

		/// <summary>
		/// Get the first world anchor.
		/// </summary>
		/// <value></value>
		public override sealed Vector2 worldAnchorA { get; set; }

		/// <summary>
		/// Get the second world anchor.
		/// </summary>
		/// <value></value>
		public override sealed Vector2 worldAnchorB { get; set; }

		/// <summary>
		/// Get the current length of the segment attached to body1.
		/// </summary>
		/// <value></value>
		public float lengthA;

		/// <summary>
		/// Get the current length of the segment attached to body2.
		/// </summary>
		/// <value></value>
		public float lengthB;

		/// <summary>
		/// The current length between the anchor point on BodyA and WorldAnchorA
		/// </summary>
		public float currentLengthA
		{
			get
			{
				var p = bodyA.getWorldPoint( localAnchorA );
				var s = worldAnchorA;
				var d = p - s;
				return d.Length();
			}
		}

		/// <summary>
		/// The current length between the anchor point on BodyB and WorldAnchorB
		/// </summary>
		public float currentLengthB
		{
			get
			{
				var p = bodyB.getWorldPoint( localAnchorB );
				var s = worldAnchorB;
				var d = p - s;
				return d.Length();
			}
		}

		/// <summary>
		/// Get the pulley ratio.
		/// </summary>
		/// <value></value>
		public float ratio;

		// FPE note: Only used for serialization.
		internal float constant;

		// Solver shared
		float _impulse;

		// Solver temp
		int _indexA;
		int _indexB;
		Vector2 _uA;
		Vector2 _uB;
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


		internal PulleyJoint()
		{
			jointType = JointType.Pulley;
		}

		/// <summary>
		/// Constructor for PulleyJoint.
		/// </summary>
		/// <param name="bodyA">The first body.</param>
		/// <param name="bodyB">The second body.</param>
		/// <param name="anchorA">The anchor on the first body.</param>
		/// <param name="anchorB">The anchor on the second body.</param>
		/// <param name="worldAnchorA">The world anchor for the first body.</param>
		/// <param name="worldAnchorB">The world anchor for the second body.</param>
		/// <param name="ratio">The ratio.</param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public PulleyJoint( Body bodyA, Body bodyB, Vector2 anchorA, Vector2 anchorB, Vector2 worldAnchorA, Vector2 worldAnchorB, float ratio, bool useWorldCoordinates = false )
			: base( bodyA, bodyB )
		{
			jointType = JointType.Pulley;

			this.worldAnchorA = worldAnchorA;
			this.worldAnchorB = worldAnchorB;

			if( useWorldCoordinates )
			{
				localAnchorA = base.bodyA.getLocalPoint( anchorA );
				localAnchorB = base.bodyB.getLocalPoint( anchorB );

				var dA = anchorA - worldAnchorA;
				lengthA = dA.Length();
				var dB = anchorB - worldAnchorB;
				lengthB = dB.Length();
			}
			else
			{
				localAnchorA = anchorA;
				localAnchorB = anchorB;

				Vector2 dA = anchorA - base.bodyA.getLocalPoint( worldAnchorA );
				lengthA = dA.Length();
				Vector2 dB = anchorB - base.bodyB.getLocalPoint( worldAnchorB );
				lengthB = dB.Length();
			}

			Debug.Assert( ratio != 0.0f );
			Debug.Assert( ratio > Settings.epsilon );

			this.ratio = ratio;
			constant = lengthA + ratio * lengthB;
			_impulse = 0.0f;
		}

		public override Vector2 getReactionForce( float invDt )
		{
			Vector2 P = _impulse * _uB;
			return invDt * P;
		}

		public override float getReactionTorque( float invDt )
		{
			return 0.0f;
		}

		internal override void initVelocityConstraints( ref SolverData data )
		{
			_indexA = bodyA.islandIndex;
			_indexB = bodyB.islandIndex;
			_localCenterA = bodyA._sweep.localCenter;
			_localCenterB = bodyB._sweep.localCenter;
			_invMassA = bodyA._invMass;
			_invMassB = bodyB._invMass;
			_invIA = bodyA._invI;
			_invIB = bodyB._invI;

			Vector2 cA = data.positions[_indexA].c;
			float aA = data.positions[_indexA].a;
			Vector2 vA = data.velocities[_indexA].v;
			float wA = data.velocities[_indexA].w;

			Vector2 cB = data.positions[_indexB].c;
			float aB = data.positions[_indexB].a;
			Vector2 vB = data.velocities[_indexB].v;
			float wB = data.velocities[_indexB].w;

			Rot qA = new Rot( aA ), qB = new Rot( aB );

			_rA = MathUtils.mul( qA, localAnchorA - _localCenterA );
			_rB = MathUtils.mul( qB, localAnchorB - _localCenterB );

			// Get the pulley axes.
			_uA = cA + _rA - worldAnchorA;
			_uB = cB + _rB - worldAnchorB;

			float lengthA = _uA.Length();
			float lengthB = _uB.Length();

			if( lengthA > 10.0f * Settings.linearSlop )
			{
				_uA *= 1.0f / lengthA;
			}
			else
			{
				_uA = Vector2.Zero;
			}

			if( lengthB > 10.0f * Settings.linearSlop )
			{
				_uB *= 1.0f / lengthB;
			}
			else
			{
				_uB = Vector2.Zero;
			}

			// Compute effective mass.
			float ruA = MathUtils.cross( _rA, _uA );
			float ruB = MathUtils.cross( _rB, _uB );

			float mA = _invMassA + _invIA * ruA * ruA;
			float mB = _invMassB + _invIB * ruB * ruB;

			_mass = mA + ratio * ratio * mB;

			if( _mass > 0.0f )
			{
				_mass = 1.0f / _mass;
			}

			if( Settings.enableWarmstarting )
			{
				// Scale impulses to support variable time steps.
				_impulse *= data.step.dtRatio;

				// Warm starting.
				Vector2 PA = -( _impulse ) * _uA;
				Vector2 PB = ( -ratio * _impulse ) * _uB;

				vA += _invMassA * PA;
				wA += _invIA * MathUtils.cross( _rA, PA );
				vB += _invMassB * PB;
				wB += _invIB * MathUtils.cross( _rB, PB );
			}
			else
			{
				_impulse = 0.0f;
			}

			data.velocities[_indexA].v = vA;
			data.velocities[_indexA].w = wA;
			data.velocities[_indexB].v = vB;
			data.velocities[_indexB].w = wB;
		}

		internal override void solveVelocityConstraints( ref SolverData data )
		{
			Vector2 vA = data.velocities[_indexA].v;
			float wA = data.velocities[_indexA].w;
			Vector2 vB = data.velocities[_indexB].v;
			float wB = data.velocities[_indexB].w;

			Vector2 vpA = vA + MathUtils.cross( wA, _rA );
			Vector2 vpB = vB + MathUtils.cross( wB, _rB );

			float Cdot = -Vector2.Dot( _uA, vpA ) - ratio * Vector2.Dot( _uB, vpB );
			float impulse = -_mass * Cdot;
			_impulse += impulse;

			Vector2 PA = -impulse * _uA;
			Vector2 PB = -ratio * impulse * _uB;
			vA += _invMassA * PA;
			wA += _invIA * MathUtils.cross( _rA, PA );
			vB += _invMassB * PB;
			wB += _invIB * MathUtils.cross( _rB, PB );

			data.velocities[_indexA].v = vA;
			data.velocities[_indexA].w = wA;
			data.velocities[_indexB].v = vB;
			data.velocities[_indexB].w = wB;
		}

		internal override bool solvePositionConstraints( ref SolverData data )
		{
			Vector2 cA = data.positions[_indexA].c;
			float aA = data.positions[_indexA].a;
			Vector2 cB = data.positions[_indexB].c;
			float aB = data.positions[_indexB].a;

			Rot qA = new Rot( aA ), qB = new Rot( aB );

			Vector2 rA = MathUtils.mul( qA, localAnchorA - _localCenterA );
			Vector2 rB = MathUtils.mul( qB, localAnchorB - _localCenterB );

			// Get the pulley axes.
			Vector2 uA = cA + rA - worldAnchorA;
			Vector2 uB = cB + rB - worldAnchorB;

			float lengthA = uA.Length();
			float lengthB = uB.Length();

			if( lengthA > 10.0f * Settings.linearSlop )
			{
				uA *= 1.0f / lengthA;
			}
			else
			{
				uA = Vector2.Zero;
			}

			if( lengthB > 10.0f * Settings.linearSlop )
			{
				uB *= 1.0f / lengthB;
			}
			else
			{
				uB = Vector2.Zero;
			}

			// Compute effective mass.
			float ruA = MathUtils.cross( rA, uA );
			float ruB = MathUtils.cross( rB, uB );

			float mA = _invMassA + _invIA * ruA * ruA;
			float mB = _invMassB + _invIB * ruB * ruB;

			float mass = mA + ratio * ratio * mB;

			if( mass > 0.0f )
			{
				mass = 1.0f / mass;
			}

			float C = constant - lengthA - ratio * lengthB;
			float linearError = Math.Abs( C );

			float impulse = -mass * C;

			Vector2 PA = -impulse * uA;
			Vector2 PB = -ratio * impulse * uB;

			cA += _invMassA * PA;
			aA += _invIA * MathUtils.cross( rA, PA );
			cB += _invMassB * PB;
			aB += _invIB * MathUtils.cross( rB, PB );

			data.positions[_indexA].c = cA;
			data.positions[_indexA].a = aA;
			data.positions[_indexB].c = cB;
			data.positions[_indexB].a = aB;

			return linearError < Settings.linearSlop;
		}
	
	}
}