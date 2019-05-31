using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez
{
	/// <summary>
	/// post processor to assist with making blended poly lights. Usage is as follows:
	/// - render all sprite lights with a separate Renderer to a RenderTarget. The clear color of the Renderer is your ambient light color.
	/// - render all normal objects in standard fashion
	/// - add this PostProcessor with the RenderTarget from your lights Renderer
	/// </summary>
	public class PolyLightPostProcessor : PostProcessor
	{
		/// <summary>
		/// multiplicative factor for the blend of the base and light render targets. Defaults to 1.
		/// </summary>
		/// <value>The multiplicative factor.</value>
		public float multiplicativeFactor
		{
			get => _multiplicativeFactor;
			set => setMultiplicativeFactor( value );
		}

		/// <summary>
		/// enables/disables a gaussian blur of the light texture before it is combined with the scene render
		/// </summary>
		/// <value><c>true</c> if enable blur; otherwise, <c>false</c>.</value>
		public bool enableBlur
		{
			get => _blurEnabled;
			set => setEnableBlur( value );
		}

		/// <summary>
		/// scale of the internal RenderTargets used for the blur. For high resolution renders a half sized RT is usually more than enough.
		/// Defaults to 0.5.
		/// </summary>
		public float blurRenderTargetScale
		{
			get => _blurRenderTargetScale;
			set => setBlurRenderTargetScale( value );
		}

		/// <summary>
		/// amount to blur. A range of 0.5 - 6 works well. Defaults to 2.
		/// </summary>
		/// <value>The blur amount.</value>
		public float blurAmount
		{
			get => _blurEffect != null ? _blurEffect.blurAmount : -1;
			set
			{
				if( _blurEffect != null )
					_blurEffect.blurAmount = value;
			}
		}

		float _multiplicativeFactor = 1f;
		bool _blurEnabled;
		float _blurRenderTargetScale = 0.5f;

		GaussianBlurEffect _blurEffect;
		RenderTexture _lightsRenderTexture;


		public PolyLightPostProcessor( int executionOrder, RenderTexture lightsRenderTexture ) : base( executionOrder )
		{
			_lightsRenderTexture = lightsRenderTexture;
		}

		/// <summary>
		/// updates the GaussianBlurEffect with the new vertical and horizontal deltas after a back buffer size or blurRenderTargetScale change
		/// </summary>
		void updateBlurEffectDeltas()
		{
			var sceneRenderTargetSize = _scene.sceneRenderTargetSize;
			_blurEffect.horizontalBlurDelta = 1f / ( sceneRenderTargetSize.X * _blurRenderTargetScale );
			_blurEffect.verticalBlurDelta = 1f / ( sceneRenderTargetSize.Y * _blurRenderTargetScale );
		}


		#region chainable setters

		public PolyLightPostProcessor setMultiplicativeFactor( float multiplicativeFactor )
		{
			_multiplicativeFactor = multiplicativeFactor;
			if( effect != null )
				effect.Parameters["_multiplicativeFactor"].SetValue( multiplicativeFactor );
			
			return this;
		}

		public PolyLightPostProcessor setEnableBlur( bool enableBlur )
		{
			if( enableBlur != _blurEnabled )
			{
				_blurEnabled = enableBlur;

				if( _blurEnabled && _blurEffect == null && _scene != null )
				{
					_blurEffect = _scene.content.loadNezEffect<GaussianBlurEffect>();
					if( _scene.sceneRenderTarget != null )
						updateBlurEffectDeltas();
				}
			}

			return this;
		}

		public PolyLightPostProcessor setBlurRenderTargetScale( float blurRenderTargetScale )
		{
			if( _blurRenderTargetScale != blurRenderTargetScale )
			{
				_blurRenderTargetScale = blurRenderTargetScale;
				if( _blurEffect != null && _scene.sceneRenderTarget != null )
					updateBlurEffectDeltas();
			}

			return this;
		}

		public PolyLightPostProcessor setBlurAmount( float blurAmount )
		{
			if( _blurEffect != null )
				_blurEffect.blurAmount = blurAmount;

			return this;
		}

		#endregion


		public override void onAddedToScene( Scene scene )
		{
			base.onAddedToScene( scene );

			effect = scene.content.loadEffect<Effect>( "spriteLightMultiply", EffectResource.spriteLightMultiplyBytes );
			effect.Parameters["_lightTexture"].SetValue( _lightsRenderTexture );
			effect.Parameters["_multiplicativeFactor"].SetValue( _multiplicativeFactor );

			if( _blurEnabled )
				_blurEffect = scene.content.loadNezEffect<GaussianBlurEffect>();
		}

		public override void unload()
		{
			if( _lightsRenderTexture != null )
				_lightsRenderTexture.Dispose();

			if( _blurEffect != null )
				_scene.content.unloadEffect( _blurEffect );
			
			_scene.content.unloadEffect( effect );
			
			base.unload();
		}

		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			if( _blurEnabled )
			{
				// aquire a temporary rendertarget for the processing. It can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
				// the resolution in this way doesn't hurt quality, because we are going to be blurring the images in any case.
				var sceneRenderTargetSize = _scene.sceneRenderTargetSize;
				var tempRenderTarget = RenderTarget.getTemporary( (int)( sceneRenderTargetSize.X * _blurRenderTargetScale ), (int)( sceneRenderTargetSize.Y * _blurRenderTargetScale ), DepthFormat.None );


				// Pass 1: draw from _lightsRenderTexture into tempRenderTarget, applying a horizontal gaussian blur filter
				_blurEffect.prepareForHorizontalBlur();
				drawFullscreenQuad( _lightsRenderTexture, tempRenderTarget, _blurEffect );

				// Pass 2: draw from tempRenderTarget back into _lightsRenderTexture, applying a vertical gaussian blur filter
				_blurEffect.prepareForVerticalBlur();
				drawFullscreenQuad( tempRenderTarget, _lightsRenderTexture, _blurEffect );

				RenderTarget.releaseTemporary( tempRenderTarget );
			}
			
			Core.graphicsDevice.setRenderTarget( destination );
			Graphics.instance.batcher.begin( effect );
			Graphics.instance.batcher.draw( source, new Rectangle( 0, 0, destination.Width, destination.Height ), Color.White );
			Graphics.instance.batcher.end();
		}

		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			// when the RenderTexture changes we have to reset the shader param since the underlying RenderTarget will be different
			effect.Parameters["_lightTexture"].SetValue( _lightsRenderTexture );

			if( _blurEnabled )
				updateBlurEffectDeltas();
		}

	}
}

