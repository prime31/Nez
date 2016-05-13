using System;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// sweeps wind accross the screen revealing the new Scene
	/// </summary>
	public class WindTransition : SceneTransition
	{
		/// <summary>
		/// how many wind segments should be used. Defaults to 100. (1 - 1000)
		/// </summary>
		/// <value>The wind segments.</value>
		public float windSegments
		{
			set { _windEffect.Parameters["_windSegments"].SetValue( value ); }
		}

		/// <summary>
		/// size of the wind streaks. defaults to 0.3. (0.1 - 1)
		/// </summary>
		/// <value>The size.</value>
		public float size
		{
			set { _windEffect.Parameters["_size"].SetValue( value ); }
		}

		/// <summary>
		/// duration for the wind transition
		/// </summary>
		public float duration = 1f;

		/// <summary>
		/// ease equation to use for the animation
		/// </summary>
		public EaseType easeType = EaseType.QuartOut;

		Effect _windEffect;
		Rectangle _destinationRect;


		public WindTransition( Func<Scene> sceneLoadAction ) : base( sceneLoadAction, true )
		{
			_destinationRect = previousSceneRender.Bounds;

			// load Effect and set defaults
			_windEffect = Core.contentManager.loadEffect( "Content/nez/effects/transitions/Wind.mgfxo" );
			size = 0.3f;
			windSegments = 100;
		}


		public override IEnumerator onBeginTransition()
		{
			// load up the new Scene
			yield return Core.startCoroutine( loadNextScene() );

			// wind to the new Scene
			yield return Core.startCoroutine( tickEffectProgressProperty( _windEffect, duration, easeType ) );

			transitionComplete();

			// cleanup
			Core.contentManager.unloadEffect( _windEffect.Name );
		}


		public override void render( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( null );
			graphics.batcher.begin( BlendState.NonPremultiplied, Core.defaultSamplerState, DepthStencilState.None, null, _windEffect );
			graphics.batcher.draw( previousSceneRender, _destinationRect, Color.White );
			graphics.batcher.end();
		}
	}
}

