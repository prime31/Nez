using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// optional interface that can be added to any object for special cases where the final render to screen needs to be overridden. Note that
	/// the Scene.screenshotRequestCallback will not function as expected if an IFinalRenderDelegate is present. This is because the
	/// screenshot will grab the RenderTarget before the IFinalRenderDelegate does its thing.
	/// </summary>
	public interface IFinalRenderDelegate
	{
		/// <summary>
		/// called when added to the Scene
		/// </summary>
		/// <param name="scene"></param>
		void OnAddedToScene(Scene scene);

		/// <summary>
		/// called when the back buffer size changes
		/// </summary>
		/// <param name="newWidth"></param>
		/// <param name="newHeight"></param>
		void OnSceneBackBufferSizeChanged(int newWidth, int newHeight);

		/// <summary>
		/// this gets called by a Scene so that the final render can be handled. The render should be done into finalRenderTarget.
		/// In most cases, finalRenderTarget will be null so the render will just be to the backbuffer. The only time finalRenderTarget
		/// will be set is the first frame of a SceneTransition where the transition has requested the previous Scene render.
		/// </summary>
		/// <param name="finalRenderTarget"></param>
		/// <param name="letterboxColor"></param>
		/// <param name="source"></param>
		/// <param name="finalRenderDestinationRect"></param>
		/// <param name="samplerState"></param>
		/// <returns></returns>
		void HandleFinalRender(RenderTarget2D finalRenderTarget, Color letterboxColor, RenderTarget2D source,
		                       Rectangle finalRenderDestinationRect, SamplerState samplerState);

		/// <summary>
		/// called when a Scene ends. Release any resources here.
		/// </summary>
		void Unload();
	}
}