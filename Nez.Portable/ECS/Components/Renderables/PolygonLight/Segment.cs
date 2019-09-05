namespace Nez.Shadows
{
	/// <summary>
	/// Represents an occluding line segment in the visibility mesh
	/// </summary>
	internal class Segment
	{
		/// <summary>
		/// First end-point of the segment
		/// </summary>
		internal EndPoint p1;

		/// <summary>
		/// Second end-point of the segment
		/// </summary>
		internal EndPoint p2;


		internal Segment()
		{
			p1 = null;
			p2 = null;
		}


		public override bool Equals(object obj)
		{
			if (obj is Segment)
			{
				var other = obj as Segment;
				return p1.Equals(other.p1) && p2.Equals(other.p2);
			}

			return false;
		}


		public override int GetHashCode()
		{
			return p1.GetHashCode() + p2.GetHashCode();
		}


		public override string ToString()
		{
			return "{" + p1.position.ToString() + ", " + p2.position.ToString() + "}";
		}
	}
}