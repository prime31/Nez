using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	public class GamePadData
	{
		/// <summary>
		/// toggles inverting the left sticks vertical value
		/// </summary>
		public bool IsLeftStickVerticalInverted = false;

		/// <summary>
		/// toggles inverting the right sticks vertical value
		/// </summary>
		public bool IsRightStickVerticalInverted = false;

		public GamePadDeadZone DeadZone = GamePadDeadZone.IndependentAxes;

		PlayerIndex _playerIndex;
		GamePadState _previousState;
		GamePadState _currentState;
		float _rumbleTime;


		internal GamePadData(PlayerIndex playerIndex)
		{
			_playerIndex = playerIndex;
			_previousState = new GamePadState();
			_currentState = GamePad.GetState(_playerIndex);
		}


		public void Update()
		{
			_previousState = _currentState;
			_currentState = GamePad.GetState(_playerIndex, DeadZone);

			// check for controller connects/disconnects
			if (_previousState.IsConnected != _currentState.IsConnected)
			{
				var data = new InputEvent
				{
					GamePadIndex = (int) _playerIndex
				};
				Input.Emitter.Emit(
					_currentState.IsConnected ? InputEventType.GamePadConnected : InputEventType.GamePadDisconnected,
					data);
			}

			if (_rumbleTime > 0f)
			{
				_rumbleTime -= Time.DeltaTime;
				if (_rumbleTime <= 0f)
					GamePad.SetVibration(_playerIndex, 0, 0);
			}
		}


		public void SetVibration(float left, float right, float duration)
		{
			_rumbleTime = duration;
			GamePad.SetVibration(_playerIndex, left, right);
		}


		public void StopVibration()
		{
			GamePad.SetVibration(_playerIndex, 0, 0);
			_rumbleTime = 0f;
		}


		/// <summary>
		/// returns true if this game pad is connected
		/// </summary>
		/// <returns><c>true</c>, if connected was ised, <c>false</c> otherwise.</returns>
		public bool IsConnected()
		{
			return _currentState.IsConnected;
		}


		#region Buttons

		/// <summary>
		/// only true if down this frame
		/// </summary>
		/// <returns><c>true</c>, if button pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool IsButtonPressed(Buttons button)
		{
			return _currentState.IsButtonDown(button) && !_previousState.IsButtonDown(button);
		}


		/// <summary>
		/// true the entire time the button is down
		/// </summary>
		/// <returns><c>true</c>, if button down was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool IsButtonDown(Buttons button)
		{
			return _currentState.IsButtonDown(button);
		}


		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		/// <returns><c>true</c>, if button released was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool IsButtonReleased(Buttons button)
		{
			return !_currentState.IsButtonDown(button) && _previousState.IsButtonDown(button);
		}

		#endregion


		#region Sticks

		public Vector2 GetLeftStick()
		{
			var res = _currentState.ThumbSticks.Left;

			if (IsLeftStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}


		public Vector2 GetLeftStick(float deadzone)
		{
			var res = _currentState.ThumbSticks.Left;

			if (res.LengthSquared() < deadzone * deadzone)
				res = Vector2.Zero;
			else if (IsLeftStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}


		public Vector2 GetRightStick()
		{
			var res = _currentState.ThumbSticks.Right;

			if (IsRightStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}


		public Vector2 GetRightStick(float deadzone)
		{
			var res = _currentState.ThumbSticks.Right;

			if (res.LengthSquared() < deadzone * deadzone)
				res = Vector2.Zero;
			else if (IsRightStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}

		#endregion


		#region Sticks as Buttons

		public bool IsLeftStickLeft(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.X < -deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick left pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickLeftPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.X < -deadzone && _previousState.ThumbSticks.Left.X > -deadzone;
		}


		public bool IsLeftStickRight(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.X > deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick right pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickRightPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.X > deadzone && _previousState.ThumbSticks.Left.X < deadzone;
		}


		public bool IsLeftStickUp(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.Y > deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick up pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickUpPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.Y > deadzone && _previousState.ThumbSticks.Left.Y < deadzone;
		}


		public bool IsLeftStickDown(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.Y < -deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick down pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickDownPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Left.Y < -deadzone && _previousState.ThumbSticks.Left.Y > -deadzone;
		}


		public bool IsRightStickLeft(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Right.X < -deadzone;
		}


		public bool IsRightStickRight(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Right.X > deadzone;
		}


		public bool IsRightStickUp(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Right.Y > deadzone;
		}


		public bool IsRightStickDown(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return _currentState.ThumbSticks.Right.Y < -deadzone;
		}

		#endregion


		#region Dpad

		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadLeftDown => _currentState.DPad.Left == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadLeftPressed =>
			_currentState.DPad.Left == ButtonState.Pressed &&
			_previousState.DPad.Left == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadLeftReleased =>
			_currentState.DPad.Left == ButtonState.Released &&
			_previousState.DPad.Left == ButtonState.Pressed;


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadRightDown => _currentState.DPad.Right == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadRightPressed =>
			_currentState.DPad.Right == ButtonState.Pressed &&
			_previousState.DPad.Right == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadRightReleased =>
			_currentState.DPad.Right == ButtonState.Released &&
			_previousState.DPad.Right == ButtonState.Pressed;


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadUpDown => _currentState.DPad.Up == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadUpPressed => _currentState.DPad.Up == ButtonState.Pressed && _previousState.DPad.Up == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadUpReleased => _currentState.DPad.Up == ButtonState.Released && _previousState.DPad.Up == ButtonState.Pressed;


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadDownDown => _currentState.DPad.Down == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadDownPressed =>
			_currentState.DPad.Down == ButtonState.Pressed &&
			_previousState.DPad.Down == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadDownReleased =>
			_currentState.DPad.Down == ButtonState.Released &&
			_previousState.DPad.Down == ButtonState.Pressed;

		#endregion


		#region Triggers

		public float GetLeftTriggerRaw()
		{
			return _currentState.Triggers.Left;
		}


		public float GetRightTriggerRaw()
		{
			return _currentState.Triggers.Right;
		}


		/// <summary>
		/// true whenever the trigger is down past the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger down was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsLeftTriggerDown(float threshold = 0.2f)
		{
			return _currentState.Triggers.Left > threshold;
		}


		/// <summary>
		/// true only the frame that the trigger passed the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsLeftTriggerPressed(float threshold = 0.2f)
		{
			return _currentState.Triggers.Left > threshold && _previousState.Triggers.Left < threshold;
		}


		/// <summary>
		/// true the frame the trigger is released
		/// </summary>
		/// <returns><c>true</c>, if left trigger released was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsLeftTriggerReleased(float threshold = 0.2f)
		{
			return _currentState.Triggers.Left < threshold && _previousState.Triggers.Left > threshold;
		}


		/// <summary>
		/// true whenever the trigger is down past the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger down was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsRightTriggerDown(float threshold = 0.2f)
		{
			return _currentState.Triggers.Right > threshold;
		}


		/// <summary>
		/// true only the frame that the trigger passed the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsRightTriggerPressed(float threshold = 0.2f)
		{
			return _currentState.Triggers.Right > threshold && _previousState.Triggers.Right < threshold;
		}


		/// <summary>
		/// true the frame the trigger is released
		/// </summary>
		/// <returns><c>true</c>, if left trigger released was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsRightTriggerReleased(float threshold = 0.2f)
		{
			return _currentState.Triggers.Right < threshold && _previousState.Triggers.Right > threshold;
		}

		#endregion
	}
}