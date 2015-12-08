using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class ParticleSystem : RenderableComponent
	{
		public override float width { get { return 5f; } }
		public override float height { get { return 5f; } }

		Particle[] _particles;
		int _nextSlot;


		public ParticleSystem( int maxParticles )
		{
			_particles = new Particle[maxParticles];
		}


		public override void update()
		{
			for( int i = 0; i < _particles.Length; i++ )
				if( _particles[i].isActive )
					_particles[i].update();
		}


		public override void render( Graphics graphics, Camera camera )
		{
			foreach( var p in _particles )
				if( p.isActive )
					p.render( graphics, camera );
		}


		public void emit( ParticleType type, Vector2 position )
		{        
			type.create( ref _particles[_nextSlot], position );
			_nextSlot = ( _nextSlot + 1 ) % _particles.Length;
		}


		public void emit( ParticleType type, int amount, Vector2 position, Vector2 positionRange )
		{
			for( int i = 0; i < amount; i++ )
				emit( type, Random.range( position - positionRange, position + positionRange ) );
		}


		public void emit( ParticleType type, Vector2 position, float direction )
		{
			type.create( ref _particles[_nextSlot], position, direction );
			_nextSlot = ( _nextSlot + 1 ) % _particles.Length;
		}


		public void emit( ParticleType type, int amount, Vector2 position, Vector2 positionRange, float direction )
		{
			for( int i = 0; i < amount; i++ )
				emit( type, Random.range( position - positionRange, positionRange * 2 ), direction );
		}
	}
}
