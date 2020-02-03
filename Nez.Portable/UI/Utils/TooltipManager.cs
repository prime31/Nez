using System.Collections.Generic;
using Nez.Tweens;


namespace Nez.UI
{
	public class TooltipManager
	{
		static TooltipManager instance;

		/// <summary>
		/// Seconds from when an actor is hovered to when the tooltip is shown. Call {hideAll() after changing to reset internal state
		/// </summary>
		public float InitialTime = 1.5f;

		/// <summary>
		/// Once a tooltip is shown, this is used instead of initialTime. Default is 0.
		/// </summary>
		public float SubsequentTime = 0;

		/// <summary>
		/// Seconds to use subsequentTime
		/// </summary>
		public float ResetTime = 1.5f;

		/// <summary>
		/// If false, tooltips will not be shown. Default is true.
		/// </summary>
		public bool Enabled = true;

		/// <summary>
		/// If false, tooltips will be shown without animations. Default is true.
		/// </summary>
		public bool Animations = true;

		/// <summary>
		/// The maximum width of a TextTooltip. The label will wrap if needed. Default is int.MaxValue.
		/// </summary>
		public float MaxWidth = int.MaxValue;

		/// <summary>
		/// The distance from the mouse position to offset the tooltip actor
		/// </summary>
		public float OffsetX = 0, OffsetY = 10;

		/// <summary>
		/// The distance from the tooltip actor position to the edge of the screen where the actor will be shown on the other side of
		/// the mouse cursor.
		/// </summary>
		public float EdgeDistance = 8;

		List<Tooltip> _shownTooltips = new List<Tooltip>();
		float _time = 2;
		Tooltip _shownTooltip;
		ITimer _showTask, _resetTask;


		public static TooltipManager GetInstance()
		{
			if (instance == null)
				instance = new TooltipManager();
			return instance;
		}


		void StartShowTask(float time)
		{
			_showTask = Core.Schedule(time, false, this, t =>
			{
				var tm = t.Context as TooltipManager;
				var shownTooltip = tm._shownTooltip;
				if (shownTooltip == null)
					return;

				var stage = shownTooltip.GetTargetElement().GetStage();
				if (stage == null)
					return;


				stage.AddElement(shownTooltip.GetContainer());
				shownTooltip.GetContainer().ToFront();
				tm._shownTooltips.Add(shownTooltip);

				tm.ShowAction(shownTooltip);

				if (!shownTooltip.GetInstant())
				{
					tm._time = tm.SubsequentTime;
					tm.StopResetTask();
				}
			});
		}


		void StopShowTask()
		{
			if (_showTask != null)
			{
				_showTask.Stop();
				_showTask = null;
			}
		}


		void StartResetTask()
		{
			_resetTask = Core.Schedule(ResetTime, false, this, t =>
			{
				var tm = t.Context as TooltipManager;
				tm._time = tm.InitialTime;
			});
		}


		void StopResetTask()
		{
			if (_resetTask != null)
			{
				_resetTask.Stop();
				_resetTask = null;
			}
		}


		public void TouchDown(Tooltip tooltip)
		{
			StopShowTask();
			if (tooltip.GetContainer().Remove())
				StopResetTask();

			StartResetTask();
			if (Enabled || tooltip.GetAlways())
			{
				_shownTooltip = tooltip;
				StartShowTask(_time);
			}
		}


		public void Enter(Tooltip tooltip)
		{
			_shownTooltip = tooltip;
			StopShowTask();
			if (Enabled || tooltip.GetAlways())
			{
				if (_time == 0 || tooltip.GetInstant())
					StartShowTask(0);
				else
					StartShowTask(_time);
			}
		}


		public void Hide(Tooltip tooltip)
		{
			// dont go messing with the current tooltip unless it is actually us
			if (_shownTooltip == tooltip)
			{
				_shownTooltip = null;
				StopShowTask();
			}

			if (tooltip.GetContainer().HasParent())
			{
				_shownTooltips.Remove(tooltip);
				HideAction(tooltip);
				StopResetTask();
				StartResetTask();
			}
		}


		/// <summary>
		/// Called when tooltip is shown. Default implementation sets actions to animate showing.
		/// </summary>
		/// <returns>The action.</returns>
		/// <param name="tooltip">Tooltip.</param>
		protected void ShowAction(Tooltip tooltip)
		{
			var container = tooltip.GetContainer();
			if (Animations)
			{
				var actionTime = _time > 0 ? 0.3f : 0.15f;
				container.SetTransform(true);
				container.SetScale(0.5f);
				PropertyTweens.FloatPropertyTo(container, "scaleX", 1, actionTime).SetEaseType(EaseType.QuintIn)
					.Start();
				PropertyTweens.FloatPropertyTo(container, "scaleY", 1, actionTime).SetEaseType(EaseType.QuintIn)
					.Start();
			}
			else
			{
				container.SetScale(1);
			}
		}


		/// <summary>
		/// Called when tooltip is hidden. Default implementation sets actions to animate hiding and to remove the Element from the stage
		/// when the actions are complete.
		/// </summary>
		/// <returns>The action.</returns>
		/// <param name="tooltip">Tooltip.</param>
		protected void HideAction(Tooltip tooltip)
		{
			var container = tooltip.GetContainer();
			if (Animations)
			{
				PropertyTweens.FloatPropertyTo(container, "scaleX", 0.2f, 0.2f).SetEaseType(EaseType.QuintOut).Start();
				PropertyTweens.FloatPropertyTo(container, "scaleY", 0.2f, 0.2f).SetEaseType(EaseType.QuintOut)
					.SetCompletionHandler(t => container.Remove())
					.Start();
			}
			else
			{
				container.Remove();
			}
		}


		public void HideAll()
		{
			StopResetTask();
			StopShowTask();
			_time = InitialTime;
			_shownTooltip = null;

			foreach (var tooltip in _shownTooltips)
				Hide(tooltip);
			_shownTooltips.Clear();
		}


		/// <summary>
		/// Shows all tooltips on hover without a delay for resetTime seconds.
		/// </summary>
		public void Instant()
		{
			_time = 0;
			StartShowTask(0);
			StopShowTask();
		}
	}
}