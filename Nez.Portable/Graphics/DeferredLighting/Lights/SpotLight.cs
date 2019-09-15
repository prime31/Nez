using System;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	public class SpotLight : PointLight
	{
		/// <summary>
		/// wrapper for entity.transform.rotation to ease in setting up direction of spots to point at specific locations
		/// </summary>
		public Vector2 Direction => new Vector2(Mathf.Cos(Entity.Transform.Rotation), Mathf.Sin(Entity.Transform.Rotation));

		/// <summary>
		/// angle in degrees of the cone
		/// </summary>
		public float ConeAngle = 90f;


		public SpotLight() : base()
		{
		}

		public SpotLight(Color color)
		{
			Color = color;
		}


		#region Point light setters

		public new SpotLight SetZPosition(float z)
		{
			ZPosition = z;
			return this;
		}

		/// <summary>
		/// how far does this light reach
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="radius">Radius.</param>
		public new SpotLight SetRadius(float radius)
		{
			base.SetRadius(radius);
			return this;
		}

		/// <summary>
		/// brightness of the light
		/// </summary>
		/// <returns>The intensity.</returns>
		/// <param name="intensity">Intensity.</param>
		public new SpotLight SetIntensity(float intensity)
		{
			Intensity = intensity;
			return this;
		}

		#endregion


		public SpotLight SetConeAngle(float coneAngle)
		{
			ConeAngle = coneAngle;
			return this;
		}

		/// <summary>
		/// wrapper for entity.transform.rotation to ease in setting up direction of spots to point at specific locations
		/// </summary>
		/// <returns>The direction.</returns>
		/// <param name="direction">Direction.</param>
		public SpotLight SetDirection(Vector2 direction)
		{
			Entity.Transform.Rotation = (float) Math.Atan2(direction.Y, direction.X);
			return this;
		}
	}
}