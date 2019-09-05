using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez
{
	public class GaussianBlurPostProcessor : PostProcessor<GaussianBlurEffect>
	{
		/// <summary>
		/// scale of the internal RenderTargets. For high resolution renders a half sized RT is usually more than enough. Defaults to 1.
		/// </summary>
		public float RenderTargetScale
		{
			get => _renderTargetScale;
			set
			{
				if (_renderTargetScale != value)
				{
					_renderTargetScale = value;
					UpdateEffectDeltas();
				}
			}
		}

		float _renderTargetScale = 1f;


		public GaussianBlurPostProcessor(int executionOrder) : base(executionOrder)
		{
		}

		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);
			Effect = _scene.Content.LoadNezEffect<GaussianBlurEffect>();
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(Effect);
			base.Unload();
		}

		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			UpdateEffectDeltas();
		}

		/// <summary>
		/// updates the Effect with the new vertical and horizontal deltas
		/// </summary>
		void UpdateEffectDeltas()
		{
			var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
			Effect.HorizontalBlurDelta = 1f / (sceneRenderTargetSize.X * _renderTargetScale);
			Effect.VerticalBlurDelta = 1f / (sceneRenderTargetSize.Y * _renderTargetScale);
		}

		public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			// aquire a temporary rendertarget for the processing. It can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
			// the resolution in this way doesn't hurt quality, because we are going to be blurring the images in any case.
			var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
			var tempRenderTarget = RenderTarget.GetTemporary((int) (sceneRenderTargetSize.X * _renderTargetScale),
				(int) (sceneRenderTargetSize.Y * _renderTargetScale), DepthFormat.None);


			// Pass 1: draw from source into tempRenderTarget, applying a horizontal gaussian blur filter.
			Effect.PrepareForHorizontalBlur();
			DrawFullscreenQuad(source, tempRenderTarget, Effect);

			// Pass 2: draw from tempRenderTarget into destination, applying a vertical gaussian blur filter.
			Effect.PrepareForVerticalBlur();
			DrawFullscreenQuad(tempRenderTarget, destination, Effect);

			RenderTarget.ReleaseTemporary(tempRenderTarget);
		}
	}
}