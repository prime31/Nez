using System;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class FadeTransition : SceneTransition
	{
		public Color fromColor = Color.White;
		public Color toColor = Color.TransparentBlack;

		Texture2D _blackTexture;
		Color _color = Color.White;
		Rectangle _destinationRect;


		public FadeTransition( Func<Scene> sceneLoadAction ) : base( sceneLoadAction, true )
		{
			_destinationRect = previousSceneRender.Bounds;
		}


		public override IEnumerator onBeginTransition()
		{
			yield return null;

			var duration = 1f;
			var elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				_color = Lerps.ease( EaseType.QuartIn, ref fromColor, ref toColor, elapsed, duration );

				yield return null;
			}

			// load up the new Scene
			Core.scene = sceneLoadAction();

			// dispose of our previousSceneRender. We dont need it anymore.
			previousSceneRender.Dispose();
			previousSceneRender = null;

			// create a single pixel black texture
			_blackTexture = Graphics.createSingleColorTexture( 1, 1, Color.Black );

			elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				_color = Lerps.ease( EaseType.QuartIn, ref fromColor, ref toColor, elapsed, duration );

				yield return null;
			}

			transitionComplete();
			_blackTexture.Dispose();
		}


		public override void render()
		{
			Core.graphicsDevice.SetRenderTarget( null );
			Graphics.instance.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.NonPremultiplied, Core.defaultSamplerState );
			Graphics.instance.spriteBatch.Draw( _blackTexture ?? previousSceneRender, _destinationRect, _color );
			Graphics.instance.spriteBatch.End();
		}
	}
}

