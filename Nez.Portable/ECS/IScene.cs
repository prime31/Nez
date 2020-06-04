using Microsoft.Xna.Framework.Graphics;

namespace Nez
{
	public interface IScene
	{
		void Begin();
		void End();
		void Update();
		void Render();

		/// <summary>
		/// any PostProcessors present get to do their processing then we do the final render of the RenderTarget to the screen.
		/// In almost all cases finalRenderTarget will be null. The only time it will have a value is the first frame of a
		/// SceneTransition if the transition is requesting the render.
		/// </summary>
		/// <returns>The render.</returns>
		void PostRender(RenderTarget2D finalRenderTarget = null);
	}
}