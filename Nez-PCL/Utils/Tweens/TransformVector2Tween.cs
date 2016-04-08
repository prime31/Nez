using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez.Tweens
{
	/// <summary>
	/// useful enum for any Transform related property tweens
	/// </summary>
	public enum TransformTargetType
	{
		Position,
		LocalPosition,
		Scale,
		LocalScale,
		RotationDegrees,
		LocalRotationDegrees
	}

	/// <summary>
	/// this is a special case since Transforms are by far the most tweened object. we encapsulate the Tween and the ITweenTarget
	/// in a single, cacheable class
	/// </summary>
	public class TransformVector2Tween : Vector2Tween, ITweenTarget<Vector2>
	{
		Transform _transform;
		TransformTargetType _targetType;


		public void setTweenedValue( Vector2 value )
		{
			switch( _targetType )
			{
				case TransformTargetType.Position:
					_transform.position = value;
					break;
				case TransformTargetType.LocalPosition:
					_transform.localPosition = value;
					break;
				case TransformTargetType.Scale:
					_transform.scale = value;
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


		public Vector2 getTweenedValue()
		{
			switch( _targetType )
			{
				case TransformTargetType.Position:
					return _transform.position;
				case TransformTargetType.LocalPosition:
					return _transform.localPosition;
				case TransformTargetType.Scale:
				return _transform.scale;
				case TransformTargetType.LocalScale:
					return _transform.localScale;
				case TransformTargetType.RotationDegrees:
					return new Vector2( _transform.rotationDegrees );
				case TransformTargetType.LocalRotationDegrees:
					return new Vector2( _transform.localRotationDegrees, 0 );
				default:
					throw new System.ArgumentOutOfRangeException();
			}
		}


		public new object getTargetObject()
		{
			return _transform;
		}


		public void setTargetAndType( Transform transform, TransformTargetType targetType )
		{
			_transform = transform;
			_targetType = targetType;
		}


		protected override void updateValue()
		{
			// special case for non-relative angle lerps so that they take the shortest possible rotation
			if( ( _targetType == TransformTargetType.RotationDegrees || _targetType == TransformTargetType.LocalRotationDegrees ) && !_isRelative )
			{
				setTweenedValue( Lerps.easeAngle( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
			}
			else
			{
				setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
			}
		}


		public override void recycleSelf()
		{
			if( _shouldRecycleTween )
			{
				_target = null;
				_nextTween = null;
				_transform = null;
				Pool<TransformVector2Tween>.free( this );
			}
		}
	
	}
}