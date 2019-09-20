using Microsoft.Xna.Framework;


namespace Nez
{
	public struct RaycastHit
	{
		/// <summary>
		/// The collider hit by the ray
		/// </summary>
		public Collider Collider;

		/// <summary>
		/// Fraction of the distance along the ray that the hit occurred.
		/// </summary>
		public float Fraction;

		/// <summary>
		/// The distance from the ray origin to the impact point
		/// </summary>
		public float Distance;

		/// <summary>
		/// The point in world space where the ray hit the collider's surface
		/// </summary>
		public Vector2 Point;

		/// <summary>
		/// The normal vector of the surface hit by the ray
		/// </summary>
		public Vector2 Normal;

		/// <summary>
		/// The centroid of the primitive used to perform the cast. Where the shape would be positioned for it to contact.
		/// </summary>
		public Vector2 Centroid;


		public RaycastHit(Collider collider, float fraction, float distance, Vector2 point, Vector2 normal)
		{
			Collider = collider;
			Fraction = fraction;
			Distance = distance;
			Point = point;
			Normal = normal;
			Centroid = Vector2.Zero;
		}


		internal void SetValues(Collider collider, float fraction, float distance, Vector2 point)
		{
			Collider = collider;
			Fraction = fraction;
			Distance = distance;
			Point = point;
		}


		internal void SetValues(float fraction, float distance, Vector2 point, Vector2 normal)
		{
			Fraction = fraction;
			Distance = distance;
			Point = point;
			Normal = normal;
		}


		internal void Reset()
		{
			Collider = null;
			Fraction = Distance = 0f;
		}


		public override string ToString()
		{
			return string.Format("[RaycastHit] fraction: {0}, distance: {1}, normal: {2}, centroid: {3}, point: {4}",
				Fraction, Distance, Normal, Centroid, Point);
		}
	}
}