using Microsoft.Xna.Framework;


namespace Nez.UI
{
	/// <summary>
	/// A listener that shows a tooltip Element when another Element is hovered over with the mouse.
	/// </summary>
	public class Tooltip : Element
	{
		protected Container _container;
		protected Element _targetElement;

		TooltipManager _manager;
		bool _instant, _always;
		bool _isMouseOver;


		public Tooltip( Element contents, Element targetElement )
		{
			_manager = TooltipManager.getInstance();

			_container = new Container( contents );
			_container.setOrigin( AlignInternal.center );
			_targetElement = targetElement;
			_container.setTouchable( Touchable.Disabled );
		}


		#region config

		public TooltipManager getManager()
		{
			return _manager;
		}


		public Container getContainer()
		{
			return _container;
		}


		public Tooltip setElement( Element contents )
		{
			_container.setElement( contents );
			return this;
		}


		public Element getElement()
		{
			return _container.getElement();
		}


		public T getElement<T>() where T : Element
		{
			return _container.getElement<T>();
		}


		public Tooltip setTargetElement( Element targetElement )
		{
			_targetElement = targetElement;
			return this;
		}


		public Element getTargetElement()
		{
			return _targetElement;
		}


		/// <summary>
		/// If true, this tooltip is shown without delay when hovered
		/// </summary>
		/// <returns>The instant.</returns>
		/// <param name="instant">Instant.</param>
		public Tooltip setInstant( bool instant )
		{
			_instant = instant;
			return this;
		}


		public bool getInstant()
		{
			return _instant;
		}


		/// <summary>
		/// If true, this tooltip is shown even when tooltips are not TooltipManager#enabled
		/// </summary>
		/// <returns>The always.</returns>
		/// <param name="always">Always.</param>
		public Tooltip setAlways( bool always )
		{
			_always = always;
			return this;
		}


		public bool getAlways()
		{
			return _always;
		}

		#endregion


		public override Element hit( Vector2 point )
		{
			// we do some rejiggering here by checking for hits on our target and using that
			var local = _targetElement.screenToLocalCoordinates( point );
			if( _targetElement.hit( local ) != null )
			{
				if( !_isMouseOver )
				{
					_isMouseOver = true;
					_manager.enter( this );
				}
				setContainerPosition( local.X, local.Y );
			}
			else if( _isMouseOver )
			{
				_isMouseOver = false;
				_manager.hide( this );
			}
			return null;
		}


		void setContainerPosition( float x, float y )
		{
			var stage = _targetElement.getStage();
			if( stage == null )
				return;

			_container.pack();
			float offsetX = _manager.offsetX, offsetY = _manager.offsetY, dist = _manager.edgeDistance;
			var point = _targetElement.localToStageCoordinates( new Vector2( x + offsetX - _container.getWidth() / 2, y - offsetY - _container.getHeight() ) );
			if( point.Y < dist )
				point = _targetElement.localToStageCoordinates( new Vector2( x + offsetX, y + offsetY ) );
			if( point.X < dist )
				point.X = dist;
			if( point.X + _container.getWidth() > stage.getWidth() - dist )
				point.X = stage.getWidth() - dist - _container.getWidth();
			if( point.Y + _container.getHeight() > stage.getHeight() - dist )
				point.Y = stage.getHeight() - dist - _container.getHeight();
			_container.setPosition( point.X, point.Y );

			point = _targetElement.localToStageCoordinates( new Vector2( _targetElement.getWidth() / 2, _targetElement.getHeight() / 2 ) );
			point -= new Vector2( _container.getX(), _container.getY() );
			_container.setOrigin( point.X, point.Y );
		}

	}
}

