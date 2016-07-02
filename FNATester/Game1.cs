using Nez;


namespace FNATester
{
	public class Game1 : Core
	{
		protected override void Initialize()
		{
			base.Initialize();

			//scene = Scene.createWithDefaultRenderer( Color.Orchid );
			scene = new TestScene();
		}
	}
}

