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
	// Linear constraint (point-to-line)
	// d = pB - pA = xB + rB - xA - rA
	// C = dot(ay, d)
	// Cdot = dot(d, cross(wA, ay)) + dot(ay, vB + cross(wB, rB) - vA - cross(wA, rA))
	//      = -dot(ay, vA) - dot(cross(d + rA, ay), wA) + dot(ay, vB) + dot(cross(rB, ay), vB)
	// J = [-ay, -cross(d + rA, ay), ay, cross(rB, ay)]

	// Spring linear constraint
	// C = dot(ax, d)
	// Cdot = = -dot(ax, vA) - dot(cross(d + rA, ax), wA) + dot(ax, vB) + dot(cross(rB, ax), vB)
	// J = [-ax -cross(d+rA, ax) ax cross(rB, ax)]

	// Motor rotational constraint
	// Cdot = wB - wA
	// J = [0 0 -1 0 0 1]

	/// <summary>
	/// A wheel joint. This joint provides two degrees of freedom: translation
	/// along an axis fixed in bodyA and rotation in the plane. You can use a
	/// joint limit to restrict the range of motion and a joint motor to drive
	/// the rotation or to model rotational friction.
	/// This joint is designed for vehicle suspensions.
	/// </summary>
	public class WheelJoint : Joint
	{
		#region Properties/Fields

		/// <summary>
		/// The local anchor point on BodyA
		/// </summary>
		public Vector2 localAnchorA;

		/// <summary>
		/// The local anchor point on BodyB
		/// </summary>
		public Vector2 localAnchorB;

		public override Vector2 worldAnchorA
		{
			get { return bodyA.getWorldPoint( localAnchorA ); }
			set { localAnchorA = bodyA.getLocalPoint( value ); }
		}

		public override Vector2 worldAnchorB
		{
			get { return bodyB.getWorldPoint( localAnchorB ); }
			set { localAnchorB = bodyB.getLocalPoint( value ); }
		}

		/// <summary>
		/// The axis at which the suspension moves.
		/// </summary>
		public Vector2 axis
		{
			get { return _axis; }
			set
			{
				_axis = value;
				localXAxis = bodyA.getLocalVector( _axis );
				_localYAxis = MathUtils.cross( 1.0f, localXAxis );
			}
		}

		/// <summary>
		/// The axis in local coordinates relative to BodyA
		/// </summary>
		public Vector2 localXAxis { get; private set; }

		/// <summary>
		/// The desired motor speed in radians per second.
		/// </summary>
		public float motorSpeed
		{
			get { return _motorSpeed; }
			set
			{
				wakeBodies();
				_motorSpeed = value;
			}
		}

		/// <summary>
		/// The maximum motor torque, usually in N-m.
		/// </summary>
		public float maxMotorTorque
		{
			get { return _maxMotorTorque; }
			set
			{
				wakeBodies();
				_maxMotorTorque = value;
			}
		}

		/// <summary>
		/// Suspension frequency, zero indicates no suspension
		/// </summary>
		public float frequency;

		/// <summary>
		/// Suspension damping ratio, one indicates critical damping
		/// </summary>
		public float dampingRatio;

		/// <summary>
		/// Gets the translation along the axis
		/// </summary>
		public float jointTranslation
		{
			get
			{
				var bA = bodyA;
				var bB = bodyB;

				var pA = bA.getWorldPoint( localAnchorA );
				var pB = bB.getWorldPoint( localAnchorB );
				var d = pB - pA;
				var axis = bA.getWorldVector( localXAxis );

				float translation = Vector2.Dot( d, axis );
				return translation;
			}
		}

		/// <summary>
		/// Gets the angular velocity of the joint
		/// </summary>
		public float jointSpeed
		{
			get
			{
				float wA = bodyA.angularVelocity;
				float wB = bodyB.angularVelocity;
				return wB - wA;
			}
		}

		/// <summary>
		/// Enable/disable the joint motor.
		/// </summary>
		public bool motorEnabled
		{
			get { return _enableMotor; }
			set
			{
				wakeBodies();
				_enableMotor = value;
			}
		}

		// Solver shared
		Vector2 _localYAxis;

		float _impulse;
		float _motorImpulse;
		float _springImpulse;

		float _maxMotorTorque;
		float _motorSpeed;
		bool _enableMotor;

		// Solver temp
		int _indexA;
		int _indexB;
		Vector2 _localCenterA;
		Vector2 _localCenterB;
		float _invMassA;
		float _invMassB;
		float _invIA;
		float _invIB;

		Vector2 _ax, _ay;
		float _sAx, _sBx;
		float _sAy, _sBy;

		float _mass;
		float _motorMass;
		float _springMass;

		float _bias;
		float _gamma;
		Vector2 _axis;

		#endregion


		internal WheelJoint()
		{
			jointType = JointType.Wheel;
		}

		/// <summary>
		/// Constructor for WheelJoint
		/// </summary>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		/// <param name="anchor">The anchor point</param>
		/// <param name="axis">The axis</param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public WheelJoint( Body bodyA, Body bodyB, Vector2 anchor, Vector2 axis, bool useWorldCoordinates = false )
			: base( bodyA, bodyB )
		{
			jointType = JointType.Wheel;

			if( useWorldCoordinates )
			{
				localAnchorA = bodyA.getLocalPoint( anchor );
				localAnchorB = bodyB.getLocalPoint( anchor );
			}
			else
			{
				localAnchorA = bodyA.getLocalPoint( bodyB.getWorldPoint( anchor ) );
				localAnchorB = anchor;
			}

			this.axis = axis; //FPE only: We maintain the original value as it is supposed to.
		}

		/// <summary>
		/// Gets the torque of the motor
		/// </summary>
		/// <param name="invDt">inverse delta time</param>
		public float getMotorTorque( float invDt )
		{
			return invDt * _motorImpulse;
		}

		public override Vector2 getReactionForce( float invDt )
		{
			return invDt * ( _impulse * _ay + _springImpulse * _ax );
		}

		public override float getReactionTorque( float invDt )
		{
			return invDt * _motorImpulse;
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

			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			Vector2 cA = data.positions[_indexA].c;
			float aA = data.positions[_indexA].a;
			Vector2 vA = data.velocities[_indexA].v;
			float wA = data.velocities[_indexA].w;

			Vector2 cB = data.positions[_indexB].c;
			float aB = data.positions[_indexB].a;
			Vector2 vB = data.velocities[_indexB].v;
			float wB = data.velocities[_indexB].w;

			Rot qA = new Rot( aA ), qB = new Rot( aB );

			// Compute the effective masses.
			Vector2 rA = MathUtils.mul( qA, localAnchorA - _localCenterA );
			Vector2 rB = MathUtils.mul( qB, localAnchorB - _localCenterB );
			Vector2 d1 = cB + rB - cA - rA;

			// Point to line constraint
			{
				_ay = MathUtils.mul( qA, _localYAxis );
				_sAy = MathUtils.cross( d1 + rA, _ay );
				_sBy = MathUtils.cross( rB, _ay );

				_mass = mA + mB + iA * _sAy * _sAy + iB * _sBy * _sBy;

				if( _mass > 0.0f )
				{
					_mass = 1.0f / _mass;
				}
			}

			// Spring constraint
			_springMass = 0.0f;
			_bias = 0.0f;
			_gamma = 0.0f;
			if( frequency > 0.0f )
			{
				_ax = MathUtils.mul( qA, localXAxis );
				_sAx = MathUtils.cross( d1 + rA, _ax );
				_sBx = MathUtils.cross( rB, _ax );

				float invMass = mA + mB + iA * _sAx * _sAx + iB * _sBx * _sBx;

				if( invMass > 0.0f )
				{
					_springMass = 1.0f / invMass;

					float C = Vector2.Dot( d1, _ax );

					// Frequency
					float omega = 2.0f * Settings.pi * frequency;

					// Damping coefficient
					float d = 2.0f * _springMass * dampingRatio * omega;

					// Spring stiffness
					float k = _springMass * omega * omega;

					// magic formulas
					float h = data.step.dt;
					_gamma = h * ( d + h * k );
					if( _gamma > 0.0f )
					{
						_gamma = 1.0f / _gamma;
					}

					_bias = C * h * k * _gamma;

					_springMass = invMass + _gamma;
					if( _springMass > 0.0f )
					{
						_springMass = 1.0f / _springMass;
					}
				}
			}
			else
			{
				_springImpulse = 0.0f;
			}

			// Rotational motor
			if( _enableMotor )
			{
				_motorMass = iA + iB;
				if( _motorMass > 0.0f )
				{
					_motorMass = 1.0f / _motorMass;
				}
			}
			else
			{
				_motorMass = 0.0f;
				_motorImpulse = 0.0f;
			}

			if( Settings.enableWarmstarting )
			{
				// Account for variable time step.
				_impulse *= data.step.dtRatio;
				_springImpulse *= data.step.dtRatio;
				_motorImpulse *= data.step.dtRatio;

				Vector2 P = _impulse * _ay + _springImpulse * _ax;
				float LA = _impulse * _sAy + _springImpulse * _sAx + _motorImpulse;
				float LB = _impulse * _sBy + _springImpulse * _sBx + _motorImpulse;

				vA -= _invMassA * P;
				wA -= _invIA * LA;

				vB += _invMassB * P;
				wB += _invIB * LB;
			}
			else
			{
				_impulse = 0.0f;
				_springImpulse = 0.0f;
				_motorImpulse = 0.0f;
			}

			data.velocities[_indexA].v = vA;
			data.velocities[_indexA].w = wA;
			data.velocities[_indexB].v = vB;
			data.velocities[_indexB].w = wB;
		}

		internal override void solveVelocityConstraints( ref SolverData data )
		{
			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			Vector2 vA = data.velocities[_indexA].v;
			float wA = data.velocities[_indexA].w;
			Vector2 vB = data.velocities[_indexB].v;
			float wB = data.velocities[_indexB].w;

			// Solve spring constraint
			{
				float Cdot = Vector2.Dot( _ax, vB - vA ) + _sBx * wB - _sAx * wA;
				float impulse = -_springMass * ( Cdot + _bias + _gamma * _springImpulse );
				_springImpulse += impulse;

				Vector2 P = impulse * _ax;
				float LA = impulse * _sAx;
				float LB = impulse * _sBx;

				vA -= mA * P;
				wA -= iA * LA;

				vB += mB * P;
				wB += iB * LB;
			}

			// Solve rotational motor constraint
			{
				float Cdot = wB - wA - _motorSpeed;
				float impulse = -_motorMass * Cdot;

				float oldImpulse = _motorImpulse;
				float maxImpulse = data.step.dt * _maxMotorTorque;
				_motorImpulse = MathUtils.clamp( _motorImpulse + impulse, -maxImpulse, maxImpulse );
				impulse = _motorImpulse - oldImpulse;

				wA -= iA * impulse;
				wB += iB * impulse;
			}

			// Solve point to line constraint
			{
				float Cdot = Vector2.Dot( _ay, vB - vA ) + _sBy * wB - _sAy * wA;
				float impulse = -_mass * Cdot;
				_impulse += impulse;

				Vector2 P = impulse * _ay;
				float LA = impulse * _sAy;
				float LB = impulse * _sBy;

				vA -= mA * P;
				wA -= iA * LA;

				vB += mB * P;
				wB += iB * LB;
			}

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
			Vector2 d = ( cB - cA ) + rB - rA;

			Vector2 ay = MathUtils.mul( qA, _localYAxis );

			float sAy = MathUtils.cross( d + rA, ay );
			float sBy = MathUtils.cross( rB, ay );

			float C = Vector2.Dot( d, ay );

			float k = _invMassA + _invMassB + _invIA * _sAy * _sAy + _invIB * _sBy * _sBy;

			float impulse;
			if( k != 0.0f )
			{
				impulse = -C / k;
			}
			else
			{
				impulse = 0.0f;
			}

			Vector2 P = impulse * ay;
			float LA = impulse * sAy;
			float LB = impulse * sBy;

			cA -= _invMassA * P;
			aA -= _invIA * LA;
			cB += _invMassB * P;
			aB += _invIB * LB;

			data.positions[_indexA].c = cA;
			data.positions[_indexA].a = aA;
			data.positions[_indexB].c = cB;
			data.positions[_indexB].a = aB;

			return Math.Abs( C ) <= Settings.linearSlop;
		}
	
	}
}