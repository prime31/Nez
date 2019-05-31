using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Systems;
using System.Runtime.CompilerServices;


namespace Nez
{
	public static class Input
	{
		public static Emitter<InputEventType, InputEvent> emitter;

		public static GamePadData[] gamePads;
		public const float DEFAULT_DEADZONE = 0.1f;

		internal static Vector2 _resolutionScale;
		internal static Point _resolutionOffset;
		static KeyboardState _previousKbState;
		static KeyboardState _currentKbState;
		static MouseState _previousMouseState;
		static MouseState _currentMouseState;
		static internal FastList<VirtualInput> _virtualInputs = new FastList<VirtualInput>();
		static int _maxSupportedGamePads;

		/// <summary>
		/// the TouchInput details when on a device that supports touch
		/// </summary>
		public static TouchInput touch;

		/// <summary>
		/// set by the Scene and used to scale mouse input for cases where the Scene render target is a different size
		/// than the backbuffer. This situation basically results in mouse coordinates in screen space instead of
		/// in the render target coordinate system;
		/// </summary>
		public static Vector2 resolutionScale => _resolutionScale;

		/// <summary>
		/// set by the Scene and used to get mouse input from raw screen coordinates to render target coordinates. Any
		/// SceneResolutionPolicy that can result in letterboxing could potentially have an offset (basically, the
		/// letterbox portion of the render).
		/// </summary>
		/// <returns></returns>
		public static Vector2 resolutionOffset => _resolutionOffset.ToVector2();

		/// <summary>
		/// gets/sets the maximum supported gamepads
		/// </summary>
		/// <value></value>
		public static int maxSupportedGamePads
		{
			get { return _maxSupportedGamePads; }
			set
			{
#if FNA
				_maxSupportedGamePads = Mathf.clamp( value, 1, 8 );
#else
				_maxSupportedGamePads = Mathf.clamp( value, 1, GamePad.MaximumGamePadCount );
#endif
				gamePads = new GamePadData[_maxSupportedGamePads];
				for( var i = 0; i < _maxSupportedGamePads; i++ )
					gamePads[i] = new GamePadData( (PlayerIndex)i );
			}
		}


		static Input()
		{
			emitter = new Emitter<InputEventType, InputEvent>();
			touch = new TouchInput();

			_previousKbState = new KeyboardState();
			_currentKbState = Keyboard.GetState();

			_previousMouseState = new MouseState();
			_currentMouseState = Mouse.GetState();

			maxSupportedGamePads = 1;
		}


		public static void update()
		{
			touch.update();

			_previousKbState = _currentKbState;
			_currentKbState = Keyboard.GetState();

			_previousMouseState = _currentMouseState;
			_currentMouseState = Mouse.GetState();

			for( var i = 0; i < _maxSupportedGamePads; i++ )
				gamePads[i].update();

			for( var i = 0; i < _virtualInputs.length; i++ )
				_virtualInputs.buffer[i].update();
		}

		/// <summary>
		/// this takes into account the SceneResolutionPolicy and returns the value scaled to the RenderTargets coordinates
		/// </summary>
		/// <value>The scaled position.</value>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public static Vector2 scaledPosition( Vector2 position )
		{
			var scaledPos = new Vector2( position.X - _resolutionOffset.X, position.Y - _resolutionOffset.Y );
			return scaledPos * _resolutionScale;
		}

		/// <summary>
		/// to be used with great care! This lets you override the current MouseState. This is useful
		/// when the Nez render is embedded in a larger window so that mouse coordinates can be translated
		/// to Nez space from the outer window coordinates and for simulating mouse input.
		/// </summary>
		/// <param name="state"></param>
		public static void setCurrentMouseState( MouseState state )
		{
			_currentMouseState = state;
		}


		/// <summary>
		/// useful for simulating keyboard input
		/// </summary>
		/// <param name="state">State.</param>
		public static void setCurrentKeyboardState( KeyboardState state )
		{
			_currentKbState = state;
		}

		#region Keyboard

		/// <summary>
		/// returns the previous KeyboardState from the last frame
		/// </summary>
		/// <value></value>
		public static KeyboardState previousKeyboardState => _previousKbState;

		/// <summary>
		/// returns the KeyboardState from this frame
		/// </summary>
		/// <value></value>
		public static KeyboardState currentKeyboardState => _currentKbState;


		/// <summary>
		/// only true if down this frame and not down the previous frame
		/// </summary>
		/// <returns><c>true</c>, if key pressed was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyPressed( Keys key )
		{
			return _currentKbState.IsKeyDown( key ) && !_previousKbState.IsKeyDown( key );
		}


		/// <summary>
		/// true the entire time the key is down
		/// </summary>
		/// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyDown( Keys key )
		{
			return _currentKbState.IsKeyDown( key );
		}


		/// <summary>
		/// true only the frame the key is released
		/// </summary>
		/// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyReleased( Keys key )
		{
			return !_currentKbState.IsKeyDown( key ) && _previousKbState.IsKeyDown( key );
		}


		/// <summary>
		/// only true if one of the keys is down this frame
		/// </summary>
		/// <returns><c>true</c>, if key pressed was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyPressed( Keys keyA, Keys keyB )
		{
			return isKeyPressed( keyA ) || isKeyPressed( keyB );
		}


		/// <summary>
		/// true while either of the keys are down
		/// </summary>
		/// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyDown( Keys keyA, Keys keyB )
		{
			return isKeyDown( keyA ) || isKeyDown( keyB );
		}


		/// <summary>
		/// true only the frame one of the keys are released
		/// </summary>
		/// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyReleased( Keys keyA, Keys keyB )
		{
			return isKeyReleased( keyA ) || isKeyReleased( keyB );
		}

		#endregion


		#region Mouse

		/// <summary>
		/// returns the previous mouse state. Use with caution as it only contains raw data and does not take camera scaling into affect like
		/// Input.mousePosition does.
		/// </summary>
		/// <value>The state of the previous mouse.</value>
		public static MouseState previousMouseState => _previousMouseState;

		/// <summary>
		/// returns the current mouse state. Use with caution as it only contains raw data and does not take camera scaling into affect like
		/// Input.mousePosition does.
		/// </summary>
		public static MouseState currentMouseState => _currentMouseState;

		/// <summary>
		/// only true if down this frame
		/// </summary>
		public static bool leftMouseButtonPressed
		{
			get { return _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released; }
		}

		/// <summary>
		/// true while the button is down
		/// </summary>
		public static bool leftMouseButtonDown
		{
			get { return _currentMouseState.LeftButton == ButtonState.Pressed; }
		}

		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		public static bool leftMouseButtonReleased
		{
			get
			{
				return _currentMouseState.LeftButton == ButtonState.Released && _previousMouseState.LeftButton == ButtonState.Pressed;
			}
		}

		/// <summary>
		/// only true if pressed this frame
		/// </summary>
		public static bool rightMouseButtonPressed
		{
			get
			{
				return _currentMouseState.RightButton == ButtonState.Pressed && _previousMouseState.RightButton == ButtonState.Released;
			}
		}

		/// <summary>
		/// true while the button is down
		/// </summary>
		public static bool rightMouseButtonDown
		{
			get { return _currentMouseState.RightButton == ButtonState.Pressed; }
		}

		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		public static bool rightMouseButtonReleased
		{
			get
			{
				return _currentMouseState.RightButton == ButtonState.Released && _previousMouseState.RightButton == ButtonState.Pressed;
			}
		}

		/// <summary>
		/// only true if down this frame
		/// </summary>
		public static bool middleMouseButtonPressed
		{
			get { return _currentMouseState.MiddleButton == ButtonState.Pressed && _previousMouseState.MiddleButton == ButtonState.Released; }
		}

		/// <summary>
		/// true while the button is down
		/// </summary>
		public static bool middleMouseButtonDown
		{
			get { return _currentMouseState.MiddleButton == ButtonState.Pressed; }
		}

		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		public static bool middleMouseButtonReleased
		{
			get
			{
				return _currentMouseState.MiddleButton == ButtonState.Released && _previousMouseState.MiddleButton == ButtonState.Pressed;
			}
		}

		/// <summary>
		/// only true if down this frame
		/// </summary>
		public static bool firstExtendedMouseButtonPressed
		{
			get { return _currentMouseState.XButton1 == ButtonState.Pressed && _previousMouseState.XButton1 == ButtonState.Released; }
		}

		/// <summary>
		/// true while the button is down
		/// </summary>
		public static bool firstExtendedMouseButtonDown
		{
			get { return _currentMouseState.XButton1 == ButtonState.Pressed; }
		}

		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		public static bool firstExtendedMouseButtonReleased
		{
			get { return _currentMouseState.XButton1 == ButtonState.Released && _previousMouseState.XButton1 == ButtonState.Pressed; }
		}

		/// <summary>
		/// only true if down this frame
		/// </summary>
		public static bool secondExtendedMouseButtonPressed
		{
			get { return _currentMouseState.XButton2 == ButtonState.Pressed && _previousMouseState.XButton2 == ButtonState.Released; }
		}

		/// <summary>
		/// true while the button is down
		/// </summary>
		public static bool secondExtendedMouseButtonDown
		{
			get { return _currentMouseState.XButton2 == ButtonState.Pressed; }
		}

		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		public static bool secondExtendedMouseButtonReleased
		{
			get { return _currentMouseState.XButton2 == ButtonState.Released && _previousMouseState.XButton2 == ButtonState.Pressed; }
		}

		/// <summary>
		/// gets the raw ScrollWheelValue
		/// </summary>
		/// <value>The mouse wheel.</value>
		public static int mouseWheel
		{
			get { return _currentMouseState.ScrollWheelValue; }
		}

		/// <summary>
		/// gets the delta ScrollWheelValue
		/// </summary>
		/// <value>The mouse wheel delta.</value>
		public static int mouseWheelDelta
		{
			get { return _currentMouseState.ScrollWheelValue - _previousMouseState.ScrollWheelValue; }
		}

		/// <summary>
		/// unscaled mouse position. This is the actual screen space value
		/// </summary>
		/// <value>The raw mouse position.</value>
		public static Point rawMousePosition
		{
			get { return new Point( _currentMouseState.X, _currentMouseState.Y ); }
		}

		/// <summary>
		/// alias for scaledMousePosition
		/// </summary>
		/// <value>The mouse position.</value>
		public static Vector2 mousePosition { get { return scaledMousePosition; } }

		/// <summary>
		/// this takes into account the SceneResolutionPolicy and returns the value scaled to the RenderTargets coordinates
		/// </summary>
		/// <value>The scaled mouse position.</value>
		public static Vector2 scaledMousePosition { get { return scaledPosition( new Vector2( _currentMouseState.X, _currentMouseState.Y ) ); } }

		public static Point mousePositionDelta
		{
			get { return new Point( _currentMouseState.X, _currentMouseState.Y ) - new Point( _previousMouseState.X, _previousMouseState.Y ); }
		}

		public static Vector2 scaledMousePositionDelta
		{
			get
			{
				var pastPos = new Vector2( _previousMouseState.X - _resolutionOffset.X, _previousMouseState.Y - _resolutionOffset.Y );
				pastPos *= _resolutionScale;
				return scaledMousePosition - pastPos;
			}
		}

		#endregion

	}


	public enum InputEventType
	{
		GamePadConnected,
		GamePadDisconnected
	}


	public struct InputEvent
	{
		public int gamePadIndex;
	}


	/// <summary>
	/// comparer that should be passed to a dictionary constructor to avoid boxing/unboxing when using an enum as a key
	/// on Mono
	/// </summary>
	public struct InputEventTypeComparer : IEqualityComparer<InputEventType>
	{
		public bool Equals( InputEventType x, InputEventType y )
		{
			return x == y;
		}


		public int GetHashCode( InputEventType obj )
		{
			return (int)obj;
		}
	}
}

