using Microsoft.Xna.Framework;


namespace Nez.Verlet
{
	/// <summary>
	/// single Particle composite
	/// </summary>
	public class Ball : Composite
	{
		public Ball( Vector2 position, float radius = 10 )
		{
			addParticle( new Particle( position ) ).radius = radius;
		}
	}
}
