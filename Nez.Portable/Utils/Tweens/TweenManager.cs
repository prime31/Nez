using System.Collections.Generic;


namespace Nez.Tweens
{
	public class TweenManager : GlobalManager
	{
		public static EaseType DefaultEaseType = EaseType.QuartIn;

		/// <summary>
		/// if true, the active tween list will be cleared when a new level loads
		/// </summary>
		public static bool RemoveAllTweensOnLevelLoad = false;


		#region Caching rules

		/// <summary>
		/// automatic caching of various types is supported here. Note that caching will only work when using extension methods to start
		/// the tweens or if you fetch a tween from the cache when doing custom tweens. See the extension method implementations for
		/// how to fetch a cached tween.
		/// </summary>
		public static bool CacheIntTweens = true;

		public static bool CacheFloatTweens = true;
		public static bool CacheVector2Tweens = true;
		public static bool CacheVector3Tweens;
		public static bool CacheVector4Tweens;
		public static bool CacheQuaternionTweens;
		public static bool CacheColorTweens = true;
		public static bool CacheRectTweens;

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


		public override void Update()
		{
			_isUpdating = true;

			// loop backwards so we can remove completed tweens
			for (var i = _activeTweens.Length - 1; i >= 0; --i)
			{
				var tween = _activeTweens.Buffer[i];
				if (tween.Tick())
					_tempTweens.Add(tween);
			}

			_isUpdating = false;

			// kill the dead Tweens
			for (var i = 0; i < _tempTweens.Length; i++)
			{
				_tempTweens.Buffer[i].RecycleSelf();
				_activeTweens.Remove(_tempTweens[i]);
			}

			_tempTweens.Clear();
		}


		#region Tween management

		/// <summary>
		/// adds a tween to the active tweens list
		/// </summary>
		/// <param name="tween">Tween.</param>
		public static void AddTween(ITweenable tween)
		{
			_instance._activeTweens.Add(tween);
		}


		/// <summary>
		/// removes a tween from the active tweens list
		/// </summary>
		/// <param name="tween">Tween.</param>
		public static void RemoveTween(ITweenable tween)
		{
			if (_instance._isUpdating)
			{
				_instance._tempTweens.Add(tween);
			}
			else
			{
				tween.RecycleSelf();
				_instance._activeTweens.Remove(tween);
			}
		}


		/// <summary>
		/// stops all tweens optionlly bringing them all to completion
		/// </summary>
		/// <param name="bringToCompletion">If set to <c>true</c> bring to completion.</param>
		public static void StopAllTweens(bool bringToCompletion = false)
		{
			for (var i = _instance._activeTweens.Length - 1; i >= 0; --i)
				_instance._activeTweens.Buffer[i].Stop(bringToCompletion);
		}


		/// <summary>
		/// returns all the tweens that have a specific context. Tweens are returned as ITweenable since that is all
		/// that TweenManager knows about.
		/// </summary>
		/// <returns>The tweens with context.</returns>
		/// <param name="context">Context.</param>
		public static List<ITweenable> AllTweensWithContext(object context)
		{
			var foundTweens = new List<ITweenable>();

			for (var i = 0; i < _instance._activeTweens.Length; i++)
			{
				if (_instance._activeTweens.Buffer[i] is ITweenable &&
				    (_instance._activeTweens.Buffer[i] as ITweenControl).Context == context)
					foundTweens.Add(_instance._activeTweens.Buffer[i]);
			}

			return foundTweens;
		}


		/// <summary>
		/// stops all the tweens with a given context
		/// </summary>
		/// <returns>The tweens with context.</returns>
		/// <param name="context">Context.</param>
		public static void StopAllTweensWithContext(object context, bool bringToCompletion = false)
		{
			for (var i = _instance._activeTweens.Length - 1; i >= 0; --i)
			{
				if (_instance._activeTweens.Buffer[i] is ITweenable &&
				    (_instance._activeTweens.Buffer[i] as ITweenControl).Context == context)
					_instance._activeTweens.Buffer[i].Stop(bringToCompletion);
			}
		}


		/// <summary>
		/// returns all the tweens that have a specific target. Tweens are returned as ITweenControl since that is all
		/// that TweenManager knows about.
		/// </summary>
		/// <returns>The tweens with target.</returns>
		/// <param name="target">target.</param>
		public static List<ITweenable> AllTweensWithTarget(object target)
		{
			var foundTweens = new List<ITweenable>();

			for (var i = 0; i < _instance._activeTweens.Length; i++)
			{
				if (_instance._activeTweens[i] is ITweenControl)
				{
					var tweenControl = _instance._activeTweens.Buffer[i] as ITweenControl;
					if (tweenControl.GetTargetObject() == target)
						foundTweens.Add(_instance._activeTweens[i] as ITweenable);
				}
			}

			return foundTweens;
		}


		/// <summary>
		/// stops all the tweens that have a specific target
		/// that TweenManager knows about.
		/// </summary>
		/// <param name="target">target.</param>
		public static void StopAllTweensWithTarget(object target, bool bringToCompletion = false)
		{
			for (var i = _instance._activeTweens.Length - 1; i >= 0; --i)
			{
				if (_instance._activeTweens[i] is ITweenControl)
				{
					var tweenControl = _instance._activeTweens.Buffer[i] as ITweenControl;
					if (tweenControl.GetTargetObject() == target)
						tweenControl.Stop(bringToCompletion);
				}
			}
		}

		#endregion
	}
}