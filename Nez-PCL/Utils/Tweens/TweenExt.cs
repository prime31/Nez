using System;
using Nez.Tweens;
using Microsoft.Xna.Framework;


namespace Nez
{
	public static class TweenExt
	{
		/// <summary>
		/// transform.position tween
		/// </summary>
		/// <returns>The kposition to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenPositionTo( this Transform self, Vector2 to, float duration = 0.3f )
		{
			var tween = Pool<TransformVector2Tween>.obtain();
			tween.setTargetAndType( self, TransformTargetType.Position );
			tween.initialize( tween, to, duration );

			return tween;
		}


		/// <summary>
		/// transform.localPosition tween
		/// </summary>
		/// <returns>The klocal position to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenLocalPositionTo( this Transform self, Vector2 to, float duration = 0.3f )
		{
			var tween = Pool<TransformVector2Tween>.obtain();
			tween.setTargetAndType( self, TransformTargetType.LocalPosition );
			tween.initialize( tween, to, duration );

			return tween;
		}


		/// <summary>
		/// transform.scale tween
		/// </summary>
		/// <returns>The scale to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenScaleTo( this Transform self, float to, float duration = 0.3f )
		{
			return self.tweenScaleTo( new Vector2( to ), duration );
		}


		/// <summary>
		/// transform.scale tween
		/// </summary>
		/// <returns>The scale to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenScaleTo( this Transform self, Vector2 to, float duration = 0.3f )
		{
			var tween = Pool<TransformVector2Tween>.obtain();
			tween.setTargetAndType( self, TransformTargetType.Scale );
			tween.initialize( tween, to, duration );

			return tween;
		}


		/// <summary>
		/// transform.localScale tween
		/// </summary>
		/// <returns>The klocal scale to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenLocalScaleTo( this Transform self, float to, float duration = 0.3f )
		{
			return self.tweenLocalScaleTo( new Vector2( to ), duration );
		}


		/// <summary>
		/// transform.localScale tween
		/// </summary>
		/// <returns>The klocal scale to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenLocalScaleTo( this Transform self, Vector2 to, float duration = 0.3f )
		{
			var tween = Pool<TransformVector2Tween>.obtain();
			tween.setTargetAndType( self, TransformTargetType.LocalScale );
			tween.initialize( tween, to, duration );

			return tween;
		}


		/// <summary>
		/// transform.rotation tween
		/// </summary>
		/// <returns>The rotation to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenRotationDegreesTo( this Transform self, float to, float duration = 0.3f )
		{
			var tween = Pool<TransformVector2Tween>.obtain();
			tween.setTargetAndType( self, TransformTargetType.RotationDegrees );
			tween.initialize( tween, new Vector2( to ), duration );

			return tween;
		}


		/// <summary>
		/// transform.localEulers tween
		/// </summary>
		/// <returns>The klocal eulers to.</returns>
		/// <param name="self">Self.</param>
		/// <param name="to">To.</param>
		/// <param name="duration">Duration.</param>
		public static ITween<Vector2> tweenLocalRotationDegreesTo( this Transform self, float to, float duration = 0.3f )
		{
			var tween = Pool<TransformVector2Tween>.obtain();
			tween.setTargetAndType( self, TransformTargetType.LocalRotationDegrees );
			tween.initialize( tween, new Vector2( to ), duration );

			return tween;
		}

	}
}

