using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FarseerScene : Scene
	{
		/// <summary>
		/// the Farseer World. Responsible for all physics simulation
		/// </summary>
		public World world;

		/// <summary>
		/// minimum delta time step for the simulation. The min of Time.deltaTime and this will be used for the physics step
		/// </summary>
		public float minimumUpdateDeltaTime = 1f / 30;


		public override void initialize()
		{
			world = new World( new Vector2( 0, 9.82f ) );
		}


		public override void update()
		{
			world.Step( MathHelper.Min( Time.deltaTime, minimumUpdateDeltaTime ) );
			base.update();
		}


		public override void unload()
		{
			world.Clear();
		}


		public static implicit operator World( FarseerScene self )
		{
			return self.world;
		}

	}
}
