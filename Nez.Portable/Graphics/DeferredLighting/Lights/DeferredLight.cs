namespace Nez.DeferredLighting
{
	public abstract class DeferredLight : RenderableComponent
	{
		/// <summary>
		/// we dont render lights normally so this method will do nothing and never be called. The DeferredLightingRenderer takes care of
		/// light rendering so that it can cache and reuse the light meshes.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="camera">Camera.</param>
		public override void Render(Batcher batcher, Camera camera)
		{
		}
	}
}