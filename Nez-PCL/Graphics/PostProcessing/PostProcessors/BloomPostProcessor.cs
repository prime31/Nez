using System;
using Nez;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Nez.Systems;
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
		/// scale of the internal RenderTargets. For high resolution renders a half sized RT is usually more than enough.
		/// </summary>
		public float renderTargetScale = 1f;

		BloomSettings _settings;

		Effect _bloomExtractEffect;
		Effect _bloomCombineEffect;
		Effect _gaussianBlurEffect;

		// extract params
		EffectParameter _bloomExtractThresholdParam;
		// combine params
		EffectParameter _bloomIntensityParam, _bloomBaseIntensityParam, _bloomSaturationParam, _bloomBaseSaturationParam, _bloomBaseMapParm;
		// blur params
		EffectParameter _blurWeightsParam, _blurOffsetsParam;

		
		public BloomPostProcessor( int executionOrder ) : base( executionOrder )
		{
			_settings = BloomSettings.presetSettings[3];
		}


		public override void onAddedToScene()
		{
			_bloomExtractEffect = scene.content.loadEffect<Effect>( "bloomExtract", EffectResource.bloomExtractBytes );
			_bloomCombineEffect = scene.content.loadEffect<Effect>( "bloomCombine", EffectResource.bloomCombineBytes );
			_gaussianBlurEffect = scene.content.loadEffect<Effect>( "gaussianBlur", EffectResource.gaussianBlurBytes );

			_bloomExtractThresholdParam = _bloomExtractEffect.Parameters["_bloomThreshold"];

			_bloomIntensityParam = _bloomCombineEffect.Parameters["_bloomIntensity"];
			_bloomBaseIntensityParam = _bloomCombineEffect.Parameters["_baseIntensity"];
			_bloomSaturationParam = _bloomCombineEffect.Parameters["_bloomSaturation"];
			_bloomBaseSaturationParam = _bloomCombineEffect.Parameters["_baseSaturation"];
			_bloomBaseMapParm = _bloomCombineEffect.Parameters["_baseMap"];

			_blurWeightsParam = _gaussianBlurEffect.Parameters["_sampleWeights"];
			_blurOffsetsParam = _gaussianBlurEffect.Parameters["_sampleOffsets"];

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
		}


		/// <summary>
		/// computes sample weightings and texture coordinate offsets for one pass of a separable gaussian blur filter.
		/// </summary>
		void setBlurEffectParameters( float dx, float dy )
		{
			// Look up how many samples our gaussian blur effect supports.
			var sampleCount = _blurWeightsParam.Elements.Count;

			// Create temporary arrays for computing our filter settings.
			var sampleWeights = new float[sampleCount];
			var sampleOffsets = new Vector2[sampleCount];

			// The first sample always has a zero offset.
			sampleWeights[0] = computeGaussian( 0 );
			sampleOffsets[0] = Vector2.Zero;

			// Maintain a sum of all the weighting values.
			var totalWeights = sampleWeights[0];

			// Add pairs of additional sample taps, positioned along a line in both directions from the center.
			for( var i = 0; i < sampleCount / 2; i++ )
			{
				// Store weights for the positive and negative taps.
				var weight = computeGaussian( i + 1 );

				sampleWeights[i * 2 + 1] = weight;
				sampleWeights[i * 2 + 2] = weight;

				totalWeights += weight * 2;

				// To get the maximum amount of blurring from a limited number of pixel shader samples, we take advantage of the bilinear filtering
				// hardware inside the texture fetch unit. If we position our texture coordinates exactly halfway between two texels, the filtering unit
				// will average them for us, giving two samples for the price of one. This allows us to step in units of two texels per sample, rather
				// than just one at a time. The 1.5 offset kicks things off by positioning us nicely in between two texels.
				var sampleOffset = i * 2 + 1.5f;

				var delta = new Vector2( dx, dy ) * sampleOffset;

				// Store texture coordinate offsets for the positive and negative taps.
				sampleOffsets[i * 2 + 1] = delta;
				sampleOffsets[i * 2 + 2] = -delta;
			}

			// Normalize the list of sample weightings, so they will always sum to one.
			for( var i = 0; i < sampleWeights.Length; i++ )
				sampleWeights[i] /= totalWeights;

			// Tell the effect about our new filter settings.
			_blurWeightsParam.SetValue( sampleWeights );
			_blurOffsetsParam.SetValue( sampleOffsets );
		}


		/// <summary>
		/// Evaluates a single point on the gaussian falloff curve.
		/// Used for setting up the blur filter weightings.
		/// </summary>
		float computeGaussian( float n )
		{
			var theta = _settings.blurAmount;
			return (float)( ( 1.0 / Math.Sqrt( 2 * Math.PI * theta ) ) * Math.Exp( -( n * n ) / ( 2 * theta * theta ) ) );
		}


		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			// aquire two rendertargets for the bloom processing. These can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
			// the resolution in this way doesn't hurt quality, because we are going to be blurring the bloom images in any case.
			// the demo uses a tiny backbuffer so no need to reduce size any further
			var sceneRenderTargetSize = scene.sceneRenderTargetSize;
			var renderTarget1 = RenderTarget.getTemporary( (int)( sceneRenderTargetSize.X * renderTargetScale ), (int)( sceneRenderTargetSize.Y * renderTargetScale ), DepthFormat.None );
			var renderTarget2 = RenderTarget.getTemporary( (int)( sceneRenderTargetSize.X * renderTargetScale ), (int)( sceneRenderTargetSize.Y * renderTargetScale ), DepthFormat.None );

			// Pass 1: draw the scene into rendertarget 1, using a shader that extracts only the brightest parts of the image.
			drawFullscreenQuad( source, renderTarget1, _bloomExtractEffect );

			// Pass 2: draw from rendertarget 1 into rendertarget 2, using a shader to apply a horizontal gaussian blur filter.
			setBlurEffectParameters( 1.0f / (float)renderTarget1.Width, 0 );
			drawFullscreenQuad( renderTarget1, renderTarget2, _gaussianBlurEffect );

			// Pass 3: draw from rendertarget 2 back into rendertarget 1, using a shader to apply a vertical gaussian blur filter.
			setBlurEffectParameters( 0, 1.0f / (float)renderTarget1.Height );
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

