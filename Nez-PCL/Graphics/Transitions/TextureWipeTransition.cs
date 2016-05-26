using System;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using System.Collections;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// uses a texture (transitionTexture) to control a wipe animation. the blue channel of the texture determines if color is shown or the
	/// previous scenes render. Sample textures are 
	/// based on: https://www.youtube.com/watch?v=LnAoD7hgDxw
	/// </summary>
	public class TextureWipeTransition : SceneTransition
	{
		/// <summary>
		/// opacity of the wipe
		/// </summary>
		/// <value>The opacity.</value>
		public float opacity
		{
			set { _textureWipeEffect.Parameters["_opacity"].SetValue( value ); }
		}

		/// <summary>
		/// color to wipe to
		/// </summary>
		/// <value>The color.</value>
		public Color color
		{
			set { _textureWipeEffect.Parameters["_color"].SetValue( value.ToVector4() ); }
		}

		/// <summary>
		/// texture used for the transition. During the transition whenever the blue channel of this texture is less than progress (which is ticked
		/// from 0 - 1) the color will be used else the previous scene render will be used
		/// </summary>
		/// <value>The transition texture.</value>
		public Texture2D transitionTexture
		{
			set { _textureWipeEffect.Parameters["_transitionTex"].SetValue( value ); }
		}

		/// <summary>
		/// if true, the red and green channels of the transitionTexture will be used to offset the texture lookup during the transition
		/// </summary>
		/// <value><c>true</c> if use red green channels for distortion; otherwise, <c>false</c>.</value>
		public bool useRedGreenChannelsForDistortion
		{
			set { _textureWipeEffect.CurrentTechnique = _textureWipeEffect.Techniques[value ? "TextureWipeWithDistort" : "TextureWipe"]; }
		}

		/// <summary>
		/// duration for the wind transition
		/// </summary>
		public float duration = 1f;

		/// <summary>
		/// ease equation to use for the animation
		/// </summary>
		public EaseType easeType = EaseType.Linear;

		Effect _textureWipeEffect;
		Rectangle _destinationRect;
		Texture2D _overlayTexture;


		public TextureWipeTransition( Func<Scene> sceneLoadAction, Texture2D transitionTexture ) : base( sceneLoadAction, true )
		{
			_destinationRect = previousSceneRender.Bounds;

			// load Effect and set defaults
			_textureWipeEffect = Core.contentManager.loadEffect( "Content/nez/effects/transitions/TextureWipe.mgfxo" );
			opacity = 1f;
			color = Color.Black;
			this.transitionTexture = transitionTexture;
		}


		public TextureWipeTransition( Func<Scene> sceneLoadAction ) : this( sceneLoadAction, Core.contentManager.Load<Texture2D>( "nez/textures/textureWipeTransition/angular" ) )
		{}


		public override IEnumerator onBeginTransition()
		{
			// create a single pixel transparent texture so we can do our squares out to the next scene
			_overlayTexture = Graphics.createSingleColorTexture( 1, 1, Color.Transparent );

			// obscure the screen
			yield return Core.startCoroutine( tickEffectProgressProperty( _textureWipeEffect, duration, easeType ) );

			// load up the new Scene
			yield return Core.startCoroutine( loadNextScene() );

			// dispose of our previousSceneRender. We dont need it anymore.
			previousSceneRender.Dispose();
			previousSceneRender = null;

			// undo the effect
			yield return Core.startCoroutine( tickEffectProgressProperty( _textureWipeEffect, duration, EaseHelper.oppositeEaseType( easeType ), true ) );

			transitionComplete();

			// cleanup
			_overlayTexture.Dispose();
			Core.contentManager.unloadEffect( _textureWipeEffect );
		}


		public override void render( Graphics graphics )
		{
			Core.graphicsDevice.setRenderTarget( null );
			graphics.batcher.begin( BlendState.AlphaBlend, Core.defaultSamplerState, DepthStencilState.None, null, _textureWipeEffect );

			// we only render the previousSceneRender while populating the squares
			if( !_isNewSceneLoaded )
				graphics.batcher.draw( previousSceneRender, _destinationRect, Color.White );
			else
				graphics.batcher.draw( _overlayTexture, new Rectangle( 0, 0, Screen.width, Screen.height ), Color.Transparent );

			graphics.batcher.end();
		}
	}
}

