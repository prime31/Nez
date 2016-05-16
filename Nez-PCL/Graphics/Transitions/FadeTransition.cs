using System;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// fades to fadeToColor then fades to the new Scene
	/// </summary>
	public class FadeTransition : SceneTransition
	{
		/// <summary>
		/// the color we will fade to/from
		/// </summary>
		public Color fadeToColor = Color.Black;

		/// <summary>
		/// duration to fade to fadeToColor
		/// </summary>
		public float fadeOutDuration = 0.6f;

		/// <summary>
		/// delay to start fading out
		/// </summary>
		public float delayBeforeFadeInDuration = 0.2f;

		/// <summary>
		/// duration to fade from fadeToColor to the new Scene
		/// </summary>
		public float fadeInDuration = 0.8f;

		/// <summary>
		/// ease equation to use for the fade
		/// </summary>
		public EaseType fadeEaseType = EaseType.QuartOut;

		Color _fromColor = Color.White;
		Color _toColor = Color.Transparent;

		Texture2D _overlayTexture;
		Color _color = Color.White;
		Rectangle _destinationRect;


		public FadeTransition( Func<Scene> sceneLoadAction ) : base( sceneLoadAction, true )
		{
			_destinationRect = previousSceneRender.Bounds;
		}


		public override IEnumerator onBeginTransition()
		{
			// create a single pixel texture of our fadeToColor
			_overlayTexture = Graphics.createSingleColorTexture( 1, 1, fadeToColor );

			var elapsed = 0f;
			while( elapsed < fadeOutDuration )
			{
				elapsed += Time.deltaTime;
				_color = Lerps.ease( fadeEaseType, ref _toColor, ref _fromColor, elapsed, fadeOutDuration );

				yield return null;
			}

			// load up the new Scene
			yield return Core.startCoroutine( loadNextScene() );

			// dispose of our previousSceneRender. We dont need it anymore.
			previousSceneRender.Dispose();
			previousSceneRender = null;

			//_color = _fromColor;
			yield return delayBeforeFadeInDuration;

			elapsed = 0f;
			while( elapsed < fadeInDuration )
			{
				elapsed += Time.deltaTime;
				_color = Lerps.ease( EaseHelper.oppositeEaseType( fadeEaseType ), ref _fromColor, ref _toColor, elapsed, fadeInDuration );

				yield return null;
			}

			transitionComplete();
			_overlayTexture.Dispose();
		}


		public override void render( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( null );
			graphics.batcher.begin( BlendState.NonPremultiplied, Core.defaultSamplerState, DepthStencilState.None, null );

			// we only render the previousSceneRender while fading to _color. It will be null after that.
			if( !_isNewSceneLoaded )
				graphics.batcher.draw( previousSceneRender, _destinationRect, Color.White );
			
			graphics.batcher.draw( _overlayTexture, new Rectangle( 0, 0, Screen.width, Screen.height ), _color );
			
			graphics.batcher.end();
		}
	}
}

