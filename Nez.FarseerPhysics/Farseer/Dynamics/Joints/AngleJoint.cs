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
		/// The desired angle between BodyA and BodyB
		/// </summary>
		public float TargetAngle
		{
			get => _targetAngle;
			set
			{
				if (value != _targetAngle)
				{
					_targetAngle = value;
					WakeBodies();
				}
			}
		}

		/// <summary>
		/// Gets or sets the bias factor.
		/// Defaults to 0.2
		/// </summary>
		public float BiasFactor = 0.2f;

		/// <summary>
		/// Gets or sets the maximum impulse
		/// Defaults to float.MaxValue
		/// </summary>
		public float MaxImpulse = float.MaxValue;

		/// <summary>
		/// Gets or sets the softness of the joint
		/// Defaults to 0
		/// </summary>
		public float Softness;

		float _bias;
		float _jointError;
		float _massFactor;
		float _targetAngle;

		#endregion


		internal AngleJoint()
		{
			JointType = JointType.Angle;
		}

		/// <summary>
		/// Constructor for AngleJoint
		/// </summary>
		/// <param name="bodyA">The first body</param>
		/// <param name="bodyB">The second body</param>
		public AngleJoint(Body bodyA, Body bodyB) : base(bodyA, bodyB)
		{
			JointType = JointType.Angle;
		}

		public override Vector2 GetReactionForce(float invDt)
		{
			//TODO
			//return _inv_dt * _impulse;
			return Vector2.Zero;
		}

		public override float GetReactionTorque(float invDt)
		{
			return 0;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			int indexA = BodyA.IslandIndex;
			int indexB = BodyB.IslandIndex;

			float aW = data.Positions[indexA].A;
			float bW = data.Positions[indexB].A;

			_jointError = (bW - aW - TargetAngle);
			_bias = -BiasFactor * data.Step.Inv_dt * _jointError;
			_massFactor = (1 - Softness) / (BodyA._invI + BodyB._invI);

			if (float.IsInfinity(_massFactor))
				_massFactor = float.MaxValue;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			int indexA = BodyA.IslandIndex;
			int indexB = BodyB.IslandIndex;

			var p = (_bias - data.Velocities[indexB].W + data.Velocities[indexA].W) * _massFactor;

			data.Velocities[indexA].W -= BodyA._invI * Math.Sign(p) * Math.Min(Math.Abs(p), MaxImpulse);
			data.Velocities[indexB].W += BodyB._invI * Math.Sign(p) * Math.Min(Math.Abs(p), MaxImpulse);
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			// no position solving for this joint
			return true;
		}
	}
}