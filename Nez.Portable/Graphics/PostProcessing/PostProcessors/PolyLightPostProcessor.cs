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
		public float MultiplicativeFactor
		{
			get => _multiplicativeFactor;
			set => SetMultiplicativeFactor(value);
		}

		/// <summary>
		/// enables/disables a gaussian blur of the light texture before it is combined with the scene render
		/// </summary>
		/// <value><c>true</c> if enable blur; otherwise, <c>false</c>.</value>
		public bool EnableBlur
		{
			get => _blurEnabled;
			set => SetEnableBlur(value);
		}

		/// <summary>
		/// scale of the internal RenderTargets used for the blur. For high resolution renders a half sized RT is usually more than enough.
		/// Defaults to 0.5.
		/// </summary>
		public float BlurRenderTargetScale
		{
			get => _blurRenderTargetScale;
			set => SetBlurRenderTargetScale(value);
		}

		/// <summary>
		/// amount to blur. A range of 0.5 - 6 works well. Defaults to 2.
		/// </summary>
		/// <value>The blur amount.</value>
		public float BlurAmount
		{
			get => _blurEffect != null ? _blurEffect.BlurAmount : -1;
			set
			{
				if (_blurEffect != null)
					_blurEffect.BlurAmount = value;
			}
		}

		float _multiplicativeFactor = 1f;
		bool _blurEnabled;
		float _blurRenderTargetScale = 0.5f;

		GaussianBlurEffect _blurEffect;
		RenderTexture _lightsRenderTexture;


		public PolyLightPostProcessor(int executionOrder, RenderTexture lightsRenderTexture) : base(executionOrder)
		{
			_lightsRenderTexture = lightsRenderTexture;
		}

		/// <summary>
		/// updates the GaussianBlurEffect with the new vertical and horizontal deltas after a back buffer size or blurRenderTargetScale change
		/// </summary>
		void UpdateBlurEffectDeltas()
		{
			var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
			_blurEffect.HorizontalBlurDelta = 1f / (sceneRenderTargetSize.X * _blurRenderTargetScale);
			_blurEffect.VerticalBlurDelta = 1f / (sceneRenderTargetSize.Y * _blurRenderTargetScale);
		}


		#region chainable setters

		public PolyLightPostProcessor SetMultiplicativeFactor(float multiplicativeFactor)
		{
			_multiplicativeFactor = multiplicativeFactor;
			if (Effect != null)
				Effect.Parameters["_multiplicativeFactor"].SetValue(multiplicativeFactor);

			return this;
		}

		public PolyLightPostProcessor SetEnableBlur(bool enableBlur)
		{
			if (enableBlur != _blurEnabled)
			{
				_blurEnabled = enableBlur;

				if (_blurEnabled && _blurEffect == null && _scene != null)
				{
					_blurEffect = _scene.Content.LoadNezEffect<GaussianBlurEffect>();
					if (_scene.SceneRenderTarget != null)
						UpdateBlurEffectDeltas();
				}
			}

			return this;
		}

		public PolyLightPostProcessor SetBlurRenderTargetScale(float blurRenderTargetScale)
		{
			if (_blurRenderTargetScale != blurRenderTargetScale)
			{
				_blurRenderTargetScale = blurRenderTargetScale;
				if (_blurEffect != null && _scene.SceneRenderTarget != null)
					UpdateBlurEffectDeltas();
			}

			return this;
		}

		public PolyLightPostProcessor SetBlurAmount(float blurAmount)
		{
			if (_blurEffect != null)
				_blurEffect.BlurAmount = blurAmount;

			return this;
		}

		#endregion


		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);

			Effect = scene.Content.LoadEffect<Effect>("spriteLightMultiply", EffectResource.SpriteLightMultiplyBytes);
			Effect.Parameters["_lightTexture"].SetValue(_lightsRenderTexture);
			Effect.Parameters["_multiplicativeFactor"].SetValue(_multiplicativeFactor);

			if (_blurEnabled)
				_blurEffect = scene.Content.LoadNezEffect<GaussianBlurEffect>();
		}

		public override void Unload()
		{
			if (_lightsRenderTexture != null)
				_lightsRenderTexture.Dispose();

			if (_blurEffect != null)
				_scene.Content.UnloadEffect(_blurEffect);

			_scene.Content.UnloadEffect(Effect);

			base.Unload();
		}

		public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			if (_blurEnabled)
			{
				// aquire a temporary rendertarget for the processing. It can be scaled via renderTargetScale in order to minimize fillrate costs. Reducing
				// the resolution in this way doesn't hurt quality, because we are going to be blurring the images in any case.
				var sceneRenderTargetSize = _scene.SceneRenderTargetSize;
				var tempRenderTarget = RenderTarget.GetTemporary(
					(int) (sceneRenderTargetSize.X * _blurRenderTargetScale),
					(int) (sceneRenderTargetSize.Y * _blurRenderTargetScale), DepthFormat.None);


				// Pass 1: draw from _lightsRenderTexture into tempRenderTarget, applying a horizontal gaussian blur filter
				_blurEffect.PrepareForHorizontalBlur();
				DrawFullscreenQuad(_lightsRenderTexture, tempRenderTarget, _blurEffect);

				// Pass 2: draw from tempRenderTarget back into _lightsRenderTexture, applying a vertical gaussian blur filter
				_blurEffect.PrepareForVerticalBlur();
				DrawFullscreenQuad(tempRenderTarget, _lightsRenderTexture, _blurEffect);

				RenderTarget.ReleaseTemporary(tempRenderTarget);
			}

			Core.GraphicsDevice.SetRenderTarget(destination);
			Graphics.Instance.Batcher.Begin(Effect);
			Graphics.Instance.Batcher.Draw(source, new Rectangle(0, 0, destination.Width, destination.Height),
				Color.White);
			Graphics.Instance.Batcher.End();
		}

		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			// when the RenderTexture changes we have to reset the shader param since the underlying RenderTarget will be different
			Effect.Parameters["_lightTexture"].SetValue(_lightsRenderTexture);

			if (_blurEnabled)
				UpdateBlurEffectDeltas();
		}
	}
}