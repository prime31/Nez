using System;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Textures
{
	public class RenderTexture : IDisposable
	{
		/// <summary>
		/// handles what should happen when onSceneBackBufferSizeChanged. Defaults to SizeToSceneRenderTarget
		/// </summary>
		public enum RenderTextureResizeBehavior
		{
			None,
			SizeToSceneRenderTarget,
			SizeToScreen
		}

		/// <summary>
		/// the RenderTarget2D this RenderTexture manages
		/// </summary>
		public RenderTarget2D renderTarget;

		/// <summary>
		/// resize behavior that should occur when onSceneBackBufferSizeChanged is called
		/// </summary>
		public RenderTextureResizeBehavior resizeBehavior = RenderTextureResizeBehavior.SizeToSceneRenderTarget;


		#region constructors

		/// <summary>
		/// helper for creating a full screen RenderTarget2D
		/// </summary>
		public RenderTexture()
		{
			renderTarget = RenderTarget.create( Screen.backBufferWidth, Screen.backBufferHeight, Screen.backBufferFormat, Screen.preferredDepthStencilFormat );
		}


		/// <summary>
		/// helper for creating a full screen RenderTarget2D with a specific DepthFormat
		/// </summary>
		/// <param name="preferredDepthFormat">Preferred depth format.</param>
		public RenderTexture( DepthFormat preferredDepthFormat )
		{
			renderTarget = RenderTarget.create( Screen.backBufferWidth, Screen.backBufferHeight, Screen.backBufferFormat, preferredDepthFormat );
		}


		/// <summary>
		/// helper for creating a RenderTarget2D
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public RenderTexture( int width, int height )
		{
			renderTarget = RenderTarget.create( width, height, Screen.backBufferFormat, Screen.preferredDepthStencilFormat );
		}


		/// <summary>
		/// helper for creating a RenderTarget2D
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="preferredDepthFormat">Preferred depth format.</param>
		public RenderTexture( int width, int height, DepthFormat preferredDepthFormat )
		{
			renderTarget = RenderTarget.create( width, height, Screen.backBufferFormat, preferredDepthFormat );
		}


		/// <summary>
		/// helper for creating a RenderTarget2D
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="preferredFormat">Preferred format.</param>
		/// <param name="preferredDepthFormat">Preferred depth format.</param>
		public RenderTexture( int width, int height, SurfaceFormat preferredFormat, DepthFormat preferredDepthFormat )
		{
			renderTarget = new RenderTarget2D( Core.graphicsDevice, width, height, false, preferredFormat, preferredDepthFormat, 0, RenderTargetUsage.PreserveContents );
		}

		#endregion


		/// <summary>
		/// called by Renderers automatically when appropriate. Lets the resizeBehavior kick in so auto resizing can occur
		/// </summary>
		/// <param name="newWidth">New width.</param>
		/// <param name="newHeight">New height.</param>
		public void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			switch( resizeBehavior )
			{
				case RenderTextureResizeBehavior.None:
					break;
				case RenderTextureResizeBehavior.SizeToSceneRenderTarget:
					resize( newWidth, newHeight );
					break;
				case RenderTextureResizeBehavior.SizeToScreen:
					resize( Screen.width, Screen.height );
					break;
			}
		}


		/// <summary>
		/// resizes the RenderTarget2D to match the back buffer size
		/// </summary>
		public void resizeToFitBackbuffer()
		{
			resize( Screen.backBufferWidth, Screen.backBufferHeight );
		}


		/// <summary>
		/// resizes the RenderTarget2D to the specified size
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public void resize( int width, int height )
		{
			// no need to resize if we are already the right size
			if( renderTarget.Width == width && renderTarget.Height == height && !renderTarget.IsDisposed )
				return;

			// retain the same DepthFormat when we recreate the RenderTarget2D
			var depthFormat = renderTarget.DepthStencilFormat;

			// unload if necessary
			Dispose();

			renderTarget = RenderTarget.create( width, height, depthFormat );
		}


		public void Dispose()
		{
			if( renderTarget != null && !renderTarget.IsDisposed )
			{
				renderTarget.Dispose();
				renderTarget = null;
			}
		}


		public static implicit operator RenderTarget2D( RenderTexture tex )
		{
			return tex.renderTarget;
		}
	}
}

