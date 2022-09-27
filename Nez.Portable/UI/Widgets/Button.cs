using Microsoft.Xna.Framework;
using System;


namespace Nez.UI
{
	public class Button : Table, IInputListener, IGamepadFocusable
	{
		public event Action<bool> OnChanged;
		public event Action<Button> OnClicked, OnRightClicked;

		public override float PreferredWidth
		{
			get
			{
				var prefWidth = base.PreferredWidth;
				if (style.Up != null)
					prefWidth = Math.Max(prefWidth, style.Up.MinWidth);
				if (style.Down != null)
					prefWidth = Math.Max(prefWidth, style.Down.MinWidth);
				if (style.Checked != null)
					prefWidth = Math.Max(prefWidth, style.Checked.MinWidth);
				return prefWidth;
			}
		}

		public override float PreferredHeight
		{
			get
			{
				var prefHeight = base.PreferredHeight;
				if (style.Up != null)
					prefHeight = Math.Max(prefHeight, style.Up.MinHeight);
				if (style.Down != null)
					prefHeight = Math.Max(prefHeight, style.Down.MinHeight);
				if (style.Checked != null)
					prefHeight = Math.Max(prefHeight, style.Checked.MinHeight);
				return prefHeight;
			}
		}

		public override float MinWidth => PreferredWidth;

		public override float MinHeight => PreferredHeight;

		public bool IsChecked
		{
			get => _isChecked;
			set => SetChecked(value, ProgrammaticChangeEvents);
		}

		public bool ProgrammaticChangeEvents;

		/// <summary>
		/// the maximum distance outside the button the mouse can move when pressing it to cause it to be unfocused
		/// </summary>
		public float ButtonBoundaryThreshold = 50f;

		internal ButtonGroup _buttonGroup;
		protected bool _mouseOver, _mouseDown;
		protected bool _isChecked;
		protected bool _isDisabled;
		ButtonStyle style;


		#region Constructors

		public Button(ButtonStyle style)
		{
			SetTouchable(Touchable.Enabled);
			SetStyle(style);
			SetSize(PreferredWidth, PreferredHeight);
		}


		public Button(Skin skin, string styleName = null) : this(skin.Get<ButtonStyle>(styleName))
		{ }


		public Button(IDrawable up) : this(new ButtonStyle(up, null, null))
		{ }


		public Button(IDrawable up, IDrawable down) : this(new ButtonStyle(up, down, null))
		{ }


		public Button(IDrawable up, IDrawable down, IDrawable checked_) : this(new ButtonStyle(up, down, checked_))
		{ }

		#endregion


		#region IInputListener

		void IInputListener.OnMouseEnter()
		{
			_mouseOver = true;
		}


		void IInputListener.OnMouseExit()
		{
			_mouseOver = _mouseDown = false;
		}


		bool IInputListener.OnLeftMousePressed(Vector2 mousePos)
		{
			if (_isDisabled)
				return false;

			_mouseDown = true;
			return true;
		}

		bool IInputListener.OnRightMousePressed(Vector2 mousePos)
		{
			if (_isDisabled)
				return false;

			_mouseDown = true;
			return true;
		}


		void IInputListener.OnMouseMoved(Vector2 mousePos)
		{
			// if we get too far outside the button cancel future events
			if (DistanceOutsideBoundsToPoint(mousePos) > ButtonBoundaryThreshold)
			{
				_mouseDown = _mouseOver = false;
				GetStage().RemoveInputFocusListener(this);
			}
		}


		void IInputListener.OnLeftMouseUp(Vector2 mousePos)
		{
			_mouseDown = false;

			SetChecked(!_isChecked, true);

			if (OnClicked != null)
				OnClicked(this);
		}

		void IInputListener.OnRightMouseUp(Vector2 mousePos)
		{
			_mouseDown = false;

			SetChecked(!_isChecked, true);

			if (OnRightClicked != null)
				OnRightClicked(this);
		}


		bool IInputListener.OnMouseScrolled(int mouseWheelDelta)
		{
			return false;
		}

		#endregion


		#region IGamepadFocusable

		public bool ShouldUseExplicitFocusableControl { get; set; }
		public IGamepadFocusable GamepadUpElement { get; set; }
		public IGamepadFocusable GamepadDownElement { get; set; }
		public IGamepadFocusable GamepadLeftElement { get; set; }
		public IGamepadFocusable GamepadRightElement { get; set; }


		public void EnableExplicitFocusableControl(IGamepadFocusable upEle, IGamepadFocusable downEle,
												   IGamepadFocusable leftEle, IGamepadFocusable rightEle)
		{
			ShouldUseExplicitFocusableControl = true;
			GamepadUpElement = upEle;
			GamepadDownElement = downEle;
			GamepadLeftElement = leftEle;
			GamepadRightElement = rightEle;
		}


		void IGamepadFocusable.OnUnhandledDirectionPressed(Direction direction)
		{ }


		void IGamepadFocusable.OnFocused()
		{
			OnFocused();
		}


		void IGamepadFocusable.OnUnfocused()
		{
			OnUnfocused();
		}


		void IGamepadFocusable.OnActionButtonPressed()
		{
			OnActionButtonPressed();
		}


		void IGamepadFocusable.OnActionButtonReleased()
		{
			OnActionButtonReleased();
		}

		#endregion


		#region overrideable focus handlers

		protected virtual void OnFocused()
		{
			_mouseOver = true;
		}


		protected virtual void OnUnfocused()
		{
			_mouseOver = _mouseDown = false;
		}


		protected virtual void OnActionButtonPressed()
		{
			if (_isDisabled)
				return;

			_mouseDown = true;
		}


		protected virtual void OnActionButtonReleased()
		{
			if (_isDisabled)
				return;

			_mouseDown = false;

			SetChecked(!_isChecked, true);

			if (OnClicked != null)
				OnClicked(this);
		}

		#endregion


		public virtual void SetStyle(ButtonStyle style)
		{
			this.style = style;

			if (_mouseDown && !_isDisabled)
			{
				_background = style.Down == null ? style.Up : style.Down;
			}
			else
			{
				if (_isDisabled && style.Disabled != null)
					_background = style.Disabled;
				else if (_isChecked && style.Checked != null)
					_background = (_mouseOver && style.CheckedOver != null) ? style.CheckedOver : style.Checked;
				else if (_mouseOver && style.Over != null)
					_background = style.Over;
				else
					_background = style.Up;
			}

			SetBackground(_background);
		}


		void SetChecked(bool isCheckked, bool fireEvent)
		{
			if (_isChecked == isCheckked)
				return;

			if (_buttonGroup != null && !_buttonGroup.CanCheck(this, isCheckked))
				return;

			_isChecked = isCheckked;

			if (fireEvent && OnChanged != null)
			{
				OnChanged(_isChecked);
			}
		}


		/// <summary>
		/// Toggles the checked state. This method changes the checked state, which fires a {@link onChangedEvent} (if programmatic change
		/// events are enabled), so can be used to simulate a button click.
		/// </summary>
		public void Toggle()
		{
			IsChecked = !_isChecked;
		}


		/// <summary>
		/// Returns the button's style. Modifying the returned style may not have an effect until {@link #setStyle(ButtonStyle)} is called.
		/// </summary>
		/// <returns>The style.</returns>
		public virtual ButtonStyle GetStyle()
		{
			return style;
		}


		/// <summary>
		/// May be null
		/// </summary>
		/// <returns>The button group.</returns>
		public ButtonGroup GetButtonGroup()
		{
			return _buttonGroup;
		}


		public void SetDisabled(bool disabled) => _isDisabled = disabled;


		public bool GetDisabled() => _isDisabled;


		public override void Draw(Batcher batcher, float parentAlpha)
		{
			Validate();

			if (_isDisabled && style.Disabled != null)
				_background = style.Disabled;
			else if (_mouseDown && style.Down != null)
				_background = style.Down;
			else if (_isChecked && style.Checked != null)
				_background = (style.CheckedOver != null && _mouseOver) ? style.CheckedOver : style.Checked;
			else if (_mouseOver && style.Over != null)
				_background = style.Over;
			else if (style.Up != null) //
				_background = style.Up;
			SetBackground(_background);

			float offsetX = 0, offsetY = 0;
			if (_mouseDown && !_isDisabled)
			{
				offsetX = style.PressedOffsetX;
				offsetY = style.PressedOffsetY;
			}
			else if (_isChecked && !_isDisabled)
			{
				offsetX = style.CheckedOffsetX;
				offsetY = style.CheckedOffsetY;
			}
			else
			{
				offsetX = style.UnpressedOffsetX;
				offsetY = style.UnpressedOffsetY;
			}

			for (var i = 0; i < children.Count; i++)
				children[i].MoveBy(offsetX, offsetY);

			base.Draw(batcher, parentAlpha);

			for (int i = 0; i < children.Count; i++)
				children[i].MoveBy(-offsetX, -offsetY);
		}


		public override string ToString()
		{
			return string.Format("[Button]");
		}
	}


	/// <summary>
	/// The style for a button
	/// </summary>
	public class ButtonStyle
	{
		/** Optional. */
		public IDrawable Up, Down, Over, Checked, CheckedOver, Disabled;

		/** Optional. offsets children (labels for example). */
		public float PressedOffsetX, PressedOffsetY, UnpressedOffsetX, UnpressedOffsetY, CheckedOffsetX, CheckedOffsetY;


		public ButtonStyle()
		{ }


		public ButtonStyle(IDrawable up, IDrawable down, IDrawable over)
		{
			Up = up;
			Down = down;
			Over = over;
		}


		public static ButtonStyle Create(Color upColor, Color downColor, Color overColor)
		{
			return new ButtonStyle
			{
				Up = new PrimitiveDrawable(upColor),
				Down = new PrimitiveDrawable(downColor),
				Over = new PrimitiveDrawable(overColor)
			};
		}


		public ButtonStyle Clone()
		{
			return new ButtonStyle
			{
				Up = Up,
				Down = Down,
				Over = Over,
				Checked = Checked,
				CheckedOver = CheckedOver,
				Disabled = Disabled
			};
		}
	}
}