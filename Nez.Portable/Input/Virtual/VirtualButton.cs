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
		public List<Node> nodes;
		public float bufferTime;
		public float firstRepeatTime;
		public float multiRepeatTime;
		public bool isRepeating { get; private set; }

		float _bufferCounter;
		float _repeatCounter;
		bool _willRepeat;


		public VirtualButton( float bufferTime )
		{
			nodes = new List<Node>();
			this.bufferTime = bufferTime;
		}


		public VirtualButton() : this( 0 )
		{ }


		public VirtualButton( float bufferTime, params Node[] nodes )
		{
			this.nodes = new List<Node>( nodes );
			this.bufferTime = bufferTime;
		}


		public VirtualButton( params Node[] nodes ) : this( 0, nodes )
		{ }


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


		#region Node management

		/// <summary>
		/// adds a keyboard key to this VirtualButton
		/// </summary>
		/// <returns>The keyboard key.</returns>
		/// <param name="key">Key.</param>
		public VirtualButton addKeyboardKey( Keys key )
		{
			nodes.Add( new KeyboardKey( key ) );
			return this;
		}


		/// <summary>
		/// adds a keyboard key with modifier to this VirtualButton. modifier must be in the down state for isPressed/isDown to be true.
		/// </summary>
		/// <returns>The keyboard key.</returns>
		/// <param name="key">Key.</param>
		/// <param name="modifier">Modifier.</param>
		public VirtualButton addKeyboardKey( Keys key, Keys modifier )
		{
			nodes.Add( new KeyboardModifiedKey( key, modifier ) );
			return this;
		}


		/// <summary>
		/// adds a GamePad buttons press to this VirtualButton
		/// </summary>
		/// <returns>The game pad button.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="button">Button.</param>
		public VirtualButton addGamePadButton( int gamepadIndex, Buttons button )
		{
			nodes.Add( new GamePadButton( gamepadIndex, button ) );
			return this;
		}


		/// <summary>
		/// adds a GamePad left trigger press to this VirtualButton
		/// </summary>
		/// <returns>The game pad left trigger.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="threshold">Threshold.</param>
		public VirtualButton addGamePadLeftTrigger( int gamepadIndex, float threshold )
		{
			nodes.Add( new GamePadLeftTrigger( gamepadIndex, threshold ) );
			return this;
		}


		/// <summary>
		/// adds a GamePad right trigger press to this VirtualButton
		/// </summary>
		/// <returns>The game pad right trigger.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="threshold">Threshold.</param>
		public VirtualButton addGamePadRightTrigger( int gamepadIndex, float threshold )
		{
			nodes.Add( new GamePadRightTrigger( gamepadIndex, threshold ) );
			return this;
		}


		/// <summary>
		/// adds a GamePad DPad press to this VirtualButton
		/// </summary>
		/// <returns>The game pad DP ad.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="direction">Direction.</param>
		public VirtualButton addGamePadDPad( int gamepadIndex, Direction direction )
		{
			switch( direction )
			{
				case Direction.Up:
					nodes.Add( new GamePadDPadUp( gamepadIndex ) );
					break;
				case Direction.Down:
					nodes.Add( new GamePadDPadDown( gamepadIndex ) );
					break;
				case Direction.Left:
					nodes.Add( new GamePadDPadLeft( gamepadIndex ) );
					break;
				case Direction.Right:
					nodes.Add( new GamePadDPadRight( gamepadIndex ) );
					break;
			}

			return this;
		}


		/// <summary>
		/// adds a left mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse left button.</returns>
		public VirtualButton addMouseLeftButton()
		{
			nodes.Add( new MouseLeftButton() );
			return this;
		}


		/// <summary>
		/// adds a right mouse click to this VirtualButton
		/// </summary>
		/// <returns>The mouse right button.</returns>
		public VirtualButton addMouseRightButton()
		{
			nodes.Add( new MouseRightButton() );
			return this;
		}

		#endregion


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


		#region Keyboard

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


		/// <summary>
		/// works like KeyboardKey except the modifier key must also be down for isDown/isPressed to be true. isReleased checks only key.
		/// </summary>
		public class KeyboardModifiedKey : Node
		{
			public Keys key;
			public Keys modifier;


			public KeyboardModifiedKey( Keys key, Keys modifier )
			{
				this.key = key;
				this.modifier = modifier;
			}


			public override bool isDown
			{
				get { return Input.isKeyDown( modifier ) && Input.isKeyDown( key ); }
			}


			public override bool isPressed
			{
				get { return Input.isKeyDown( modifier ) && Input.isKeyPressed( key ); }
			}


			public override bool isReleased
			{
				get { return Input.isKeyReleased( key ); }
			}
		}

		#endregion


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

