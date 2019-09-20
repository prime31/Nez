using Microsoft.Xna.Framework;
using System;


namespace Nez.Tweens
{
	public class TransformSpringTween : AbstractTweenable
	{
		public TransformTargetType TargetType => _targetType;

		Transform _transform;
		TransformTargetType _targetType;
		Vector2 _targetValue;
		Vector2 _velocity;

		// configuration of dampingRatio and angularFrequency are public for easier value tweaking at design time

		/// <summary>
		/// lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.
		/// </summary>
		public float DampingRatio = 0.23f;

		/// <summary>
		/// An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stableThe angular frequency.
		/// </summary>
		public float AngularFrequency = 25;


		/// <summary>
		/// Initializes a new instance of the TransformSpringTween class.
		/// </summary>
		public TransformSpringTween(Transform transform, TransformTargetType targetType, Vector2 targetValue)
		{
			_transform = transform;
			_targetType = targetType;
			SetTargetValue(targetValue);
		}


		/// <summary>
		/// you can call setTargetValue at any time to reset the target value to a new Vector2. If you have not called start to add the
		/// spring tween it will be called for you.
		/// </summary>
		/// <param name="targetValue">Target value.</param>
		public void SetTargetValue(Vector2 targetValue)
		{
			_velocity = Vector2.Zero;
			_targetValue = targetValue;

			if (!_isCurrentlyManagedByTweenManager)
				Start();
		}


		/// <summary>
		/// lambda should be the desired duration when the oscillation magnitude is reduced by 50%
		/// </summary>
		/// <param name="lambda">Lambda.</param>
		public void UpdateDampingRatioWithHalfLife(float lambda)
		{
			DampingRatio = (-lambda / AngularFrequency) * (float) Math.Log(0.5f);
		}


		#region AbstractTweenable

		public override bool Tick()
		{
			if (!_isPaused)
				SetTweenedValue(Lerps.FastSpring(GetCurrentValueOfTweenedTargetType(), _targetValue, ref _velocity,
					DampingRatio, AngularFrequency));

			return false;
		}

		#endregion


		void SetTweenedValue(Vector2 value)
		{
			switch (_targetType)
			{
				case TransformTargetType.Position:
					_transform.Position = value;
					break;
				case TransformTargetType.LocalPosition:
					_transform.LocalPosition = value;
					break;
				case TransformTargetType.LocalScale:
					_transform.LocalScale = value;
					break;
				case TransformTargetType.RotationDegrees:
					_transform.RotationDegrees = value.X;
					break;
				case TransformTargetType.LocalRotationDegrees:
					_transform.LocalRotationDegrees = value.X;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		Vector2 GetCurrentValueOfTweenedTargetType()
		{
			switch (_targetType)
			{
				case TransformTargetType.Position:
					return _transform.Position;
				case TransformTargetType.LocalPosition:
					return _transform.LocalPosition;
				case TransformTargetType.LocalScale:
					return _transform.LocalScale;
				case TransformTargetType.RotationDegrees:
					return new Vector2(_transform.RotationDegrees, 0);
				case TransformTargetType.LocalRotationDegrees:
					return new Vector2(_transform.LocalRotationDegrees, 0);
			}

			return Vector2.Zero;
		}
	}
}