using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// A cascade of opening vertical blinds
	/// based on: https://gl-transitions.com/editor/windowslice
	/// </summary>
	public class WindowSliceTransition : SceneTransition
	{
		/// <summary>
		/// Number of window slices. Defaults to 10. (1 - 100)
		/// </summary>
		/// <value>The number of slices.</value>
		public float Count
		{
			set => _effect.Parameters["_count"].SetValue(value);
		}

		/// <summary>
		/// Duration and speed of each slice. Defaults to 0.5. (0.0 - 1.0) 
		/// </summary>
		/// <value>The smoothness amount.</value>
		public float Smoothness
		{
			set => _effect.Parameters["_smoothness"].SetValue(value);
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


		public WindowSliceTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/WindowSlice.mgfxo");
			Count = 10;
			Smoothness = 0.5f;
		}

		public WindowSliceTransition() : this(null)
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