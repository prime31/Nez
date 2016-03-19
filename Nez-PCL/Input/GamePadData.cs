using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	public class GamePadData
	{
		public bool isGamePadStickInverted = false;

		PlayerIndex _playerIndex;
		GamePadState _previousState;
		GamePadState _currentState;
		float _rumbleTime;


		internal GamePadData( PlayerIndex playerIndex )
		{
			_playerIndex = playerIndex;
			_previousState = new GamePadState();
			_currentState = Microsoft.Xna.Framework.Input.GamePad.GetState( _playerIndex );
		}


		public void update()
		{
			_previousState = _currentState;
			_currentState = Microsoft.Xna.Framework.Input.GamePad.GetState( _playerIndex );

			// check for controller connects/disconnects
			if( _previousState.IsConnected != _currentState.IsConnected )
			{
				var data = new InputEvent {
					gamePadIndex = (int)_playerIndex
				};
				Input.emitter.emit( _currentState.IsConnected ? InputEventType.GamePadConnected : InputEventType.GamePadDisconnected, data );
			}

			if( _rumbleTime > 0f )
			{
				_rumbleTime -= Time.deltaTime;
				if( _rumbleTime <= 0f )
					GamePad.SetVibration( _playerIndex, 0, 0 );
			}
		}


		public void setVibration( float left, float right, float duration )
		{
			_rumbleTime = duration;
			GamePad.SetVibration( _playerIndex, left, right );
		}


		public void stopVibration()
		{
			GamePad.SetVibration( _playerIndex, 0, 0 );
			_rumbleTime = 0f;
		}


		public bool isConnected()
		{
			return _currentState.IsConnected;
		}


		#region Buttons

		public bool isButtonDown( Buttons button )
		{
			return _currentState.IsButtonDown( button ) && !_previousState.IsButtonDown( button );
		}


		public bool isButtonPressed( Buttons button )
		{
			return _currentState.IsButtonDown( button );
		}


		public bool isButtonReleased( Buttons button )
		{
			return !_currentState.IsButtonUp( button ) && _previousState.IsButtonUp( button );
		}

		#endregion


		#region Sticks

		public Vector2 getLeftStick()
		{
			var res = _currentState.ThumbSticks.Left;

			if( isGamePadStickInverted )
				res.Y = -res.Y;

			return res;
		}


		public Vector2 getLeftStick( float deadzone )
		{
			var res = _currentState.ThumbSticks.Left;

			if( res.LengthSquared() < deadzone * deadzone )
				res = Vector2.Zero;
			else if( isGamePadStickInverted )
				res.Y = -res.Y;

			return res;
		}


		public Vector2 getRightStick()
		{
			var res = _currentState.ThumbSticks.Right;

			if( isGamePadStickInverted )
				res.Y = -res.Y;

			return res;
		}


		public Vector2 getRightStick( float deadzone )
		{
			var res = _currentState.ThumbSticks.Right;

			if( res.LengthSquared() < deadzone * deadzone )
				res = Vector2.Zero;
			else if( isGamePadStickInverted )
				res.Y = -res.Y;

			return res;
		}

		#endregion


		#region Sticks as Buttons

		public bool isLeftStickLeft( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.X < -deadzone;
		}


		public bool isLeftStickRight( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.X < deadzone;
		}


		public bool isLeftStickUp( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.Y < deadzone;
		}


		public bool isLeftStickDown( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.Y < -deadzone;
		}


		public bool isRightStickLeft( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Right.X < -deadzone;
		}


		public bool isRightStickRight( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Right.X < deadzone;
		}


		public bool isRightStickUp( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Right.Y < deadzone;
		}


		public bool isRightStickDown( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Right.Y < -deadzone;
		}

		#endregion


		#region Dpad

		public bool DpadLeftDown
		{
			get { return _currentState.DPad.Left == ButtonState.Pressed; }
		}


		public bool DpadLeftPressed
		{
			get { return _currentState.DPad.Left == ButtonState.Pressed && _previousState.DPad.Left == ButtonState.Released; }
		}


		public bool DpadLeftReleased
		{
			get { return _currentState.DPad.Left == ButtonState.Released && _previousState.DPad.Left == ButtonState.Pressed; }
		}


		public bool DpadRightDown
		{
			get { return _currentState.DPad.Right == ButtonState.Pressed; }
		}


		public bool DpadRightPressed
		{
			get { return _currentState.DPad.Right == ButtonState.Pressed && _previousState.DPad.Right == ButtonState.Released; }
		}


		public bool DpadRightReleased
		{
			get { return _currentState.DPad.Right == ButtonState.Released && _previousState.DPad.Right == ButtonState.Pressed; }
		}


		public bool DpadUpDown
		{
			get { return _currentState.DPad.Up == ButtonState.Pressed; }
		}


		public bool DpadUpPressed
		{
			get { return _currentState.DPad.Up == ButtonState.Pressed && _previousState.DPad.Up == ButtonState.Released; }
		}


		public bool DpadUpReleased
		{
			get { return _currentState.DPad.Up == ButtonState.Released && _previousState.DPad.Up == ButtonState.Pressed; }
		}


		public bool DpadDownDown
		{
			get { return _currentState.DPad.Down == ButtonState.Pressed; }
		}


		public bool DpadDownPressed
		{
			get { return _currentState.DPad.Down == ButtonState.Pressed && _previousState.DPad.Down == ButtonState.Released; }
		}


		public bool DpadDownReleased
		{
			get { return _currentState.DPad.Down == ButtonState.Released && _previousState.DPad.Down == ButtonState.Pressed; }
		}

		#endregion


		#region Triggers

		public float getLeftTriggerRaw()
		{
			return _currentState.Triggers.Left;
		}


		public float getRightTriggerRaw()
		{
			return _currentState.Triggers.Right;
		}


		/// <summary>
		/// true whenever the trigger is down past the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger down was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool isLeftTriggerDown( float threshold = 0.2f )
		{
			return _currentState.Triggers.Left > threshold;
		}


		/// <summary>
		/// true only the frame that the trigger passed the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool isLeftTriggerPressed( float threshold = 0.2f )
		{
			return _currentState.Triggers.Left > threshold && _previousState.Triggers.Left < threshold;
		}


		/// <summary>
		/// true the frame the trigger is released
		/// </summary>
		/// <returns><c>true</c>, if left trigger released was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool isLeftTriggerReleased( float threshold = 0.2f )
		{
			return _currentState.Triggers.Left < threshold && _previousState.Triggers.Left > threshold;
		}


		/// <summary>
		/// true whenever the trigger is down past the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger down was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool isRightTriggerDown( float threshold = 0.2f )
		{
			return _currentState.Triggers.Right > threshold;
		}


		/// <summary>
		/// true only the frame that the trigger passed the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool isRightTriggerPressed( float threshold = 0.2f )
		{
			return _currentState.Triggers.Right > threshold && _previousState.Triggers.Right < threshold;
		}


		/// <summary>
		/// true the frame the trigger is released
		/// </summary>
		/// <returns><c>true</c>, if left trigger released was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool isRightTriggerReleased( float threshold = 0.2f )
		{
			return _currentState.Triggers.Right < threshold && _previousState.Triggers.Right > threshold;
		}

		#endregion

	}
}

