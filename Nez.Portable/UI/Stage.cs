using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;


namespace Nez.UI
{
	public class Stage
	{
		public static bool Debug;
		public Entity Entity;

		/// <summary>
		/// if true, the rawMousePosition will be used else the scaledMousePosition will be used. If your UI is in screen space
		/// and non-scaled (using the Scene.IFinalRenderDelegate for example) then set this to true so input is not scaled.
		/// </summary>
		public bool IsFullScreen;

		/// <summary>
		/// the button on the gamepad that activates the focused control
		/// </summary>
		public Buttons GamepadActionButton = Buttons.A;

		/// <summary>
		/// if true (default) keyboard arrow keys and the keyboardActionKey will emulate a gamepad
		/// </summary>
		public bool KeyboardEmulatesGamepad = true;

		/// <summary>
		/// the key that activates the focused control
		/// </summary>
		public Keys KeyboardActionKey = Keys.Enter;

		Group root;
		public Camera Camera;
		bool debugAll, debugUnderMouse, debugParentUnderMouse;
		Table.TableDebug debugTableUnderMouse = Table.TableDebug.None;

		Vector2 _lastMousePosition;
		Element _mouseOverElement;
		private Dictionary<int, Element> _touchOverElement = new Dictionary<int, Element>();
		List<Element> _inputFocusListeners = new List<Element>();

		static Keys[] _emptyKeys = new Keys[0];
		IKeyboardListener _keyboardFocusElement;
		Keys[] _lastPressedKeys = _emptyKeys;
		ITimer _keyRepeatTimer;
		float _keyRepeatTime = 0.2f;
		Keys _repeatKey;

		bool _isGamepadFocusEnabled;
		IGamepadFocusable _gamepadFocusElement;


		public Stage()
		{
			root = new Group();
			root.SetStage(this);
		}


		/// <summary>
		/// Adds an element to the root of the stage
		/// </summary>
		public T AddElement<T>(T element) where T : Element
		{
			return root.AddElement(element);
		}


		public void Render(Batcher batcher, Camera camera)
		{
			if (!root.IsVisible())
				return;

			Camera = camera;
			root.Draw(batcher, 1f);

			if (Debug)
			{
				DrawDebug();
				root.DebugRender(batcher);
			}
		}


		void DrawDebug()
		{
			if (debugUnderMouse || debugParentUnderMouse || debugTableUnderMouse != Table.TableDebug.None)
			{
				var mousePos = ScreenToStageCoordinates(Input.RawMousePosition.ToVector2());
				var element = Hit(mousePos);
				if (element == null)
				{
					DisableDebug(root, null);
					return;
				}

				if (debugParentUnderMouse && element.parent != null)
					element = element.parent;

				if (debugTableUnderMouse == Table.TableDebug.None)
				{
					element.SetDebug(true);
				}
				else
				{
					while (element != null)
					{
						if (element is Table)
							break;

						element = element.parent;
					}

					if (element == null)
						return;

					((Table)element).SetTableDebug(debugTableUnderMouse);
				}

				if (debugAll && element is Group)
					((Group)element).DebugAll();

				DisableDebug(root, element);
			}
			else
			{
				if (debugAll)
					root.DebugAll();
			}
		}


		/// <summary>
		/// Disables debug on all elements recursively except the specified element and any children
		/// </summary>
		/// <param name="element">element.</param>
		/// <param name="except">Except.</param>
		void DisableDebug(Element element, Element except)
		{
			if (element == except)
				return;

			element.SetDebug(false);

			if (element is Group)
			{
				var children = ((Group)element).children;
				for (int i = 0, n = children.Count; i < n; i++)
					DisableDebug(children[i], except);
			}
		}


		#region Input

		/// <summary>
		/// gets the appropriate mouse position (scaled vs raw) based on if this isFullScreen and if we have an entity
		/// </summary>
		/// <returns>The mouse position.</returns>
		public Vector2 GetMousePosition()
		{
			return Entity != null && !IsFullScreen ? Input.ScaledMousePosition : Input.RawMousePosition.ToVector2();
		}


		public void Update()
		{
			if (_isGamepadFocusEnabled)
				UpdateGamepadState();
			UpdateKeyboardState();
			UpdateInputMouse();

			if (Input.Touch.IsConnected && Input.Touch.CurrentTouches.Count > 0)
			{
				UpdateInputTouch();
			}
		}


		/// <summary>
		/// Handle mouse input events.
		/// </summary>
		void UpdateInputMouse()
		{
			// consolidate input checks so that we can add touch input easily later
			var currentMousePosition = GetMousePosition();

			var mouseMoved = false;
			if (_lastMousePosition != currentMousePosition)
			{
				mouseMoved = true;
				_lastMousePosition = currentMousePosition;
			}

			var inputPos = ScreenToStageCoordinates(currentMousePosition);

			UpdateInputPoint(inputPos, Input.LeftMouseButtonPressed, Input.RightMouseButtonPressed, Input.LeftMouseButtonReleased, Input.RightMouseButtonReleased,
				mouseMoved, ref _mouseOverElement);
		}


		/// <summary>
		/// Handle all the touch input events.
		/// </summary>
		void UpdateInputTouch()
		{
			foreach (var touch in Input.Touch.CurrentTouches)
			{
				var inputPos = ScreenToStageCoordinates(touch.Position);
				var inputPressed = touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed;
				var inputReleased = touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released ||
									touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Invalid;
				var inputMoved = false;
				Microsoft.Xna.Framework.Input.Touch.TouchLocation prevTouch;
				if (touch.TryGetPreviousLocation(out prevTouch))
				{
					if (Vector2.Distance(touch.Position, prevTouch.Position) >= float.Epsilon)
						inputMoved = true;
				}

				Element lastOver;
				_touchOverElement.TryGetValue(touch.Id, out lastOver);

				if (Entity != null && !IsFullScreen)
					inputPos = Input.ScaledPosition(inputPos);

				UpdateInputPoint(inputPos, inputPressed, inputPressed, inputReleased, inputReleased, inputMoved, ref lastOver);

				if (inputReleased)
					_touchOverElement.Remove(touch.Id);
				else
					_touchOverElement[touch.Id] = lastOver;
			}
		}


		/// <summary>
		/// Process events for Mouse or Touch input.
		/// </summary>
		/// <param name="inputPos">location of cursor</param>
		/// <param name="inputPressed">down this frame</param>
		/// <param name="secondaryInputPressed">down this frame</param>
		/// <param name="inputReleased">up this frame</param>
		/// <param name="secondaryInputReleased">up this frame</param>
		/// <param name="inputMoved">cursor in a different location</param>
		/// <param name="lastOver">last element that the cursor was over, ref is saved here for next update</param>
		void UpdateInputPoint(Vector2 inputPos, bool inputPressed, bool secondaryInputPressed, bool inputReleased, bool secondaryInputReleased, bool inputMoved,
							  ref Element lastOver)
		{
			var over = Hit(inputPos);
			if (over != null)
				HandleMouseWheel(over);

			if (inputPressed)
			{
				UpdatePrimaryInputDown(inputPos, over);
			}

			if (secondaryInputPressed)
			{
				UpdatePrimaryInputDown(inputPos, over);
			}

			if (inputMoved)
			{
				UpdateInputMoved(inputPos, over, lastOver);
			}

			if (inputReleased)
			{
				UpdatePrimaryInputReleased(inputPos);
			}

			if (secondaryInputReleased)
			{
				UpdateSecondaryInputReleased(inputPos);
			}

			lastOver = over;
		}


		/// <summary>
		/// Mouse or touch is down this frame.
		/// </summary>
		/// <param name="inputPos">location of cursor</param>
		/// <param name="over">element under cursor</param>
		void UpdatePrimaryInputDown(Vector2 inputPos, Element over)
		{
			// lose keyboard focus if we click outside of the keyboardFocusElement
			if (_keyboardFocusElement != null && over != _keyboardFocusElement)
				SetKeyboardFocus(null);

			// if we are over an element and the left button was pressed we notify our listener
			if (over is IInputListener)
			{
				var elementLocal = over.StageToLocalCoordinates(inputPos);
				var listener = over as IInputListener;

				// add the listener to be notified for all onMouseDown and onMouseUp events
				if (listener.OnLeftMousePressed(elementLocal))
					_inputFocusListeners.Add(over);
			}
		}


		/// <summary>
		/// Mouse or touch is being moved.
		/// </summary>
		/// <param name="inputPos">location of cursor</param>
		/// <param name="over">element under cursor</param>
		/// <param name="lastOver">element that was previously under the cursor</param>
		void UpdateInputMoved(Vector2 inputPos, Element over, Element lastOver)
		{
			for (var i = _inputFocusListeners.Count - 1; i >= 0; i--)
				((IInputListener)_inputFocusListeners[i]).OnMouseMoved(_inputFocusListeners[i]
					.StageToLocalCoordinates(inputPos));

			if (over != lastOver)
			{
				(over as IInputListener)?.OnMouseEnter();
				(lastOver as IInputListener)?.OnMouseExit();
			}
		}


		/// <summary>
		/// Mouse or touch is being released this frame.
		/// </summary>
		/// <param name="inputPos">location under cursor</param>
		void UpdatePrimaryInputReleased(Vector2 inputPos)
		{
			for (var i = _inputFocusListeners.Count - 1; i >= 0; i--)
				((IInputListener)_inputFocusListeners[i]).OnLeftMouseUp(_inputFocusListeners[i]
					.StageToLocalCoordinates(inputPos));
			_inputFocusListeners.Clear();
		}

		/// <summary>
		/// Right mouse click or touch is being released this frame.
		/// </summary>
		/// <param name="inputPos">location under cursor</param>
		void UpdateSecondaryInputReleased(Vector2 inputPos)
		{
			for (var i = _inputFocusListeners.Count - 1; i >= 0; i--)
				((IInputListener)_inputFocusListeners[i]).OnRightMouseUp(_inputFocusListeners[i]
					.StageToLocalCoordinates(inputPos));
			_inputFocusListeners.Clear();
		}


		/// <summary>
		/// bubbles the onMouseScrolled event from mouseOverElement to all parents until one of them handles it
		/// </summary>
		/// <returns>The mouse wheel.</returns>
		/// <param name="mouseOverElement">Mouse over element.</param>
		void HandleMouseWheel(Element mouseOverElement)
		{
			// bail out if we have no mouse wheel motion
			if (Input.MouseWheelDelta == 0)
				return;

			// check the deepest Element first then check all of its parents that are IInputListeners
			var listener = mouseOverElement as IInputListener;
			if (listener != null && listener.OnMouseScrolled(Input.MouseWheelDelta))
				return;

			while (mouseOverElement.parent != null)
			{
				mouseOverElement = mouseOverElement.parent;
				listener = mouseOverElement as IInputListener;
				if (listener != null && listener.OnMouseScrolled(Input.MouseWheelDelta))
					return;
			}
		}


		void UpdateKeyboardState()
		{
			// dont process if we have no focused text element
			if (_keyboardFocusElement == null)
				return;

			var currentPressedKeys = Input.CurrentKeyboardState.GetPressedKeys();

			// keys down
			for (var i = 0; i < currentPressedKeys.Length; i++)
			{
				var key = currentPressedKeys[i];
				if (!_lastPressedKeys.Contains(key))
				{
					_keyboardFocusElement.KeyDown(key);

					// if alt isnt pressed we will call keyPressed
					if (!InputUtils.IsAltDown())
					{
						var c = key.GetChar();
						if (c.HasValue)
						{
							ClearKeyRepeatTimer();
							_keyboardFocusElement.KeyPressed(key, c.Value);

							// if we dont have a control key pressed setup a repeat timer for the key
							if (!InputUtils.IsControlDown())
							{
								_repeatKey = key;
								_keyRepeatTimer = Core.Schedule(_keyRepeatTime, true, this, t =>
								{
									var self = t.Context as Stage;
									if (self._keyboardFocusElement != null)
										self._keyboardFocusElement.KeyPressed(_repeatKey, _repeatKey.GetChar().Value);
								});
							}
						}
					}
				}
			}

			// keys released
			for (var i = 0; i < _lastPressedKeys.Length; i++)
			{
				var key = _lastPressedKeys[i];
				if (!currentPressedKeys.Contains(key))
				{
					_keyboardFocusElement.KeyReleased(key);
					ClearKeyRepeatTimer();
				}
			}

			_lastPressedKeys = currentPressedKeys;
		}


		void UpdateGamepadState()
		{
			if (_gamepadFocusElement != null)
			{
				if (Input.GamePads[0].IsButtonPressed(GamepadActionButton) ||
					(KeyboardEmulatesGamepad && Input.IsKeyPressed(KeyboardActionKey)))
					_gamepadFocusElement.OnActionButtonPressed();
				else if (Input.GamePads[0].IsButtonReleased(GamepadActionButton) ||
						 (KeyboardEmulatesGamepad && Input.IsKeyReleased(KeyboardActionKey)))
					_gamepadFocusElement.OnActionButtonReleased();
			}

			IGamepadFocusable nextElement = null;
			var direction = Direction.None;
			if (Input.GamePads[0].DpadLeftPressed || Input.GamePads[0].IsLeftStickLeftPressed() ||
				(KeyboardEmulatesGamepad && Input.IsKeyPressed(Keys.Left)))
				direction = Direction.Left;
			else if (Input.GamePads[0].DpadRightPressed || Input.GamePads[0].IsLeftStickRightPressed() ||
					 (KeyboardEmulatesGamepad && Input.IsKeyPressed(Keys.Right)))
				direction = Direction.Right;
			else if (Input.GamePads[0].DpadUpPressed || Input.GamePads[0].IsLeftStickUpPressed() ||
					 (KeyboardEmulatesGamepad && Input.IsKeyPressed(Keys.Up)))
				direction = Direction.Up;
			else if (Input.GamePads[0].DpadDownPressed || Input.GamePads[0].IsLeftStickDownPressed() ||
					 (KeyboardEmulatesGamepad && Input.IsKeyPressed(Keys.Down)))
				direction = Direction.Down;

			// make sure we have a valid direction
			if (direction != Direction.None)
			{
				nextElement = FindNextGamepadFocusable(_gamepadFocusElement, direction);
				if (nextElement == null)
				{
					// we have no next Element so if the current Element has explicit focuasable control send along the unhandled direction
					if (_gamepadFocusElement.ShouldUseExplicitFocusableControl)
						_gamepadFocusElement.OnUnhandledDirectionPressed(direction);
				}
				else
				{
					SetGamepadFocusElement(nextElement);
				}
			}
		}


		/// <summary>
		/// Removes the listener from being notified for all touchDragged and touchUp events for the specified pointer and button. Note
		/// the listener may never receive a touchUp event if this method is used.
		/// </summary>
		public void RemoveInputFocusListener(Element element)
		{
			_inputFocusListeners.Remove(element);
		}

		#endregion


		/// <summary>
		/// stops and nulls the keyRepeatTimer if it is running
		/// </summary>
		void ClearKeyRepeatTimer()
		{
			if (_keyRepeatTimer != null)
			{
				_keyRepeatTimer.Stop();
				_keyRepeatTimer = null;
			}
		}


		/// <summary>
		/// this should be called when the Component is removed to ensure all objects are freed
		/// </summary>
		public void Dispose()
		{
			root.Clear();
		}


		#region Getters/Setters

		public List<Element> GetElements()
		{
			return root.children;
		}


		/// <summary>
		/// Returns the root group which holds all elements in the stageCoords
		/// </summary>
		/// <returns>The root.</returns>
		public Group GetRoot()
		{
			return root;
		}


		/// <summary>
		/// The Stages world width
		/// </summary>
		/// <returns>The width.</returns>
		public float GetWidth()
		{
			if (Entity != null && !IsFullScreen)
				return Entity.Scene.SceneRenderTargetSize.X;

			return Screen.Width;
		}


		/// <summary>
		/// The Stages world height
		/// </summary>
		/// <returns>The height.</returns>
		public float GetHeight()
		{
			if (Entity != null && !IsFullScreen)
				return Entity.Scene.SceneRenderTargetSize.Y;

			return Screen.Height;
		}


		public bool GetDebugAll()
		{
			return debugAll;
		}


		/// <summary>
		/// If true, debug lines are shown for all elements
		/// </summary>
		/// <param name="debugAll">If set to <c>true</c> debug all.</param>
		public void SetDebugAll(bool debugAll)
		{
			if (this.debugAll == debugAll)
				return;

			this.debugAll = debugAll;
			if (debugAll)
				Debug = true;
			else
				root.SetDebug(false, true);
		}


		/// <summary>
		/// If true, debug is enabled only for the element under the mouse. Can be combined with {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugUnderMouse">If set to <c>true</c> debug under mouse.</param>
		public void SetDebugUnderMouse(bool debugUnderMouse)
		{
			if (this.debugUnderMouse == debugUnderMouse)
				return;

			this.debugUnderMouse = debugUnderMouse;
			if (debugUnderMouse)
				Debug = true;
			else
				root.SetDebug(false, true);
		}


		/// <summary>
		/// If true, debug is enabled only for the parent of the element under the mouse. Can be combined with {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugParentUnderMouse">If set to <c>true</c> debug parent under mouse.</param>
		public void SetDebugParentUnderMouse(bool debugParentUnderMouse)
		{
			if (this.debugParentUnderMouse == debugParentUnderMouse)
				return;

			this.debugParentUnderMouse = debugParentUnderMouse;
			if (debugParentUnderMouse)
				Debug = true;
			else
				root.SetDebug(false, true);
		}


		/// <summary>
		/// If not {@link TableDebug#none}, debug is enabled only for the first ascendant of the element under the mouse that is a table. Can
		/// be combined with {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugTableUnderMouse">Debug table under mouse.</param>
		public void SetDebugTableUnderMouse(Table.TableDebug debugTableUnderMouse)
		{
			if (this.debugTableUnderMouse == debugTableUnderMouse)
				return;

			this.debugTableUnderMouse = debugTableUnderMouse;
			if (debugTableUnderMouse != Table.TableDebug.None)
				Debug = true;
			else
				root.SetDebug(false, true);
		}


		/// <summary>
		/// If true, debug is enabled only for the first ascendant of the element under the mouse that is a table. Can be combined with
		/// {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugTableUnderMouse">If set to <c>true</c> debug table under mouse.</param>
		public void SetDebugTableUnderMouse(bool debugTableUnderMouse)
		{
			SetDebugTableUnderMouse(debugTableUnderMouse ? Table.TableDebug.All : Table.TableDebug.None);
		}


		/// <summary>
		/// Removes the touch, keyboard, and scroll focused elements
		/// </summary>
		public void UnfocusAll()
		{
			SetKeyboardFocus(null);
		}


		/// <summary>
		/// Sets the element that will receive key events
		/// </summary>
		/// <param name="element">element.</param>
		public void SetKeyboardFocus(IKeyboardListener element)
		{
			// clean up if we are removing focus
			if (element == null)
			{
				_lastPressedKeys = _emptyKeys;
				ClearKeyRepeatTimer();
			}

			if (_keyboardFocusElement == element)
				return;

			var oldKeyboardFocus = _keyboardFocusElement;
			if (oldKeyboardFocus != null)
				oldKeyboardFocus.LostFocus();

			_keyboardFocusElement = element;
			if (_keyboardFocusElement != null)
				_keyboardFocusElement.GainedFocus();
		}


		/// <summary>
		/// sets the gamepad focus element and also turns on gamepad focus for this Stage. For gamepad focus to work you must set an initially
		/// focused element.
		/// </summary>
		/// <param name="focusable">Focusable.</param>
		public void SetGamepadFocusElement(IGamepadFocusable focusable)
		{
			_isGamepadFocusEnabled = true;

			if (_gamepadFocusElement == focusable)
				return;

			if (focusable != null)
				focusable.OnFocused();

			if (_gamepadFocusElement != null)
				_gamepadFocusElement.OnUnfocused();
			_gamepadFocusElement = focusable;
		}
		
		/// <summary>
		/// unset the gamepad focus element and turns off gamepad focus for this Stage.
		/// </summary>
		public void DisableGamepadFocus()
		{
    		_gamepadFocusElement = null;
    		_isGamepadFocusEnabled = false;  
		}

		/// <summary>
		/// Gets the element that will receive key events.
		/// </summary>
		/// <returns>The keyboard focus.</returns>
		public IKeyboardListener GetKeyboardFocus()
		{
			return _keyboardFocusElement;
		}

		#endregion


		public Element Hit(Vector2 point)
		{
			point = root.ParentToLocalCoordinates(point);
			return root.Hit(point);
		}


		/// <summary>
		/// Transforms the screen coordinates to stage coordinates
		/// </summary>
		/// <returns>The to stage coordinates.</returns>
		/// <param name="screenCoords">Screen coords.</param>
		public Vector2 ScreenToStageCoordinates(Vector2 screenCoords)
		{
			if (Camera == null)
				return screenCoords;

			return Camera.ScreenToWorldPoint(screenCoords);
		}


		/// <summary>
		/// Transforms the stage coordinates to screen coordinates
		/// </summary>
		/// <returns>The to screen coordinates.</returns>
		/// <param name="stageCoords">Stage coords.</param>
		public Vector2 StageToScreenCoordinates(Vector2 stageCoords)
		{
			if (Camera == null)
				return stageCoords;

			return Camera.WorldToScreenPoint(stageCoords);
		}


		IGamepadFocusable FindNextGamepadFocusable(IGamepadFocusable relativeToFocusable, Direction direction)
		{
			// first, we check to see if the IGamepadFocusable has hard-wired control.
			if (relativeToFocusable.ShouldUseExplicitFocusableControl)
			{
				switch (direction)
				{
					case Direction.Up:
						return relativeToFocusable.GamepadUpElement;
					case Direction.Down:
						return relativeToFocusable.GamepadDownElement;
					case Direction.Left:
						return relativeToFocusable.GamepadLeftElement;
					case Direction.Right:
						return relativeToFocusable.GamepadRightElement;
				}
			}

			IGamepadFocusable nextFocusable = null;
			var distanceToNextButton = float.MaxValue;

			var focusableEle = relativeToFocusable as Element;
			var currentCoords = focusableEle.GetParent()
				.LocalToStageCoordinates(new Vector2(focusableEle.GetX(), focusableEle.GetY()));
			var buttons = FindAllElementsOfType<IGamepadFocusable>();
			for (var i = 0; i < buttons.Count; i++)
			{
				if (buttons[i] == relativeToFocusable)
					continue;

				// filter out buttons that are not in the disired direction
				var element = buttons[i] as Element;
				var buttonCoords = element.GetParent()
					.LocalToStageCoordinates(new Vector2(element.GetX(), element.GetY()));
				var isDirectionMatch = false;
				switch (direction)
				{
					case Direction.Up:
						if (buttonCoords.Y < currentCoords.Y)
							isDirectionMatch = true;
						break;
					case Direction.Down:
						if (buttonCoords.Y > currentCoords.Y)
							isDirectionMatch = true;
						break;
					case Direction.Left:
						if (buttonCoords.X < currentCoords.X)
							isDirectionMatch = true;
						break;
					case Direction.Right:
						if (buttonCoords.X > currentCoords.X)
							isDirectionMatch = true;
						break;
				}

				// keep only the closest button if we have a match
				if (isDirectionMatch)
				{
					if (nextFocusable == null)
					{
						nextFocusable = buttons[i];
						distanceToNextButton = Vector2.DistanceSquared(currentCoords, buttonCoords);
					}
					else
					{
						var distance = Vector2.DistanceSquared(currentCoords, buttonCoords);
						if (distance < distanceToNextButton)
						{
							nextFocusable = buttons[i];
							distanceToNextButton = distance;
						}
					}
				}
			}

			return nextFocusable;
		}


		/// <summary>
		/// finds all the Elements of type T in the Stage
		/// </summary>
		/// <returns>The all elements of type.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> FindAllElementsOfType<T>() where T : class
		{
			var eles = new List<T>();
			FindAllElementsOfType(root.children, eles);
			return eles;
		}


		void FindAllElementsOfType<T>(List<Element> elements, List<T> foundElements) where T : class
		{
			for (var i = 0; i < elements.Count; i++)
			{
				if (elements[i] is T)
					foundElements.Add(elements[i] as T);
				else if (elements[i] is Group)
					FindAllElementsOfType(((Group)elements[i]).children, foundElements);
			}
		}
	}
}
