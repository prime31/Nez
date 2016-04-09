using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	public class GamePadData
	{
		/// <summary>
		/// toggles inverting the left sticks vertical value
		/// </summary>
		public bool isLeftStickVertcialInverted = false;

		/// <summary>
		/// toggles inverting the right sticks vertical value
		/// </summary>
		public bool isRightStickVertcialInverted = false;

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


		/// <summary>
		/// returns true if this game pad is connected
		/// </summary>
		/// <returns><c>true</c>, if connected was ised, <c>false</c> otherwise.</returns>
		public bool isConnected()
		{
			return _currentState.IsConnected;
		}


		#region Buttons

		/// <summary>
		/// only true if down this frame
		/// </summary>
		/// <returns><c>true</c>, if button pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool isButtonPressed( Buttons button )
		{
			return _currentState.IsButtonDown( button ) && !_previousState.IsButtonDown( button );
		}


		/// <summary>
		/// true the entire time the button is down
		/// </summary>
		/// <returns><c>true</c>, if button down was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool isButtonDown( Buttons button )
		{
			return _currentState.IsButtonDown( button );
		}


		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		/// <returns><c>true</c>, if button released was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool isButtonReleased( Buttons button )
		{
			return !_currentState.IsButtonDown( button ) && _previousState.IsButtonDown( button );
		}

		#endregion


		#region Sticks

		public Vector2 getLeftStick()
		{
			var res = _currentState.ThumbSticks.Left;

			if( isLeftStickVertcialInverted )
				res.Y = -res.Y;

			return res;
		}


		public Vector2 getLeftStick( float deadzone )
		{
			var res = _currentState.ThumbSticks.Left;

			if( res.LengthSquared() < deadzone * deadzone )
				res = Vector2.Zero;
			else if( isLeftStickVertcialInverted )
				res.Y = -res.Y;

			return res;
		}


		public Vector2 getRightStick()
		{
			var res = _currentState.ThumbSticks.Right;

			if( isRightStickVertcialInverted )
				res.Y = -res.Y;

			return res;
		}


		public Vector2 getRightStick( float deadzone )
		{
			var res = _currentState.ThumbSticks.Right;

			if( res.LengthSquared() < deadzone * deadzone )
				res = Vector2.Zero;
			else if( isRightStickVertcialInverted )
				res.Y = -res.Y;

			return res;
		}

		#endregion


		#region Sticks as Buttons

		public bool isLeftStickLeft( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.X < -deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick left pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool isLeftStickLeftPressed( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.X < -deadzone && _previousState.ThumbSticks.Left.X > -deadzone;
		}


		public bool isLeftStickRight( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.X < deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick right pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool isLeftStickRightPressed( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.X < deadzone && _previousState.ThumbSticks.Left.X > deadzone;
		}


		public bool isLeftStickUp( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.Y < deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick up pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool isLeftStickUpPressed( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.Y < deadzone && _previousState.ThumbSticks.Left.Y > deadzone;
		}


		public bool isLeftStickDown( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.Y < -deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick down pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool isLeftStickDownPressed( float deadzone = Input.DEFAULT_DEADZONE )
		{
			return _currentState.ThumbSticks.Left.Y < -deadzone && _previousState.ThumbSticks.Left.Y > -deadzone;
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

		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadLeftDown
		{
			get { return _currentState.DPad.Left == ButtonState.Pressed; }
		}


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadLeftPressed
		{
			get { return _currentState.DPad.Left == ButtonState.Pressed && _previousState.DPad.Left == ButtonState.Released; }
		}


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadLeftReleased
		{
			get { return _currentState.DPad.Left == ButtonState.Released && _previousState.DPad.Left == ButtonState.Pressed; }
		}


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadRightDown
		{
			get { return _currentState.DPad.Right == ButtonState.Pressed; }
		}


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadRightPressed
		{
			get { return _currentState.DPad.Right == ButtonState.Pressed && _previousState.DPad.Right == ButtonState.Released; }
		}


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadRightReleased
		{
			get { return _currentState.DPad.Right == ButtonState.Released && _previousState.DPad.Right == ButtonState.Pressed; }
		}


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadUpDown
		{
			get { return _currentState.DPad.Up == ButtonState.Pressed; }
		}


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadUpPressed
		{
			get { return _currentState.DPad.Up == ButtonState.Pressed && _previousState.DPad.Up == ButtonState.Released; }
		}


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadUpReleased
		{
			get { return _currentState.DPad.Up == ButtonState.Released && _previousState.DPad.Up == ButtonState.Pressed; }
		}


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadDownDown
		{
			get { return _currentState.DPad.Down == ButtonState.Pressed; }
		}


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadDownPressed
		{
			get { return _currentState.DPad.Down == ButtonState.Pressed && _previousState.DPad.Down == ButtonState.Released; }
		}


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
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

