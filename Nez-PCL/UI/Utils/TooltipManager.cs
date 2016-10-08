using System;
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
		public float initialTime = 1.5f;

		/// <summary>
		/// Once a tooltip is shown, this is used instead of initialTime. Default is 0.
		/// </summary>
		public float subsequentTime = 0;

		/// <summary>
		/// Seconds to use subsequentTime
		/// </summary>
		public float resetTime = 1.5f;

		/// <summary>
		/// If false, tooltips will not be shown. Default is true.
		/// </summary>
		public bool enabled = true;

		/// <summary>
		/// If false, tooltips will be shown without animations. Default is true.
		/// </summary>
		public bool animations = true;

		/// <summary>
		/// The maximum width of a TextTooltip. The label will wrap if needed. Default is int.MaxValue.
		/// </summary>
		public float maxWidth = int.MaxValue;

		/// <summary>
		/// The distance from the mouse position to offset the tooltip actor
		/// </summary>
		public float offsetX = 0, offsetY = 10;

		/// <summary>
		/// The distance from the tooltip actor position to the edge of the screen where the actor will be shown on the other side of
		/// the mouse cursor.
		/// </summary>
		public float edgeDistance = 8;

		List<Tooltip> _shownTooltips = new List<Tooltip>();
		float _time = 2;
		Tooltip _shownTooltip;
		ITimer _showTask, _resetTask;


		static public TooltipManager getInstance()
		{
			if( instance == null )
				instance = new TooltipManager();
			return instance;
		}


		void startShowTask( float time )
		{
			_showTask = Core.schedule( time, false, this, t =>
			{
				var tm = t.context as TooltipManager;
				var shownTooltip = tm._shownTooltip;
				if( shownTooltip == null )
					return;

				var stage = shownTooltip.getTargetElement().getStage();
				if( stage == null )
					return;


				stage.addElement( shownTooltip.getContainer() );
				shownTooltip.getContainer().toFront();
				tm._shownTooltips.Add( shownTooltip );

				tm.showAction( shownTooltip );

				if( !shownTooltip.getInstant() )
				{
					tm._time = tm.subsequentTime;
					tm.stopResetTask();
				}
			} );
		}


		void stopShowTask()
		{
			if( _showTask != null )
			{
				_showTask.stop();
				_showTask = null;
			}
		}


		void startResetTask()
		{
			_resetTask = Core.schedule( resetTime, false, this, t =>
			{
				var tm = t.context as TooltipManager;
				tm._time = tm.initialTime;
			} );
		}


		void stopResetTask()
		{
			if( _resetTask != null )
			{
				_resetTask.stop();
				_resetTask = null;
			}
		}


		public void touchDown( Tooltip tooltip )
		{
			stopShowTask();
			if( tooltip.getContainer().remove() )
				stopResetTask();
			
			startResetTask();
			if( enabled || tooltip.getAlways() )
			{
				_shownTooltip = tooltip;
				startShowTask( _time );
			}
		}


		public void enter( Tooltip tooltip )
		{
			_shownTooltip = tooltip;
			stopShowTask();
			if( enabled || tooltip.getAlways() )
			{
				if( _time == 0 || tooltip.getInstant() )
					startShowTask( 0 );
				else
					startShowTask( _time );
			}
		}


		public void hide( Tooltip tooltip )
		{
			// dont go messing with the current tooltip unless it is actually us
			if( _shownTooltip == tooltip )
			{
				_shownTooltip = null;
				stopShowTask();
			}

			if( tooltip.getContainer().hasParent() )
			{
				_shownTooltips.Remove( tooltip );
				hideAction( tooltip );
				stopResetTask();
				startResetTask();
			}
		}


		/// <summary>
		/// Called when tooltip is shown. Default implementation sets actions to animate showing.
		/// </summary>
		/// <returns>The action.</returns>
		/// <param name="tooltip">Tooltip.</param>
		protected void showAction( Tooltip tooltip )
		{
			var container = tooltip.getContainer();
			if( animations )
			{
				var actionTime = _time > 0 ? 0.3f : 0.15f;
				container.setTransform( true );
				container.setScale( 0.5f );
				PropertyTweens.floatPropertyTo( container, "scaleX", 1, actionTime ).setEaseType( EaseType.QuintIn ).start();
				PropertyTweens.floatPropertyTo( container, "scaleY", 1, actionTime ).setEaseType( EaseType.QuintIn ).start();				
			}
			else
			{
				container.setScale( 1 );
			}
		}


		/// <summary>
		/// Called when tooltip is hidden. Default implementation sets actions to animate hiding and to remove the Element from the stage
		/// when the actions are complete.
		/// </summary>
		/// <returns>The action.</returns>
		/// <param name="tooltip">Tooltip.</param>
		protected void hideAction( Tooltip tooltip )
		{
			var container = tooltip.getContainer();
			if( animations )
			{
				PropertyTweens.floatPropertyTo( container, "scaleX", 0.2f, 0.2f ).setEaseType( EaseType.QuintOut ).start();
				PropertyTweens.floatPropertyTo( container, "scaleY", 0.2f, 0.2f ).setEaseType( EaseType.QuintOut )
							  .setCompletionHandler( t => container.remove() )
							  .start();
			}
			else
			{
				container.remove();
			}
		}


		public void hideAll()
		{
			stopResetTask();
			stopShowTask();
			_time = initialTime;
			_shownTooltip = null;

			foreach( var tooltip in _shownTooltips )
				hide( tooltip );
			_shownTooltips.Clear();
		}


		/// <summary>
		/// Shows all tooltips on hover without a delay for resetTime seconds.
		/// </summary>
		public void instant()
		{
			_time = 0;
			startShowTask( 0 );
			stopShowTask();
		}

	}
}

