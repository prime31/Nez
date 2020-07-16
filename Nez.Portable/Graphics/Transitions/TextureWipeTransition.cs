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
		public float Opacity
		{
			set => _textureWipeEffect.Parameters["_opacity"].SetValue(value);
		}

		/// <summary>
		/// color to wipe to
		/// </summary>
		/// <value>The color.</value>
		public Color Color
		{
			set => _textureWipeEffect.Parameters["_color"].SetValue(value.ToVector4());
		}

		/// <summary>
		/// texture used for the transition. During the transition whenever the blue channel of this texture is less than progress (which is ticked
		/// from 0 - 1) the color will be used else the previous scene render will be used
		/// </summary>
		/// <value>The transition texture.</value>
		public Texture2D TransitionTexture
		{
			set => _textureWipeEffect.Parameters["_transitionTex"].SetValue(value);
		}

		/// <summary>
		/// if true, the red and green channels of the transitionTexture will be used to offset the texture lookup during the transition
		/// </summary>
		/// <value><c>true</c> if use red green channels for distortion; otherwise, <c>false</c>.</value>
		public bool UseRedGreenChannelsForDistortion
		{
			set => _textureWipeEffect.CurrentTechnique =
				_textureWipeEffect.Techniques[value ? "TextureWipeWithDistort" : "TextureWipe"];
		}

		/// <summary>
		/// duration for the wind transition
		/// </summary>
		public float Duration = 1f;

		/// <summary>
		/// ease equation to use for the animation
		/// </summary>
		public EaseType EaseType = EaseType.Linear;

		Effect _textureWipeEffect;
		Rectangle _destinationRect;
		Texture2D _overlayTexture;


		public TextureWipeTransition(Func<Scene> sceneLoadAction, Texture2D transitionTexture) : base(sceneLoadAction,
			true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_textureWipeEffect = Core.Content.LoadEffect("Content/nez/effects/transitions/TextureWipe.mgfxo");
			Opacity = 1f;
			Color = Color.Black;
			TransitionTexture = transitionTexture;
		}

		public TextureWipeTransition(Func<Scene> sceneLoadAction) : this(sceneLoadAction,
			Core.Content.Load<Texture2D>("nez/textures/textureWipeTransition/angular"))
		{
		}

		public TextureWipeTransition() : this(null,
			Core.Content.Load<Texture2D>("nez/textures/textureWipeTransition/angular"))
		{
		}

		public TextureWipeTransition(Texture2D transitionTexture) : this(null, transitionTexture)
		{
		}

		public override IEnumerator OnBeginTransition()
		{
			// create a single pixel transparent texture. Our shader handles the rest.
			_overlayTexture = Graphics.CreateSingleColorTexture(1, 1, Color.Transparent);

			// obscure the screen
			yield return Core.StartCoroutine(TickEffectProgressProperty(_textureWipeEffect, Duration, EaseType));

			// load up the new Scene
			yield return Core.StartCoroutine(LoadNextScene());

			// undo the effect
			yield return Core.StartCoroutine(TickEffectProgressProperty(_textureWipeEffect, Duration,
				EaseHelper.OppositeEaseType(EaseType), true));

			TransitionComplete();

			// cleanup
			_overlayTexture.Dispose();
			Core.Content.UnloadEffect(_textureWipeEffect);
		}

		public override void Render(Batcher batcher)
		{
			Core.GraphicsDevice.SetRenderTarget(null);
			batcher.Begin(BlendState.AlphaBlend, Core.DefaultSamplerState, DepthStencilState.None, null,
				_textureWipeEffect);

			// we only render the previousSceneRender until we load up the new Scene
			if (!_isNewSceneLoaded)
				batcher.Draw(PreviousSceneRender, _destinationRect, Color.White);
			else
				batcher.Draw(_overlayTexture, new Rectangle(0, 0, Screen.Width, Screen.Height),
					Color.Transparent);

			batcher.End();
		}
	}
}