using System;
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
		public List<Node> nodes = new List<Node>();
		public bool normalized;

		public Vector2 value
		{
			get
			{
				for( int i = 0; i < nodes.Count; i++ )
				{
					var value = nodes[i].value;
					if( value != Vector2.Zero )
					{
						if( normalized )
							value.Normalize();
						return value;
					}
				}

				return Vector2.Zero;
			}
		}


		public VirtualJoystick( bool normalized ) : base()
		{
			this.normalized = normalized;
		}


		public VirtualJoystick( bool normalized, params Node[] nodes ) : base()
		{
			this.normalized = normalized;
			this.nodes.AddRange( nodes );
		}


		public override void update()
		{
			for( int i = 0; i < nodes.Count; i++ )
				nodes[i].update();
		}


		static public implicit operator Vector2( VirtualJoystick joystick )
		{
			return joystick.value;
		}


		#region Node types

		public abstract class Node : VirtualInputNode
		{
			public abstract Vector2 value { get; }
		}


		public class PadLeftStick : Node
		{
			public int gamepadIndex;
			public float deadzone;


			public PadLeftStick( int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE )
			{
				this.gamepadIndex = gamepadIndex;
				this.deadzone = deadzone;
			}


			public override Vector2 value
			{
				get
				{
					return Input.gamePads[gamepadIndex].getLeftStick( deadzone );
				}
			}
		}


		public class PadRightStick : Node
		{
			public int gamepadIndex;
			public float deadzone;


			public PadRightStick( int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE )
			{
				this.gamepadIndex = gamepadIndex;
				this.deadzone = deadzone;
			}

			public override Vector2 value
			{
				get
				{
					return Input.gamePads[gamepadIndex].getRightStick( deadzone );
				}
			}
		}


		public class GamePadDpad : Node
		{
			public int gamepadIndex;


			public GamePadDpad( int gamepadIndex = 0 )
			{
				this.gamepadIndex = gamepadIndex;
			}


			public override Vector2 value
			{
				get
				{
					var _value = Vector2.Zero;

					if( Input.gamePads[gamepadIndex].DpadRightDown )
						_value.X = 1f;
					else if( Input.gamePads[gamepadIndex].DpadLeftDown )
						_value.X = -1f;

					if( Input.gamePads[gamepadIndex].DpadDownDown )
						_value.Y = 1f;
					else if( Input.gamePads[gamepadIndex].DpadUpDown )
						_value.Y = -1f;

					return _value;
				}
			}
		}


		public class KeyboardKeys : Node
		{
			public OverlapBehavior overlapBehavior;
			public Keys left;
			public Keys right;
			public Keys up;
			public Keys down;

			private bool _turnedX;
			private bool _turnedY;
			private Vector2 _value;


			public KeyboardKeys( OverlapBehavior overlapBehavior, Keys left, Keys right, Keys up, Keys down )
			{
				this.overlapBehavior = overlapBehavior;
				this.left = left;
				this.right = right;
				this.up = up;
				this.down = down;
			}


			public override void update()
			{
				//X Axis
				if( Input.isKeyDown( left ) )
				{
					if( Input.isKeyDown( right ) )
					{
						switch( overlapBehavior )
						{
							default:
							case OverlapBehavior.CancelOut:
								_value.X = 0;
								break;
							case OverlapBehavior.TakeNewer:
								if( !_turnedX )
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
				else if( Input.isKeyDown( right ) )
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
				if( Input.isKeyDown( up ) )
				{
					if( Input.isKeyDown( down ) )
					{
						switch( overlapBehavior )
						{
							default:
							case OverlapBehavior.CancelOut:
								_value.Y = 0;
								break;
							case OverlapBehavior.TakeNewer:
								if( !_turnedY )
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
				else if( Input.isKeyDown( down ) )
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


			public override Vector2 value
			{
				get { return _value; }
			}
		}

		#endregion

	}
}

