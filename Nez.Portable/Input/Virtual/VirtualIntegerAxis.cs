using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;


namespace Nez
{
	/// <summary>
	/// A virtual input that is represented as a int that is either -1, 0, or 1. It corresponds to input that can range from on to nuetral to off
	/// such as GamePad DPad left/right. Can also use two keyboard Keys as the positive/negative checks.
	/// </summary>
	public class VirtualIntegerAxis : VirtualInput
	{
		public List<VirtualAxis.Node> nodes = new List<VirtualAxis.Node>();

		public int value
		{
			get
			{
				for( var i = 0; i < nodes.Count; i++ )
				{
					var val = nodes[i].value;
					if( val != 0 )
						return Math.Sign( val );
				}

				return 0;
			}
		}


		public VirtualIntegerAxis() { }


		public VirtualIntegerAxis( params VirtualAxis.Node[] nodes )
		{
			this.nodes.AddRange( nodes );
		}


		public override void update()
		{
			for( var i = 0; i < nodes.Count; i++ )
				nodes[i].update();
		}


		#region Node management

		/// <summary>
		/// adds GamePad left stick X to this VirtualInput
		/// </summary>
		/// <returns>The game pad left stick x.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis addGamePadLeftStickX( int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE )
		{
			nodes.Add( new VirtualAxis.GamePadLeftStickX( gamepadIndex, deadzone ) );
			return this;
		}


		/// <summary>
		/// adds GamePad left stick Y to this VirtualInput
		/// </summary>
		/// <returns>The game pad left stick y.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis addGamePadLeftStickY( int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE )
		{
			nodes.Add( new VirtualAxis.GamePadLeftStickY( gamepadIndex, deadzone ) );
			return this;
		}


		/// <summary>
		/// adds GamePad right stick X to this VirtualInput
		/// </summary>
		/// <returns>The game pad right stick x.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis addGamePadRightStickX( int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE )
		{
			nodes.Add( new VirtualAxis.GamePadRightStickX( gamepadIndex, deadzone ) );
			return this;
		}


		/// <summary>
		/// adds GamePad right stick Y to this VirtualInput
		/// </summary>
		/// <returns>The game pad right stick y.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis addGamePadRightStickY( int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE )
		{
			nodes.Add( new VirtualAxis.GamePadRightStickY( gamepadIndex, deadzone ) );
			return this;
		}


		/// <summary>
		/// adds GamePad DPad up/down to this VirtualInput
		/// </summary>
		/// <returns>The game pad DP ad up down.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		public VirtualIntegerAxis addGamePadDPadUpDown( int gamepadIndex = 0 )
		{
			nodes.Add( new VirtualAxis.GamePadDpadUpDown( gamepadIndex ) );
			return this;
		}


		/// <summary>
		/// adds GamePad DPad left/right to this VirtualInput
		/// </summary>
		/// <returns>The game pad DP ad left right.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		public VirtualIntegerAxis addGamePadDPadLeftRight( int gamepadIndex = 0 )
		{
			nodes.Add( new VirtualAxis.GamePadDpadLeftRight( gamepadIndex ) );
			return this;
		}


		/// <summary>
		/// adds keyboard Keys to emulate left/right or up/down to this VirtualInput
		/// </summary>
		/// <returns>The keyboard keys.</returns>
		/// <param name="overlapBehavior">Overlap behavior.</param>
		/// <param name="negative">Negative.</param>
		/// <param name="positive">Positive.</param>
		public VirtualIntegerAxis addKeyboardKeys( OverlapBehavior overlapBehavior, Keys negative, Keys positive )
		{
			nodes.Add( new VirtualAxis.KeyboardKeys( overlapBehavior, negative, positive ) );
			return this;
		}

		#endregion


		static public implicit operator int( VirtualIntegerAxis axis )
		{
			return axis.value;
		}

	}
}

