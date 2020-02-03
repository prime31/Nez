using System;


namespace Nez
{
	/// <summary>
	/// utility class to assist with dealing with bitmasks. All methods except isFlagSet expect the flag parameter to be a non-shifted flag.
	/// This lets you use plain old ints (0, 1, 2, 3, etc) to set/unset your flags.
	/// </summary>
	public static class Flags
	{
		/// <summary>
		/// checks to see if the bit flag is set in the int. This check expects flag to be shifted already!
		/// </summary>
		/// <returns><c>true</c>, if flag set was ised, <c>false</c> otherwise.</returns>
		/// <param name="self">Self.</param>
		/// <param name="flag">Flag.</param>
		public static bool IsFlagSet(this int self, int flag)
		{
			return (self & flag) != 0;
		}


		/// <summary>
		/// checks to see if the bit flag is set in the int
		/// </summary>
		/// <returns><c>true</c>, if flag set was ised, <c>false</c> otherwise.</returns>
		/// <param name="self">Self.</param>
		/// <param name="flag">Flag.</param>
		public static bool IsUnshiftedFlagSet(this int self, int flag)
		{
			flag = 1 << flag;
			return (self & flag) != 0;
		}


		/// <summary>
		/// sets the flag bit of the int removing any already set flags
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="flag">Flag.</param>
		public static void SetFlagExclusive(ref int self, int flag)
		{
			self = 1 << flag;
		}


		/// <summary>
		/// sets the flag bit of the int
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="flag">Flag.</param>
		public static void SetFlag(ref int self, int flag)
		{
			self = (self | 1 << flag);
		}


		/// <summary>
		/// unsets the flag bit of the int
		/// </summary>
		/// <param name="self">Self.</param>
		/// <param name="flag">Flag.</param>
		public static void UnsetFlag(ref int self, int flag)
		{
			flag = 1 << flag;
			self = (self & (~flag));
		}


		/// <summary>
		/// inverts the set bits of the int
		/// </summary>
		/// <param name="self">Self.</param>
		public static void InvertFlags(ref int self)
		{
			self = ~self;
		}


		/// <summary>
		/// prints the binary representation of the int. Handy for debugging int flag overlaps visually.
		/// </summary>
		/// <returns>The string representation.</returns>
		/// <param name="self">Self.</param>
		/// <param name="leftPadWidth">Left pad width.</param>
		public static string BinaryStringRepresentation(this int self, int leftPadWidth = 10)
		{
			return Convert.ToString(self, 2).PadLeft(leftPadWidth, '0');
		}
	}
}