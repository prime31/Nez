using System;
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
		/// if true, the rawMousePosition will be used else the scaledMousePosition will be used. If your UI is in screen space (using a 
		/// ScreenSpaceRenderer for example) then set this to true so input is not scaled.
		/// </summary>
		public bool isFullScreen;

		Group root;
		Camera _camera;
		bool debugAll, debugUnderMouse, debugParentUnderMouse;
		Table.TableDebug debugTableUnderMouse = Table.TableDebug.None;

		Vector2 _lastMousePosition;
		Element _mouseOverElement;
		List<Element> _inputFocusListeners = new List<Element>();

		static Keys[] _emptyKeys = new Keys[0];
		IKeyboardListener _keyboardFocusElement;
		Keys[] _lastPressedKeys = _emptyKeys;
		ITimer _keyRepeatTimer;
		float _keyRepeatTime = 0.2f;
		Keys _repeatKey;


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

			_camera = camera;
			root.draw( graphics, 1 );

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

		public void update()
		{
			updateKeyboardState();

			var currentMousePosition = entity != null && !isFullScreen ? Input.scaledMousePosition : Input.rawMousePosition.ToVector2();
			var didMouseMove = false;
			if( _lastMousePosition != currentMousePosition )
			{
				didMouseMove = true;
				_lastMousePosition = currentMousePosition;
			}

			var mousePos = screenToStageCoordinates( currentMousePosition );

			// mouse moved and released events are only sent to inputFocusListeners
			if( ( Input.leftMouseButtonDown && !Input.leftMouseButtonPressed ) || Input.leftMouseButtonReleased )
			{
				if( Input.leftMouseButtonDown && didMouseMove )
				{
					for( var i = _inputFocusListeners.Count - 1; i >= 0; i-- )
						( (IInputListener)_inputFocusListeners[i] ).onMouseMoved( _inputFocusListeners[i].stageToLocalCoordinates( mousePos ) );
				}
				else if( Input.leftMouseButtonReleased )
				{
					for( var i = _inputFocusListeners.Count - 1; i >= 0; i-- )
						( (IInputListener)_inputFocusListeners[i] ).onMouseUp( _inputFocusListeners[i].stageToLocalCoordinates( mousePos ) );
					_inputFocusListeners.Clear();
				}
			}
			else
			{
				var over = hit( mousePos );

				// lose keyboard focus if we click outside of the keyboardFocusElement
				if( Input.leftMouseButtonPressed && _keyboardFocusElement != null && over != _keyboardFocusElement )
					setKeyboardFocus( null );

				// if we are over the same element and the left button was pressed we notify our listener
				if( over != null && over == _mouseOverElement )
				{
					var elementLocal = _mouseOverElement.stageToLocalCoordinates( mousePos );

					if( Input.leftMouseButtonPressed && _mouseOverElement is IInputListener )
					{
						var listener = _mouseOverElement as IInputListener;
						// add the listener to be notified for all onMouseDown and onMouseUp events for the specified pointer and button
						if( listener.onMousePressed( elementLocal ) )
							_inputFocusListeners.Add( _mouseOverElement );
					}
				}
				else // not a mouse pressed event
				{
					// enter event
					if( over != null && over is IInputListener )
					{
						var listener = over as IInputListener;
						listener.onMouseEnter();
					}

					// exit event
					if( _mouseOverElement != null && _mouseOverElement is IInputListener )
					{
						var listener = _mouseOverElement as IInputListener;
						listener.onMouseExit();
					}
				}

				_mouseOverElement = over;
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
					if( !InputUtils.altDown() )
					{
						var c = key.getChar();
						if( c.HasValue )
						{
							clearKeyRepeatTimer();
							_keyboardFocusElement.keyPressed( key, c.Value );

							// if we dont have a control key pressed setup a repeat timer for the key
							if( !InputUtils.controlDown() )
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
			if( _camera == null )
				return screenCoords;
			return _camera.screenToWorldPoint( screenCoords );
		}


		/// <summary>
		/// Transforms the stage coordinates to screen coordinates
		/// </summary>
		/// <returns>The to screen coordinates.</returns>
		/// <param name="stageCoords">Stage coords.</param>
		public Vector2 stageToScreenCoordinates( Vector2 stageCoords )
		{
			if( _camera == null )
				return stageCoords;
			return _camera.worldToScreenPoint( stageCoords );
		}

	}
}

