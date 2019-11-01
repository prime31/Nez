using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	/// <summary>
	/// A virtual input that is represented as a Vector2, with both X and Y as values between -1 and 1
	/// </summary>
	public class VirtualJoystick : VirtualInput
	{
		public List<Node> Nodes = new List<Node>();
		public bool Normalized;

		public Vector2 Value
		{
			get
			{
				for (int i = 0; i < Nodes.Count; i++)
				{
					var val = Nodes[i].Value;
					if (val != Vector2.Zero)
					{
						if (Normalized)
							val.Normalize();
						return val;
					}
				}

				return Vector2.Zero;
			}
		}


		public VirtualJoystick(bool normalized) : base()
		{
			Normalized = normalized;
		}


		public VirtualJoystick(bool normalized, params Node[] nodes) : base()
		{
			Normalized = normalized;
			Nodes.AddRange(nodes);
		}


		public override void Update()
		{
			for (int i = 0; i < Nodes.Count; i++)
				Nodes[i].Update();
		}


		#region Node management

		/// <summary>
		/// adds GamePad left stick input to this VirtualJoystick
		/// </summary>
		/// <returns>The game pad left stick.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualJoystick AddGamePadLeftStick(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
		{
			Nodes.Add(new GamePadLeftStick(gamepadIndex, deadzone));
			return this;
		}


		/// <summary>
		/// adds GamePad right stick input to this VirtualJoystick
		/// </summary>
		/// <returns>The game pad right stick.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualJoystick AddGamePadRightStick(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
		{
			Nodes.Add(new GamePadRightStick(gamepadIndex, deadzone));
			return this;
		}


		/// <summary>
		/// adds GamePad DPad input to this VirtualJoystick
		/// </summary>
		/// <returns>The game pad DP ad.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		public VirtualJoystick AddGamePadDPad(int gamepadIndex = 0)
		{
			Nodes.Add(new GamePadDpad(gamepadIndex));
			return this;
		}


		/// <summary>
		/// adds keyboard keys input to this VirtualJoystick. Four keyboard keys will emulate left/right/up/down. For example WASD or the arrow
		/// keys.
		/// </summary>
		/// <returns>The keyboard keys.</returns>
		/// <param name="overlapBehavior">Overlap behavior.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		/// <param name="up">Up.</param>
		/// <param name="down">Down.</param>
		public VirtualJoystick AddKeyboardKeys(OverlapBehavior overlapBehavior, Keys left, Keys right, Keys up,
		                                       Keys down)
		{
			Nodes.Add(new KeyboardKeys(overlapBehavior, left, right, up, down));
			return this;
		}

		#endregion


		public static implicit operator Vector2(VirtualJoystick joystick)
		{
			return joystick.Value;
		}


		#region Node types

		public abstract class Node : VirtualInputNode
		{
			public abstract Vector2 Value { get; }
		}


		public class GamePadLeftStick : Node
		{
			public int GamepadIndex;
			public float Deadzone;


			public GamePadLeftStick(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
			{
				GamepadIndex = gamepadIndex;
				Deadzone = deadzone;
			}


			public override Vector2 Value => Input.GamePads[GamepadIndex].GetLeftStick(Deadzone);
		}


		public class GamePadRightStick : Node
		{
			public int GamepadIndex;
			public float Deadzone;


			public GamePadRightStick(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
			{
				GamepadIndex = gamepadIndex;
				Deadzone = deadzone;
			}

			public override Vector2 Value => Input.GamePads[GamepadIndex].GetRightStick(Deadzone);
		}


		public class GamePadDpad : Node
		{
			public int GamepadIndex;


			public GamePadDpad(int gamepadIndex = 0)
			{
				GamepadIndex = gamepadIndex;
			}


			public override Vector2 Value
			{
				get
				{
					var _value = Vector2.Zero;

					if (Input.GamePads[GamepadIndex].DpadRightDown)
						_value.X = 1f;
					else if (Input.GamePads[GamepadIndex].DpadLeftDown)
						_value.X = -1f;

					if (Input.GamePads[GamepadIndex].DpadDownDown)
						_value.Y = 1f;
					else if (Input.GamePads[GamepadIndex].DpadUpDown)
						_value.Y = -1f;

					return _value;
				}
			}
		}


		public class KeyboardKeys : Node
		{
			public OverlapBehavior OverlapBehavior;
			public Keys Left;
			public Keys Right;
			public Keys Up;
			public Keys Down;

			private bool _turnedX;
			private bool _turnedY;
			private Vector2 _value;


			public KeyboardKeys(OverlapBehavior overlapBehavior, Keys left, Keys right, Keys up, Keys down)
			{
				OverlapBehavior = overlapBehavior;
				Left = left;
				Right = right;
				Up = up;
				Down = down;
			}


			public override void Update()
			{
				//X Axis
				if (Input.IsKeyDown(Left))
				{
					if (Input.IsKeyDown(Right))
					{
						switch (OverlapBehavior)
						{
							default:
							case OverlapBehavior.CancelOut:
								_value.X = 0;
								break;
							case OverlapBehavior.TakeNewer:
								if (!_turnedX)
								{
									_value.X *= -1;
									_turnedX = true;
								}

								break;
							case OverlapBehavior.TakeOlder:
								//X stays the same
								break;
						}
					}
					else
					{
						_turnedX = false;
						_value.X = -1;
					}
				}
				else if (Input.IsKeyDown(Right))
				{
					_turnedX = false;
					_value.X = 1;
				}
				else
				{
					_turnedX = false;
					_value.X = 0;
				}

				//Y Axis
				if (Input.IsKeyDown(Up))
				{
					if (Input.IsKeyDown(Down))
					{
						switch (OverlapBehavior)
						{
							default:
							case OverlapBehavior.CancelOut:
								_value.Y = 0;
								break;
							case OverlapBehavior.TakeNewer:
								if (!_turnedY)
								{
									_value.Y *= -1;
									_turnedY = true;
								}

								break;
							case OverlapBehavior.TakeOlder:
								//Y stays the same
								break;
						}
					}
					else
					{
						_turnedY = false;
						_value.Y = -1;
					}
				}
				else if (Input.IsKeyDown(Down))
				{
					_turnedY = false;
					_value.Y = 1;
				}
				else
				{
					_turnedY = false;
					_value.Y = 0;
				}
			}


			public override Vector2 Value => _value;
		}

		#endregion
	}
}