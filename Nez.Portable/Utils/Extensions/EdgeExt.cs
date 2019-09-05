using System;
using System.Runtime.CompilerServices;


namespace Nez
{
	public static class EdgeExt
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static Edge OppositeEdge(this Edge self)
		{
			switch (self)
			{
				case Edge.Bottom:
					return Edge.Top;
				case Edge.Top:
					return Edge.Bottom;
				case Edge.Left:
					return Edge.Right;
				case Edge.Right:
					return Edge.Left;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}


		/// <summary>
		/// returns true if the Edge is Right or Bottom
		/// </summary>
		/// <returns>The max.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsMax(this Edge self)
		{
			return self == Edge.Right || self == Edge.Bottom;
		}


		/// <summary>
		/// returns true if the Edge is Left or Top
		/// </summary>
		/// <returns>The minimum.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsMin(this Edge self)
		{
			return self == Edge.Left || self == Edge.Top;
		}


		/// <summary>
		/// returns true if the Edge is Right or Left
		/// </summary>
		/// <returns>The horizontal.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsHorizontal(this Edge self)
		{
			return self == Edge.Right || self == Edge.Left;
		}


		/// <summary>
		/// returns true if the Edge is Top or Bottom
		/// </summary>
		/// <returns>The vertical.</returns>
		/// <param name="self">Self.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool IsVertical(this Edge self)
		{
			return self == Edge.Top || self == Edge.Bottom;
		}
	}
}