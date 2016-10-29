using System;
using Nez;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class BloomPostProcessor : PostProcessor
	{
		/// <summary>
		/// the settings used by the bloom and blur shaders. If changed, you must call setBloomSettings for the changes to take effect.
		/// </summary>
		public BloomSettings settings
		{
			get { return _settings; }
			set { setBloomSettings( value ); }
		}

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
					updateBlurEffectDeltas();
				}
			}
		}

		float _renderTargetScale = 1f;
		BloomSettings _settings;

		Effect _bloomExtractEffect;
		Effect _bloomCombineEffect;
		GaussianBlurEffect _gaussianBlurEffect;

		// extract params
		EffectParameter _bloomExtractThresholdParam;
		// combine params
		EffectParameter _bloomIntensityParam, _bloomBaseIntensityParam, _bloomSaturationParam, _bloomBaseSaturationParam, _bloomBaseMapParm;

		
		public BloomPostProcessor( int executionOrder ) : base( executionOrder )
		{
			_settings = BloomSettings.presetSettings[3];
		}


		public override void onAddedToScene()
		{
			_bloomExtractEffect = scene.content.loadEffect<Effect>( "bloomExtract", EffectResource.bloomExtractBytes );
			_bloomCombineEffect = scene.content.loadEffect<Effect>( "bloomCombine", EffectResource.bloomCombineBytes );
			_gaussianBlurEffect = scene.content.loadNezEffect<GaussianBlurEffect>();

			_bloomExtractThresholdParam = _bloomExtractEffect.Parameters["_bloomThreshold"];

			_bloomIntensityParam = _bloomCombineEffect.Parameters["_bloomIntensity"];
			_bloomBaseIntensityParam = _bloomCombineEffect.Parameters["_baseIntensity"];
			_bloomSaturationParam = _bloomCombineEffect.Parameters["_bloomSaturation"];
			_bloomBaseSaturationParam = _bloomCombineEffect.Parameters["_baseSaturation"];
			_bloomBaseMapParm = _bloomCombineEffect.Parameters["_baseMap"];

			setBloomSettings( _settings );
		}


		/// <summary>
		/// sets the settings used by the bloom and blur shaders
		/// </summary>
		/// <param name="settings">Settings.</param>
		public void setBloomSettings( BloomSettings settings )
		{
			_settings = settings;

			_bloomExtractThresholdParam.SetValue( _settings.threshold );

			_bloomIntensityParam.SetValue( _settings.intensity );
			_bloomBaseIntensityParam.SetValue( _settings.baseIntensity );
			_bloomSaturationParam.SetValue( _settings.saturation );
			_bloomBaseSaturationParam.SetValue( _settings.baseSaturation );

			_gaussianBlurEffect.blurAmount = _settings.blurAmount;
		}


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			updateBlurEffectDeltas();
		}


		/// <summary>
		/// updates the Effect with the new vertical and horizontal deltas
		/// </summary>
		void updateBlurEffectDeltas()
		{
			var sceneRenderTargetSize = scene.sceneRenderTargetSize;
			_gaussianBlurEffect.horizontalBlurDelta = 1f / ( sceneRenderTargetSize.X * _renderTargetScale );
			_gaussianBlurEffect.verticalBlurDelta = 1f / ( sceneRenderTargetSize.Y * _renderTargetScale );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			// aquire two rendertargets for the bloom processing. These can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
			// the resolution in this way doesn't hurt quality, because we are going to be blurring the bloom images in any case.
			var sceneRenderTargetSize = scene.sceneRenderTargetSize;
			var renderTarget1 = RenderTarget.getTemporary( (int)( sceneRenderTargetSize.X * renderTargetScale ), (int)( sceneRenderTargetSize.Y * renderTargetScale ), DepthFormat.None );
			var renderTarget2 = RenderTarget.getTemporary( (int)( sceneRenderTargetSize.X * renderTargetScale ), (int)( sceneRenderTargetSize.Y * renderTargetScale ), DepthFormat.None );

			// Pass 1: draw the scene into rendertarget 1, using a shader that extracts only the brightest parts of the image.
			drawFullscreenQuad( source, renderTarget1, _bloomExtractEffect );

			// Pass 2: draw from rendertarget 1 into rendertarget 2, using a shader to apply a horizontal gaussian blur filter.
			_gaussianBlurEffect.prepareForHorizontalBlur();
			drawFullscreenQuad( renderTarget1, renderTarget2, _gaussianBlurEffect );

			// Pass 3: draw from rendertarget 2 back into rendertarget 1, using a shader to apply a vertical gaussian blur filter.
			_gaussianBlurEffect.prepareForVerticalBlur();
			drawFullscreenQuad( renderTarget2, renderTarget1, _gaussianBlurEffect );

			// Pass 4: draw both rendertarget 1 and the original scene image back into the main backbuffer, using a shader that
			// combines them to produce the final bloomed result.
			Core.graphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
			_bloomBaseMapParm.SetValue( source );

			drawFullscreenQuad( renderTarget1, destination, _bloomCombineEffect );

			RenderTarget.releaseTemporary( renderTarget1 );
			RenderTarget.releaseTemporary( renderTarget2 );
		}

	}
}

