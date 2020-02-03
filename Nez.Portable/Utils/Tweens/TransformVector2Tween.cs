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


		public void SetTweenedValue(Vector2 value)
		{
			switch (_targetType)
			{
				case TransformTargetType.Position:
					_transform.Position = value;
					break;
				case TransformTargetType.LocalPosition:
					_transform.LocalPosition = value;
					break;
				case TransformTargetType.Scale:
					_transform.Scale = value;
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
					throw new System.ArgumentOutOfRangeException();
			}
		}


		public Vector2 GetTweenedValue()
		{
			switch (_targetType)
			{
				case TransformTargetType.Position:
					return _transform.Position;
				case TransformTargetType.LocalPosition:
					return _transform.LocalPosition;
				case TransformTargetType.Scale:
					return _transform.Scale;
				case TransformTargetType.LocalScale:
					return _transform.LocalScale;
				case TransformTargetType.RotationDegrees:
					return new Vector2(_transform.RotationDegrees);
				case TransformTargetType.LocalRotationDegrees:
					return new Vector2(_transform.LocalRotationDegrees, 0);
				default:
					throw new System.ArgumentOutOfRangeException();
			}
		}


		public new object GetTargetObject()
		{
			return _transform;
		}


		public void SetTargetAndType(Transform transform, TransformTargetType targetType)
		{
			_transform = transform;
			_targetType = targetType;
		}


		protected override void UpdateValue()
		{
			// special case for non-relative angle lerps so that they take the shortest possible rotation
			if ((_targetType == TransformTargetType.RotationDegrees ||
			     _targetType == TransformTargetType.LocalRotationDegrees) && !_isRelative)
				SetTweenedValue(Lerps.EaseAngle(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
			else
				SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			if (_shouldRecycleTween)
			{
				_target = null;
				_nextTween = null;
				_transform = null;
				Pool<TransformVector2Tween>.Free(this);
			}
		}
	}
}