using System;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// point light
	/// </summary>
	public class PointLight : DeferredLight
	{
		public override float width { get { return _radius * 2; } }

		public override float height { get { return _radius * 2; } }

		/// <summary>
		/// "height" above the scene in the z direction
		/// </summary>
		public float zPosition = 150f;
		
		/// <summary>
		/// how far does this light reaches
		/// </summary>
		public float radius { get { return _radius; } }

		/// <summary>
		/// brightness of the light
		/// </summary>
		public float intensity = 3f;


		protected float _radius;


		public PointLight()
		{
			setRadius( 400f );
		}


		public PointLight( Color color ) : this()
		{
			this.color = color;
		}


		public PointLight setZPosition( float z )
		{
			zPosition = z;
			return this;
		}


		/// <summary>
		/// how far does this light reach
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="radius">Radius.</param>
		public PointLight setRadius( float radius )
		{
			_radius = radius;
			originNormalized = Vector2Ext.halfVector();
			_areBoundsDirty = true;
			return this;
		}


		/// <summary>
		/// brightness of the light
		/// </summary>
		/// <returns>The intensity.</returns>
		/// <param name="intensity">Intensity.</param>
		public PointLight setIntensity( float intensity )
		{
			this.intensity = intensity;
			return this;
		}

	}
}

