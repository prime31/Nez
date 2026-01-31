using System;
using System.Collections;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Nez.Tweens;

namespace Nez
{
	/// <summary>
	/// Circle collapsing to black wipe.
	/// based on: https://gl-transitions.com/editor/CircleCrop
	/// </summary>
	public class CircleCropTransition : SceneTransition
	{
		/// <summary>
		/// Aspect ratio of the screen. Defaults to Screen.Width/Height
		/// </summary>
		/// <value>Ratio of the screen.</value>
		public float Ratio
		{
			set => _effect.Parameters["_ratio"].SetValue(value);
		}

		/// <summary>
		/// Center position of the circle. Defaults to Screen Size / 2
		/// </summary>
		/// <value>Ratio of the screen.</value>
		public Vector2 CirclePosition
		{
			set => _effect.Parameters["_circlePosition"].SetValue(value);
		}

		/// <summary>
		/// Background color. Defaults to Black
		/// </summary>
		/// <value>The color for the background.</value>
		public Color BgColor
		{
			set => _effect.Parameters["_bgcolor"].SetValue(value.ToVector4());
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

		public CircleCropTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
			_destinationRect = PreviousSceneRender.Bounds;

			// load Effect and set defaults
			_effect = Core.Content.LoadEffect("Content/nez/effects/transitions/CircleCrop.mgfxo");
			Ratio = (float)Screen.Width / Screen.Height;
			CirclePosition = new Vector2(0.5f, 0.5f);
			BgColor = Color.Black;
		}

		public CircleCropTransition() : this(null)
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