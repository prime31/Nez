using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;
using System.Collections;


namespace Nez
{
	/// <summary>
	/// uses an image to mask out part of the scene scaling it from max-to-min then from min-to-max with rotation. Note that the Texture
	/// should be loaded in the main Core.contentManager, not a Scene contentManager. The transition will unload it for you. The Texture
	/// should be transparent where it should be masked out and white where it should be masked in.
	/// </summary>
	public class ImageMaskTransition : SceneTransition
	{
		/// <summary>
		/// duration of the transition both in and out
		/// </summary>
		public float Duration = 1f;

		/// <summary>
		/// delay after the mask-in before the mark-out begins
		/// </summary>
		public float DelayBeforeMaskOut = 0.2f;

		/// <summary>
		/// minimum scale of the mask
		/// </summary>
		public float MinScale = 0.01f;

		/// <summary>
		/// maximum scale of the mask
		/// </summary>
		public float MaxScale = 10f;

		/// <summary>
		/// ease equation to use for the scale animation
		/// </summary>
		public EaseType ScaleEaseType = EaseType.ExpoOut;

		/// <summary>
		/// minimum rotation of the mask animation
		/// </summary>
		public float MinRotation = 0;

		/// <summary>
		/// maximum rotation of the mask animation
		/// </summary>
		public float MaxRotation = MathHelper.TwoPi;

		/// <summary>
		/// ease equation to use for the rotation animation
		/// </summary>
		public EaseType RotationEaseType = EaseType.Linear;


		float _renderScale;
		float _renderRotation;

		/// <summary>
		/// the Texture used as a mask. It should be white where the mask shows the underlying Scene and transparent elsewhere
		/// </summary>
		Texture2D _maskTexture;

		/// <summary>
		/// position of the mask, the center of the screen
		/// </summary>
		Vector2 _maskPosition;

		/// <summary>
		/// origin of the mask, the center of the Texture
		/// </summary>
		Vector2 _maskOrigin;

		/// <summary>
		/// multiplicative BlendState used for rendering the mask
		/// </summary>
		BlendState _blendState;

		/// <summary>
		/// the mask is first rendered into a RenderTarget
		/// </summary>
		RenderTarget2D _maskRenderTarget;


		public ImageMaskTransition(Func<Scene> sceneLoadAction, Texture2D maskTexture) : base(sceneLoadAction, true)
		{
			_maskPosition = new Vector2(Screen.Width / 2, Screen.Height / 2);
			_maskRenderTarget = new RenderTarget2D(Core.GraphicsDevice, Screen.Width, Screen.Height, false,
				SurfaceFormat.Color, DepthFormat.None);
			_maskTexture = maskTexture;
			_maskOrigin = new Vector2(_maskTexture.Bounds.Width / 2, _maskTexture.Bounds.Height / 2);

			_blendState = new BlendState
			{
				ColorSourceBlend = Blend.DestinationColor,
				ColorDestinationBlend = Blend.Zero,
				ColorBlendFunction = BlendFunction.Add
			};
		}


		public ImageMaskTransition(Texture2D maskTexture) : this(null, maskTexture)
		{
		}


		public override IEnumerator OnBeginTransition()
		{
			yield return null;

			var elapsed = 0f;
			while (elapsed < Duration)
			{
				elapsed += Time.DeltaTime;
				_renderScale = Lerps.Ease(ScaleEaseType, MaxScale, MinScale, elapsed, Duration);
				_renderRotation = Lerps.Ease(RotationEaseType, MinRotation, MaxRotation, elapsed, Duration);

				yield return null;
			}

			// load up the new Scene
			yield return Core.StartCoroutine(LoadNextScene());

			// dispose of our previousSceneRender. We dont need it anymore.
			PreviousSceneRender.Dispose();
			PreviousSceneRender = null;

			yield return Coroutine.WaitForSeconds(DelayBeforeMaskOut);

			elapsed = 0f;
			while (elapsed < Duration)
			{
				elapsed += Time.DeltaTime;
				_renderScale = Lerps.Ease(EaseHelper.OppositeEaseType(ScaleEaseType), MinScale, MaxScale, elapsed,
					Duration);
				_renderRotation = Lerps.Ease(EaseHelper.OppositeEaseType(RotationEaseType), MaxRotation, MinRotation,
					elapsed, Duration);

				yield return null;
			}

			TransitionComplete();
		}


		public override void PreRender(Batcher batcher)
		{
			Core.GraphicsDevice.SetRenderTarget(_maskRenderTarget);
			batcher.Begin(BlendState.AlphaBlend, Core.DefaultSamplerState, DepthStencilState.None, null);
			batcher.Draw(_maskTexture, _maskPosition, null, Color.White, _renderRotation, _maskOrigin,
				_renderScale, SpriteEffects.None, 0);
			batcher.End();
			Core.GraphicsDevice.SetRenderTarget(null);
		}


		protected override void TransitionComplete()
		{
			base.TransitionComplete();

			Core.Content.UnloadAsset<Texture2D>(_maskTexture.Name);
			_maskRenderTarget.Dispose();
			_blendState.Dispose();
		}


		public override void Render(Batcher batcher)
		{
			Core.GraphicsDevice.SetRenderTarget(null);

			// if we are scaling out we dont need to render the previous scene anymore since we want the new scene to be visible
			if (!_isNewSceneLoaded)
			{
				batcher.Begin(BlendState.Opaque, Core.DefaultSamplerState, DepthStencilState.None, null);
				batcher.Draw(PreviousSceneRender, Vector2.Zero, Color.White);
				batcher.End();
			}

			batcher.Begin(_blendState, Core.DefaultSamplerState, DepthStencilState.None, null);
			batcher.Draw(_maskRenderTarget, Vector2.Zero, Color.White);
			batcher.End();
		}
	}
}