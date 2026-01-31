using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// Colorful twisting blurring transition
	/// based on: https://gl-transitions.com/editor/kaleidoscope
	/// </summary>
	public class KaleidoscopeTransition : SceneTransition
	{
		/// <summary>
		/// Speed of the rotation. Defaults to 1.0.
		/// </summary>
		/// <value>The speed of rotation.</value>
		public float Speed
		{
			set => _effect.Parameters["_speed"].SetValue(value);
		}

		/// <summary>
		/// How much to twist. Defaults to 1.0.
		/// </summary>
		/// <value>The rotation angle.</value>
		public float Angle
		{
			set => _effect.Parameters["_angle"].SetValue(value);
		}

		/// <summary>
		/// Power of the distortion. Defaults to 1.5.
		/// </summary>
		/// <value>The distortion power.</value>
		public float Power
		{
			set => _effect.Parameters["_power"].SetValue(value);
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

		public KaleidoscopeTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/Kaleidoscope.mgfxo");
			Speed = 1.0f;
			Angle = 1.0f;
			Power = 1.5f;
		}

		public KaleidoscopeTransition() : this(null)
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