using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Nez.Textures;
using System.Diagnostics;
using Nez.BitmapFonts;


namespace Nez
{
	/// <summary>
	/// wrapper class that holds in instance of a Batcher and helpers so that it can be passed around and draw anything.
	/// </summary>
	public class Graphics
	{
		public static Graphics instance;

		/// <summary>
		/// All 2D rendering is done through this Batcher instance
		/// </summary>
		public Batcher batcher;

		/// <summary>
		/// default font is loaded up and stored here for easy access. Nez uses it for the DebugConsole
		/// </summary>
		public BitmapFont bitmapFont;

		/// <summary>
		/// A subtexture used to draw rectangles, lines, circles, etc. 
		/// Will be generated at startup, but you can replace this with a subtexture from your atlas to reduce texture swaps.
		/// Should be a 1x1 white pixel
		/// </summary>
		public Subtexture pixelTexture;


		public Graphics()
		{}


		public Graphics( BitmapFont font )
		{
			batcher = new Batcher( Core.graphicsDevice );
			bitmapFont = font;

			// the bottom/right pixel is white on the default font so we'll use that for the pixelTexture
			var fontTex = bitmapFont.defaultCharacterRegion.subtexture.texture2D;
			pixelTexture = new Subtexture( fontTex, fontTex.Width - 1, fontTex.Height - 1, 1, 1 );
		}


		/// <summary>
		/// helper method that generates a single color texture of the given dimensions
		/// </summary>
		/// <returns>The single color texture.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="color">Color.</param>
		public static Texture2D createSingleColorTexture( int width, int height, Color color )
		{
			var texture = new Texture2D( Core.graphicsDevice, width, height );
			var data = new Color[width * height];
			for( var i = 0; i < data.Length; i++ )
				data[i] = color;
			
			texture.SetData<Color>( data );
			return texture;
		}


		public void unload()
		{
			if( pixelTexture != null )
				pixelTexture.texture2D.Dispose();
			pixelTexture = null;

			batcher.Dispose();
			batcher = null;
		}

	}
}
