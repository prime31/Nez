using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public struct Particle
	{
		public ParticleType type;
		public bool isActive;
		public Color color;
		public Vector2 position;
		public Vector2 speed;
		public float size;
		public float life;
		public float colorSwitch;
		public float rotation;
		public float rotationRate;
		public float sizeChange;

		public Vector2 renderPosition { get { return Mathf.floor( position ); } }


		public void update()
		{
			// Life
			life -= Time.deltaTime;
			if( life <= 0 )
			{
				isActive = false;
				return;
			}

			// Color switch
			if( colorSwitch > 0 )
			{
				colorSwitch -= Time.deltaTime;
				if( colorSwitch <= 0 )
				{
					if( type.colorSwitchLoop )
						colorSwitch = type.colorSwitch;

					if( color == type.color )
						color = type.color2;
					else
						color = type.color;
				}
			}

			// Speed
			position += speed * Time.deltaTime;
			speed += type.acceleration * Time.deltaTime;
			if( type.speedMultiplier != 1 )
				speed *= (float)Math.Pow( type.speedMultiplier, Time.deltaTime );

			// rotation
			rotation += rotationRate * Time.deltaTime;

			// Scale Out
			size += sizeChange * Time.deltaTime;
		}


		public void render( Graphics graphics, Camera camera )
		{
			if( type.subtexture == null )
				graphics.spriteBatch.Draw( graphics.particleTexture, renderPosition, graphics.particleTexture.sourceRect, color, rotation, Vector2.One, size * 0.5f, SpriteEffects.None, 0 );
			else
				graphics.spriteBatch.Draw( type.subtexture.texture2D, renderPosition, type.subtexture.sourceRect, color, rotation, type.subtexture.center, size, SpriteEffects.None, 0 );
		}

	}
}

