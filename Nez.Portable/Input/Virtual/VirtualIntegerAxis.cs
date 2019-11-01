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
		public List<VirtualAxis.Node> Nodes = new List<VirtualAxis.Node>();

		public int Value
		{
			get
			{
				for (var i = 0; i < Nodes.Count; i++)
				{
					var val = Nodes[i].Value;
					if (val != 0)
						return Math.Sign(val);
				}

				return 0;
			}
		}


		public VirtualIntegerAxis()
		{
		}


		public VirtualIntegerAxis(params VirtualAxis.Node[] nodes)
		{
			Nodes.AddRange(nodes);
		}


		public override void Update()
		{
			for (var i = 0; i < Nodes.Count; i++)
				Nodes[i].Update();
		}


		#region Node management

		/// <summary>
		/// adds GamePad left stick X to this VirtualInput
		/// </summary>
		/// <returns>The game pad left stick x.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis AddGamePadLeftStickX(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
		{
			Nodes.Add(new VirtualAxis.GamePadLeftStickX(gamepadIndex, deadzone));
			return this;
		}


		/// <summary>
		/// adds GamePad left stick Y to this VirtualInput
		/// </summary>
		/// <returns>The game pad left stick y.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis AddGamePadLeftStickY(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
		{
			Nodes.Add(new VirtualAxis.GamePadLeftStickY(gamepadIndex, deadzone));
			return this;
		}


		/// <summary>
		/// adds GamePad right stick X to this VirtualInput
		/// </summary>
		/// <returns>The game pad right stick x.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis AddGamePadRightStickX(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
		{
			Nodes.Add(new VirtualAxis.GamePadRightStickX(gamepadIndex, deadzone));
			return this;
		}


		/// <summary>
		/// adds GamePad right stick Y to this VirtualInput
		/// </summary>
		/// <returns>The game pad right stick y.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		/// <param name="deadzone">Deadzone.</param>
		public VirtualIntegerAxis AddGamePadRightStickY(int gamepadIndex = 0, float deadzone = Input.DEFAULT_DEADZONE)
		{
			Nodes.Add(new VirtualAxis.GamePadRightStickY(gamepadIndex, deadzone));
			return this;
		}


		/// <summary>
		/// adds GamePad DPad up/down to this VirtualInput
		/// </summary>
		/// <returns>The game pad DP ad up down.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		public VirtualIntegerAxis AddGamePadDPadUpDown(int gamepadIndex = 0)
		{
			Nodes.Add(new VirtualAxis.GamePadDpadUpDown(gamepadIndex));
			return this;
		}


		/// <summary>
		/// adds GamePad DPad left/right to this VirtualInput
		/// </summary>
		/// <returns>The game pad DP ad left right.</returns>
		/// <param name="gamepadIndex">Gamepad index.</param>
		public VirtualIntegerAxis AddGamePadDPadLeftRight(int gamepadIndex = 0)
		{
			Nodes.Add(new VirtualAxis.GamePadDpadLeftRight(gamepadIndex));
			return this;
		}


		/// <summary>
		/// adds keyboard Keys to emulate left/right or up/down to this VirtualInput
		/// </summary>
		/// <returns>The keyboard keys.</returns>
		/// <param name="overlapBehavior">Overlap behavior.</param>
		/// <param name="negative">Negative.</param>
		/// <param name="positive">Positive.</param>
		public VirtualIntegerAxis AddKeyboardKeys(OverlapBehavior overlapBehavior, Keys negative, Keys positive)
		{
			Nodes.Add(new VirtualAxis.KeyboardKeys(overlapBehavior, negative, positive));
			return this;
		}

		#endregion


		public static implicit operator int(VirtualIntegerAxis axis)
		{
			return axis.Value;
		}
	}
}