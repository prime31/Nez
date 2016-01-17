using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Content;


namespace Nez.Textures
{
	public class RenderTexture
	{
		public RenderTarget2D renderTarget2D;


		/// <summary>
		/// Creates a RenderTexture with the passed in size
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public RenderTexture( int width, int height )
		{
			renderTarget2D = new RenderTarget2D( Core.graphicsDevice, width, height, false, SurfaceFormat.Color, Screen.preferredDepthStencilFormat );
		}


		/// <summary>
		/// Creates a RenderTexture with the full size of the back buffer
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		public RenderTexture() : this( Screen.backBufferWidth, Screen.backBufferHeight )
		{}


		/// <summary>
		/// Creates a RenderTexture with the contents of the passed in Texture
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		/// <param name="tex">Tex.</param>
		public RenderTexture( Texture2D tex ) : this( tex.Bounds.Width, tex.Bounds.Height )
		{
			var data = new Color[tex.Bounds.Width * tex.Bounds.Height];
			tex.GetData<Color>( data );
			renderTarget2D.SetData<Color>( data );
		}


		/// <summary>
		/// disposes of the RenderTarget2D
		/// </summary>
		public void unload()
		{
			renderTarget2D.Dispose();
			renderTarget2D = null;
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
			if( renderTarget2D.Width == width && renderTarget2D.Height == height )
				return;
			
			// unload if necessary
			if( renderTarget2D != null )
				unload();
			
			renderTarget2D = new RenderTarget2D( Core.graphicsDevice, width, height );
		}


		public static implicit operator RenderTarget2D( RenderTexture tex )
		{
			return tex.renderTarget2D;
		}


		public static implicit operator Texture2D( RenderTexture tex )
		{
			return tex.renderTarget2D;
		}

	}
}

