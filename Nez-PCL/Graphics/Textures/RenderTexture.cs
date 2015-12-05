using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;
using Microsoft.Xna.Framework.Content;


namespace Nez.Textures
{
	public class RenderTexture : Texture
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
			renderTarget2D = new RenderTarget2D( Core.graphicsDevice, width, height );
			texture2D = (Texture2D)renderTarget2D;
			textureBounds = new Rectangle( 0, 0, width, height );
		}


		/// <summary>
		/// Creates a RenderTexture with the full size of the back buffer
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		public RenderTexture() : this( Core.graphicsDevice.PresentationParameters.BackBufferWidth, Core.graphicsDevice.PresentationParameters.BackBufferHeight )
		{}


		/// <summary>
		/// Creates a RenderTexture with the contents of the passed in Texture
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		/// <param name="tex">Tex.</param>
		public RenderTexture( Texture tex ) : this( tex.textureBounds.Width, tex.textureBounds.Height )
		{
			var data = new Color[tex.textureBounds.Width * tex.textureBounds.Height];
			tex.texture2D.GetData<Color>( data );
			renderTarget2D.SetData<Color>( data );
		}


		public override void load( string imagePath, ContentManager content )
		{
			throw new Exception( "Cannot load a RenderTexture" );
		}


		public override void unload()
		{
			renderTarget2D.Dispose();
			renderTarget2D = null;
			texture2D = null;
		}


		public static implicit operator RenderTarget2D( RenderTexture tex )
		{
			return tex.renderTarget2D;
		}

	}
}

