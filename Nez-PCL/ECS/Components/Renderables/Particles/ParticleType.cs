using System;
using System.Collections.Generic;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;


namespace Nez
{
	public class ParticleType
	{
		static private List<ParticleType> AllTypes = new List<ParticleType>();

		[ContentSerializerIgnore]
		public Subtexture subtexture;
		public Color color;
		public Color color2;
		public int colorSwitch;
		public bool colorSwitchLoop;
		public float speed;
		public float speedRange;
		public float speedMultiplier;
		public Vector2 acceleration;
		public float direction;
		public float directionRange;
		public int life;
		public int lifeRange;
		public float size;
		public float sizeRange;
		public bool rotated;
		public bool randomRotate;
		public float rotationRate;
		public float rotationRateRange;
		public bool scaleOut;


		public ParticleType()
		{
			color = color2 = Color.White;
			colorSwitch = 0;
			colorSwitchLoop = true;
			speed = speedRange = 0;
			speedMultiplier = 1;
			acceleration = Vector2.Zero;
			direction = directionRange = 0;
			life = lifeRange = 0;
			size = 2;
			sizeRange = 0;
			rotated = true;

			AllTypes.Add( this );
		}


		public ParticleType( ParticleType copy )
		{
			subtexture = copy.subtexture;
			color = copy.color;
			color2 = copy.color2;
			colorSwitch = copy.colorSwitch;
			colorSwitchLoop = copy.colorSwitchLoop;
			speed = copy.speed;
			speedRange = copy.speedRange;
			speedMultiplier = copy.speedMultiplier;
			acceleration = copy.acceleration;
			direction = copy.direction;
			directionRange = copy.directionRange;
			life = copy.life;
			lifeRange = copy.lifeRange;
			size = copy.size;
			sizeRange = copy.sizeRange;
			rotated = copy.rotated;
			randomRotate = copy.randomRotate;
			rotationRate = copy.rotationRate;
			rotationRateRange = copy.rotationRateRange;
			scaleOut = copy.scaleOut;

			AllTypes.Add( this );
		}


		public Particle create( ref Particle particle, Vector2 position )
		{
			return create( ref particle, position, direction );
		}


		public Particle create( ref Particle particle, Vector2 position, float direction )
		{
			particle.type = this;
			particle.isActive = true;
			particle.position = position;

			if( subtexture == null )
				particle.size = (int)Random.range( size - sizeRange, size + sizeRange );
			else
				particle.size = Random.range( size - sizeRange, size + sizeRange );
			
			particle.color = color;
			particle.speed = Mathf.angleToVector( direction - directionRange / 2 + Random.nextFloat() * directionRange, Random.range( speed - speedRange, speed + speedRange ) );
			particle.life = life + Random.range( life - lifeRange, life + lifeRange );
			particle.colorSwitch = colorSwitch;

			if( randomRotate )
			{
				particle.rotation = Random.nextAngle();
				particle.rotationRate = Random.range( rotationRate - rotationRateRange, rotationRate + rotationRateRange );
			}
			else if( rotated )
			{
				particle.rotation = direction;
			}

			if( scaleOut )
				particle.sizeChange = -( particle.size / ( particle.life * 2f ) );
			else
				particle.sizeChange = 0;

			return particle;
		}

	}
}

