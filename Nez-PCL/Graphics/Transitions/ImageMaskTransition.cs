using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;
using System.Collections;
using Nez.Textures;


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
		public float duration = 1f;

		/// <summary>
		/// delay after the mask-in before the mark-out begins
		/// </summary>
		public float delayBeforeMaskOut = 0.2f;

		/// <summary>
		/// minimum scale of the mask
		/// </summary>
		public float minScale = 0.01f;

		/// <summary>
		/// maximum scale of the mask
		/// </summary>
		public float maxScale = 10f;

		/// <summary>
		/// ease equation to use for the scale animation
		/// </summary>
		public EaseType scaleEaseType = EaseType.ExpoOut;

		/// <summary>
		/// minimum rotation of the mask animation
		/// </summary>
		public float minRotation = 0;

		/// <summary>
		/// maximum rotation of the mask animation
		/// </summary>
		public float maxRotation = MathHelper.TwoPi;

		/// <summary>
		/// ease equation to use for the rotation animation
		/// </summary>
		public EaseType rotationEaseType = EaseType.Linear;


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


		public ImageMaskTransition( Func<Scene> sceneLoadAction, Texture2D maskTexture ) : base( sceneLoadAction, true )
		{
			_maskPosition = new Vector2( Screen.width / 2, Screen.height / 2 );
			_maskRenderTarget = new RenderTarget2D( Core.graphicsDevice, Screen.width, Screen.height, false, SurfaceFormat.Color, DepthFormat.None );
			_maskTexture = maskTexture;
			_maskOrigin = new Vector2( _maskTexture.Bounds.Width / 2, _maskTexture.Bounds.Height / 2 );

			_blendState = new BlendState {
				ColorSourceBlend = Blend.DestinationColor,
				ColorDestinationBlend = Blend.Zero,
				ColorBlendFunction = BlendFunction.Add
			};
		}


		public override IEnumerator onBeginTransition()
		{
			yield return null;

			var elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				_renderScale = Lerps.ease( scaleEaseType, maxScale, minScale, elapsed, duration );
				_renderRotation = Lerps.ease( rotationEaseType, minRotation, maxRotation, elapsed, duration );

				yield return null;
			}

			// load up the new Scene
			yield return Core.startCoroutine( loadNextScene() );

			// dispose of our previousSceneRender. We dont need it anymore.
			previousSceneRender.Dispose();
			previousSceneRender = null;

			yield return delayBeforeMaskOut;

			elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				_renderScale = Lerps.ease( EaseHelper.oppositeEaseType( scaleEaseType ), minScale, maxScale, elapsed, duration );
				_renderRotation = Lerps.ease( EaseHelper.oppositeEaseType( rotationEaseType ), maxRotation, minRotation, elapsed, duration );

				yield return null;
			}

			transitionComplete();
		}


		public override void preRender( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( _maskRenderTarget );
			graphics.batcher.begin( BlendState.AlphaBlend, Core.defaultSamplerState, DepthStencilState.None, null );
			graphics.batcher.draw( _maskTexture, _maskPosition, null, Color.White, _renderRotation, _maskOrigin, _renderScale, SpriteEffects.None, 0 );
			graphics.batcher.end();
			Core.graphicsDevice.setRenderTarget( null );
		}


		public override void render( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( null );

			// if we are scaling out we dont need to render the previous scene anymore since we want the new scene to be visible
			if( !_isNewSceneLoaded )
			{
				graphics.batcher.begin( BlendState.Opaque, Core.defaultSamplerState, DepthStencilState.None, null );
				graphics.batcher.draw( previousSceneRender, Vector2.Zero, Color.White );
				graphics.batcher.end();
			}

			graphics.batcher.begin( _blendState, Core.defaultSamplerState, DepthStencilState.None, null );
			graphics.batcher.draw( _maskRenderTarget, Vector2.Zero, Color.White );
			graphics.batcher.end();
		}


		protected override void transitionComplete()
		{
			base.transitionComplete();

			Core.contentManager.unloadAsset<Texture2D>( _maskTexture.Name );
			_maskRenderTarget.Dispose();
			_blendState.Dispose();
		}
	}
}

