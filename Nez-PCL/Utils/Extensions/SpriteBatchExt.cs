using System;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System.Reflection;


namespace Nez
{
	public static class SpriteBatchExt
	{
		static Rectangle _tempRect;


		public static Matrix getSpriteBatchMatrix( this SpriteBatch spriteBatch )
		{
			var fieldInfo = ReflectionUtils.getFieldInfo( spriteBatch, "_matrix" );
			return (Matrix)fieldInfo.GetValue( spriteBatch );
		}


		#region Line

		public static void drawLine( this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color )
		{
			drawLineAngle( spriteBatch, start, Mathf.angleBetweenVectors( start, end ), Vector2.Distance( start, end ), color );
		}


		public static void drawLine( this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, float thickness )
		{
			drawLineAngle( spriteBatch, start, Mathf.angleBetweenVectors( start, end ), Vector2.Distance( start, end ), color, thickness );
		}


		public static void drawLine( this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color )
		{
			drawLine( spriteBatch, new Vector2( x1, y1 ), new Vector2( x2, y2 ), color );
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void drawPoints( this SpriteBatch spriteBatch, List<Vector2> points, Color color, float thickness = 1 )
		{
			if( points.Count < 2 )
				return;

			for( int i = 1; i < points.Count; i++ )
				drawLine( spriteBatch, points[i - 1], points[i], color, thickness );
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void drawPoints( this SpriteBatch spriteBatch, Vector2[] points, Color color, float thickness = 1 )
		{
			if( points.Length < 2 )
				return;

			for( int i = 1; i < points.Length; i++ )
				drawLine( spriteBatch, points[i - 1], points[i], color, thickness );
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		/// <param name="closePoly">If set to <c>true</c> the first and last points will be connected.</param>
		public static void drawPoints( this SpriteBatch spriteBatch, Vector2 position, Vector2[] points, Color color, bool closePoly = true, float thickness = 1 )
		{
			if( points.Length < 2 )
				return;

			for( int i = 1; i < points.Length; i++ )
				drawLine( spriteBatch, position + points[i - 1], position + points[i], color, thickness );

			if( closePoly )
				drawLine( spriteBatch, position + points[points.Length - 1], position + points[0], color, thickness );
		}


		public static void drawPolygon( this SpriteBatch spriteBatch, Vector2 position, Vector2[] points, Color color, bool closePoly = true, float thickness = 1 )
		{
			if( points.Length < 2 )
				return;

			for( int i = 1; i < points.Length; i++ )
				drawLine( spriteBatch, position + points[i - 1], position + points[i], color, thickness );


			if( closePoly )
				drawLine( spriteBatch, position + points[points.Length - 1], position + points[0], color, thickness );
		}

		#endregion


		#region Line Angle

		public static void drawLineAngle( this SpriteBatch spriteBatch, Vector2 start, float angle, float length, Color color )
		{
			spriteBatch.Draw( Graphics.instance.pixelTexture, start, Graphics.instance.pixelTexture.sourceRect, color, angle, Vector2.Zero, new Vector2( length, 1 ), SpriteEffects.None, 0 );
		}


		public static void drawLineAngle( this SpriteBatch spriteBatch, Vector2 start, float angle, float length, Color color, float thickness )
		{
			spriteBatch.Draw( Graphics.instance.pixelTexture, start, Graphics.instance.pixelTexture.sourceRect, color, angle, new Vector2( 0f, 0.5f ), new Vector2( length, thickness ), SpriteEffects.None, 0 );
		}


		public static void drawLineAngle( this SpriteBatch spriteBatch, float startX, float startY, float angle, float length, Color color )
		{
			drawLineAngle( spriteBatch, new Vector2( startX, startY ), angle, length, color );
		}

		#endregion


		#region Circle

		public static void drawCircle( this SpriteBatch spriteBatch, Vector2 position, float radius, Color color, float thickness = 1f, int resolution = 12 )
		{
			var last = Vector2.UnitX * radius;
			var lastP = Mathf.perpendicularVector( last );

			for( int i = 1; i <= resolution; i++ )
			{
				var at = Mathf.angleToVector( i * MathHelper.PiOver2 / resolution, radius );
				var atP = Mathf.perpendicularVector( at );

				drawLine( spriteBatch, position + last, position + at, color, thickness );
				drawLine( spriteBatch, position - last, position - at, color, thickness );
				drawLine( spriteBatch, position + lastP, position + atP, color, thickness );
				drawLine( spriteBatch, position - lastP, position - atP, color, thickness );

				last = at;
				lastP = atP;
			}
		}


		public static void drawCircle( this SpriteBatch spriteBatch, float x, float y, float radius, Color color, int resolution )
		{
			drawCircle( spriteBatch, new Vector2( x, y ), radius, color, resolution );
		}

		#endregion


		#region Rect

		public static void drawRect( this SpriteBatch spriteBatch, float x, float y, float width, float height, Color color )
		{
			_tempRect.X = (int)x;
			_tempRect.Y = (int)y;
			_tempRect.Width = (int)width;
			_tempRect.Height = (int)height;
			spriteBatch.Draw( Graphics.instance.pixelTexture, _tempRect, Graphics.instance.pixelTexture.sourceRect, color );
		}


		public static void drawRect( this SpriteBatch spriteBatch, Vector2 position, float width, float height, Color color )
		{
			drawRect( spriteBatch, position.X, position.Y, width, height, color );
		}


		public static void drawRect( this SpriteBatch spriteBatch, Rectangle rect, Color color )
		{
			_tempRect = rect;
			spriteBatch.Draw( Graphics.instance.pixelTexture, rect, Graphics.instance.pixelTexture.sourceRect, color );
		}

		#endregion


		#region Hollow Rect

		public static void drawHollowRect( this SpriteBatch spriteBatch, float x, float y, float width, float height, Color color )
		{
			_tempRect.X = (int)x;
			_tempRect.Y = (int)y;
			_tempRect.Width = (int)width;
			_tempRect.Height = 1;

			spriteBatch.Draw( Graphics.instance.pixelTexture, _tempRect, Graphics.instance.pixelTexture.sourceRect, color );

			_tempRect.Y += (int)height - 1;

			spriteBatch.Draw( Graphics.instance.pixelTexture, _tempRect, Graphics.instance.pixelTexture.sourceRect, color );

			_tempRect.Y -= (int)height - 1;
			_tempRect.Width = 1;
			_tempRect.Height = (int)height;

			spriteBatch.Draw( Graphics.instance.pixelTexture, _tempRect, Graphics.instance.pixelTexture.sourceRect, color );

			_tempRect.X += (int)width - 1;

			spriteBatch.Draw( Graphics.instance.pixelTexture, _tempRect, Graphics.instance.pixelTexture.sourceRect, color );
		}


		public static void drawHollowRect( this SpriteBatch spriteBatch, Vector2 position, float width, float height, Color color )
		{
			drawHollowRect( spriteBatch, position.X, position.Y, width, height, color );
		}


		public static void drawHollowRect( this SpriteBatch spriteBatch, Rectangle rect, Color color )
		{
			drawHollowRect( spriteBatch, rect.X, rect.Y, rect.Width, rect.Height, color );
		}


		public static void drawHollowRect( this SpriteBatch spriteBatch, RectangleF rect, Color color )
		{
			drawHollowRect( spriteBatch, rect.x, rect.y, rect.width, rect.height, color );
		}

		#endregion


		#region Pixel

		public static void drawPixel( this SpriteBatch spriteBatch, float x, float y, Color color )
		{
			drawPixel( spriteBatch, new Vector2( x, y ), color );
		}


		public static void drawPixel( this SpriteBatch spriteBatch, Vector2 position, Color color, int size = 1 )
		{
			var sourceRect = Graphics.instance.pixelTexture.sourceRect;
			if( size != 1 )
			{
				position.X -= size * 0.5f;
				position.Y -= size * 0.5f;
				sourceRect.Width *= size;
				sourceRect.Height *= size;
			}
			spriteBatch.Draw( Graphics.instance.pixelTexture, position, sourceRect, color );
		}

		#endregion

	}
}

