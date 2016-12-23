using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public struct RaycastHit
	{
		/// <summary>
		/// The collider hit by the ray
		/// </summary>
		public Collider collider;

		/// <summary>
		/// Fraction of the distance along the ray that the hit occurred.
		/// </summary>
		public float fraction;

		/// <summary>
		/// The distance from the ray origin to the impact point
		/// </summary>
		public float distance;

		/// <summary>
		/// The point in world space where the ray hit the collider's surface
		/// </summary>
		public Vector2 point;

		/// <summary>
		/// The normal vector of the surface hit by the ray
		/// </summary>
		public Vector2 normal;

		/// <summary>
		/// The centroid of the primitive used to perform the cast. Where the shape would be positioned for it to contact.
		/// </summary>
		public Vector2 centroid;


		public RaycastHit( Collider collider, float fraction, float distance, Vector2 point, Vector2 normal )
		{
			this.collider = collider;
			this.fraction = fraction;
			this.distance = distance;
			this.point = point;
			this.normal = normal;
			this.centroid = Vector2.Zero;
		}


		internal void setValues( Collider collider, float fraction, float distance, Vector2 point )
		{
			this.collider = collider;
			this.fraction = fraction;
			this.distance = distance;
			this.point = point;
		}


		internal void setValues( float fraction, float distance, Vector2 point, Vector2 normal )
		{
			this.fraction = fraction;
			this.distance = distance;
			this.point = point;
			this.normal = normal;
		}


		internal void reset()
		{
			collider = null;
			fraction = distance = 0f;
		}


		public override string ToString()
		{
			return string.Format( "[RaycastHit] fraction: {0}, distance: {1}, normal: {2}, centroid: {3}, point: {4}", fraction, distance, normal, centroid, point );
		}

	}
}

