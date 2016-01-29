using System;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input.Touch;
using System.Collections.Generic;


namespace Nez
{
	public static class Input
	{
		static readonly int MAX_SUPPORTED_GAMEPADS = 2;

		public static GamePadData[] gamePads = new GamePadData[MAX_SUPPORTED_GAMEPADS];
		public const float DEFAULT_DEADZONE = 0.1f;

		/// <summary>
		/// set by the Scene and used to scale mouse input
		/// </summary>
		internal static Vector2 _resolutionScale;
		/// <summary>
		/// set by the Scene and used to scale mouse input
		/// </summary>
		internal static Point _resolutionOffset;
		static KeyboardState _previousKbState;
		static KeyboardState _currentKbState;
		static MouseState _previousMouseState;
		static MouseState _currentMouseState;
		static internal List<VirtualInput> _virtualInputs = new List<VirtualInput>();
		static TouchCollection _previousTouches;
		static TouchCollection _currentTouches;
		static List<GestureSample> _previousGestures = new List<GestureSample>();
		static List<GestureSample> _currentGestures = new List<GestureSample>();


		static Input()
		{
			_previousKbState = new KeyboardState();
			_currentKbState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

			_previousMouseState = new MouseState();
			_currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

			int count = 0;
			try
			{
				for( var i = 0; i < 2; i++ )
				{
					Microsoft.Xna.Framework.Input.GamePad.GetCapabilities( (PlayerIndex)i );
					count++;
				}
			}
			catch( Exception )
			{}
			finally
			{
				MAX_SUPPORTED_GAMEPADS = count;
			}

			for( var i = 0; i < MAX_SUPPORTED_GAMEPADS; i++ )
				gamePads[i] = new GamePadData( (PlayerIndex)i );

			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
			onGraphicsDeviceReset();
		}


		public static void update()
		{
			_previousKbState = _currentKbState;
			_currentKbState = Microsoft.Xna.Framework.Input.Keyboard.GetState();

			_previousMouseState = _currentMouseState;
			_currentMouseState = Microsoft.Xna.Framework.Input.Mouse.GetState();

			for( var i = 0; i < MAX_SUPPORTED_GAMEPADS; i++ )
				gamePads[i].update();

			for( var i = 0; i < _virtualInputs.Count; i++ )
				_virtualInputs[i].update();

			_previousTouches = _currentTouches;
			_currentTouches = TouchPanel.GetState();

			_previousGestures = _currentGestures;
			_currentGestures.Clear();
			while( TouchPanel.IsGestureAvailable )
			{
				var gesture = TouchPanel.ReadGesture();
				_currentGestures.Add( gesture );
			}
		}
	

		public static void onGraphicsDeviceReset()
		{
			// TODO: find a way to grab the GameWindow OnOrientationChange event inside the PCL
			// For the time being you can just call this method hooking the event on your own
			// Game class like this: 			
			// Window.OrientationChanged += OnOrientationChanged;

			TouchPanel.DisplayWidth = Core.graphicsDevice.Viewport.Width;
			TouchPanel.DisplayHeight = Core.graphicsDevice.Viewport.Height;
			TouchPanel.DisplayOrientation = Core.graphicsDevice.PresentationParameters.DisplayOrientation;
		}


		#region Keyboard

		/// <summary>
		/// true the entire time the key is down
		/// </summary>
		/// <returns><c>true</c>, if key down was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyDown( Keys key )
		{
			return _currentKbState.IsKeyDown( key );
		}


		/// <summary>
		/// only true if down this frame
		/// </summary>
		/// <returns><c>true</c>, if key pressed was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyPressed( Keys key )
		{
			return _currentKbState.IsKeyDown( key ) && !_previousKbState.IsKeyDown( key );
		}


		/// <summary>
		/// true only the frame the key is released
		/// </summary>
		/// <returns><c>true</c>, if key up was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyReleased( Keys key )
		{
			return !_currentKbState.IsKeyUp( key ) && _previousKbState.IsKeyUp( key );
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
		/// only true if one of the keys is down this frame
		/// </summary>
		/// <returns><c>true</c>, if key pressed was gotten, <c>false</c> otherwise.</returns>
		public static bool isKeyPressed( Keys keyA, Keys keyB )
		{
			return isKeyPressed( keyA ) || isKeyPressed( keyB );
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
		/// true while the button is down
		/// </summary>
		public static bool leftMouseButtonDown
		{
			get { return _currentMouseState.LeftButton == ButtonState.Pressed; }
		}


		/// <summary>
		/// only true if down this frame
		/// </summary>
		public static bool leftMouseButtonPressed
		{
			get { return _currentMouseState.LeftButton == ButtonState.Pressed && _previousMouseState.LeftButton == ButtonState.Released; }
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
		/// true while the button is down
		/// </summary>
		public static bool rightMouseButtonDown
		{
			get
			{
				return _currentMouseState.RightButton == ButtonState.Pressed;
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
		/// true only the frame the button is released
		/// </summary>
		public static bool rightMouseButtonReleased
		{
			get
			{
				return _currentMouseState.RightButton == ButtonState.Released && _previousMouseState.RightButton == ButtonState.Pressed;
			}
		}


		public static int mouseWheel
		{
			get { return _currentMouseState.ScrollWheelValue; }
		}


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
			get { return _currentMouseState.Position; }
		}


		/// <summary>
		/// this takes into account the SceneResolutionPolicy and returns the value scaled to the RenderTargets coordinates
		/// </summary>
		/// <value>The scaled mouse position.</value>
		public static Vector2 scaledMousePosition
		{
			get
			{
				var mousePosition = _currentMouseState.Position - _resolutionOffset;
				return mousePosition.ToVector2() * _resolutionScale;
			}
		}


		public static Point mousePositionDelta
		{
			get { return _currentMouseState.Position - _previousMouseState.Position; }
		}


		public static Vector2 scaledMousePositionDelta
		{
			get
			{
				var pastPos = ( _previousMouseState.Position - _resolutionOffset ).ToVector2();
				pastPos *= _resolutionScale;
				return scaledMousePosition - pastPos;
			}
		}

		#endregion


		#region Touches

		public static TouchCollection currentTouches
		{
			get { return _currentTouches; }
		}

		public static TouchCollection previousTouches
		{
			get { return _previousTouches; }
		}

		public static List<GestureSample> previousGestures
		{
			get { return _previousGestures; }
		}

		public static List<GestureSample> currentGestures
		{
			get { return _currentGestures; }
		}

		#endregion
	
	}
}

