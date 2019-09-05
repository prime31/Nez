using Microsoft.Xna.Framework;


// concrete implementations of all tweenable types
namespace Nez.Tweens
{
	public class IntTween : Tween<int>
	{
		public static IntTween Create()
		{
			return TweenManager.CacheIntTweens ? Pool<IntTween>.Obtain() : new IntTween();
		}


		public IntTween()
		{
		}


		public IntTween(ITweenTarget<int> target, int to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<int> SetIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue((int) Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheIntTweens)
				Pool<IntTween>.Free(this);
		}
	}


	public class FloatTween : Tween<float>
	{
		public static FloatTween Create()
		{
			return TweenManager.CacheFloatTweens ? Pool<FloatTween>.Obtain() : new FloatTween();
		}


		public FloatTween()
		{
		}


		public FloatTween(ITweenTarget<float> target, float to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<float> SetIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheFloatTweens)
				Pool<FloatTween>.Free(this);
		}
	}


	public class Vector2Tween : Tween<Vector2>
	{
		public static Vector2Tween Create()
		{
			return TweenManager.CacheVector2Tweens ? Pool<Vector2Tween>.Obtain() : new Vector2Tween();
		}


		public Vector2Tween()
		{
		}


		public Vector2Tween(ITweenTarget<Vector2> target, Vector2 to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<Vector2> SetIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheVector2Tweens)
				Pool<Vector2Tween>.Free(this);
		}
	}


	public class Vector3Tween : Tween<Vector3>
	{
		public static Vector3Tween Create()
		{
			return TweenManager.CacheVector3Tweens ? Pool<Vector3Tween>.Obtain() : new Vector3Tween();
		}


		public Vector3Tween()
		{
		}


		public Vector3Tween(ITweenTarget<Vector3> target, Vector3 to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<Vector3> SetIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheVector3Tweens)
				Pool<Vector3Tween>.Free(this);
		}
	}


	public class Vector4Tween : Tween<Vector4>
	{
		public static Vector4Tween Create()
		{
			return TweenManager.CacheVector4Tweens ? Pool<Vector4Tween>.Obtain() : new Vector4Tween();
		}


		public Vector4Tween()
		{
		}


		public Vector4Tween(ITweenTarget<Vector4> target, Vector4 to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<Vector4> SetIsRelative()
		{
			_isRelative = true;
			_toValue += _fromValue;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheVector4Tweens)
				Pool<Vector4Tween>.Free(this);
		}
	}


	public class QuaternionTween : Tween<Quaternion>
	{
		public static QuaternionTween Create()
		{
			return TweenManager.CacheQuaternionTweens ? Pool<QuaternionTween>.Obtain() : new QuaternionTween();
		}


		public QuaternionTween()
		{
		}


		public QuaternionTween(ITweenTarget<Quaternion> target, Quaternion to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<Quaternion> SetIsRelative()
		{
			_isRelative = true;
			_toValue *= _fromValue;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheQuaternionTweens)
				Pool<QuaternionTween>.Free(this);
		}
	}


	public class ColorTween : Tween<Color>
	{
		public static ColorTween Create()
		{
			return TweenManager.CacheColorTweens ? Pool<ColorTween>.Obtain() : new ColorTween();
		}


		public ColorTween()
		{
		}


		public ColorTween(ITweenTarget<Color> target, Color to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<Color> SetIsRelative()
		{
			_isRelative = true;
			_toValue.R += _fromValue.R;
			_toValue.G += _fromValue.G;
			_toValue.B += _fromValue.B;
			_toValue.A += _fromValue.A;
			return this;
		}


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheColorTweens)
				Pool<ColorTween>.Free(this);
		}
	}


	public class RectangleTween : Tween<Rectangle>
	{
		public static RectangleTween Create()
		{
			return TweenManager.CacheRectTweens ? Pool<RectangleTween>.Obtain() : new RectangleTween();
		}


		public RectangleTween()
		{
		}


		public RectangleTween(ITweenTarget<Rectangle> target, Rectangle to, float duration)
		{
			Initialize(target, to, duration);
		}


		public override ITween<Rectangle> SetIsRelative()
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


		protected override void UpdateValue()
		{
			_target.SetTweenedValue(Lerps.Ease(_easeType, _fromValue, _toValue, _elapsedTime, _duration));
		}


		public override void RecycleSelf()
		{
			base.RecycleSelf();

			if (_shouldRecycleTween && TweenManager.CacheRectTweens)
				Pool<RectangleTween>.Free(this);
		}
	}
}