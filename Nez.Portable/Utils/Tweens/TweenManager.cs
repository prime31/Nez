using System.Collections.Generic;


namespace Nez.Tweens
{
	public class TweenManager : GlobalManager
	{
		public static EaseType defaultEaseType = EaseType.QuartIn;

		/// <summary>
		/// if true, the active tween list will be cleared when a new level loads
		/// </summary>
		public static bool removeAllTweensOnLevelLoad = false;


		#region Caching rules

		/// <summary>
		/// automatic caching of various types is supported here. Note that caching will only work when using extension methods to start
		/// the tweens or if you fetch a tween from the cache when doing custom tweens. See the extension method implementations for
		/// how to fetch a cached tween.
		/// </summary>
		public static bool cacheIntTweens = true;
		public static bool cacheFloatTweens = true;
		public static bool cacheVector2Tweens = true;
		public static bool cacheVector3Tweens;
		public static bool cacheVector4Tweens;
		public static bool cacheQuaternionTweens;
		public static bool cacheColorTweens = true;
		public static bool cacheRectTweens;

		#endregion


		/// <summary>
		/// internal list of all the currently active tweens
		/// </summary>
		FastList<ITweenable> _activeTweens = new FastList<ITweenable>();
		
		/// <summary>
		/// stores tweens marked for removal
		/// </summary>
		FastList<ITweenable> _tempTweens = new FastList<ITweenable>();

		/// <summary>
		/// flag indicating the tween update loop is running
		/// </summary>
		bool _isUpdating;

		/// <summary>
		/// facilitates exposing a static API for easy access
		/// </summary>
		static TweenManager _instance;


		public TweenManager()
		{
			_instance = this;
		}


		public override void update()
		{
			_isUpdating = true;

			// loop backwards so we can remove completed tweens
			for( var i = _activeTweens.length - 1; i >= 0; --i )
			{
				var tween = _activeTweens.buffer[i];
				if( tween.tick() )
					_tempTweens.add( tween );
			}

			_isUpdating = false;

			// kill the dead Tweens
			for( var i = 0; i < _tempTweens.length; i++ )
			{
				_tempTweens.buffer[i].recycleSelf();
				_activeTweens.remove( _tempTweens[i] );
			}
			_tempTweens.clear();
		}


		#region Tween management

		/// <summary>
		/// adds a tween to the active tweens list
		/// </summary>
		/// <param name="tween">Tween.</param>
		public static void addTween( ITweenable tween )
		{
			_instance._activeTweens.add( tween );
		}


		/// <summary>
		/// removes a tween from the active tweens list
		/// </summary>
		/// <param name="tween">Tween.</param>
		public static void removeTween( ITweenable tween )
		{
			if( _instance._isUpdating )
			{
				_instance._tempTweens.add( tween );
			}
			else
			{
				tween.recycleSelf();
				_instance._activeTweens.remove( tween );
			}
		}


		/// <summary>
		/// stops all tweens optionlly bringing them all to completion
		/// </summary>
		/// <param name="bringToCompletion">If set to <c>true</c> bring to completion.</param>
		public static void stopAllTweens( bool bringToCompletion = false )
		{
			for( var i = _instance._activeTweens.length - 1; i >= 0; --i )
				_instance._activeTweens.buffer[i].stop( bringToCompletion );
		}


		/// <summary>
		/// returns all the tweens that have a specific context. Tweens are returned as ITweenable since that is all
		/// that TweenManager knows about.
		/// </summary>
		/// <returns>The tweens with context.</returns>
		/// <param name="context">Context.</param>
		public static List<ITweenable> allTweensWithContext( object context )
		{
			var foundTweens = new List<ITweenable>();

			for( var i = 0; i < _instance._activeTweens.length; i++ )
			{
				if( _instance._activeTweens.buffer[i] is ITweenable && ( _instance._activeTweens.buffer[i] as ITweenControl ).context == context )
					foundTweens.Add( _instance._activeTweens.buffer[i] );
			}

			return foundTweens;
		}


		/// <summary>
		/// stops all the tweens with a given context
		/// </summary>
		/// <returns>The tweens with context.</returns>
		/// <param name="context">Context.</param>
		public static void stopAllTweensWithContext( object context, bool bringToCompletion = false )
		{
			for( var i = _instance._activeTweens.length - 1; i >= 0; --i )
			{
				if( _instance._activeTweens.buffer[i] is ITweenable && ( _instance._activeTweens.buffer[i] as ITweenControl ).context == context )
					_instance._activeTweens.buffer[i].stop( bringToCompletion );
			}
		}


		/// <summary>
		/// returns all the tweens that have a specific target. Tweens are returned as ITweenControl since that is all
		/// that TweenManager knows about.
		/// </summary>
		/// <returns>The tweens with target.</returns>
		/// <param name="target">target.</param>
		public static List<ITweenable> allTweensWithTarget( object target )
		{
			var foundTweens = new List<ITweenable>();

			for( var i = 0; i < _instance._activeTweens.length; i++ )
			{
				if( _instance._activeTweens[i] is ITweenControl )
				{
					var tweenControl = _instance._activeTweens.buffer[i] as ITweenControl;
					if( tweenControl.getTargetObject() == target )
						foundTweens.Add( _instance._activeTweens[i] as ITweenable );
				}
			}

			return foundTweens;
		}


		/// <summary>
		/// stops all the tweens that have a specific target
		/// that TweenManager knows about.
		/// </summary>
		/// <param name="target">target.</param>
		public static void stopAllTweensWithTarget( object target, bool bringToCompletion = false )
		{
			for( var i = _instance._activeTweens.length - 1; i >= 0; --i )
			{
				if( _instance._activeTweens[i] is ITweenControl )
				{
					var tweenControl = _instance._activeTweens.buffer[i] as ITweenControl;
					if( tweenControl.getTargetObject() == target )
						tweenControl.stop( bringToCompletion );
				}
			}
		}

		#endregion

	}
}
