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
			return TweenManager.cacheIntTweens ? QuickCache<IntTween>.pop() : new IntTween();
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
				QuickCache<IntTween>.push( this );
		}
	}


	public class FloatTween : Tween<float>
	{
		public static FloatTween create()
		{
			return TweenManager.cacheFloatTweens ? QuickCache<FloatTween>.pop() : new FloatTween();
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
				QuickCache<FloatTween>.push( this );
		}
	}


	public class Vector2Tween : Tween<Vector2>
	{
		public static Vector2Tween create()
		{
			return TweenManager.cacheVector2Tweens ? QuickCache<Vector2Tween>.pop() : new Vector2Tween();
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
				QuickCache<Vector2Tween>.push( this );
		}
	}


	public class Vector3Tween : Tween<Vector3>
	{
		public static Vector3Tween create()
		{
			return TweenManager.cacheVector3Tweens ? QuickCache<Vector3Tween>.pop() : new Vector3Tween();
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
				QuickCache<Vector3Tween>.push( this );
		}
	}


	public class Vector4Tween : Tween<Vector4>
	{
		public static Vector4Tween create()
		{
			return TweenManager.cacheVector4Tweens ? QuickCache<Vector4Tween>.pop() : new Vector4Tween();
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
				QuickCache<Vector4Tween>.push( this );
		}
	}


	public class QuaternionTween : Tween<Quaternion>
	{
		public static QuaternionTween create()
		{
			return TweenManager.cacheQuaternionTweens ? QuickCache<QuaternionTween>.pop() : new QuaternionTween();
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
				QuickCache<QuaternionTween>.push( this );
		}
	}


	public class ColorTween : Tween<Color>
	{
		public static ColorTween create()
		{
			return TweenManager.cacheColorTweens ? QuickCache<ColorTween>.pop() : new ColorTween();
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
				QuickCache<ColorTween>.push( this );
		}
	}


	public class RectangleTween : Tween<Rectangle>
	{
		public static RectangleTween create()
		{
			return TweenManager.cacheRectTweens ? QuickCache<RectangleTween>.pop() : new RectangleTween();
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
				QuickCache<RectangleTween>.push( this );
		}
	}

}