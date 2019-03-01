

namespace Nez
{
	public class ScanlinesPostProcessor : PostProcessor<ScanlinesEffect>
	{
		public ScanlinesPostProcessor( int executionOrder ) : base( executionOrder )
		{}
		
		public override void onAddedToScene(Scene scene)
		{
			base.onAddedToScene( scene );
			effect = _scene.content.loadNezEffect<ScanlinesEffect>();
		}

		public override void unload()
		{
			_scene.content.unloadEffect( effect );
			base.unload();
		}

	}
}

