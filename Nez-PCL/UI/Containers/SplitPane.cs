using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class SplitPane : Group, IInputListener
	{
		public override float preferredWidth
		{
			get
			{
				var first = _firstWidget == null ? 0 : ( _firstWidget is ILayout ? ( (ILayout)_firstWidget ).preferredWidth : _firstWidget.width );
				var second = _secondWidget == null ? 0 : ( _secondWidget is ILayout ? ( (ILayout)_secondWidget ).preferredWidth : _secondWidget.width );

				if( _vertical )
					return Math.Max( first, second );
				return first + _style.handle.minWidth + second;
			}
		}

		public override float preferredHeight
		{
			get
			{
				var first = _firstWidget == null ? 0 : ( _firstWidget is ILayout ? ( (ILayout)_firstWidget ).preferredHeight : _firstWidget.height );
				var second = _secondWidget == null ? 0 : ( _secondWidget is ILayout ? ( (ILayout)_secondWidget ).preferredHeight : _secondWidget.height );

				if( !_vertical )
					return Math.Max( first, second );
				return first + _style.handle.minHeight + second;
			}
		}

		SplitPaneStyle _style;
		float _splitAmount = 0.5f;
		float _minAmount;
		float _maxAmount = 1;

		Element _firstWidget;
		Element _secondWidget;

		RectangleF _firstWidgetBounds;
		RectangleF _secondWidgetBounds;
		RectangleF _handleBounds;

		bool _vertical;
		Vector2 _lastPoint;
		Vector2 _handlePosition;


		public SplitPane( Element firstWidget, Element secondWidget, SplitPaneStyle style, bool vertical = false )
		{
			setStyle( style );
			setFirstWidget( firstWidget );
			setSecondWidget( secondWidget );

			_vertical = vertical;
			setSize( preferredWidth, preferredHeight );
		}


		public SplitPane( Element firstWidget, Element secondWidget, IDrawable handle, bool vertical = false ) : this( firstWidget, secondWidget, new SplitPaneStyle( handle ), vertical )
		{}


		public SplitPane( SplitPaneStyle style, bool vertical = false ) : this( null, null, style, vertical )
		{}


		#region IInputListener

		void IInputListener.onMouseEnter()
		{
		}


		void IInputListener.onMouseExit()
		{
		}


		bool IInputListener.onMousePressed( Vector2 mousePos )
		{
			if( _handleBounds.contains( mousePos ) )
			{
				_lastPoint = mousePos;
				_handlePosition = _handleBounds.location;
				return true;
			}
			return false;
		}


		void IInputListener.onMouseMoved( Vector2 mousePos )
		{
			if( _vertical )
			{
				var delta = mousePos.Y - _lastPoint.Y;
				var availHeight = height - _style.handle.minHeight;
				var dragY = _handlePosition.Y + delta;
				_handlePosition.Y = dragY;
				dragY = Math.Max( 0, dragY );
				dragY = Math.Min( availHeight, dragY );
				_splitAmount = 1 - ( dragY / availHeight );
				_splitAmount = Mathf.clamp( _splitAmount, _minAmount, _maxAmount );

				_lastPoint = mousePos;
			}
			else
			{
				var delta = mousePos.X - _lastPoint.X;
				var availWidth = width - _style.handle.minWidth;
				var dragX = _handlePosition.X + delta;
				_handlePosition.X = dragX;
				dragX = Math.Max( 0, dragX );
				dragX = Math.Min( availWidth, dragX );
				_splitAmount = dragX / availWidth;
				_splitAmount = Mathf.clamp( _splitAmount, _minAmount, _maxAmount );

				_lastPoint = mousePos;
			}
			invalidate();
		}


		void IInputListener.onMouseUp( Vector2 mousePos )
		{
		}

		#endregion


		public override void layout()
		{
			if( _vertical )
				calculateVertBoundsAndPositions();
			else
				calculateHorizBoundsAndPositions();

			if( _firstWidget != null )
			{
				var firstWidgetBounds = this._firstWidgetBounds;
				_firstWidget.setBounds( firstWidgetBounds.x, firstWidgetBounds.y, firstWidgetBounds.width, firstWidgetBounds.height );

				if( _firstWidget is ILayout )
					( (ILayout)_firstWidget ).validate();
			}
				
			if( _secondWidget != null )
			{
				var secondWidgetBounds = this._secondWidgetBounds;
				_secondWidget.setBounds( secondWidgetBounds.x, secondWidgetBounds.y, secondWidgetBounds.width, secondWidgetBounds.height );

				if( _secondWidget is ILayout )
					( (ILayout)_secondWidget ).validate();
			}
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			validate();

			if( transform )
				applyTransform( graphics, computeTransform() );
			if( _firstWidget != null )
			{
				//batch.flush();
				//getStage().calculateScissors(firstWidgetBounds, firstScissors);
				//if (ScissorStack.pushScissors(firstScissors))
				{
					if( _firstWidget.isVisible() )
						_firstWidget.draw( graphics, parentAlpha * color.A );
					//batch.flush();
					//ScissorStack.popScissors();
				}
			}

			if( _secondWidget != null )
			{
				//batch.flush();
				//getStage().calculateScissors( secondWidgetBounds, secondScissors );
				//if( ScissorStack.pushScissors( secondScissors ) )
				{
					if( _secondWidget.isVisible() )
						_secondWidget.draw( graphics, parentAlpha * color.A );
					//batch.flush();
					//ScissorStack.popScissors();
				}
			}
				
			_style.handle.draw( graphics, _handleBounds.x, _handleBounds.y, _handleBounds.width, _handleBounds.height, new Color( color, color.A * parentAlpha ) );

			if( transform )
				resetTransform( graphics );
		}


		void calculateHorizBoundsAndPositions()
		{
			var availWidth = width - _style.handle.minWidth;
			var leftAreaWidth = (int)( availWidth * _splitAmount );
			var rightAreaWidth = availWidth - leftAreaWidth;
			var handleWidth = _style.handle.minWidth;

			_firstWidgetBounds = new RectangleF( 0, 0, leftAreaWidth, height );
			_secondWidgetBounds = new RectangleF( leftAreaWidth + handleWidth, 0, rightAreaWidth, height );
			_handleBounds = new RectangleF( leftAreaWidth, 0, handleWidth, height );
		}


		void calculateVertBoundsAndPositions()
		{
			var availHeight = height - _style.handle.minHeight;
			var topAreaHeight = (int)( availHeight * _splitAmount );
			var bottomAreaHeight = availHeight - topAreaHeight;

			_firstWidgetBounds = new RectangleF( 0, height - topAreaHeight, width, topAreaHeight );
			_secondWidgetBounds = new RectangleF( 0, 0, width, bottomAreaHeight );
			_handleBounds = new RectangleF( 0, bottomAreaHeight, width, _style.handle.minHeight );
		}


		#region Configuration

		public SplitPane setStyle( SplitPaneStyle style )
		{
			_style = style;
			setHandle( _style.handle );
			return this;
		}


		public SplitPaneStyle getStyle()
		{
			return _style;
		}


		public SplitPane setHandle( IDrawable handle )
		{
			_style.handle = handle;
			invalidate();

			return this;
		}


		public SplitPane setFirstWidget( Element firstWidget )
		{
			if( _firstWidget != null )
				removeElement( _firstWidget );

			_firstWidget = firstWidget;
			if( _firstWidget != null )
				addElement( _firstWidget );
			invalidate();

			return this;
		}


		public SplitPane setSecondWidget( Element secondWidget )
		{
			if( _secondWidget != null )
				removeElement( _secondWidget );

			_secondWidget = secondWidget;
			if( _secondWidget != null )
				addElement( _secondWidget );
			invalidate();

			return this;
		}


		/// <summary>
		/// The split amount between the min and max amount
		/// </summary>
		/// <returns>The split amount.</returns>
		/// <param name="amount">Amount.</param>
		public SplitPane setSplitAmount( float amount )
		{
			_splitAmount = Mathf.clamp( amount, _minAmount, _maxAmount );
			return this;
		}


		public SplitPane setMinSplitAmount( float amount )
		{
			Assert.isTrue( amount < 0, "minAmount has to be >= 0" );
			_minAmount = amount;
			return this;
		}


		public SplitPane setMaxSplitAmount( float amount )
		{
			Assert.isTrue( amount > 0, "maxAmount has to be <= 1" );
			_maxAmount = amount;
			return this;
		}

		#endregion

	}


	public class SplitPaneStyle
	{
		public IDrawable handle;


		public SplitPaneStyle()
		{}


		public SplitPaneStyle( IDrawable handle )
		{
			this.handle = handle;
		}


		public SplitPaneStyle clone()
		{
			return new SplitPaneStyle {
				handle = handle
			};
		}
	}
}

