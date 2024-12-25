using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// Ripples on water surface
	/// based on: https://gl-transitions.com/editor/WaterDrop
	/// </summary>
	public class WaterDropTransition : SceneTransition
	{
		/// <summary>
		/// Amplitude of the ripples. Defaults to 30.
		/// </summary>
		/// <value>The amplitude of the ripples.</value>
		public float Amplitude
		{
			set => _effect.Parameters["_amplitude"].SetValue(value);
		}

		/// <summary>
		/// Speed of ripple propagation. Defaults to 30.
		/// </summary>
		/// <value>The speed of the ripples.</value>
		public float Speed
		{
			set => _effect.Parameters["_speed"].SetValue(value);
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

		public WaterDropTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/WaterDrop.mgfxo");
			Amplitude = 30f;
			Speed = 30f;
		}

		public WaterDropTransition() : this(null)
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