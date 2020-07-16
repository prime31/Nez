using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez
{
	/// <summary>
	/// post processor to assist with making blended sprite lights. Usage is as follows:
	/// - render all sprite lights with a separate Renderer to a RenderTarget. The clear color of the Renderer is your ambient light color.
	/// - render all normal objects in standard fashion
	/// - add this PostProcessor with the RenderTarget from your lights Renderer
	/// </summary>
	public class SpriteLightPostProcessor : PostProcessor
	{
		/// <summary>
		/// multiplicative factor for the blend of the base and light render targets. Defaults to 1.
		/// </summary>
		/// <value>The multiplicative factor.</value>
		public float MultiplicativeFactor
		{
			get => _multiplicativeFactor;
			set
			{
				if (Effect != null)
					Effect.Parameters["_multiplicativeFactor"].SetValue(value);
				_multiplicativeFactor = value;
			}
		}

		float _multiplicativeFactor = 1f;
		RenderTexture _lightsRenderTexture;


		public SpriteLightPostProcessor(int executionOrder, RenderTexture lightsRenderTexture) : base(executionOrder)
		{
			_lightsRenderTexture = lightsRenderTexture;
		}

		public override void OnAddedToScene(Scene scene)
		{
			base.OnAddedToScene(scene);

			Effect = scene.Content.LoadEffect<Effect>("spriteLightMultiply", EffectResource.SpriteLightMultiplyBytes);
			Effect.Parameters["_lightTexture"].SetValue(_lightsRenderTexture);
			Effect.Parameters["_multiplicativeFactor"].SetValue(_multiplicativeFactor);
		}

		public override void Unload()
		{
			_scene.Content.UnloadEffect(Effect);
			Effect = null;
			_lightsRenderTexture.Dispose();

			base.Unload();
		}

		public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			Core.GraphicsDevice.SetRenderTarget(destination);
			Graphics.Instance.Batcher.Begin(effect: Effect);
			Graphics.Instance.Batcher.Draw(source, new Rectangle(0, 0, destination.Width, destination.Height), Color.White);
			Graphics.Instance.Batcher.End();
		}

		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			// when the RenderTexture changes we have to reset the shader param since the underlying RenderTarget will be different
			Effect.Parameters["_lightTexture"].SetValue(_lightsRenderTexture);
		}
	}
}