using System;
using Microsoft.Xna.Framework;


namespace Nez.UI
{
	public class Slider : ProgressBar, IInputListener, IGamepadFocusable
	{
		/// <summary>
		/// the maximum distance outside the slider the mouse can move when pressing it to cause it to be unfocused
		/// </summary>
		public float sliderBoundaryThreshold = 50f;

		SliderStyle style;
		bool _mouseOver, _mouseDown;


		/// <summary>
		/// Creates a new slider. It's width is determined by the given prefWidth parameter, its height is determined by the maximum of
		///  the height of either the slider {@link NinePatch} or slider handle {@link TextureRegion}. The min and max values determine
		/// the range the values of this slider can take on, the stepSize parameter specifies the distance between individual values.
		/// E.g. min could be 4, max could be 10 and stepSize could be 0.2, giving you a total of 30 values, 4.0 4.2, 4.4 and so on.
		/// </summary>
		/// <param name="min">Minimum.</param>
		/// <param name="max">Max.</param>
		/// <param name="stepSize">Step size.</param>
		/// <param name="vertical">If set to <c>true</c> vertical.</param>
		/// <param name="background">Background.</param>
		public Slider( float min, float max, float stepSize, bool vertical, SliderStyle style ) : base( min, max, stepSize, vertical, style )
		{
			shiftIgnoresSnap = true;
			this.style = style;
		}

		public Slider( float min, float max, float stepSize, bool vertical, Skin skin, string styleName = null ) : this( min, max, stepSize, vertical, skin.get<SliderStyle>(styleName) )
		{}

		public Slider( Skin skin, string styleName = null ) : this( 0, 1, 0.1f, false, skin.get<SliderStyle>( styleName ) )
		{}

		// Leaving this constructor for backwards-compatibility
		public Slider( Skin skin, string styleName = null, float min = 0, float max = 1, float step = 0.1f ) : this( min, max, step, false, skin.get<SliderStyle>( styleName ) )
		{}

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
			calculatePositionAndValue( mousePos );
			_mouseDown = true;
			return true;
		}


		void IInputListener.onMouseMoved( Vector2 mousePos )
		{
			if( distanceOutsideBoundsToPoint( mousePos ) > sliderBoundaryThreshold )
			{
				_mouseDown = _mouseOver = false;
				getStage().removeInputFocusListener( this );
			}
			else
			{
				calculatePositionAndValue( mousePos );
			}
		}


		void IInputListener.onMouseUp( Vector2 mousePos )
		{
			_mouseDown = false;
		}


		bool IInputListener.onMouseScrolled( int mouseWheelDelta )
		{
			return false;
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
		{
			onUnhandledDirectionPressed( direction );
		}


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

		protected virtual void onUnhandledDirectionPressed( Direction direction )
		{
			if( direction == Direction.Up || direction == Direction.Right )
				setValue( _value + stepSize );
			else
				setValue( _value - stepSize );
		}


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
		}

		#endregion


		public Slider setStyle( SliderStyle style )
		{
			Assert.isTrue( style is SliderStyle, "style must be a SliderStyle" );

			base.setStyle( style );
			this.style = style;
			return this;
		}


		/// <summary>
		/// Returns the slider's style. Modifying the returned style may not have an effect until {@link #setStyle(SliderStyle)} is called
		/// </summary>
		/// <returns>The style.</returns>
		public new SliderStyle getStyle()
		{
			return style;
		}


		public bool isDragging()
		{
			return _mouseDown && _mouseOver;
		}


		protected override IDrawable getKnobDrawable()
		{
			if( disabled && style.disabledKnob != null )
				return style.disabledKnob;
			
			if( isDragging() && style.knobDown != null )
				return style.knobDown;

			if( _mouseOver && style.knobOver != null )
				return style.knobOver;

			return style.knob;
		}


		void calculatePositionAndValue( Vector2 mousePos )
		{
			var knob = getKnobDrawable();

			float value;
			if( _vertical )
			{
				var height = this.height - style.background.topHeight - style.background.bottomHeight;
				var knobHeight = knob == null ? 0 : knob.minHeight;
				position = mousePos.Y - style.background.bottomHeight - knobHeight * 0.5f;
				value = min + ( max - min ) * ( position / ( height - knobHeight ) );
				position = Math.Max( 0, position );
				position = Math.Min( height - knobHeight, position );
			}
			else
			{
				var width = this.width - style.background.leftWidth - style.background.rightWidth;
				var knobWidth = knob == null ? 0 : knob.minWidth;
				position = mousePos.X - style.background.leftWidth - knobWidth * 0.5f;
				value = min + ( max - min ) * ( position / ( width - knobWidth ) );
				position = Math.Max( 0, position );
				position = Math.Min( width - knobWidth, position );
			}
				
			setValue( value );
		}

	}


	public class SliderStyle : ProgressBarStyle
	{
		/** Optional. */
		public IDrawable knobOver, knobDown;


		public SliderStyle()
		{
		}


		public SliderStyle( IDrawable background, IDrawable knob ) : base( background, knob )
		{
		}


		public new static SliderStyle create( Color backgroundColor, Color knobColor )
		{
			var background = new PrimitiveDrawable( backgroundColor );
			background.minWidth = background.minHeight = 10;

			var knob = new PrimitiveDrawable( knobColor );
			knob.minWidth = knob.minHeight = 20;

			return new SliderStyle {
				background = background,
				knob = knob
			};
		}


		public new SliderStyle clone()
		{
			return new SliderStyle {
				background = background,
				disabledBackground = disabledBackground,
				knob = knob,
				disabledKnob = disabledKnob,
				knobBefore = knobBefore,
				knobAfter = knobAfter,
				disabledKnobBefore = disabledKnobBefore,
				disabledKnobAfter = disabledKnobAfter,
					
				knobOver = knobOver,
				knobDown = knobDown
			};
		}
	}
}

