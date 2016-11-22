using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Nez.UI
{
	public class SelectBoxList<T> : ScrollPane where T : class
	{
		public int maxListCount;
		public ListBox<T> listBox;

		SelectBox<T> _selectBox;
		Element _previousScrollFocus;
		Vector2 _screenPosition;
		bool _isListBelowSelectBox;


		public SelectBoxList( SelectBox<T> selectBox ) : base( null, selectBox.getStyle().scrollStyle )
		{
			_selectBox = selectBox;

			setOverscroll( false, false );
			setFadeScrollBars( false );
			setScrollingDisabled( true, false );

			listBox = new ListBox<T>( selectBox.getStyle().listStyle );
			listBox.setTouchable( Touchable.Disabled );
			setWidget( listBox );

			listBox.onChanged += item =>
			{
				selectBox.getSelection().choose( item );
				if( selectBox.onChanged != null )
					selectBox.onChanged( item );
				hide();
			};
		}


		public void show( Stage stage )
		{
			if( listBox.isTouchable() )
				return;

			stage.addElement( this );

			_screenPosition = _selectBox.localToStageCoordinates( Vector2.Zero );

			// show the list above or below the select box, limited to a number of items and the available height in the stage.
			float itemHeight = listBox.getItemHeight();
			float height = itemHeight * ( maxListCount <= 0 ? _selectBox.getItems().Count : Math.Min( maxListCount, _selectBox.getItems().Count ) );
			var scrollPaneBackground = getStyle().background;
			if( scrollPaneBackground != null )
				height += scrollPaneBackground.topHeight + scrollPaneBackground.bottomHeight;
			var listBackground = listBox.getStyle().background;
			if( listBackground != null )
				height += listBackground.topHeight + listBackground.bottomHeight;

			float heightAbove = _screenPosition.Y;
			float heightBelow = Screen.height /*camera.viewportHeight */ - _screenPosition.Y - _selectBox.getHeight();
			_isListBelowSelectBox = true;
			if( height > heightBelow )
			{
				if( heightAbove > heightBelow )
				{
					_isListBelowSelectBox = false;
					height = Math.Min( height, heightAbove );
				}
				else
				{
					height = heightBelow;
				}
			}

			if( !_isListBelowSelectBox )
				setY( _screenPosition.Y - height );
			else
				setY( _screenPosition.Y + _selectBox.getHeight() );
			setX( _screenPosition.X );
			setHeight( height );
			validate();

			var width = Math.Max( preferredWidth, _selectBox.getWidth() );
			if( preferredHeight > height && !_disableY )
				width += getScrollBarWidth();
			setWidth( width );

			validate();
			scrollTo( 0, listBox.getHeight() - _selectBox.getSelectedIndex() * itemHeight - itemHeight / 2, 0, 0, true, true );
			updateVisualScroll();

			_previousScrollFocus = null;

			listBox.getSelection().set( _selectBox.getSelected() );
			listBox.setTouchable( Touchable.Enabled );
			_selectBox.onShow( this, _isListBelowSelectBox );
		}


		public void hide()
		{
			if( !listBox.isTouchable() || !hasParent() )
				return;
			
			listBox.setTouchable( Touchable.Disabled );

			if( stage != null )
			{
				if( _previousScrollFocus != null && _previousScrollFocus.getStage() == null )
					_previousScrollFocus = null;
			}

			_selectBox.onHide( this );
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			var temp = _selectBox.localToStageCoordinates( Vector2.Zero );
			if( temp != _screenPosition )
				Core.schedule( 0f, false, this, t => ((SelectBoxList<T>)t.context).hide() );
			
			base.draw( graphics, parentAlpha );
		}


		protected override void update()
		{
			if( Input.isKeyPressed( Keys.Escape ) )
			{
				Core.schedule( 0f, false, this, t => ( (SelectBoxList<T>)t.context ).hide() );
				return;
			}

			if( Input.leftMouseButtonPressed )
			{
				var point = stage.getMousePosition();
				point = screenToLocalCoordinates( point );

				float yMin = 0, yMax = height;

				// we need to include the list and the select box for our click checker. if the list is above the select box we expand the
				// height to include it. If the list is below we check for positions up to -_selectBox.height
				if( _isListBelowSelectBox )
					yMin -= _selectBox.height;
				else
					yMax += _selectBox.height;
				
				if( point.X < 0 || point.X > width || point.Y > yMax || point.Y < yMin )
					Core.schedule( 0f, false, this, t => ( (SelectBoxList<T>)t.context ).hide() );
			}
			
			base.update();
			toFront();
		}
	
	}
}

