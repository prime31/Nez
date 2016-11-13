using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


/// <summary>
/// concrete implementations of all tweenable types
/// </summary>
namespace Nez.Tweens
{
	public class IntTween : Tween<int>
	{
		public static IntTween create()
		{
			return TweenManager.cacheIntTweens ? Pool<IntTween>.obtain() : new IntTween();
		}


		public IntTween()
		{}


		public IntTween( ITweenTarget<int> target, int to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<int> setIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( (int)Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheIntTweens )
				Pool<IntTween>.free( this );
		}
	}


	public class FloatTween : Tween<float>
	{
		public static FloatTween create()
		{
			return TweenManager.cacheFloatTweens ? Pool<FloatTween>.obtain() : new FloatTween();
		}


		public FloatTween()
		{}


		public FloatTween( ITweenTarget<float> target, float from, float to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<float> setIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheFloatTweens )
				Pool<FloatTween>.free( this );
		}
	}


	public class Vector2Tween : Tween<Vector2>
	{
		public static Vector2Tween create()
		{
			return TweenManager.cacheVector2Tweens ? Pool<Vector2Tween>.obtain() : new Vector2Tween();
		}


		public Vector2Tween()
		{}


		public Vector2Tween( ITweenTarget<Vector2> target, Vector2 from, Vector2 to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<Vector2> setIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheVector2Tweens )
				Pool<Vector2Tween>.free( this );
		}
	}


	public class Vector3Tween : Tween<Vector3>
	{
		public static Vector3Tween create()
		{
			return TweenManager.cacheVector3Tweens ? Pool<Vector3Tween>.obtain() : new Vector3Tween();
		}


		public Vector3Tween()
		{}


		public Vector3Tween( ITweenTarget<Vector3> target, Vector3 from, Vector3 to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<Vector3> setIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheVector3Tweens )
				Pool<Vector3Tween>.free( this );
		}
	}


	public class Vector4Tween : Tween<Vector4>
	{
		public static Vector4Tween create()
		{
			return TweenManager.cacheVector4Tweens ? Pool<Vector4Tween>.obtain() : new Vector4Tween();
		}


		public Vector4Tween()
		{}


		public Vector4Tween( ITweenTarget<Vector4> target, Vector4 from, Vector4 to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<Vector4> setIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheVector4Tweens )
				Pool<Vector4Tween>.free( this );
		}
	}


	public class QuaternionTween : Tween<Quaternion>
	{
		public static QuaternionTween create()
		{
			return TweenManager.cacheQuaternionTweens ? Pool<QuaternionTween>.obtain() : new QuaternionTween();
		}


		public QuaternionTween()
		{}


		public QuaternionTween( ITweenTarget<Quaternion> target, Quaternion from, Quaternion to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<Quaternion> setIsRelative()
		{
			_isRelative = true;
			_toValue *= _fromValue;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheQuaternionTweens )
				Pool<QuaternionTween>.free( this );
		}
	}


	public class ColorTween : Tween<Color>
	{
		public static ColorTween create()
		{
			return TweenManager.cacheColorTweens ? Pool<ColorTween>.obtain() : new ColorTween();
		}


		public ColorTween()
		{}


		public ColorTween( ITweenTarget<Color> target, Color from, Color to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<Color> setIsRelative()
		{
			_isRelative = true;
			_toValue.R += _fromValue.R;
			_toValue.G += _fromValue.G;
			_toValue.B += _fromValue.B;
			_toValue.A += _fromValue.A;
			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheColorTweens )
				Pool<ColorTween>.free( this );
		}
	}


	public class RectangleTween : Tween<Rectangle>
	{
		public static RectangleTween create()
		{
			return TweenManager.cacheRectTweens ? Pool<RectangleTween>.obtain() : new RectangleTween();
		}


		public RectangleTween()
		{}


		public RectangleTween( ITweenTarget<Rectangle> target, Rectangle from, Rectangle to, float duration )
		{
			initialize( target, to, duration );
		}


		public override ITween<Rectangle> setIsRelative()
		{
			_isRelative = true;
			_toValue = new Rectangle
			(
				_toValue.X + _fromValue.X,
				_toValue.Y + _fromValue.Y,
				_toValue.Width + _fromValue.Width,
				_toValue.Height + _fromValue.Height
			);

			return this;
		}


		protected override void updateValue()
		{
			_target.setTweenedValue( Lerps.ease( _easeType, _fromValue, _toValue, _elapsedTime, _duration ) );
		}


		public override void recycleSelf()
		{
			base.recycleSelf();

			if( _shouldRecycleTween && TweenManager.cacheRectTweens )
				Pool<RectangleTween>.free( this );
		}
	}

}