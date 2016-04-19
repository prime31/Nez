using System;
using Microsoft.Xna.Framework.Graphics;
using Nez.Textures;


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
		public new T effect;


		public PostProcessor( int executionOrder ) : base( executionOrder )
		{}


		/// <summary>
		/// we have to override the default implementation here because we use a custom Effect subclass and the effect field of the base class
		/// will be null.
		/// </summary>
		/// <param name="source">Source.</param>
		/// <param name="destination">Destination.</param>
		public override void process( RenderTarget2D source, RenderTarget2D destination )
		{
			drawFullscreenQuad( source, destination, effect );
		}


		public override void unload()
		{
			if( effect != null )
			{
				effect.Dispose();
				effect = null;
			}
		}
	}
}

