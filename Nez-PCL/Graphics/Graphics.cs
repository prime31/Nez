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
	/// wrapper class that holds in instance of a SpriteBatch and helpers so that it can be passed around and draw anything.
	/// </summary>
	public class Graphics
	{
		public static Graphics instance;

		/// <summary>
		/// All 2D rendering is done through this SpriteBatch instance
		/// </summary>
		public SpriteBatch spriteBatch;

		/// <summary>
		/// default font is loaded up and stored here for easy access. Nez uses it for the DebugConsole
		/// </summary>
		public BitmapFont bitmapFont;

		/// <summary>
		/// A subtexture used to draw rectangles, lines, circles, etc. 
		/// Will be generated at startup, but you can replace this with a subtexture from your Atlas to reduce texture swaps.
		/// Use the top left pixel of your Particle Subtexture if you replace it!
		/// Should be a 1x1 white pixel
		/// </summary>
		public Subtexture pixelTexture;


		public Graphics( BitmapFont font )
		{
			spriteBatch = new SpriteBatch( Core.graphicsDevice );
			bitmapFont = font;

			var tex = createSingleColorTexture( 1, 1, Color.White );
			pixelTexture = new Subtexture( tex, 0, 0, 1, 1 );
		}


		/// <summary>
		/// helper method that generates a single color texture of the given dimensions
		/// </summary>
		/// <returns>The single color texture.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="color">Color.</param>
		Texture2D createSingleColorTexture( int width, int height, Color color )
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

			spriteBatch.Dispose();
			spriteBatch = null;
		}

	}
}
