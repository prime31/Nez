namespace Nez
{
	public class ScanlinesPostProcessor : PostProcessor<ScanlinesEffect>
	{
		public ScanlinesPostProcessor(int executionOrder) : base(executionOrder)
		{
		}

		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);
			Effect = _scene.Content.LoadNezEffect<ScanlinesEffect>();
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(Effect);
			base.Unload();
		}
	}
}