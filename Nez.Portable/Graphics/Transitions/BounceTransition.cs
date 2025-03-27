using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// Screen falls down and bounces at the bottom
	/// based on: https://gl-transitions.com/editor/Bounce
	/// </summary>
	public class BounceTransition : SceneTransition
	{
		/// <summary>
		/// Color of the dropshadow. Uses alpha. Defaults to Color.FromNonPremultiplied(0, 0, 0, 153).
		/// </summary>
		/// <value>The color of the dropshadow.</value>
		public Vector4 ShadowColor
		{
			set => _effect.Parameters["_shadowColor"].SetValue(value);
		}

		/// <summary>
		/// The height of the dropshadow. Defaults to 0.075 (0.0 - 1.0)
		/// </summary>
		/// <value>The height of the dropshadow.</value>
		public float ShadowHeight
		{
			set => _effect.Parameters["_shadowHeight"].SetValue(value);
		}

		/// <summary>
		/// Number of times to bounce. Defaults to 3.
		/// </summary>
		/// <value>The number of bounces</value>
		public float Bounces
		{
			set => _effect.Parameters["_bounces"].SetValue(value);
		}

		/// <summary>
		/// duration for the transition
		/// </summary>
		public float Duration = 1f;

		/// <summary>
		/// ease equation to use for the animation
		/// </summary>
		public EaseType EaseType = EaseType.Linear;

		Effect _effect;
		Rectangle _destinationRect;

		public BounceTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/Bounce.mgfxo");
			ShadowColor = new Vector4(0, 0, 0, 0.6f);
			ShadowHeight = 0.075f;
			Bounces = 3;
		}

		public BounceTransition() : this(null)
		{ }

		public override IEnumerator OnBeginTransition()
		{
			yield return null;

			// load up the new Scene
			yield return Core.StartCoroutine(LoadNextScene());

			// apply the effect
			yield return Core.StartCoroutine(TickEffectProgressProperty(_effect, Duration, EaseType));

			TransitionComplete();

			// cleanup
			Core.Content.UnloadEffect(_effect);
		}

		public override void Render(Batcher batcher)
		{
			Core.GraphicsDevice.SetRenderTarget(null);
			batcher.Begin(BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null, _effect);
			batcher.Draw(PreviousSceneRender, _destinationRect, Color.White);
			batcher.End();
		}
	}
}