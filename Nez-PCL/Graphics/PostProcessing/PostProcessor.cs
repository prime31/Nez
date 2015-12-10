using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez
{
	/// <summary>
	/// Post Processing step for rendering actions after everthing done.
	/// </summary>
	public abstract class PostProcessor
	{
		internal static Comparison<PostProcessor> comparePostProcessorOrder = ( a, b ) =>
		{
			return Math.Sign( b.executionOrder - a.executionOrder );
		};

		/// <summary>
		/// Step is Enabled or not.
		/// </summary>
		public bool enabled { get; protected set; }

		/// <summary>
		/// specifies the order in which the Renderers will be called by the scene
		/// </summary>
		public readonly int executionOrder = 0;


		public PostProcessor( int executionOrder )
		{
			enabled = true;
			this.executionOrder = executionOrder;
		}


		abstract public void process( RenderTexture source, RenderTexture destination );


		/// <summary>
		/// called when a scene is ended. use this for cleanup.
		/// </summary>
		public virtual void unload()
		{}


		/// <summary>
		/// helper for drawing a texture into a rendertarget, optionally using a custom shader to apply postprocessing effects.
		/// </summary>
		protected void drawFullscreenQuad( Texture2D texture, RenderTexture renderTexture, Effect effect = null )
		{
			Core.graphicsDevice.SetRenderTarget( renderTexture );
			drawFullscreenQuad( texture, renderTexture.renderTarget2D.Width, renderTexture.renderTarget2D.Height, effect );
		}


		/// <summary>
		/// helper for drawing a texture into the current rendertarget, optionally using a custom shader to apply postprocessing effects.
		/// </summary>
		protected void drawFullscreenQuad( Texture2D texture, int width, int height, Effect effect )
		{
			Graphics.instance.spriteBatch.Begin( 0, BlendState.Opaque, null, null, null, effect );
			Graphics.instance.spriteBatch.Draw( texture, new Rectangle( 0, 0, width, height ), Color.White );
			Graphics.instance.spriteBatch.End();
		}

	}
}

