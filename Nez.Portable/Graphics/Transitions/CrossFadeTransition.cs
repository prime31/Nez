using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Nez.Tweens;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// fades from the current Scene to the new Scene
	/// </summary>
	public class CrossFadeTransition : SceneTransition
	{
		/// <summary>
		/// duration for the fade
		/// </summary>
		public float FadeDuration = 1f;

		/// <summary>
		/// ease equation to use for the cross fade
		/// </summary>
		public EaseType FadeEaseType = EaseType.QuartIn;

		Color _fromColor = Color.White;
		Color _toColor = Color.Transparent;
		Color _color = Color.White;


		public CrossFadeTransition(Func<Scene> sceneLoadAction) : base(sceneLoadAction, true)
		{
		}

		public CrossFadeTransition() : this(null)
		{
		}

		public override IEnumerator OnBeginTransition()
		{
			yield return null;

			// load up the new Scene
			yield return Core.StartCoroutine(LoadNextScene());

			var elapsed = 0f;
			while (elapsed < FadeDuration)
			{
				elapsed += Time.DeltaTime;
				_color = Lerps.Ease(FadeEaseType, ref _fromColor, ref _toColor, elapsed, FadeDuration);

				yield return null;
			}

			TransitionComplete();
		}

		public override void Render(Batcher batcher)
		{
			Core.GraphicsDevice.SetRenderTarget(null);
			batcher.Begin(BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null);
			batcher.Draw(PreviousSceneRender, Vector2.Zero, _color);
			batcher.End();
		}
	}
}