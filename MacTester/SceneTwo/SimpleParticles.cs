using System;
using Nez;
using Microsoft.Xna.Framework;


namespace MacTester
{
	public class SimpleParticles : ParticleSystem
	{
		ParticleType _prototype;


		public SimpleParticles() : base( 500 )
		{
			_prototype = new ParticleType
			{
				color = ColorExt.hexToColor( "B8F8B8" ),
				color2 = ColorExt.hexToColor( "00B800" ),
				colorSwitch = 1,
				size = 8f,
				sizeRange = 13f,
				speed = 4f,
				speedRange = 2f,
				direction = 100f,
				directionRange = MathHelper.Pi,
				acceleration = new Vector2( 0f, -10.15f ),
				life = 3,
				lifeRange = 2,
				scaleOut = true,
				randomRotate = true,
				rotationRate = 0f,
				rotationRateRange = 1f
			};
		}


		public override void update()
		{
			emit( _prototype, 1, entity.scene.camera.screenToWorldPoint( Input.mousePosition ), new Vector2( 15f, 15f ) );
			
			base.update();
		}

	}
}

