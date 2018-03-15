using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace Nez.UI
{
	public class Stage
	{
		public static bool debug;
		public Entity entity;

		/// <summary>
		/// if true, the rawMousePosition will be used else the scaledMousePosition will be used. If your UI is in screen space
		/// and non-scaled (using the Scene.IFinalRenderDelegate for example) then set this to true so input is not scaled.
		/// </summary>
		public bool isFullScreen;

		/// <summary>
		/// the button on the gamepad that activates the focused control
		/// </summary>
		public Buttons gamepadActionButton = Buttons.A;

		/// <summary>
		/// if true (default) keyboard arrow keys and the keyboardActionKey will emulate a gamepad
		/// </summary>
		public bool keyboardEmulatesGamepad = true;

		/// <summary>
		/// the key that activates the focused control
		/// </summary>
		public Keys keyboardActionKey = Keys.Enter;

		Group root;
		public Camera camera;
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
			root.setStage( this );
		}


		/// <summary>
		/// Adds an element to the root of the stage
		/// </summary>
		/// <param name="element">element.</param>
		public T addElement<T>( T element ) where T : Element
		{
			return root.addElement( element );
		}


		public void render( Graphics graphics, Camera camera )
		{
			if( !root.isVisible() )
				return;

			this.camera = camera;
			root.draw( graphics, 1f );

			if( debug )
			{
				drawDebug();
				root.debugRender( graphics );
			}
		}


		void drawDebug()
		{
			if( debugUnderMouse || debugParentUnderMouse || debugTableUnderMouse != Table.TableDebug.None )
			{
				var mousePos = screenToStageCoordinates( Input.rawMousePosition.ToVector2() );
				var element = hit( mousePos );
				if( element == null )
				{
					disableDebug( root, null );
					return;
				}

				if( debugParentUnderMouse && element.parent != null )
					element = element.parent;

				if( debugTableUnderMouse == Table.TableDebug.None )
				{
					element.setDebug( true );
				}
				else
				{
					while( element != null )
					{
						if( element is Table )
							break;
						element = element.parent;
					}

					if( element == null )
						return;
					
					( (Table)element ).tableDebug( debugTableUnderMouse );
				}

				if( debugAll && element is Group )
					( (Group)element ).debugAll();

				disableDebug( root, element );
			}
			else
			{
				if( debugAll )
					root.debugAll();
			}
		}


		/// <summary>
		/// Disables debug on all elements recursively except the specified element and any children
		/// </summary>
		/// <param name="element">element.</param>
		/// <param name="except">Except.</param>
		void disableDebug( Element element, Element except )
		{
			if( element == except )
				return;
			element.setDebug( false );

			if( element is Group )
			{
				var children = ( (Group)element ).children;
				for( int i = 0, n = children.Count; i < n; i++ )
					disableDebug( children[i], except );
			}
		}


		#region Input

		/// <summary>
		/// gets the appropriate mouse position (scaled vs raw) based on if this isFullScreen and if we have an entity
		/// </summary>
		/// <returns>The mouse position.</returns>
		public Vector2 getMousePosition()
		{
			return entity != null && !isFullScreen ? Input.scaledMousePosition : Input.rawMousePosition.ToVector2();
		}


		public void update()
		{
			if( _isGamepadFocusEnabled )
				updateGamepadState();
			updateKeyboardState();
			updateInputMouse();

			#if !FNA
			if( Input.touch.isConnected && Input.touch.currentTouches.Count > 0 )
			{
				updateInputTouch();
			}
			#endif
		}


		/// <summary>
		/// Handle mouse input events.
		/// </summary>
		void updateInputMouse()
		{
			// consolidate input checks so that we can add touch input easily later
			var currentMousePosition = getMousePosition();

			var mouseMoved = false;
			if( _lastMousePosition != currentMousePosition )
			{
				mouseMoved = true;
				_lastMousePosition = currentMousePosition;
			}

			var inputPos = screenToStageCoordinates( currentMousePosition );

			updateInputPoint( inputPos, Input.leftMouseButtonPressed, Input.leftMouseButtonReleased,
				mouseMoved, ref _mouseOverElement );
		}


		/// <summary>
		/// Handle all the touch input events.
		/// </summary>
		void updateInputTouch()
		{
			#if !FNA
			foreach( var touch in Input.touch.currentTouches )
			{
				var inputPos = screenToStageCoordinates( touch.Position );
				var inputPressed = touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Pressed;
				var inputReleased = touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Released || touch.State == Microsoft.Xna.Framework.Input.Touch.TouchLocationState.Invalid;
				var inputMoved = false;
				Microsoft.Xna.Framework.Input.Touch.TouchLocation prevTouch;
				if( touch.TryGetPreviousLocation( out prevTouch ) )
				{
					if( Vector2.Distance( touch.Position, prevTouch.Position ) >= float.Epsilon )
						inputMoved = true;
				}
				Element lastOver;
				_touchOverElement.TryGetValue( touch.Id, out lastOver );

				if ( entity != null && !isFullScreen )
					inputPos = Input.scaledPosition( inputPos );

				updateInputPoint( inputPos, inputPressed, inputReleased, inputMoved, ref lastOver );

				if ( inputReleased )
					_touchOverElement.Remove( touch.Id );
				else
					_touchOverElement[touch.Id] = lastOver;
			}
			#endif
		}


		/// <summary>
		/// Process events for Mouse or Touch input.
		/// </summary>
		/// <param name="inputPos">location of cursor</param>
		/// <param name="inputPressed">down this frame</param>
		/// <param name="inputReleased">up this frame</param>
		/// <param name="inputMoved">cursor in a different location</param>
		/// <param name="lastOver">last element that the cursor was over, ref is saved here for next update</param>
		void updateInputPoint( Vector2 inputPos, bool inputPressed, bool inputReleased, bool inputMoved, ref Element lastOver )
		{
			var over = hit( inputPos );
			if( over != null )
				handleMouseWheel( over );

			if( inputPressed )
			{
				updateInputDown( inputPos, over );
			}
			if( inputMoved )
			{
				updateInputMoved( inputPos, over, lastOver );
			}
			if( inputReleased )
			{
				updateInputReleased( inputPos );
			}

			lastOver = over;
		}


		/// <summary>
		/// Mouse or touch is down this frame.
		/// </summary>
		/// <param name="inputPos">location of cursor</param>
		/// <param name="over">element under cursor</param>
		void updateInputDown( Vector2 inputPos, Element over )
		{
			// lose keyboard focus if we click outside of the keyboardFocusElement
			if( _keyboardFocusElement != null && over != _keyboardFocusElement )
				setKeyboardFocus( null );

			// if we are over an element and the left button was pressed we notify our listener
			if( over is IInputListener )
			{
				var elementLocal = over.stageToLocalCoordinates( inputPos );
				var listener = over as IInputListener;
				// add the listener to be notified for all onMouseDown and onMouseUp events
				if( listener.onMousePressed( elementLocal ) )
					_inputFocusListeners.Add( over );
			}
		}


		/// <summary>
		/// Mouse or touch is being moved.
		/// </summary>
		/// <param name="inputPos">location of cursor</param>
		/// <param name="over">element under cursor</param>
		/// <param name="lastOver">element that was previously under the cursor</param>
		void updateInputMoved( Vector2 inputPos, Element over, Element lastOver )
		{
			for( var i = _inputFocusListeners.Count - 1; i >= 0; i-- )
				( (IInputListener)_inputFocusListeners[i] ).onMouseMoved( _inputFocusListeners[i].stageToLocalCoordinates( inputPos ) );

			if( over != lastOver )
			{
				( over as IInputListener )?.onMouseEnter();
				( lastOver as IInputListener )?.onMouseExit();
			}
		}


		/// <summary>
		/// Mouse or touch is being released this frame.
		/// </summary>
		/// <param name="inputPos">location under cursor</param>
		void updateInputReleased( Vector2 inputPos )
		{
			for( var i = _inputFocusListeners.Count - 1; i >= 0; i-- )
				( (IInputListener)_inputFocusListeners[i] ).onMouseUp( _inputFocusListeners[i].stageToLocalCoordinates( inputPos ) );
			_inputFocusListeners.Clear();
		}


		/// <summary>
		/// bubbles the onMouseScrolled event from mouseOverElement to all parents until one of them handles it
		/// </summary>
		/// <returns>The mouse wheel.</returns>
		/// <param name="mouseOverElement">Mouse over element.</param>
		void handleMouseWheel( Element mouseOverElement )
		{
			// bail out if we have no mouse wheel motion
			if( Input.mouseWheelDelta == 0 )
				return;

			// check the deepest Element first then check all of its parents that are IInputListeners
			var listener = mouseOverElement as IInputListener;
			if( listener != null && listener.onMouseScrolled( Input.mouseWheelDelta ) )
				return;
			
			while( mouseOverElement.parent != null )
			{
				mouseOverElement = mouseOverElement.parent;
				listener = mouseOverElement as IInputListener;
				if( listener != null && listener.onMouseScrolled( Input.mouseWheelDelta ) )
					return;
			}
		}


		void updateKeyboardState()
		{
			// dont process if we have no focused text element
			if( _keyboardFocusElement == null )
				return;
			
			var currentPressedKeys = Input.currentKeyboardState.GetPressedKeys();

			// keys down
			for( var i = 0; i < currentPressedKeys.Length; i++ )
			{
				var key = currentPressedKeys[i];
				if( !_lastPressedKeys.contains( key ) )
				{
					_keyboardFocusElement.keyDown( key );

					// if alt isnt pressed we will call keyPressed
					if( !InputUtils.isAltDown() )
					{
						var c = key.getChar();
						if( c.HasValue )
						{
							clearKeyRepeatTimer();
							_keyboardFocusElement.keyPressed( key, c.Value );

							// if we dont have a control key pressed setup a repeat timer for the key
							if( !InputUtils.isControlDown() )
							{
								_repeatKey = key;
								_keyRepeatTimer = Core.schedule( _keyRepeatTime, true, this, t =>
								{
									var self = t.context as Stage;
									if( self._keyboardFocusElement != null )
										self._keyboardFocusElement.keyPressed( _repeatKey, _repeatKey.getChar().Value );
								} );
							}
						}
					}
				}
			}

			// keys released
			for( var i = 0; i < _lastPressedKeys.Length; i++ )
			{
				var key = _lastPressedKeys[i];
				if( !currentPressedKeys.contains( key ) )
				{
					_keyboardFocusElement.keyReleased( key );
					clearKeyRepeatTimer();
				}
			}

			_lastPressedKeys = currentPressedKeys;
		}


		void updateGamepadState()
		{
			if( _gamepadFocusElement != null )
			{
				if( Input.gamePads[0].isButtonPressed( gamepadActionButton ) || ( keyboardEmulatesGamepad && Input.isKeyPressed( keyboardActionKey ) ) )
					_gamepadFocusElement.onActionButtonPressed();
				else if( Input.gamePads[0].isButtonReleased( gamepadActionButton ) || ( keyboardEmulatesGamepad && Input.isKeyReleased( keyboardActionKey ) ) )
					_gamepadFocusElement.onActionButtonReleased();
			}
			
			IGamepadFocusable nextElement = null;
			var direction = Direction.None;
			if( Input.gamePads[0].DpadLeftPressed || Input.gamePads[0].isLeftStickLeftPressed() || ( keyboardEmulatesGamepad && Input.isKeyPressed( Keys.Left ) ) )
				direction = Direction.Left;
			else if( Input.gamePads[0].DpadRightPressed || Input.gamePads[0].isLeftStickRightPressed() || ( keyboardEmulatesGamepad && Input.isKeyPressed( Keys.Right ) ) )
				direction = Direction.Right;
			else if( Input.gamePads[0].DpadUpPressed || Input.gamePads[0].isLeftStickUpPressed() || ( keyboardEmulatesGamepad && Input.isKeyPressed( Keys.Up ) ) )
				direction = Direction.Up;
			else if( Input.gamePads[0].DpadDownPressed || Input.gamePads[0].isLeftStickDownPressed() || ( keyboardEmulatesGamepad && Input.isKeyPressed( Keys.Down ) ) )
				direction = Direction.Down;

			// make sure we have a valid direction
			if( direction != Direction.None )
			{
				nextElement = findNextGamepadFocusable( _gamepadFocusElement, direction );
				if( nextElement == null )
				{
					// we have no next Element so if the current Element has explicit focuasable control send along the unhandled direction
					if( _gamepadFocusElement.shouldUseExplicitFocusableControl )
						_gamepadFocusElement.onUnhandledDirectionPressed( direction );
				}
				else
				{
					setGamepadFocusElement( nextElement );
				}
			}
		}


		/// <summary>
		/// Removes the listener from being notified for all touchDragged and touchUp events for the specified pointer and button. Note
		/// the listener may never receive a touchUp event if this method is used.
		/// </summary>
		public void removeInputFocusListener( Element element )
		{
			_inputFocusListeners.Remove( element );
		}

		#endregion


		/// <summary>
		/// stops and nulls the keyRepeatTimer if it is running
		/// </summary>
		void clearKeyRepeatTimer()
		{
			if( _keyRepeatTimer != null )
			{
				_keyRepeatTimer.stop();
				_keyRepeatTimer = null;
			}
		}


		/// <summary>
		/// this should be called when the Component is removed to ensure all objects are freed
		/// </summary>
		public void dispose()
		{
			root.clear();
		}


		#region Getters/Setters

		public List<Element> getElements()
		{
			return root.children;
		}
		

		/// <summary>
		/// Returns the root group which holds all elements in the stageCoords
		/// </summary>
		/// <returns>The root.</returns>
		public Group getRoot()
		{
			return root;
		}


		/// <summary>
		/// The Stages world width
		/// </summary>
		/// <returns>The width.</returns>
		public float getWidth()
		{
			if( entity != null && !isFullScreen )
				return entity.scene.sceneRenderTargetSize.X;
			return Screen.width;
		}


		/// <summary>
		/// The Stages world height
		/// </summary>
		/// <returns>The height.</returns>
		public float getHeight()
		{
			if( entity != null && !isFullScreen )
				return entity.scene.sceneRenderTargetSize.Y;
			return Screen.height;
		}


		public bool getDebugAll()
		{
			return debugAll;
		}


		/// <summary>
		/// If true, debug lines are shown for all elements
		/// </summary>
		/// <param name="debugAll">If set to <c>true</c> debug all.</param>
		public void setDebugAll( bool debugAll )
		{
			if( this.debugAll == debugAll )
				return;
			
			this.debugAll = debugAll;
			if( debugAll )
				debug = true;
			else
				root.setDebug( false, true );
		}


		/// <summary>
		/// If true, debug is enabled only for the element under the mouse. Can be combined with {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugUnderMouse">If set to <c>true</c> debug under mouse.</param>
		public void setDebugUnderMouse( bool debugUnderMouse )
		{
			if( this.debugUnderMouse == debugUnderMouse )
				return;
			
			this.debugUnderMouse = debugUnderMouse;
			if( debugUnderMouse )
				debug = true;
			else
				root.setDebug( false, true );
		}


		/// <summary>
		/// If true, debug is enabled only for the parent of the element under the mouse. Can be combined with {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugParentUnderMouse">If set to <c>true</c> debug parent under mouse.</param>
		public void setDebugParentUnderMouse( bool debugParentUnderMouse )
		{
			if( this.debugParentUnderMouse == debugParentUnderMouse )
				return;
			
			this.debugParentUnderMouse = debugParentUnderMouse;
			if( debugParentUnderMouse )
				debug = true;
			else
				root.setDebug( false, true );
		}

	
		/// <summary>
		/// If not {@link TableDebug#none}, debug is enabled only for the first ascendant of the element under the mouse that is a table. Can
		/// be combined with {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugTableUnderMouse">Debug table under mouse.</param>
		public void setDebugTableUnderMouse( Table.TableDebug debugTableUnderMouse )
		{
			if( this.debugTableUnderMouse == debugTableUnderMouse )
				return;
			
			this.debugTableUnderMouse = debugTableUnderMouse;
			if( debugTableUnderMouse != Table.TableDebug.None )
				debug = true;
			else
				root.setDebug( false, true );
		}


		/// <summary>
		/// If true, debug is enabled only for the first ascendant of the element under the mouse that is a table. Can be combined with
		/// {@link #setDebugAll(bool)}
		/// </summary>
		/// <param name="debugTableUnderMouse">If set to <c>true</c> debug table under mouse.</param>
		public void setDebugTableUnderMouse( bool debugTableUnderMouse )
		{
			setDebugTableUnderMouse( debugTableUnderMouse ? Table.TableDebug.All : Table.TableDebug.None );
		}


		/// <summary>
		/// Removes the touch, keyboard, and scroll focused elements
		/// </summary>
		public void unfocusAll()
		{
			setKeyboardFocus( null );
		}

	
		/// <summary>
		/// Sets the element that will receive key events
		/// </summary>
		/// <param name="element">element.</param>
		public void setKeyboardFocus( IKeyboardListener element )
		{
			// clean up if we are removing focus
			if( element == null )
			{
				_lastPressedKeys = _emptyKeys;
				clearKeyRepeatTimer();
			}

			if( _keyboardFocusElement == element )
				return;

			var oldKeyboardFocus = _keyboardFocusElement;
			if( oldKeyboardFocus != null )
				oldKeyboardFocus.lostFocus();

			_keyboardFocusElement = element;
			if( _keyboardFocusElement != null )
				_keyboardFocusElement.gainedFocus();
		}


		/// <summary>
		/// sets the gamepad focus element and also turns on gamepad focus for this Stage. For gamepad focus to work you must set an initially
		/// focused element.
		/// </summary>
		/// <param name="focusable">Focusable.</param>
		public void setGamepadFocusElement( IGamepadFocusable focusable )
		{
			_isGamepadFocusEnabled = true;

			if( focusable != null )
				focusable.onFocused();

			if( _gamepadFocusElement != null )
				_gamepadFocusElement.onUnfocused();
			_gamepadFocusElement = focusable;
		}


		/// <summary>
		/// Gets the element that will receive key events.
		/// </summary>
		/// <returns>The keyboard focus.</returns>
		public IKeyboardListener getKeyboardFocus()
		{
			return _keyboardFocusElement;
		}

		#endregion


		public Element hit( Vector2 point )
		{
			point = root.parentToLocalCoordinates( point );
			return root.hit( point );
		}


		/// <summary>
		/// Transforms the screen coordinates to stage coordinates
		/// </summary>
		/// <returns>The to stage coordinates.</returns>
		/// <param name="screenCoords">Screen coords.</param>
		public Vector2 screenToStageCoordinates( Vector2 screenCoords )
		{
			if( camera == null )
				return screenCoords;
			return camera.screenToWorldPoint( screenCoords );
		}


		/// <summary>
		/// Transforms the stage coordinates to screen coordinates
		/// </summary>
		/// <returns>The to screen coordinates.</returns>
		/// <param name="stageCoords">Stage coords.</param>
		public Vector2 stageToScreenCoordinates( Vector2 stageCoords )
		{
			if( camera == null )
				return stageCoords;
			return camera.worldToScreenPoint( stageCoords );
		}


		IGamepadFocusable findNextGamepadFocusable( IGamepadFocusable relativeToFocusable, Direction direction )
		{
			// first, we check to see if the IGamepadFocusable has hard-wired control. 
			if( relativeToFocusable.shouldUseExplicitFocusableControl )
			{
				switch( direction )
				{
					case Direction.Up:
						return relativeToFocusable.gamepadUpElement;
					case Direction.Down:
						return relativeToFocusable.gamepadDownElement;
					case Direction.Left:
						return relativeToFocusable.gamepadLeftElement;
					case Direction.Right:
						return relativeToFocusable.gamepadRightElement;
				}
			}

			IGamepadFocusable nextFocusable = null;
			var distanceToNextButton = float.MaxValue;

			var focusableEle = relativeToFocusable as Element;
			var currentCoords = focusableEle.getParent().localToStageCoordinates( new Vector2( focusableEle.getX(), focusableEle.getY() ) );
			var buttons = findAllElementsOfType<IGamepadFocusable>();
			for( var i = 0; i < buttons.Count; i++ )
			{
				if( buttons[i] == relativeToFocusable )
					continue;
				
				// filter out buttons that are not in the disired direction
				var element = buttons[i] as Element;
				var buttonCoords = element.getParent().localToStageCoordinates( new Vector2( element.getX(), element.getY() ) );
				var isDirectionMatch = false;
				switch( direction )
				{
					case Direction.Up:
						if( buttonCoords.Y < currentCoords.Y )
							isDirectionMatch = true;
						break;
					case Direction.Down:
						if( buttonCoords.Y > currentCoords.Y )
							isDirectionMatch = true;
						break;
					case Direction.Left:
						if( buttonCoords.X < currentCoords.X )
							isDirectionMatch = true;
						break;
					case Direction.Right:
						if( buttonCoords.X > currentCoords.X )
							isDirectionMatch = true;
						break;
				}

				// keep only the closest button if we have a match
				if( isDirectionMatch )
				{
					if( nextFocusable == null )
					{
						nextFocusable = buttons[i];
						distanceToNextButton = Vector2.DistanceSquared( currentCoords, buttonCoords );
					}
					else
					{
						var distance = Vector2.DistanceSquared( currentCoords, buttonCoords );
						if( distance < distanceToNextButton )
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
		public List<T> findAllElementsOfType<T>() where T : class
		{
			var eles = new List<T>();
			findAllElementsOfType<T>( root.children, eles );
			return eles;
		}


		void findAllElementsOfType<T>( List<Element> elements, List<T> foundElements ) where T : class
		{
			for( var i = 0; i < elements.Count; i++ )
			{
				if( elements[i] is T )
					foundElements.Add( elements[i] as T );
				else if( elements[i] is Group )
					findAllElementsOfType<T>( ((Group)elements[i]).children, foundElements );
			}
		}

	}
}

