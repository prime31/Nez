using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;


namespace FarseerPhysics.Dynamics.Joints
{
	/// <summary>
	/// Maintains a fixed angle between two bodies
	/// </summary>
	public class AngleJoint : Joint
	{
		#region Properties/Fields

		public override Vector2 worldAnchorA
		{
			get { return bodyA.position; }
			set { Debug.Assert( false, "You can't set the world anchor on this joint type." ); }
		}

		public override Vector2 worldAnchorB
		{
			get { return bodyB.position; }
			set { Debug.Assert( false, "You can't set the world anchor on this joint type." ); }
		}

		/// <summary>
		/// The desired angle between BodyA and BodyB
		/// </summary>
		public float targetAngle
		{
			get { return _targetAngle; }
			set
			{
				if( value != _targetAngle )
				{
					_targetAngle = value;
					wakeBodies();
				}
			}
		}

		/// <summary>
		/// Gets or sets the bias factor.
		/// Defaults to 0.2
		/// </summary>
		public float biasFactor = 0.2f;

		/// <summary>
		/// Gets or sets the maximum impulse
		/// Defaults to float.MaxValue
		/// </summary>
		public float maxImpulse = float.MaxValue;

		/// <summary>
		/// Gets or sets the softness of the joint
		/// Defaults to 0
		/// </summary>
		public float softness;

		float _bias;
		float _jointError;
		float _massFactor;
		float _targetAngle;

		#endregion


		internal AngleJoint()
		{
			jointType = JointType.Angle;
		}

		/// <summary>
		/// Constructor for AngleJoint
		/// </summary>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		public AngleJoint( Body bodyA, Body bodyB ) : base( bodyA, bodyB )
		{
			jointType = JointType.Angle;
		}

		public override Vector2 getReactionForce( float invDt )
		{
			//TODO
			//return _inv_dt * _impulse;
			return Vector2.Zero;
		}

		public override float getReactionTorque( float invDt )
		{
			return 0;
		}

		internal override void initVelocityConstraints( ref SolverData data )
		{
			int indexA = bodyA.islandIndex;
			int indexB = bodyB.islandIndex;

			float aW = data.positions[indexA].a;
			float bW = data.positions[indexB].a;

			_jointError = ( bW - aW - targetAngle );
			_bias = -biasFactor * data.step.inv_dt * _jointError;
			_massFactor = ( 1 - softness ) / ( bodyA._invI + bodyB._invI );

			if( float.IsInfinity( _massFactor ) )
				_massFactor = float.MaxValue;
		}

		internal override void solveVelocityConstraints( ref SolverData data )
		{
			int indexA = bodyA.islandIndex;
			int indexB = bodyB.islandIndex;

			var p = ( _bias - data.velocities[indexB].w + data.velocities[indexA].w ) * _massFactor;

			data.velocities[indexA].w -= bodyA._invI * Math.Sign( p ) * Math.Min( Math.Abs( p ), maxImpulse );
			data.velocities[indexB].w += bodyB._invI * Math.Sign( p ) * Math.Min( Math.Abs( p ), maxImpulse );
		}

		internal override bool solvePositionConstraints( ref SolverData data )
		{
			// no position solving for this joint
			return true;
		}
	
	}
}