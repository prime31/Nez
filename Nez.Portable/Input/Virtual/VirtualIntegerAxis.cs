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

		public float FirstRepeatTime;
		public float MultiRepeatTime;
		public bool IsRepeating { get; private set; }

		float _repeatCounter;
		bool _willRepeat;
		int _repeatingDirection = 0;

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


		/// <summary>
		/// The direction that this axis was pushed (or repeated, if repeating is enabled) this frame.
		/// </summary>
		/// <value>-1 or 1 if it was just pushed in that direction, or 0 if it was not.</value>
		public int DirectionJustPushed
		{
			get
			{
				if (IsRepeating)
					return _repeatingDirection;
				
				foreach (var node in Nodes)
				{
					if (node.JustPushed(-1))
						return -1;
					else if(node.JustPushed(1))
						return 1;
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

		/// <summary>
		/// Returns true if this input was pushed in a direction (from 0) this frame. If SetRepeat() is used, this will
		/// repeatedly return true after the provided intervals.
		/// </summary>
		/// <param name="direction">The direction to check, should be -1 or 1.</param>
		/// <returns></returns>
		public bool JustPushed(int direction)
		{
			if (IsRepeating && _repeatingDirection == direction)
				return true;

			foreach (var node in Nodes)
				if (node.JustPushed(direction))
					return true;

			return false;
		}


		/// <summary>
		/// Set the repeat interval used for JustPushed().
		/// </summary>
		/// <param name="repeatTime">Interval between repetitions.</param>
		public void SetRepeat(float repeatTime)
		{
			SetRepeat(repeatTime, repeatTime);
		}


		/// <summary>
		/// Set the repeat interval used for JustPushed().
		/// </summary>
		/// <param name="firstRepeatTime">Delay after the initial push.</param>
		/// <param name="multiRepeatTime">Delay after subsequent repetitions.</param>
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
			IsRepeating = false;

			bool check = false;
			for (var i = 0; i < Nodes.Count; i++)
			{
				Nodes[i].Update();

				int direction = Nodes[i].DirectionJustPushed;
				if (direction != 0)
				{
					_repeatCounter = FirstRepeatTime;
					_repeatingDirection = direction;
					check = true;
				}
				else if (Nodes[i].Value == _repeatingDirection)
				{
					check = true;
				}
			}

			if(check && _willRepeat)
			{
				_repeatCounter -= Time.UnscaledDeltaTime;
				if (_repeatCounter <= 0)
				{
					IsRepeating = true;
					_repeatCounter = MultiRepeatTime;
				}
			}
			if(!check)
			{
				_repeatingDirection = 0;
			}
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
