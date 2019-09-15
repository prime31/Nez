using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// helper subclass for strongly typed Effects loaded from EffectResource. The effect will automatically be unloaded when the scene
	/// completes.
	/// </summary>
	public class PostProcessor<T> : PostProcessor where T : Effect
	{
		/// <summary>
		/// The effect used to render the scene with
		/// </summary>
		public new T Effect;


		public PostProcessor(int executionOrder, T effect = null) : base(executionOrder, effect)
		{
			Effect = effect;
		}

		/// <summary>
		/// we have to override the default implementation here because we use a custom Effect subclass that differes from the effect
		/// field of the base class
		/// will be null.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public override void Process(RenderTarget2D source, RenderTarget2D destination)
		{
			DrawFullscreenQuad(source, destination, Effect);
		}

		public override void Unload()
		{
			base.Unload();
		}
	}
}