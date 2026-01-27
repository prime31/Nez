using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// Dissolve using a generated Perlin noise pattern
	/// based on: https://gl-transitions.com/editor/perlin
	/// </summary>
	public class PerlinTransition : SceneTransition
	{
		/// <summary>
		/// Scale of the noise. Defaults to 4.0
		/// </summary>
		/// <value>The scale of the Perlin noise.</value>
		public float Scale
		{
			set => _effect.Parameters["_scale"].SetValue(value);
		}

		/// <summary>
		/// Smoothness of the noise. Defaults to 0.01. (0.0 - 1.0)
		/// </summary>
		/// <value>The smoothness of the noise pattern</value>
		public float Smoothness
		{
			set => _effect.Parameters["_smoothness"].SetValue(value);
		}

		/// <summary>
		/// Seed for the Perlin noise. Defaults to 12.9898
		/// </summary>
		/// <value>The seed for the noise generator</value>
		public float Seed
		{
			set => _effect.Parameters["_seed"].SetValue(value);
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

		public PerlinTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/Perlin.mgfxo");
			Scale = 4.0f;
			Smoothness = 0.01f;
			Seed = 12.9898f;
		}

		public PerlinTransition() : this(null)
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