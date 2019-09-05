using System;
using System.Reflection;
using Microsoft.Xna.Framework;


namespace Nez.Tweens
{
	/// <summary>
	/// generic ITweenTarget used for all property tweens
	/// </summary>
	class PropertyTarget<T> : ITweenTarget<T> where T : struct
	{
		protected object _target;
		FieldInfo _fieldInfo;
		protected Action<T> _setter;
		protected Func<T> _getter;


		public PropertyTarget(object target, string propertyName)
		{
			_target = target;

			// try to fetch the field. if we dont find it this is a property
			if ((_fieldInfo = ReflectionUtils.GetFieldInfo(target, propertyName)) == null)
			{
				_setter = ReflectionUtils.SetterForProperty<Action<T>>(target, propertyName);
				_getter = ReflectionUtils.GetterForProperty<Func<T>>(target, propertyName);
			}

			Insist.IsTrue(_setter != null || _fieldInfo != null,
				"either the property (" + propertyName + ") setter or getter could not be found on the object " +
				target);
		}


		public object GetTargetObject()
		{
			return _target;
		}


		public void SetTweenedValue(T value)
		{
			if (_fieldInfo != null)
				_fieldInfo.SetValue(_target, value);
			else
				_setter(value);
		}


		public T GetTweenedValue()
		{
			if (_fieldInfo != null)
				return (T) _fieldInfo.GetValue(_target);

			return _getter();
		}
	}


	public static class PropertyTweens
	{
		public static ITween<int> IntPropertyTo(object self, string memberName, int to, float duration)
		{
			var tweenTarget = new PropertyTarget<int>(self, memberName);
			var tween = TweenManager.CacheIntTweens ? Pool<IntTween>.Obtain() : new IntTween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}


		public static ITween<float> FloatPropertyTo(object self, string memberName, float to, float duration)
		{
			var tweenTarget = new PropertyTarget<float>(self, memberName);
			var tween = TweenManager.CacheFloatTweens ? Pool<FloatTween>.Obtain() : new FloatTween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}


		public static ITween<Vector2> Vector2PropertyTo(object self, string memberName, Vector2 to, float duration)
		{
			var tweenTarget = new PropertyTarget<Vector2>(self, memberName);
			var tween = TweenManager.CacheVector2Tweens ? Pool<Vector2Tween>.Obtain() : new Vector2Tween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}


		public static ITween<Vector3> Vector3PropertyTo(object self, string memberName, Vector3 to, float duration)
		{
			var tweenTarget = new PropertyTarget<Vector3>(self, memberName);
			var tween = TweenManager.CacheVector3Tweens ? Pool<Vector3Tween>.Obtain() : new Vector3Tween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}


		public static ITween<Vector4> Vector4PropertyTo(object self, string memberName, Vector4 to, float duration)
		{
			var tweenTarget = new PropertyTarget<Vector4>(self, memberName);
			var tween = TweenManager.CacheVector4Tweens ? Pool<Vector4Tween>.Obtain() : new Vector4Tween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}


		public static ITween<Quaternion> QuaternionPropertyTo(object self, string memberName, Quaternion to,
		                                                      float duration)
		{
			var tweenTarget = new PropertyTarget<Quaternion>(self, memberName);
			var tween = TweenManager.CacheQuaternionTweens ? Pool<QuaternionTween>.Obtain() : new QuaternionTween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}


		public static ITween<Color> ColorPropertyTo(object self, string memberName, Color to, float duration)
		{
			var tweenTarget = new PropertyTarget<Color>(self, memberName);
			var tween = TweenManager.CacheColorTweens ? Pool<ColorTween>.Obtain() : new ColorTween();
			tween.Initialize(tweenTarget, to, duration);

			return tween;
		}
	}
}