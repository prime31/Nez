using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Post Processing step for rendering actions after everthing done.
	/// </summary>
	public class PostProcessor : IComparable<PostProcessor>
	{
		/// <summary>
		/// Step is Enabled or not.
		/// </summary>
		public bool Enabled;

		/// <summary>
		/// specifies the order in which the Renderers will be called by the scene
		/// </summary>
		public readonly int ExecutionOrder = 0;

		/// <summary>
		/// The effect used to render with
		/// </summary>
		public Effect Effect;

		/// <summary>
		/// SamplerState used for the drawFullscreenQuad method
		/// </summary>
		public SamplerState SamplerState = Core.DefaultSamplerState;

		/// <summary>
		/// BlendState used by the drawFullsceenQuad method
		/// </summary>
		public BlendState BlendState = BlendState.Opaque;

		/// <summary>
		/// the Scene this PostProcessor is attached to or null
		/// </summary>
		protected internal Scene _scene;


		public PostProcessor(int executionOrder, Effect effect = null)
		{
			Enabled = true;
			ExecutionOrder = executionOrder;
			Effect = effect;
		}

		/// <summary>
		/// called when the PostProcessor is added to the Scene. Subclasses must base!
		/// </summary>
		/// <param name="scene">Scene.</param>
		public virtual void OnAddedToScene(Scene scene)
		{
			_scene = scene;
		}

		/// <summary>
		/// called when the default scene RenderTarget is resized. If a PostProcessor is added to a scene before it begins this method will be
		/// called before the scene first renders. If the scene already started this will be called after onAddedToScene making it an ideal place
		/// to create any RenderTextures a PostProcessor might require.
		/// </summary>
		/// <param name="newWidth">New width.</param>
		/// <param name="newHeight">New height.</param>
		public virtual void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
		}

		/// <summary>
		/// this is the meat method here. The source passed in contains the full scene with any previous PostProcessors
		/// rendering. Render it into the destination RenderTarget. The drawFullScreenQuad methods are there to make
		/// the process even easier. The default implementation renders source into destination with effect.
		/// 
		/// Note that destination might have a previous render! If your PostProcessor Effect is discarding you should clear
		/// the destination before writing to it!
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public virtual void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			DrawFullscreenQuad(source, destination, Effect);
		}

		/// <summary>
		/// called when a scene is ended or this PostProcessor is removed. use this for cleanup.
		/// </summary>
		public virtual void Unload()
		{
			// Nez-specific Effects will have a null name. We don't want to try to remove them.
			if (Effect != null && Effect.Name != null)
			{
				_scene.Content.UnloadEffect(Effect);
				Effect = null;
			}

			_scene = null;
		}

		/// <summary>
		/// helper for drawing a texture into a rendertarget, optionally using a custom shader to apply postprocessing effects.
		/// </summary>
		protected void DrawFullscreenQuad(Texture2D texture, RenderTarget2D renderTarget, Effect effect = null)
		{
			Core.GraphicsDevice.SetRenderTarget(renderTarget);
			DrawFullscreenQuad(texture, renderTarget.Width, renderTarget.Height, effect);
		}

		/// <summary>
		/// helper for drawing a texture into the current rendertarget, optionally using a custom shader to apply postprocessing effects.
		/// </summary>
		protected void DrawFullscreenQuad(Texture2D texture, int width, int height, Effect effect)
		{
			Graphics.Instance.Batcher.Begin(BlendState, SamplerState, DepthStencilState.None, RasterizerState.CullNone,
				effect);
			Graphics.Instance.Batcher.Draw(texture, new Rectangle(0, 0, width, height), Color.White);
			Graphics.Instance.Batcher.End();
		}

		public int CompareTo(PostProcessor other)
		{
			return ExecutionOrder.CompareTo(other.ExecutionOrder);
		}
	}
}