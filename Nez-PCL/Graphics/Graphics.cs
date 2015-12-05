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

		public GraphicsDevice graphicsDevice;
		/// <summary>
		/// All 2D rendering is done through this SpriteBatch instance
		/// </summary>
		public SpriteBatch spriteBatch;

		/// <summary>
		/// default font is loaded up and stored here for easy access. Nez uses it for the DebugConsole
		/// </summary>
		public BitmapFont bitmapFont;

		/// <summary>
		/// A subtexture used to draw particle systems.
		/// Will be generated at startup, but you can replace this with a subtexture from your Atlas to reduce texture swaps.
		/// Should be a 2x2 white pixel
		/// </summary>
		public Subtexture particleTexture;

		/// <summary>
		/// A subtexture used to draw rectangles, lines, circles, etc. 
		/// Will be generated at startup, but you can replace this with a subtexture from your Atlas to reduce texture swaps.
		/// Use the top left pixel of your Particle Subtexture if you replace it!
		/// Should be a 1x1 white pixel
		/// </summary>
		public Subtexture pixelTexture;

		Rectangle _tempRect;


		public Graphics( GraphicsDevice graphicsDevice, BitmapFont font )
		{
			this.graphicsDevice = graphicsDevice;
			spriteBatch = new SpriteBatch( graphicsDevice );
			bitmapFont = font;

			var tex = createSingleColorTexture( 2, 2, Color.White );
			particleTexture = new Subtexture( tex, 0, 0, 2, 2 );
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
			var texture = new Texture2D( graphicsDevice, width, height );
			var data = new Color[width * height];
			for( var i = 0; i < data.Length; i++ )
				data[i] = color;
			
			texture.SetData<Color>( data );
			return texture;
		}


		public void unload()
		{
			if( pixelTexture != null )
				pixelTexture.unload();

			if( particleTexture != null )
				particleTexture.unload();

			spriteBatch.Dispose();
			spriteBatch = null;
		}


		#region Line

		public void drawLine( Vector2 start, Vector2 end, Color color )
		{
			drawLineAngle( start, Mathf.angleBetweenVectors( start, end ), Vector2.Distance( start, end ), color );
		}


		public void drawLine( Vector2 start, Vector2 end, Color color, float thickness )
		{
			drawLineAngle( start, Mathf.angleBetweenVectors( start, end ), Vector2.Distance( start, end ), color, thickness );
		}


		public void drawLine( float x1, float y1, float x2, float y2, Color color )
		{
			drawLine( new Vector2( x1, y1 ), new Vector2( x2, y2 ), color );
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public void drawPoints( List<Vector2> points, Color color, float thickness = 1 )
		{
			if( points.Count < 2 )
				return;

			for( int i = 1; i < points.Count; i++ )
				drawLine( points[i - 1], points[i], color, thickness );
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public void drawPoints( Vector2[] points, Color color, float thickness = 1 )
		{
			if( points.Length < 2 )
				return;

			for( int i = 1; i < points.Length; i++ )
				drawLine( points[i - 1], points[i], color, thickness );
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		/// <param name="closePoly">If set to <c>true</c> the first and last points will be connected.</param>
		public void drawPoints( Vector2 position, Vector2[] points, Color color, bool closePoly = true, float thickness = 1 )
		{
			if( points.Length < 2 )
				return;

			for( int i = 1; i < points.Length; i++ )
				drawLine( position + points[i - 1], position + points[i], color, thickness );

			if( closePoly )
				drawLine( position + points[points.Length - 1], position + points[0], color, thickness );
		}


		public void drawPolygon( Vector2 position, List<Vector2> points, Color color, bool closePoly = true, float thickness = 1 )
		{
			if( points.Count < 2 )
				return;

			for( int i = 1; i < points.Count; i++ )
				drawLine( position + points[i - 1], position + points[i], color, thickness );


			if( closePoly )
				drawLine( position + points[points.Count - 1], position + points[0], color, thickness );
		}

		#endregion


		#region Line Angle

		public void drawLineAngle( Vector2 start, float angle, float length, Color color )
		{
			spriteBatch.Draw( pixelTexture, start, null, pixelTexture.sourceRect, Vector2.Zero, angle, new Vector2( length, 1 ), color, SpriteEffects.None, 0 );
		}


		public void drawLineAngle( Vector2 start, float angle, float length, Color color, float thickness )
		{
			spriteBatch.Draw( pixelTexture, start, null, pixelTexture.sourceRect, new Vector2( 0f, 0.5f ), angle, new Vector2( length, thickness ), color, SpriteEffects.None, 0 );
		}


		public void drawLineAngle( float startX, float startY, float angle, float length, Color color )
		{
			drawLineAngle( new Vector2( startX, startY ), angle, length, color );
		}

		#endregion


		#region Circle

		public void drawCircle( Vector2 position, float radius, Color color, int resolution = 12 )
		{
			var last = Vector2.UnitX * radius;
			var lastP = Mathf.perpendicularVector( last );

			for( int i = 1; i <= resolution; i++ )
			{
				var at = Mathf.angleToVector( i * MathHelper.PiOver2 / resolution, radius );
				var atP = Mathf.perpendicularVector( at );

				drawLine( position + last, position + at, color );
				drawLine( position - last, position - at, color );
				drawLine( position + lastP, position + atP, color );
				drawLine( position - lastP, position - atP, color );

				last = at;
				lastP = atP;
			}
		}


		public void drawCircle( float x, float y, float radius, Color color, int resolution )
		{
			drawCircle( new Vector2( x, y ), radius, color, resolution );
		}

		#endregion


		#region Rect

		public void drawRect( float x, float y, float width, float height, Color color )
		{
			_tempRect.X = (int)x;
			_tempRect.Y = (int)y;
			_tempRect.Width = (int)width;
			_tempRect.Height = (int)height;
			spriteBatch.Draw( pixelTexture, _tempRect, pixelTexture.sourceRect, color );
		}


		public void drawRect( Vector2 position, float width, float height, Color color )
		{
			drawRect( position.X, position.Y, width, height, color );
		}


		public void drawRect( Rectangle rect, Color color )
		{
			_tempRect = rect;
			spriteBatch.Draw( pixelTexture, rect, pixelTexture.sourceRect, color );
		}

		#endregion


		#region Hollow Rect

		public void drawHollowRect( float x, float y, float width, float height, Color color )
		{
			_tempRect.X = (int)x;
			_tempRect.Y = (int)y;
			_tempRect.Width = (int)width;
			_tempRect.Height = 1;

			spriteBatch.Draw( pixelTexture, _tempRect, pixelTexture.sourceRect, color );

			_tempRect.Y += (int)height - 1;

			spriteBatch.Draw( pixelTexture, _tempRect, pixelTexture.sourceRect, color );

			_tempRect.Y -= (int)height - 1;
			_tempRect.Width = 1;
			_tempRect.Height = (int)height;

			spriteBatch.Draw( pixelTexture, _tempRect, pixelTexture.sourceRect, color );

			_tempRect.X += (int)width - 1;

			spriteBatch.Draw( pixelTexture, _tempRect, pixelTexture.sourceRect, color );
		}


		public void drawHollowRect( Vector2 position, float width, float height, Color color )
		{
			drawHollowRect( position.X, position.Y, width, height, color );
		}


		public void drawHollowRect( Rectangle rect, Color color )
		{
			drawHollowRect( rect.X, rect.Y, rect.Width, rect.Height, color );
		}

		#endregion


		#region Pixel

		public void drawPixel( float x, float y, Color color )
		{
			drawPixel( new Vector2( x, y ), color );
		}


		public void drawPixel( Vector2 position, Color color, int size = 1 )
		{
			var sourceRect = pixelTexture.sourceRect;
			if( size != 1 )
			{
				position.X -= size * 0.5f;
				position.Y -= size * 0.5f;
				sourceRect.Width *= size;
				sourceRect.Height *= size;
			}
			spriteBatch.Draw( pixelTexture, position, sourceRect, color );
		}

		#endregion

	}
}
