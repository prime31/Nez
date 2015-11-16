using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.IO;


namespace Nez.TextureAtlases
{
	public class RenderTexture : Texture
	{
		public RenderTarget2D renderTarget2D;


		public RenderTexture( GraphicsDevice graphicsDevice, int width, int height )
		{
			renderTarget2D = new RenderTarget2D( graphicsDevice, width, height );
			texture2D = (Texture2D)renderTarget2D;
			textureBounds = new Rectangle( 0, 0, width, height );
		}


		/// <summary>
		/// Creates a RenderTexture with the contents of the passed in Texture
		/// </summary>
		/// <param name="graphicsDevice">Graphics device.</param>
		/// <param name="tex">Tex.</param>
		public RenderTexture( GraphicsDevice graphicsDevice, Texture tex ) : this( graphicsDevice, tex.textureBounds.Width, tex.textureBounds.Height )
		{
			var data = new Color[tex.textureBounds.Width * tex.textureBounds.Height];
			tex.texture2D.GetData<Color>( data );
			renderTarget2D.SetData<Color>( data );
		}


		public override void load( GraphicsDevice graphicsDevice, Stream fileStream )
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

