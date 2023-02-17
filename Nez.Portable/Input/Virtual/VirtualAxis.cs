using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	/// <summary>
	/// A virtual input represented as a float between -1 and 1
	/// </summary>
	public class VirtualAxis : VirtualInput
	{
		public List<Node> Nodes = new List<Node>();

		public float Value
		{
			get
			{
				for (var i = 0; i < Nodes.Count; i++)
				{
					var val = Nodes[i].Value;
					if (val != 0)
						return val;
				}

				return 0;
			}
		}


		public VirtualAxis()
		{
		}


		public VirtualAxis(params Node[] nodes)
		{
			Nodes.AddRange(nodes);
		}


		public override void Update()
		{
			for (var i = 0; i < Nodes.Count; i++)
				Nodes[i].Update();
		}


		public static implicit operator float(VirtualAxis axis)
		{
			return axis.Value;
		}


		#region Node types

		public abstract class Node : VirtualInputNode
		{
			public abstract float Value { get; }

			protected float _previousValue = 0;
			protected bool _justPushed = false;

			public int DirectionJustPushed => _justPushed ? System.Math.Sign(Value) : 0;

			public bool JustPushed(int direction)
			{
				return Value == direction && _justPushed;
			}

			public override void Update()
			{
				_justPushed = false;
				if(Value != 0 && _previousValue == 0)
					_justPushed = true;

				_previousValue = Value;
			}
		}


		public class GamePadLeftStickX : Node
		{
			public int GamepadIndex;
			public float Deadzone;


			public GamePadLeftStickX(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
			{
				GamepadIndex = gamepadIndex;
				Deadzone = deadzone;
			}

			public override float Value => Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetLeftStick(Deadzone).X, Deadzone);
		}


		public class GamePadLeftStickY : Node
		{
			/// <summary>
			/// if true, pressing up will return -1 and down will return 1 matching GamePadDpadUpDown
			/// </summary>
			public bool InvertResult = true;

			public int GamepadIndex;
			public float Deadzone;


			public GamePadLeftStickY(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
			{
				GamepadIndex = gamepadIndex;
				Deadzone = deadzone;
			}

			public override float Value
			{
				get
				{
					var multiplier = InvertResult ? -1 : 1;
					return multiplier *
					       Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetLeftStick(Deadzone).Y, Deadzone);
				}
			}
		}


		public class GamePadRightStickX : Node
		{
			public int GamepadIndex;
			public float Deadzone;


			public GamePadRightStickX(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
			{
				GamepadIndex = gamepadIndex;
				Deadzone = deadzone;
			}

			public override float Value => Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetRightStick(Deadzone).X, Deadzone);
		}


		public class GamePadRightStickY : Node
		{
			public int GamepadIndex;
			public float Deadzone;


			public GamePadRightStickY(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
			{
				GamepadIndex = gamepadIndex;
				Deadzone = deadzone;
			}

			public override float Value => Mathf.SignThreshold(Input.GamePads[GamepadIndex].GetRightStick(Deadzone).Y, Deadzone);
		}


		public class GamePadDpadLeftRight : Node
		{
			public int GamepadIndex;


			public GamePadDpadLeftRight(int gamepadIndex = 0)
			{
				GamepadIndex = gamepadIndex;
			}


			public override float Value
			{
				get
				{
					if (Input.GamePads[GamepadIndex].DpadRightDown)
						return 1f;
					else if (Input.GamePads[GamepadIndex].DpadLeftDown)
						return -1f;
					else
						return 0f;
				}
			}
		}


		public class GamePadDpadUpDown : Node
		{
			public int GamepadIndex;


			public GamePadDpadUpDown(int gamepadIndex = 0)
			{
				GamepadIndex = gamepadIndex;
			}


			public override float Value
			{
				get
				{
					if (Input.GamePads[GamepadIndex].DpadDownDown)
						return 1f;
					else if (Input.GamePads[GamepadIndex].DpadUpDown)
						return -1f;
					else
						return 0f;
				}
			}
		}


		public class KeyboardKeys : Node
		{
			public OverlapBehavior OverlapBehavior;
			public Keys Positive;
			public Keys Negative;

			float _value;
			bool _turned;


			public KeyboardKeys(OverlapBehavior overlapBehavior, Keys negative, Keys positive)
			{
				OverlapBehavior = overlapBehavior;
				Negative = negative;
				Positive = positive;
			}


			public override void Update()
			{
				if (Input.IsKeyDown(Positive))
				{
					if (Input.IsKeyDown(Negative))
					{
						switch (OverlapBehavior)
						{
							default:
							case OverlapBehavior.CancelOut:
								_value = 0;
								break;

							case OverlapBehavior.TakeNewer:
								if (!_turned)
								{
									_value *= -1;
									_turned = true;
								}

								break;
							case OverlapBehavior.TakeOlder:
								//value stays the same
								break;
						}
					}
					else
					{
						_turned = false;
						_value = 1;
					}
				}
				else if (Input.IsKeyDown(Negative))
				{
					_turned = false;
					_value = -1;
				}
				else
				{
					_turned = false;
					_value = 0;
				}

				_justPushed = false;
				if(_value != 0 && _previousValue == 0)
				{
					_justPushed = true;
				}
				_previousValue = Value;
			}


			public override float Value => _value;
		}

		#endregion
	}
}
