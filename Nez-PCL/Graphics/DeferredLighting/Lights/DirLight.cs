using System;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// directional light with a shortended name to avoid clashes with the XNA DirectionalLight. This light type has only a direction and is
	/// never culled. It is a global light and the only light that produces specular highlights.
	/// </summary>
	public class DirLight : DeferredLight
	{
		// dir lights are infinite
		public override float width { get { return float.MaxValue; } }

		public override float height { get { return float.MinValue; } }

		/// <summary>
		/// direction of the light
		/// </summary>
		public Vector3 direction = new Vector3( 50, 20, 100 );

		/// <summary>
		/// specular intensity. 0 - 1 range
		/// </summary>
		public float specularIntensity = 0.5f;

		/// <summary>
		/// specular power. this is the exponent passed to pow() of the projection from 0,0,-1 to the light-to-normal
		/// </summary>
		public float specularPower = 2;


		public DirLight()
		{}


		public DirLight( Color color )
		{
			this.color = color;
		}


		public DirLight( Color color, Vector3 lightDirection ) : this( color )
		{
			this.direction = lightDirection;
		}


		public DirLight setDirection( Vector3 direction )
		{
			this.direction = direction;
			return this;
		}


		public DirLight setSpecularIntensity( float specularIntensity )
		{
			this.specularIntensity = specularIntensity;
			return this;
		}


		public DirLight setSpecularPower( float specularPower )
		{
			this.specularPower = specularPower;
			return this;
		}


		/// <summary>
		/// we dont want to render our bounds so we just render a direction
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public override void debugRender( Graphics graphics )
		{
			// figure out a starting corner for the line
			var root = Vector2.Zero;
			if( direction.Y > 0 )
				root.Y = 10f;
			else
				root.Y = Screen.height - 10;

			if( direction.X > 0 )
				root.X = 10;
			else
				root.X = Screen.width - 10;
			
			var angle = Mathf.atan2( direction.Y, direction.X );
			graphics.batcher.drawLineAngle( root, angle, 100, Color.Red, 3 );
		}
	}
}

