using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class BloomPostProcessor : PostProcessor
	{
		/// <summary>
		/// the settings used by the bloom and blur shaders. If changed, you must call setBloomSettings for the changes to take effect.
		/// </summary>
		public BloomSettings Settings
		{
			get => _settings;
			set => SetBloomSettings(value);
		}

		/// <summary>
		/// scale of the internal RenderTargets. For high resolution renders a half sized RT is usually more than enough. Defaults to 1.
		/// </summary>
		[Range(0.25f, 1)]
		public float RenderTargetScale
		{
			get => _renderTargetScale;
			set
			{
				if (_renderTargetScale != value)
				{
					_renderTargetScale = value;
					UpdateBlurEffectDeltas();
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
		EffectParameter _bloomIntensityParam,
			_bloomBaseIntensityParam,
			_bloomSaturationParam,
			_bloomBaseSaturationParam,
			_bloomBaseMapParm;


		public BloomPostProcessor(int executionOrder) : base(executionOrder)
		{
			_settings = BloomSettings.PresetSettings[3];
		}

		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);

			_bloomExtractEffect = scene.Content.LoadEffect<Effect>("bloomExtract", EffectResource.BloomExtractBytes);
			_bloomCombineEffect = scene.Content.LoadEffect<Effect>("bloomCombine", EffectResource.BloomCombineBytes);
			_gaussianBlurEffect = scene.Content.LoadNezEffect<GaussianBlurEffect>();

			_bloomExtractThresholdParam = _bloomExtractEffect.Parameters["_bloomThreshold"];

			_bloomIntensityParam = _bloomCombineEffect.Parameters["_bloomIntensity"];
			_bloomBaseIntensityParam = _bloomCombineEffect.Parameters["_baseIntensity"];
			_bloomSaturationParam = _bloomCombineEffect.Parameters["_bloomSaturation"];
			_bloomBaseSaturationParam = _bloomCombineEffect.Parameters["_baseSaturation"];
			_bloomBaseMapParm = _bloomCombineEffect.Parameters["_baseMap"];

			SetBloomSettings(_settings);
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(_bloomExtractEffect);
			_scene.Content.UnloadEffect(_bloomCombineEffect);
			_scene.Content.UnloadEffect(_gaussianBlurEffect);

			base.Unload();
		}

		/// <summary>
		/// sets the settings used by the bloom and blur shaders
		/// </summary>
		/// <param name="settings">Settings.</param>
		public void SetBloomSettings(BloomSettings settings)
		{
			_settings = settings;

			_bloomExtractThresholdParam.SetValue(_settings.Threshold);

			_bloomIntensityParam.SetValue(_settings.Intensity);
			_bloomBaseIntensityParam.SetValue(_settings.BaseIntensity);
			_bloomSaturationParam.SetValue(_settings.Saturation);
			_bloomBaseSaturationParam.SetValue(_settings.BaseSaturation);

			_gaussianBlurEffect.BlurAmount = _settings.BlurAmount;
		}

		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			UpdateBlurEffectDeltas();
		}

		/// <summary>
		/// updates the Effect with the new vertical and horizontal deltas
		/// </summary>
		void UpdateBlurEffectDeltas()
		{
			var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
			_gaussianBlurEffect.HorizontalBlurDelta = 1f / (sceneRenderTargetSize.X * _renderTargetScale);
			_gaussianBlurEffect.VerticalBlurDelta = 1f / (sceneRenderTargetSize.Y * _renderTargetScale);
		}

		public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			// aquire two rendertargets for the bloom processing. These can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
			// the resolution in this way doesn't hurt quality, because we are going to be blurring the bloom images in any case.
			var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
			var renderTarget1 = RenderTarget.GetTemporary((int) (sceneRenderTargetSize.X * RenderTargetScale),
				(int) (sceneRenderTargetSize.Y * RenderTargetScale), DepthFormat.None);
			var renderTarget2 = RenderTarget.GetTemporary((int) (sceneRenderTargetSize.X * RenderTargetScale),
				(int) (sceneRenderTargetSize.Y * RenderTargetScale), DepthFormat.None);

			// Pass 1: draw the scene into rendertarget 1, using a shader that extracts only the brightest parts of the image.
			DrawFullscreenQuad(source, renderTarget1, _bloomExtractEffect);

			// Pass 2: draw from rendertarget 1 into rendertarget 2, using a shader to apply a horizontal gaussian blur filter.
			_gaussianBlurEffect.PrepareForHorizontalBlur();
			DrawFullscreenQuad(renderTarget1, renderTarget2, _gaussianBlurEffect);

			// Pass 3: draw from rendertarget 2 back into rendertarget 1, using a shader to apply a vertical gaussian blur filter.
			_gaussianBlurEffect.PrepareForVerticalBlur();
			DrawFullscreenQuad(renderTarget2, renderTarget1, _gaussianBlurEffect);

			// Pass 4: draw both rendertarget 1 and the original scene image back into the main backbuffer, using a shader that
			// combines them to produce the final bloomed result.
			Core.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
			_bloomBaseMapParm.SetValue(source);

			DrawFullscreenQuad(renderTarget1, destination, _bloomCombineEffect);

			RenderTarget.ReleaseTemporary(renderTarget1);
			RenderTarget.ReleaseTemporary(renderTarget2);
		}
	}
}