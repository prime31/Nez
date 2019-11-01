using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	/// <summary>
	/// A virtual input that is represented as a boolean. As well as simply checking the current button state, you can ask whether
	/// it was just pressed or released this frame. You can also keep the button press stored in a buffer for a limited time, or
	/// until it is consumed by calling consumeBuffer()
	/// </summary>
	public class VirtualButton : VirtualInput
	{
		public List<Node> Nodes;
		public float BufferTime;
		public float FirstRepeatTime;
		public float MultiRepeatTime;
		public bool IsRepeating { get; private set; }

		float _bufferCounter;
		float _repeatCounter;
		bool _willRepeat;


		public VirtualButton(float bufferTime)
		{
			Nodes = new List<Node>();
			BufferTime = bufferTime;
		}


		public VirtualButton() : this(0)
		{
		}


		public VirtualButton(float bufferTime, params Node[] nodes)
		{
			Nodes = new List<Node>(nodes);
			BufferTime = bufferTime;
		}


		public VirtualButton(params Node[] nodes) : this(0, nodes)
		{
		}


		public void SetRepeat(float repeatTime)
		{
			SetRepeat(repeatTime, repeatTime);
		}


		public void SetRepeat(float firstRepeatTime, float multiRepeatTime)
		{
			FirstRepeatTime = firstRepeatTime;
			MultiRepeatTime = multiRepeatTime;
			_willRepeat = firstRepeatTime > 0;
			if (!_willRepeat)
				IsRepeating = false;
		}


		public override void Update()
		{
			_bufferCounter -= Time.UnscaledDeltaTime;
			IsRepeating = false;

			var check = false;
			for (var i = 0; i < Nodes.Count; i++)
			{
				Nodes[i].Update();
				if (Nodes[i].IsPressed)
				{
					_bufferCounter = BufferTime;
					check = true;
				}
				else if (Nodes[i].IsDown)
				{
					check = true;
				}
			}

			if (!check)
			{
				_repeatCounter = 0;
				_bufferCounter = 0;
			}
			else if (_willRepeat)
			{
				if (_repeatCounter == 0)
				{
					_repeatCounter = FirstRepeatTime;
				}
				else
				{
					_repeatCounter -= Time.UnscaledDeltaTime;
					if (_repeatCounter <= 0)
					{
						IsRepeating = true;
						_repeatCounter = MultiRepeatTime;
					}
				}
			}
		}


		public bool IsDown
		{
			get
			{
				foreach (var node in Nodes)
					if (node.IsDown)
						return true;

				return false;
			}
		}


		public bool IsPressed
		{
			get
			{
				if (_bufferCounter > 0 || IsRepeating)
					return true;

				foreach (var node in Nodes)
					if (node.IsPressed)
						return true;

				return false;
			}
		}


		public bool IsReleased
		{
			get
			{
				foreach (var node in Nodes)
					if (node.IsReleased)
						return true;

				return false;
			}
		}


		public void ConsumeBuffer()
		{
			_bufferCounter = 0;
		}


		#region Node management

		/// <summary>
		/// adds a keyboard key to this VirtualButton
		/// </summary>
		/// <returns>The keyboard key.</returns>
		/// <param name="key">Key.</param>
		public VirtualButton AddKeyboardKey(Keys key)
		{
			Nodes.Add(new KeyboardKey(key));
			return this;
		}


		/// <summary>
		/// adds a keyboard key with modifier to this VirtualButton. modifier must be in the down state for isPressed/isDown to be true.
		/// </summary>
		/// <returns>The keyboard key.</returns>
		/// <param name="key">Key.</param>
		/// <param name="modifier">Modifier.</param>
		public VirtualButton AddKeyboardKey(Keys key, Keys modifier)
		{
			Nodes.Add(new KeyboardModifiedKey(key, modifier));
			return this;
		}


		/// <summary>
		/// adds a GamePad buttons press to this VirtualButton
		/// </summary>
		/// <returns>The game pad button.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="button">Button.</param>
		public VirtualButton AddGamePadButton(int gamepadIndex, Buttons button)
		{
			Nodes.Add(new GamePadButton(gamepadIndex, button));
			return this;
		}


		/// <summary>
		/// adds a GamePad left trigger press to this VirtualButton
		/// </summary>
		/// <returns>The game pad left trigger.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="threshold">Threshold.</param>
		public VirtualButton AddGamePadLeftTrigger(int gamepadIndex, float threshold)
		{
			Nodes.Add(new GamePadLeftTrigger(gamepadIndex, threshold));
			return this;
		}


		/// <summary>
		/// adds a GamePad right trigger press to this VirtualButton
		/// </summary>
		/// <returns>The game pad right trigger.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="threshold">Threshold.</param>
		public VirtualButton AddGamePadRightTrigger(int gamepadIndex, float threshold)
		{
			Nodes.Add(new GamePadRightTrigger(gamepadIndex, threshold));
			return this;
		}


		/// <summary>
		/// adds a GamePad DPad press to this VirtualButton
		/// </summary>
		/// <returns>The game pad DP ad.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="direction">Direction.</param>
		public VirtualButton AddGamePadDPad(int gamepadIndex, Direction direction)
		{
			switch (direction)
			{
				case Direction.Up:
					Nodes.Add(new GamePadDPadUp(gamepadIndex));
					break;
				case Direction.Down:
					Nodes.Add(new GamePadDPadDown(gamepadIndex));
					break;
				case Direction.Left:
					Nodes.Add(new GamePadDPadLeft(gamepadIndex));
					break;
				case Direction.Right:
					Nodes.Add(new GamePadDPadRight(gamepadIndex));
					break;
			}

			return this;
		}


		/// <summary>
		/// adds a left mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse left button.</returns>
		public VirtualButton AddMouseLeftButton()
		{
			Nodes.Add(new MouseLeftButton());
			return this;
		}


		/// <summary>
		/// adds a right mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse right button.</returns>
		public VirtualButton AddMouseRightButton()
		{
			Nodes.Add(new MouseRightButton());
			return this;
		}


		/// <summary>
		/// adds a right mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse right button.</returns>
		public VirtualButton AddMouseMiddleButton()
		{
			Nodes.Add(new MouseMiddleButton());
			return this;
		}


		/// <summary>
		/// adds a right mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse right button.</returns>
		public VirtualButton AddMouseFirstExtendedButton()
		{
			Nodes.Add(new MouseFirstExtendedButton());
			return this;
		}


		/// <summary>
		/// adds a right mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse right button.</returns>
		public VirtualButton AddMouseSecondExtendedButton()
		{
			Nodes.Add(new MouseSecondExtendedButton());
			return this;
		}

		#endregion


		public static implicit operator bool(VirtualButton button)
		{
			return button.IsDown;
		}


		#region Node types

		public abstract class Node : VirtualInputNode
		{
			public abstract bool IsDown { get; }
			public abstract bool IsPressed { get; }
			public abstract bool IsReleased { get; }
		}


		#region Keyboard

		public class KeyboardKey : Node
		{
			public Keys Key;


			public KeyboardKey(Keys key)
			{
				Key = key;
			}


			public override bool IsDown => Input.IsKeyDown(Key);


			public override bool IsPressed => Input.IsKeyPressed(Key);


			public override bool IsReleased => Input.IsKeyReleased(Key);
		}


		/// <summary>
		/// works like KeyboardKey except the modifier key must also be down for isDown/isPressed to be true. isReleased checks only key.
		/// </summary>
		public class KeyboardModifiedKey : Node
		{
			public Keys Key;
			public Keys Modifier;


			public KeyboardModifiedKey(Keys key, Keys modifier)
			{
				Key = key;
				Modifier = modifier;
			}


			public override bool IsDown => Input.IsKeyDown(Modifier) && Input.IsKeyDown(Key);


			public override bool IsPressed => Input.IsKeyDown(Modifier) && Input.IsKeyPressed(Key);


			public override bool IsReleased => Input.IsKeyReleased(Key);
		}

		#endregion


		#region GamePad Buttons and Triggers

		public class GamePadButton : Node
		{
			public int GamepadIndex;
			public Buttons Button;


			public GamePadButton(int gamepadIndex, Buttons button)
			{
				GamepadIndex = gamepadIndex;
				Button = button;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].IsButtonDown(Button);


			public override bool IsPressed => Input.GamePads[GamepadIndex].IsButtonPressed(Button);


			public override bool IsReleased => Input.GamePads[GamepadIndex].IsButtonReleased(Button);
		}


		public class GamePadLeftTrigger : Node
		{
			public int GamepadIndex;
			public float Threshold;


			public GamePadLeftTrigger(int gamepadIndex, float threshold)
			{
				GamepadIndex = gamepadIndex;
				Threshold = threshold;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].IsLeftTriggerDown(Threshold);

			public override bool IsPressed => Input.GamePads[GamepadIndex].IsLeftTriggerPressed(Threshold);

			public override bool IsReleased => Input.GamePads[GamepadIndex].IsLeftTriggerReleased(Threshold);
		}


		public class GamePadRightTrigger : Node
		{
			public int GamepadIndex;
			public float Threshold;


			public GamePadRightTrigger(int gamepadIndex, float threshold)
			{
				GamepadIndex = gamepadIndex;
				Threshold = threshold;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].IsRightTriggerDown(Threshold);

			public override bool IsPressed => Input.GamePads[GamepadIndex].IsRightTriggerPressed(Threshold);

			public override bool IsReleased => Input.GamePads[GamepadIndex].IsRightTriggerReleased(Threshold);
		}

		#endregion


		#region GamePad DPad

		public class GamePadDPadRight : Node
		{
			public int GamepadIndex;


			public GamePadDPadRight(int gamepadIndex)
			{
				GamepadIndex = gamepadIndex;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].DpadRightDown;

			public override bool IsPressed => Input.GamePads[GamepadIndex].DpadRightPressed;

			public override bool IsReleased => Input.GamePads[GamepadIndex].DpadRightReleased;
		}


		public class GamePadDPadLeft : Node
		{
			public int GamepadIndex;


			public GamePadDPadLeft(int gamepadIndex)
			{
				GamepadIndex = gamepadIndex;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].DpadLeftDown;

			public override bool IsPressed => Input.GamePads[GamepadIndex].DpadLeftPressed;

			public override bool IsReleased => Input.GamePads[GamepadIndex].DpadLeftReleased;
		}


		public class GamePadDPadUp : Node
		{
			public int GamepadIndex;


			public GamePadDPadUp(int gamepadIndex)
			{
				GamepadIndex = gamepadIndex;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].DpadUpDown;

			public override bool IsPressed => Input.GamePads[GamepadIndex].DpadUpPressed;

			public override bool IsReleased => Input.GamePads[GamepadIndex].DpadUpReleased;
		}


		public class GamePadDPadDown : Node
		{
			public int GamepadIndex;


			public GamePadDPadDown(int gamepadIndex)
			{
				GamepadIndex = gamepadIndex;
			}


			public override bool IsDown => Input.GamePads[GamepadIndex].DpadDownDown;

			public override bool IsPressed => Input.GamePads[GamepadIndex].DpadDownPressed;

			public override bool IsReleased => Input.GamePads[GamepadIndex].DpadDownReleased;
		}

		#endregion


		#region Mouse

		public class MouseLeftButton : Node
		{
			public override bool IsDown => Input.LeftMouseButtonDown;

			public override bool IsPressed => Input.LeftMouseButtonPressed;

			public override bool IsReleased => Input.LeftMouseButtonReleased;
		}


		public class MouseRightButton : Node
		{
			public override bool IsDown => Input.RightMouseButtonDown;

			public override bool IsPressed => Input.RightMouseButtonPressed;

			public override bool IsReleased => Input.RightMouseButtonReleased;
		}


		public class MouseMiddleButton : Node
		{
			public override bool IsDown => Input.MiddleMouseButtonDown;

			public override bool IsPressed => Input.MiddleMouseButtonPressed;

			public override bool IsReleased => Input.MiddleMouseButtonReleased;
		}


		public class MouseFirstExtendedButton : Node
		{
			public override bool IsDown => Input.FirstExtendedMouseButtonDown;

			public override bool IsPressed => Input.FirstExtendedMouseButtonPressed;

			public override bool IsReleased => Input.FirstExtendedMouseButtonReleased;
		}


		public class MouseSecondExtendedButton : Node
		{
			public override bool IsDown => Input.SecondExtendedMouseButtonDown;

			public override bool IsPressed => Input.SecondExtendedMouseButtonPressed;

			public override bool IsReleased => Input.SecondExtendedMouseButtonReleased;
		}

		#endregion

		#endregion
	}
}