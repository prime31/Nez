using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Shadows
{
	/// <summary>    
	/// The end-point of a segment    
	/// </summary>
	internal class EndPoint
	{
		/// <summary>
		/// Position of the segment
		/// </summary>
		internal Vector2 position;

		/// <summary>
		/// If this end-point is a begin or end end-point
		/// of a segment (each segment has only one begin and one end end-point
		/// </summary>
		internal bool begin;

		/// <summary>
		/// The segment this end-point belongs to
		/// </summary>
		internal Segment segment;

		/// <summary>
		/// The angle of the end-point relative to the location of the visibility test
		/// </summary>
		internal float angle;


		internal EndPoint()
		{
			position = Vector2.Zero;
			begin = false;
			segment = null;
			angle = 0;
		}


		public override bool Equals(object obj)
		{
			if (obj is EndPoint)
			{
				var other = obj as EndPoint;
				return position.Equals(other.position) && begin.Equals(other.begin) && angle.Equals(other.angle);

				// We do not care about the segment being the same since that would create a circular reference
			}

			return false;
		}


		public override int GetHashCode()
		{
			return position.GetHashCode() + begin.GetHashCode() + angle.GetHashCode();
		}


		public override string ToString()
		{
			return "{ p:" + position.ToString() + "a: " + angle + " in " + segment.ToString() + "}";
		}
	}


	internal class EndPointComparer : IComparer<EndPoint>
	{
		internal EndPointComparer()
		{
		}


		// Helper: comparison function for sorting points by angle
		public int Compare(EndPoint a, EndPoint b)
		{
			// Traverse in angle order
			if (a.angle > b.angle)
				return 1;

			if (a.angle < b.angle)
				return -1;

			// But for ties we want Begin nodes before End nodes
			if (!a.begin && b.begin)
				return 1;

			if (a.begin && !b.begin)
				return -1;

			return 0;
		}
	}
}