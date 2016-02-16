using System.Collections;
using Microsoft.Xna.Framework;
using System;


namespace Nez.Tweens
{
	public class TransformSpringTween : AbstractTweenable
	{
		public TransformTargetType targetType { get { return _targetType; } }
		
		Transform _transform;
		TransformTargetType _targetType;
		Vector2 _targetValue;
		Vector2 _velocity;

		// configuration of dampingRatio and angularFrequency are public for easier value tweaking at design time

		/// <summary>
		/// lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.
		/// </summary>
		public float dampingRatio = 0.23f;

		/// <summary>
		/// An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stableThe angular frequency.
		/// </summary>
		public float angularFrequency = 25;


		/// <summary>
		/// Initializes a new instance of the <see cref="Prime31.ZestKit.TransformSpringTween"/> class.
		/// </summary>
		/// <param name="transform">Transform.</param>
		/// <param name="dampingRatio">lower values are less damped and higher values are more damped resulting in less springiness.
		/// should be between 0.01f, 1f to avoid unstable systems.</param>
		/// <param name="angularFrequency">An angular frequency of 2pi (radians per second) means the oscillation completes one
		/// full period over one second, i.e. 1Hz. should be less than 35 or so to remain stable</param>
		public TransformSpringTween( Transform transform, TransformTargetType targetType, Vector2 targetValue )
		{
			_transform = transform;
			_targetType = targetType;
			setTargetValue( targetValue );
		}


		/// <summary>
		/// you can call setTargetValue at any time to reset the target value to a new Vector2. If you have not called start to add the
		/// spring tween it will be called for you.
		/// </summary>
		/// <param name="targetValue">Target value.</param>
		public void setTargetValue( Vector2 targetValue )
		{
			_velocity = Vector2.Zero;
			_targetValue = targetValue;

			if( !_isCurrentlyManagedByTweenManager )
				start();
		}


		/// <summary>
		/// lambda should be the desired duration when the oscillation magnitude is reduced by 50%
		/// </summary>
		/// <param name="lambda">Lambda.</param>
		public void updateDampingRatioWithHalfLife( float lambda )
		{
			dampingRatio = ( -lambda / angularFrequency ) * (float)Math.Log( 0.5f );
		}


		#region AbstractTweenable

		public override bool tick()
		{
			if( !_isPaused )
				setTweenedValue( Lerps.fastSpring( getCurrentValueOfTweenedTargetType(), _targetValue, ref _velocity, dampingRatio, angularFrequency ) );

			return false;
		}

		#endregion


		void setTweenedValue( Vector2 value )
		{
			switch( _targetType )
			{
				case TransformTargetType.Position:
					_transform.position = value;
					break;
				case TransformTargetType.LocalPosition:
					_transform.localPosition = value;
					break;
				case TransformTargetType.LocalScale:
					_transform.localScale = value;
					break;
				case TransformTargetType.RotationDegrees:
					_transform.rotationDegrees = value.X;
					break;
				case TransformTargetType.LocalRotationDegrees:
					_transform.localRotationDegrees = value.X;
					break;
				default:
					throw new System.ArgumentOutOfRangeException();
			}
		}


		Vector2 getCurrentValueOfTweenedTargetType()
		{
			switch( _targetType )
			{
				case TransformTargetType.Position:
					return _transform.position;
				case TransformTargetType.LocalPosition:
					return _transform.localPosition;
				case TransformTargetType.LocalScale:
					return _transform.localScale;
				case TransformTargetType.RotationDegrees:
					return new Vector2( _transform.rotationDegrees, 0 );
				case TransformTargetType.LocalRotationDegrees:
					return new Vector2( _transform.localRotationDegrees, 0 );
			}

			return Vector2.Zero;
		}

	}
}