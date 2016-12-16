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
		/// The maximum friction force in N.
		/// </summary>
		public float maxForce;

		/// <summary>
		/// The maximum friction torque in N-m.
		/// </summary>
		public float maxTorque;

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
			jointType = JointType.Friction;
		}

		/// <summary>
		/// Constructor for FrictionJoint.
		/// </summary>
		/// <param name="bodyA"></param>
		/// <param name="bodyB"></param>
		/// <param name="anchor"></param>
		/// <param name="useWorldCoordinates">Set to true if you are using world coordinates as anchors.</param>
		public FrictionJoint( Body bodyA, Body bodyB, Vector2 anchor, bool useWorldCoordinates = false )
			: base( bodyA, bodyB )
		{
			jointType = JointType.Friction;

			if( useWorldCoordinates )
			{
				localAnchorA = base.bodyA.getLocalPoint( anchor );
				localAnchorB = base.bodyB.getLocalPoint( anchor );
			}
			else
			{
				localAnchorA = anchor;
				localAnchorB = anchor;
			}
		}

		public override Vector2 getReactionForce( float invDt )
		{
			return invDt * _linearImpulse;
		}

		public override float getReactionTorque( float invDt )
		{
			return invDt * _angularImpulse;
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

			float aA = data.positions[_indexA].a;
			Vector2 vA = data.velocities[_indexA].v;
			float wA = data.velocities[_indexA].w;

			float aB = data.positions[_indexB].a;
			Vector2 vB = data.velocities[_indexB].v;
			float wB = data.velocities[_indexB].w;

			Rot qA = new Rot( aA ), qB = new Rot( aB );

			// Compute the effective mass matrix.
			_rA = MathUtils.mul( qA, localAnchorA - _localCenterA );
			_rB = MathUtils.mul( qB, localAnchorB - _localCenterB );

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
			K.ex.X = mA + mB + iA * _rA.Y * _rA.Y + iB * _rB.Y * _rB.Y;
			K.ex.Y = -iA * _rA.X * _rA.Y - iB * _rB.X * _rB.Y;
			K.ey.X = K.ex.Y;
			K.ey.Y = mA + mB + iA * _rA.X * _rA.X + iB * _rB.X * _rB.X;

			_linearMass = K.Inverse;

			_angularMass = iA + iB;
			if( _angularMass > 0.0f )
			{
				_angularMass = 1.0f / _angularMass;
			}

			if( Settings.enableWarmstarting )
			{
				// Scale impulses to support a variable time step.
				_linearImpulse *= data.step.dtRatio;
				_angularImpulse *= data.step.dtRatio;

				Vector2 P = new Vector2( _linearImpulse.X, _linearImpulse.Y );
				vA -= mA * P;
				wA -= iA * ( MathUtils.cross( _rA, P ) + _angularImpulse );
				vB += mB * P;
				wB += iB * ( MathUtils.cross( _rB, P ) + _angularImpulse );
			}
			else
			{
				_linearImpulse = Vector2.Zero;
				_angularImpulse = 0.0f;
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

			float mA = _invMassA, mB = _invMassB;
			float iA = _invIA, iB = _invIB;

			float h = data.step.dt;

			// Solve angular friction
			{
				float Cdot = wB - wA;
				float impulse = -_angularMass * Cdot;

				float oldImpulse = _angularImpulse;
				float maxImpulse = h * maxTorque;
				_angularImpulse = MathUtils.clamp( _angularImpulse + impulse, -maxImpulse, maxImpulse );
				impulse = _angularImpulse - oldImpulse;

				wA -= iA * impulse;
				wB += iB * impulse;
			}

			// Solve linear friction
			{
				var Cdot = vB + MathUtils.cross( wB, _rB ) - vA - MathUtils.cross( wA, _rA );

				var impulse = -MathUtils.mul( ref _linearMass, Cdot );
				var oldImpulse = _linearImpulse;
				_linearImpulse += impulse;

				var maxImpulse = h * maxForce;
				if( _linearImpulse.LengthSquared() > maxImpulse * maxImpulse )
				{
					Nez.Vector2Ext.normalize( ref _linearImpulse );
					_linearImpulse *= maxImpulse;
				}

				impulse = _linearImpulse - oldImpulse;

				vA -= mA * impulse;
				wA -= iA * MathUtils.cross( _rA, impulse );

				vB += mB * impulse;
				wB += iB * MathUtils.cross( _rB, impulse );
			}

			data.velocities[_indexA].v = vA;
			data.velocities[_indexA].w = wA;
			data.velocities[_indexB].v = vB;
			data.velocities[_indexB].w = wB;
		}

		internal override bool solvePositionConstraints( ref SolverData data )
		{
			return true;
		}
	
	}
}