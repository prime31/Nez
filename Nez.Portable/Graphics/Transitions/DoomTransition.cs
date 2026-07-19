using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// From the classic game DOOM
	/// based on: https://gl-transitions.com/editor/DoomScreenTransition
	/// </summary>
	public class DoomTransition : SceneTransition
	{
		/// <summary>
		/// Number of total bars/columns. Defaults to 30
		/// </summary>
		/// <value>The number of total bars/columns.</value>
		public int Bars
		{
			set => _effect.Parameters["_bars"].SetValue(value);
		}

		/// <summary>
		/// Multiplier for speed ratio. Defaults to 2
		/// </summary>
		/// <value>Multiplier for speed ratio. 0 = no variation when going down, higher = some elements go much faster.</value>
		public float Amplitude
		{
			set => _effect.Parameters["_amplitude"].SetValue(value);
		}

		/// <summary>
		/// Further variations in speed. Defaults to 0.1
		/// </summary>
		/// <value>Further variations in speed. 0 = no noise, 1 = super noisy (ignore frequency)</value>
		public float Noise
		{
			set => _effect.Parameters["_noise"].SetValue(value);
		}

		/// <summary>
		/// Speed variation horizontally. Defaults to 0.5
		/// </summary>
		/// <value>Speed variation horizontally. the bigger the value, the shorter the waves</value>
		public float Frequency
		{
			set => _effect.Parameters["_frequency"].SetValue(value);
		}

		/// <summary>
		/// Runniness of bars near the center. Defaults to 0.5
		/// </summary>
		/// <value>How much the bars seem to "run" from the middle of the screen first (sticking to the sides). 0 = no drip, 1 = curved drip</value>
		public float DripScale
		{
			set => _effect.Parameters["_dripScale"].SetValue(value);
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

		public DoomTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/Doom.mgfxo");
			Bars = 30;
			Amplitude = 2f;
			Noise = 0.1f;
			Frequency = 0.5f;
			DripScale = 0.5f;
		}

		public DoomTransition() : this(null)
		{ }

		public override IEnumerator OnBeginTransition()
		{
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