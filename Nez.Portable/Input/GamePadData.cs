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
		public GamePadState PreviousState;
		public GamePadState CurrentState;
		PlayerIndex _playerIndex;
		float _rumbleTime;


		internal GamePadData(PlayerIndex playerIndex)
		{
			_playerIndex = playerIndex;
			PreviousState = new GamePadState();
			CurrentState = GamePad.GetState(_playerIndex);
		}


		public void Update()
		{
			PreviousState = CurrentState;
			CurrentState = GamePad.GetState(_playerIndex, DeadZone);

			// check for controller connects/disconnects
			if (PreviousState.IsConnected != CurrentState.IsConnected)
			{
				var data = new InputEvent
				{
					GamePadIndex = (int) _playerIndex
				};
				Input.Emitter.Emit(
					CurrentState.IsConnected ? InputEventType.GamePadConnected : InputEventType.GamePadDisconnected,
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
			return CurrentState.IsConnected;
		}

		/// <summary>
		/// returns true if the game pad state changed
		/// </summary>
		/// <returns><c>true</c>, if previous state does not equal current state, <c>false</c> otherwise.</returns>
		public bool HasGamepadStateChanged()
		{
			return PreviousState != CurrentState;
		}


		#region Buttons

		/// <summary>
		/// only true if down this frame
		/// </summary>
		/// <returns><c>true</c>, if button pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool IsButtonPressed(Buttons button)
		{
			return CurrentState.IsButtonDown(button) && !PreviousState.IsButtonDown(button);
		}


		/// <summary>
		/// true the entire time the button is down
		/// </summary>
		/// <returns><c>true</c>, if button down was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool IsButtonDown(Buttons button)
		{
			return CurrentState.IsButtonDown(button);
		}


		/// <summary>
		/// true only the frame the button is released
		/// </summary>
		/// <returns><c>true</c>, if button released was ised, <c>false</c> otherwise.</returns>
		/// <param name="button">Button.</param>
		public bool IsButtonReleased(Buttons button)
		{
			return !CurrentState.IsButtonDown(button) && PreviousState.IsButtonDown(button);
		}

		#endregion


		#region Sticks

		public Vector2 GetLeftStick()
		{
			var res = CurrentState.ThumbSticks.Left;

			if (IsLeftStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}


		public Vector2 GetLeftStick(float deadzone)
		{
			var res = CurrentState.ThumbSticks.Left;

			if (res.LengthSquared() < deadzone * deadzone)
				res = Vector2.Zero;
			else if (IsLeftStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}


		public Vector2 GetRightStick()
		{
			var res = CurrentState.ThumbSticks.Right;

			if (IsRightStickVerticalInverted)
				res.Y = -res.Y;

			return res;
		}


		public Vector2 GetRightStick(float deadzone)
		{
			var res = CurrentState.ThumbSticks.Right;

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
			return CurrentState.ThumbSticks.Left.X < -deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick left pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickLeftPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.X < -deadzone && PreviousState.ThumbSticks.Left.X > -deadzone;
		}


		public bool IsLeftStickRight(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.X > deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick right pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickRightPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.X > deadzone && PreviousState.ThumbSticks.Left.X < deadzone;
		}


		public bool IsLeftStickUp(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.Y > deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick up pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickUpPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.Y > deadzone && PreviousState.ThumbSticks.Left.Y < deadzone;
		}


		public bool IsLeftStickDown(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.Y < -deadzone;
		}


		/// <summary>
		/// true only the frame the stick passes the deadzone in the direction
		/// </summary>
		/// <returns><c>true</c>, if left stick down pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="deadzone">Deadzone.</param>
		public bool IsLeftStickDownPressed(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Left.Y < -deadzone && PreviousState.ThumbSticks.Left.Y > -deadzone;
		}


		public bool IsRightStickLeft(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Right.X < -deadzone;
		}


		public bool IsRightStickRight(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Right.X > deadzone;
		}


		public bool IsRightStickUp(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Right.Y > deadzone;
		}


		public bool IsRightStickDown(float deadzone = Input.DEFAULT_DEADZONE)
		{
			return CurrentState.ThumbSticks.Right.Y < -deadzone;
		}

		#endregion


		#region Dpad

		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadLeftDown => CurrentState.DPad.Left == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadLeftPressed =>
			CurrentState.DPad.Left == ButtonState.Pressed &&
			PreviousState.DPad.Left == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadLeftReleased =>
			CurrentState.DPad.Left == ButtonState.Released &&
			PreviousState.DPad.Left == ButtonState.Pressed;


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadRightDown => CurrentState.DPad.Right == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadRightPressed =>
			CurrentState.DPad.Right == ButtonState.Pressed &&
			PreviousState.DPad.Right == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadRightReleased =>
			CurrentState.DPad.Right == ButtonState.Released &&
			PreviousState.DPad.Right == ButtonState.Pressed;


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadUpDown => CurrentState.DPad.Up == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadUpPressed => CurrentState.DPad.Up == ButtonState.Pressed && PreviousState.DPad.Up == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadUpReleased => CurrentState.DPad.Up == ButtonState.Released && PreviousState.DPad.Up == ButtonState.Pressed;


		/// <summary>
		/// true the entire time the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left down; otherwise, <c>false</c>.</value>
		public bool DpadDownDown => CurrentState.DPad.Down == ButtonState.Pressed;


		/// <summary>
		/// true only the first frame the dpad is down
		/// </summary>
		/// <value><c>true</c> if dpad left pressed; otherwise, <c>false</c>.</value>
		public bool DpadDownPressed =>
			CurrentState.DPad.Down == ButtonState.Pressed &&
			PreviousState.DPad.Down == ButtonState.Released;


		/// <summary>
		/// true only the frame the dpad is released
		/// </summary>
		/// <value><c>true</c> if dpad left released; otherwise, <c>false</c>.</value>
		public bool DpadDownReleased =>
			CurrentState.DPad.Down == ButtonState.Released &&
			PreviousState.DPad.Down == ButtonState.Pressed;

		#endregion


		#region Triggers

		public float GetLeftTriggerRaw()
		{
			return CurrentState.Triggers.Left;
		}


		public float GetRightTriggerRaw()
		{
			return CurrentState.Triggers.Right;
		}


		/// <summary>
		/// true whenever the trigger is down past the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger down was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsLeftTriggerDown(float threshold = 0.2f)
		{
			return CurrentState.Triggers.Left > threshold;
		}


		/// <summary>
		/// true only the frame that the trigger passed the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsLeftTriggerPressed(float threshold = 0.2f)
		{
			return CurrentState.Triggers.Left > threshold && PreviousState.Triggers.Left < threshold;
		}


		/// <summary>
		/// true the frame the trigger is released
		/// </summary>
		/// <returns><c>true</c>, if left trigger released was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsLeftTriggerReleased(float threshold = 0.2f)
		{
			return CurrentState.Triggers.Left < threshold && PreviousState.Triggers.Left > threshold;
		}


		/// <summary>
		/// true whenever the trigger is down past the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger down was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsRightTriggerDown(float threshold = 0.2f)
		{
			return CurrentState.Triggers.Right > threshold;
		}


		/// <summary>
		/// true only the frame that the trigger passed the threshold
		/// </summary>
		/// <returns><c>true</c>, if left trigger pressed was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsRightTriggerPressed(float threshold = 0.2f)
		{
			return CurrentState.Triggers.Right > threshold && PreviousState.Triggers.Right < threshold;
		}


		/// <summary>
		/// true the frame the trigger is released
		/// </summary>
		/// <returns><c>true</c>, if left trigger released was ised, <c>false</c> otherwise.</returns>
		/// <param name="threshold">Threshold.</param>
		public bool IsRightTriggerReleased(float threshold = 0.2f)
		{
			return CurrentState.Triggers.Right < threshold && PreviousState.Triggers.Right > threshold;
		}

		#endregion
	}
}