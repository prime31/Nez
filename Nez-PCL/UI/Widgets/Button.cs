using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class Button : Table, IInputListener, IGamepadFocusable
	{
		public event Action<bool> onChanged;
		public event Action<Button> onClicked;

		public override float preferredWidth
		{
			get
			{
				var width = base.preferredWidth;
				if( style.up != null )
					width = Math.Max( width, style.up.minWidth );
				if( style.down != null )
					width = Math.Max( width, style.down.minWidth );
				if( style.checkked != null )
					width = Math.Max( width, style.checkked.minWidth );
				return width;
			}
		}

		public override float preferredHeight
		{
			get
			{
				var height = base.preferredHeight;
				if( style.up != null )
					height = Math.Max( height, style.up.minHeight );
				if( style.down != null )
					height = Math.Max( height, style.down.minHeight );
				if( style.checkked != null )
					height = Math.Max( height, style.checkked.minHeight );
				return height;
			}
		}

		public override float minWidth
		{
			get { return preferredWidth; }
		}

		public override float minHeight
		{
			get { return preferredHeight; }
		}

		public bool isChecked
		{
			get { return _isChecked; }
			set { setChecked( value, programmaticChangeEvents ); }
		}

		public bool programmaticChangeEvents;

		/// <summary>
		/// the maximum distance outside the button the mouse can move when pressing it to cause it to be unfocused
		/// </summary>
		public float buttonBoundaryThreshold = 50f;

		internal ButtonGroup _buttonGroup;
		protected bool _mouseOver, _mouseDown;
		protected bool _isChecked;
		protected bool _isDisabled;
		ButtonStyle style;


		#region Constructors

		public Button( ButtonStyle style )
		{
			setTouchable( Touchable.Enabled );
			setStyle( style );
			setSize( preferredWidth, preferredHeight );
		}


		public Button( Skin skin, string styleName = null ) : this( skin.get<ButtonStyle>( styleName ) )
		{}


		public Button( IDrawable up ) : this( new ButtonStyle( up, null, null ) )
		{
		}


		public Button( IDrawable up, IDrawable down ) : this( new ButtonStyle( up, down, null ) )
		{
		}


		public Button( IDrawable up, IDrawable down, IDrawable checked_ ) : this( new ButtonStyle( up, down, checked_ ) )
		{
		}

		#endregion


		#region IInputListener

		void IInputListener.onMouseEnter()
		{
			_mouseOver = true;
		}


		void IInputListener.onMouseExit()
		{
			_mouseOver = _mouseDown = false;
		}


		bool IInputListener.onMousePressed( Vector2 mousePos )
		{
			if( _isDisabled )
				return false;

			_mouseDown = true;
			return true;
		}


		void IInputListener.onMouseMoved( Vector2 mousePos )
		{
			// if we get too far outside the button cancel future events
			if( distanceOutsideBoundsToPoint( mousePos ) > buttonBoundaryThreshold )
			{
				_mouseDown = _mouseOver = false;
				getStage().removeInputFocusListener( this );
			}
		}


		void IInputListener.onMouseUp( Vector2 mousePos )
		{
			_mouseDown = false;

			setChecked( !_isChecked, true );

			if( onClicked != null )
				onClicked( this );
		}

		#endregion


		#region IGamepadFocusable

		public bool shouldUseExplicitFocusableControl { get; set; }
		public IGamepadFocusable gamepadUpElement { get; set; }
		public IGamepadFocusable gamepadDownElement { get; set; }
		public IGamepadFocusable gamepadLeftElement { get; set; }
		public IGamepadFocusable gamepadRightElement { get; set; }


		public void enableExplicitFocusableControl( IGamepadFocusable upEle, IGamepadFocusable downEle, IGamepadFocusable leftEle, IGamepadFocusable rightEle )
		{
			shouldUseExplicitFocusableControl = true;
			gamepadUpElement = upEle;
			gamepadDownElement = downEle;
			gamepadLeftElement = leftEle;
			gamepadRightElement = rightEle;
		}


		void IGamepadFocusable.onUnhandledDirectionPressed( Direction direction )
		{}


		void IGamepadFocusable.onFocused()
		{
			onFocused();
		}


		void IGamepadFocusable.onUnfocused()
		{
			onUnfocused();
		}


		void IGamepadFocusable.onActionButtonPressed()
		{
			onActionButtonPressed();
		}


		void IGamepadFocusable.onActionButtonReleased()
		{
			onActionButtonReleased();
		}

		#endregion


		#region overrideable focus handlers

		protected virtual void onFocused()
		{
			_mouseOver = true;
		}


		protected virtual void onUnfocused()
		{
			_mouseOver = _mouseDown = false;
		}


		protected virtual void onActionButtonPressed()
		{
			_mouseDown = true;
		}


		protected virtual void onActionButtonReleased()
		{
			_mouseDown = false;

			setChecked( !_isChecked, true );

			if( onClicked != null )
				onClicked( this );
		}

		#endregion


		public virtual void setStyle( ButtonStyle style )
		{
			this.style = style;

			if( _mouseDown && !_isDisabled )
			{
				_background = style.down == null ? style.up : style.down;
			}
			else
			{
				if( _isDisabled && style.disabled != null )
					_background = style.disabled;
				else if( _isChecked && style.checkked != null )
					_background = ( _mouseOver && style.checkedOver != null ) ? style.checkedOver : style.checkked;
				else if( _mouseOver && style.over != null )
					_background = style.over;
				else
					_background = style.up;
			}

			setBackground( _background );
		}


		void setChecked( bool isChecked, bool fireEvent )
		{
			if( _isChecked == isChecked )
				return;

			if( _buttonGroup != null && !_buttonGroup.canCheck( this, isChecked ) )
				return;
			_isChecked = isChecked;

			if( fireEvent && onChanged != null )
			{
				onChanged( _isChecked );
			}
		}


		/// <summary>
		/// Toggles the checked state. This method changes the checked state, which fires a {@link onChangedEvent} (if programmatic change
		/// events are enabled), so can be used to simulate a button click.
		/// </summary>
		public void toggle()
		{
			isChecked = !_isChecked;
		}


		/// <summary>
		/// Returns the button's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public virtual ButtonStyle getStyle()
		{
			return style;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The button group.</returns>
		public ButtonGroup getButtonGroup()
		{
			return _buttonGroup;
		}


		public void setDisabled( bool disabled )
		{
			_isDisabled = disabled;
		}


		public bool getDisabled()
		{
			return _isDisabled;
		}


		public override void draw( Graphics graphics, float parentAlpha )
		{
			validate();

			if( _isDisabled && style.disabled != null )
				_background = style.disabled;
			else if( _mouseDown && style.down != null )
				_background = style.down;
			else if( _isChecked && style.checkked != null )
				_background = ( style.checkedOver != null && _mouseOver ) ? style.checkedOver : style.checkked;
			else if( _mouseOver && style.over != null )
				_background = style.over;
			else if( style.up != null ) //
				_background = style.up;
			setBackground( _background );

			float offsetX = 0, offsetY = 0;
			if( _mouseDown && !_isDisabled )
			{
				offsetX = style.pressedOffsetX;
				offsetY = style.pressedOffsetY;
			}
			else if( _isChecked && !_isDisabled )
			{
				offsetX = style.checkedOffsetX;
				offsetY = style.checkedOffsetY;
			}
			else
			{
				offsetX = style.unpressedOffsetX;
				offsetY = style.unpressedOffsetY;
			}
				
			for( var i = 0; i < children.Count; i++ )
				children[i].moveBy( offsetX, offsetY );

			base.draw( graphics, parentAlpha );

			for( int i = 0; i < children.Count; i++ )
				children[i].moveBy( -offsetX, -offsetY );
		}


		public override string ToString()
		{
			return string.Format( "[Button]" );
		}

	}


	/// <summary>
	/// The style for a button
	/// </summary>
	public class ButtonStyle
	{
		/** Optional. */
		public IDrawable up, down, over, checkked, checkedOver, disabled;

		/** Optional. offsets children (labels for example). */
		public float pressedOffsetX, pressedOffsetY, unpressedOffsetX, unpressedOffsetY, checkedOffsetX, checkedOffsetY;


		public ButtonStyle()
		{}


		public ButtonStyle( IDrawable up, IDrawable down, IDrawable over )
		{
			this.up = up;
			this.down = down;
			this.over = over;
		}


		public static ButtonStyle create( Color upColor, Color downColor, Color overColor )
		{
			return new ButtonStyle {
				up = new PrimitiveDrawable( upColor ),
				down = new PrimitiveDrawable( downColor ),
				over = new PrimitiveDrawable( overColor )
			};
		}


		public ButtonStyle clone()
		{
			return new ButtonStyle {
				up = up,
				down = down,
				over = over,
				checkked = checkked,
				checkedOver = checkedOver,
				disabled = disabled
			};
		}
	
	}
}

