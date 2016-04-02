using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	/// <summary>
	/// A virtual input that is represented as a boolean. As well as simply checking the current button state, you can ask whether it was just pressed or released this frame. You can also keep the button press stored in a buffer for a limited time, or until it is consumed by calling ConsumeBuffer()
	/// </summary>
	public class VirtualButton : VirtualInput
	{
		public List<Node> nodes;
		public float bufferTime;
		public float firstRepeatTime;
		public float multiRepeatTime;
		public bool isRepeating { get; private set; }

		float _bufferCounter;
		float _repeatCounter;
		bool _willRepeat;


		public VirtualButton( float bufferTime ) : base()
		{
			nodes = new List<Node>();
			this.bufferTime = bufferTime;
		}


		public VirtualButton() : this( 0 )
		{}


		public VirtualButton( float bufferTime, params Node[] nodes ) : base()
		{
			this.nodes = new List<Node>( nodes );
			this.bufferTime = bufferTime;
		}


		public VirtualButton( params Node[] nodes ) : this( 0, nodes )
		{}


		public void setRepeat( float repeatTime )
		{
			setRepeat( repeatTime, repeatTime );
		}


		public void setRepeat( float firstRepeatTime, float multiRepeatTime )
		{
			this.firstRepeatTime = firstRepeatTime;
			this.multiRepeatTime = multiRepeatTime;
			_willRepeat = firstRepeatTime > 0;
			if( !_willRepeat )
				isRepeating = false;
		}


		public override void update()
		{
			_bufferCounter -= Time.unscaledDeltaTime;

			var check = false;
			for( var i = 0; i < nodes.Count; i++ )
			{
				nodes[i].update();
				if( nodes[i].isPressed )
				{
					_bufferCounter = bufferTime;
					check = true;
				}
				else if( nodes[i].isDown )
				{
					check = true;
				}
			}

			if( !check )
			{
				_repeatCounter = 0;
				_bufferCounter = 0;
			}
			else if( _willRepeat )
			{
				isRepeating = false;
				if( _repeatCounter == 0 )
				{
					_repeatCounter = firstRepeatTime;
				}
				else
				{
					_repeatCounter -= Time.unscaledDeltaTime;
					if( _repeatCounter <= 0 )
					{
						isRepeating = true;
						_repeatCounter = multiRepeatTime;
					}
				}
			}
		}


		public bool isDown
		{
			get
			{
				foreach( var node in nodes )
					if( node.isDown )
						return true;
				return false;
			}
		}


		public bool isPressed
		{
			get
			{
				if( _bufferCounter > 0 || isRepeating )
					return true;

				foreach( var node in nodes )
					if( node.isPressed )
						return true;
				return false;
			}
		}


		public bool isReleased
		{
			get
			{
				foreach( var node in nodes )
					if( node.isReleased )
						return true;
				return false;
			}
		}


		public void consumeBuffer()
		{
			_bufferCounter = 0;
		}


		static public implicit operator bool( VirtualButton button )
		{
			return button.isDown;
		}


		#region Node types

		public abstract class Node : VirtualInputNode
		{
			public abstract bool isDown { get; }
			public abstract bool isPressed { get; }
			public abstract bool isReleased { get; }
		}


		public class KeyboardKey : Node
		{
			public Keys key;


			public KeyboardKey( Keys key )
			{
				this.key = key;
			}


			public override bool isDown
			{
				get { return Input.isKeyDown( key ); }
			}


			public override bool isPressed
			{
				get { return Input.isKeyPressed( key ); }
			}


			public override bool isReleased
			{
				get { return Input.isKeyReleased( key ); }
			}
		}


		#region GamePad Buttons and Triggers

		public class GamePadButton : Node
		{
			public int gamepadIndex;
			public Buttons button;


			public GamePadButton( int gamepadIndex, Buttons button )
			{
				this.gamepadIndex = gamepadIndex;
				this.button = button;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].isButtonDown( button ); }
			}


			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].isButtonPressed( button ); }
			}


			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].isButtonReleased( button ); }
			}
		}


		public class GamePadLeftTrigger : Node
		{
			public int gamepadIndex;
			public float threshold;


			public GamePadLeftTrigger( int gamepadIndex, float threshold )
			{
				this.gamepadIndex = gamepadIndex;
				this.threshold = threshold;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].isLeftTriggerDown( threshold ); }
			}

			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].isLeftTriggerPressed( threshold ); }
			}

			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].isLeftTriggerReleased( threshold ); }
			}
		}


		public class GamePadRightTrigger : Node
		{
			public int gamepadIndex;
			public float threshold;


			public GamePadRightTrigger( int gamepadIndex, float threshold )
			{
				this.gamepadIndex = gamepadIndex;
				this.threshold = threshold;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].isRightTriggerDown( threshold ); }
			}

			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].isRightTriggerPressed( threshold ); }
			}

			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].isRightTriggerReleased( threshold ); }
			}
		}

		#endregion


		#region GamePad DPad

		public class GamePadDPadRight : Node
		{
			public int gamepadIndex;


			public GamePadDPadRight( int gamepadIndex )
			{
				this.gamepadIndex = gamepadIndex;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].DpadRightDown; }
			}

			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].DpadRightPressed; }
			}

			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].DpadRightReleased; }
			}
		}


		public class GamePadDPadLeft : Node
		{
			public int gamepadIndex;


			public GamePadDPadLeft( int gamepadIndex )
			{
				this.gamepadIndex = gamepadIndex;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].DpadLeftDown; }
			}

			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].DpadLeftPressed; }
			}

			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].DpadLeftReleased; }
			}
		}


		public class GamePadDPadUp : Node
		{
			public int gamepadIndex;


			public GamePadDPadUp( int gamepadIndex )
			{
				this.gamepadIndex = gamepadIndex;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].DpadUpDown; }
			}

			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].DpadUpPressed; }
			}

			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].DpadUpReleased; }
			}
		}


		public class GamePadDPadDown : Node
		{
			public int gamepadIndex;


			public GamePadDPadDown( int gamepadIndex )
			{
				this.gamepadIndex = gamepadIndex;
			}


			public override bool isDown
			{
				get { return Input.gamePads[gamepadIndex].DpadDownDown; }
			}

			public override bool isPressed
			{
				get { return Input.gamePads[gamepadIndex].DpadDownPressed; }
			}

			public override bool isReleased
			{
				get { return Input.gamePads[gamepadIndex].DpadDownReleased; }
			}
		}

		#endregion


		#region Mouse

		public class MouseLeftButton : Node
		{
			public override bool isDown
			{
				get { return Input.leftMouseButtonDown; }
			}

			public override bool isPressed
			{
				get { return Input.leftMouseButtonPressed; }
			}

			public override bool isReleased
			{
				get { return Input.leftMouseButtonReleased; }
			}
		}


		public class MouseRightButton : Node
		{
			public override bool isDown
			{
				get { return Input.rightMouseButtonDown; }
			}

			public override bool isPressed
			{
				get { return Input.rightMouseButtonPressed; }
			}

			public override bool isReleased
			{
				get { return Input.rightMouseButtonReleased; }
			}
		}

		#endregion

		#endregion

	}
}

