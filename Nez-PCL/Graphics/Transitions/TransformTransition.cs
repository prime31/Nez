using System;
using Microsoft.Xna.Framework;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;
using Nez.Tweens;


namespace Nez
{
	public class TransformTransition : SceneTransition
	{
		public enum TransformTransitionType
		{
			ZoomOut,
			ZoomIn,
			SlideRight,
			SlideLeft,
			SlideUp,
			SlideDown,
			SlideBottomRight,
			SlideBottomLeft,
			SlideTopRight,
			SlideTopLeft
		}

		/// <summary>
		/// duration for the animation
		/// </summary>
		public float duration = 1f;

		/// <summary>
		/// ease equation for the transition
		/// </summary>
		public EaseType transitionEaseType = EaseType.QuartIn;

		Rectangle _destinationRect;
		Rectangle _finalRenderRect;
		Rectangle _textureBounds;


		public TransformTransition( Func<Scene> sceneLoadAction, TransformTransitionType transitionType = TransformTransitionType.ZoomOut ) : base( sceneLoadAction, true )
		{
			_destinationRect = previousSceneRender.Bounds;
			_textureBounds = _destinationRect;

			switch( transitionType )
			{
				case TransformTransitionType.ZoomOut:
					_finalRenderRect = new Rectangle( Screen.width / 2, Screen.height / 2, 0, 0 );
					break;
				case TransformTransitionType.ZoomIn:
					_finalRenderRect = new Rectangle( -Screen.width * 5, -Screen.height * 5, _destinationRect.Width * 10, _destinationRect.Height * 10 );
					break;
				case TransformTransitionType.SlideRight:
					_finalRenderRect = new Rectangle( Screen.width, 0, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideLeft:
					_finalRenderRect = new Rectangle( -Screen.width, 0, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideUp:
					_finalRenderRect = new Rectangle( 0, -Screen.height, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideDown:
					_finalRenderRect = new Rectangle( 0, Screen.height, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideBottomRight:
					_finalRenderRect = new Rectangle( Screen.width, Screen.height, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideBottomLeft:
					_finalRenderRect = new Rectangle( -Screen.width, Screen.height, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideTopRight:
					_finalRenderRect = new Rectangle( Screen.width, -Screen.height, _destinationRect.Width, _destinationRect.Height );
				break;
				case TransformTransitionType.SlideTopLeft:
					_finalRenderRect = new Rectangle( -Screen.width, -Screen.height, _destinationRect.Width, _destinationRect.Height );
				break;
			}
		}


		public override IEnumerator onBeginTransition()
		{
			yield return null;

			// load up the new Scene
			yield return Core.startCoroutine( loadNextScene() );

			var elapsed = 0f;
			while( elapsed < duration )
			{
				elapsed += Time.deltaTime;
				_destinationRect = Lerps.ease( transitionEaseType, ref _textureBounds, ref _finalRenderRect, elapsed, duration );

				yield return null;
			}

			transitionComplete();
		}


		public override void render( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( null );
			graphics.batcher.begin( BlendState.NonPremultiplied, Core.defaultSamplerState, DepthStencilState.None, null );
			graphics.batcher.draw( previousSceneRender, _destinationRect, Color.White );
			graphics.batcher.end();
		}
	}
}

