using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// Grid of dots that grow. No aspect ratio correction
	/// based on: https://gl-transitions.com/editor/PolkaDotsCurtain
	/// </summary>
	public class PolkaDotsCurtainTransition : SceneTransition
	{
		/// <summary>
		/// Number of dots per axis. Defaults to 20.
		/// </summary>
		/// <value>The number of dots per axis.</value>
		public float Dots
		{
			set => _effect.Parameters["_dots"].SetValue(value);
		}

		/// <summary>
		/// Origin of dots animating. Defaults to Vector2(0, 0). "Top-Left"
		/// </summary>
		/// <value>The center origin.</value>
		public Vector2 Center
		{
			set => _effect.Parameters["_center"].SetValue(value);
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

		public PolkaDotsCurtainTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/PolkaDotsCurtain.mgfxo");
			Center = new Vector2(0, 0);
			Dots = 20f;
		}

		public PolkaDotsCurtainTransition() : this(null)
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