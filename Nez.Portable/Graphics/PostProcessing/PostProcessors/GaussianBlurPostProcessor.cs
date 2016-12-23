using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


namespace Nez
{
	public class GaussianBlurPostProcessor : PostProcessor<GaussianBlurEffect>
	{
		/// <summary>
		/// scale of the internal RenderTargets. For high resolution renders a half sized RT is usually more than enough. Defaults to 1.
		/// </summary>
		public float renderTargetScale
		{
			get { return _renderTargetScale; }
			set
			{
				if( _renderTargetScale != value )
				{
					_renderTargetScale = value;
					updateEffectDeltas();
				}
			}
		}

		float _renderTargetScale = 1f;


		public GaussianBlurPostProcessor( int executionOrder ) : base( executionOrder, new GaussianBlurEffect() )
		{}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			updateEffectDeltas();
		}


		/// <summary>
		/// updates the Effect with the new vertical and horizontal deltas
		/// </summary>
		void updateEffectDeltas()
		{
			var sceneRenderTargetSize = scene.sceneRenderTargetSize;
			effect.horizontalBlurDelta = 1f / ( sceneRenderTargetSize.X * _renderTargetScale );
			effect.verticalBlurDelta = 1f / ( sceneRenderTargetSize.Y * _renderTargetScale );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			// aquire a temporary rendertarget for the processing. It can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
			// the resolution in this way doesn't hurt quality, because we are going to be blurring the images in any case.
			var sceneRenderTargetSize = scene.sceneRenderTargetSize;
			var tempRenderTarget = RenderTarget.getTemporary( (int)( sceneRenderTargetSize.X * _renderTargetScale ), (int)( sceneRenderTargetSize.Y * _renderTargetScale ), DepthFormat.None );


			// Pass 1: draw from source into tempRenderTarget, applying a horizontal gaussian blur filter.
			effect.prepareForHorizontalBlur();
			drawFullscreenQuad( source, tempRenderTarget, effect );

			// Pass 2: draw from tempRenderTarget into destination, applying a vertical gaussian blur filter.
			effect.prepareForVerticalBlur();
			drawFullscreenQuad( tempRenderTarget, destination, effect );

			RenderTarget.releaseTemporary( tempRenderTarget );
		}

	}
}
