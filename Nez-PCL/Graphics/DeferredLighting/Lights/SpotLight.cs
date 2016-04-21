using System;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	public class SpotLight : PointLight
	{
		/// <summary>
		/// wrapper for entity.transform.rotation to ease in setting up direction of spots to point at specific locations
		/// </summary>
		public Vector2 direction
		{
			get
			{
				return new Vector2( Mathf.cos( entity.transform.rotation ), Mathf.sin( entity.transform.rotation ) );
			}
		}

		/// <summary>
		/// angle in degrees of the cone
		/// </summary>
		public float coneAngle = 90f;


		public SpotLight() : base()
		{}


		public SpotLight( Color color )
		{
			this.color = color;
		}


		#region Point light setters

		public new SpotLight setZPosition( float z )
		{
			zPosition = z;
			return this;
		}


		/// <summary>
		/// how far does this light reach
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="radius">Radius.</param>
		public new SpotLight setRadius( float radius )
		{
			base.setRadius( radius );
			return this;
		}


		/// <summary>
		/// brightness of the light
		/// </summary>
		/// <returns>The intensity.</returns>
		/// <param name="intensity">Intensity.</param>
		public new SpotLight setIntensity( float intensity )
		{
			this.intensity = intensity;
			return this;
		}

		#endregion


		public SpotLight setConeAngle( float coneAngle )
		{
			this.coneAngle = coneAngle;
			return this;
		}


		/// <summary>
		/// wrapper for entity.transform.rotation to ease in setting up direction of spots to point at specific locations
		/// </summary>
		/// <returns>The direction.</returns>
		/// <param name="direction">Direction.</param>
		public SpotLight setDirection( Vector2 direction )
		{
			entity.transform.rotation = (float)Math.Atan2( direction.Y, direction.X );
			return this;
		}
	}
}

