using System;


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


		public RaycastHit( Collider collider, float fraction, float distance )
		{
			this.collider = collider;
			this.fraction = fraction;
			this.distance = distance;
		}


		internal void setValues( Collider collider, float fraction, float distance )
		{
			this.collider = collider;
			this.fraction = fraction;
			this.distance = distance;
		}


		internal void reset()
		{
			collider = null;
			fraction = distance = 0f;
		}

	}
}

