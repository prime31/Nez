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


		public PropertyTarget( object target, string propertyName )
		{
			_target = target;

			// try to fetch the field. if we dont find it this is a property
			if( ( _fieldInfo = ReflectionUtils.getFieldInfo( target, propertyName ) ) == null )
			{
				_setter = ReflectionUtils.setterForProperty<Action<T>>( target, propertyName );
				_getter = ReflectionUtils.getterForProperty<Func<T>>( target, propertyName );
			}

			Insist.isTrue( _setter != null || _fieldInfo != null, "either the property (" + propertyName + ") setter or getter could not be found on the object " + target );
		}


		public object getTargetObject()
		{
			return _target;
		}


		public void setTweenedValue( T value )
		{
			if( _fieldInfo != null )
				_fieldInfo.SetValue( _target, value );
			else
				_setter( value );
		}


		public T getTweenedValue()
		{
			if( _fieldInfo != null )
				return (T)_fieldInfo.GetValue( _target );
			return _getter();
		}
	}


	public static class PropertyTweens
	{
		public static ITween<int> intPropertyTo( object self, string memberName, int to, float duration )
		{
			var tweenTarget = new PropertyTarget<int>( self, memberName );
			var tween = TweenManager.cacheIntTweens ? Pool<IntTween>.obtain() : new IntTween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}


		public static ITween<float> floatPropertyTo( object self, string memberName, float to, float duration )
		{
			var tweenTarget = new PropertyTarget<float>( self, memberName );
			var tween = TweenManager.cacheFloatTweens ? Pool<FloatTween>.obtain() : new FloatTween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}


		public static ITween<Vector2> vector2PropertyTo( object self, string memberName, Vector2 to, float duration )
		{
			var tweenTarget = new PropertyTarget<Vector2>( self, memberName );
			var tween = TweenManager.cacheVector2Tweens ? Pool<Vector2Tween>.obtain() : new Vector2Tween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}


		public static ITween<Vector3> vector3PropertyTo( object self, string memberName, Vector3 to, float duration )
		{
			var tweenTarget = new PropertyTarget<Vector3>( self, memberName );
			var tween = TweenManager.cacheVector3Tweens ? Pool<Vector3Tween>.obtain() : new Vector3Tween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}


		public static ITween<Vector4> vector4PropertyTo( object self, string memberName, Vector4 to, float duration )
		{
			var tweenTarget = new PropertyTarget<Vector4>( self, memberName );
			var tween = TweenManager.cacheVector4Tweens ? Pool<Vector4Tween>.obtain() : new Vector4Tween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}


		public static ITween<Quaternion> quaternionPropertyTo( object self, string memberName, Quaternion to, float duration )
		{
			var tweenTarget = new PropertyTarget<Quaternion>( self, memberName );
			var tween = TweenManager.cacheQuaternionTweens ? Pool<QuaternionTween>.obtain() : new QuaternionTween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}


		public static ITween<Color> colorPropertyTo( object self, string memberName, Color to, float duration )
		{
			var tweenTarget = new PropertyTarget<Color>( self, memberName );
			var tween = TweenManager.cacheColorTweens ? Pool<ColorTween>.obtain() : new ColorTween();
			tween.initialize( tweenTarget, to, duration );

			return tween;
		}

	}
}
