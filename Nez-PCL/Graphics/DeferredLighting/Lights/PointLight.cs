using System;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// PointLights radiate light in a circle. Note that PointLights are affected by Transform.scale. The Transform.scale.X value is multiplied
	/// by the lights radius when sent to the GPU. It is expected that scale will be linear.
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


		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public override void debugRender( Graphics graphics )
		{
			graphics.batcher.drawCircle( entity.transform.position + _localOffset, radius * entity.transform.scale.X, Color.DarkOrchid, 2 );
		}

	}
}

