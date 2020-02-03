using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public static class SpriteBatchExt
	{
		static Rectangle _tempRect;


		#region Line

		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color)
		{
			DrawLineAngle(spriteBatch, start, Mathf.AngleBetweenVectors(start, end), Vector2.Distance(start, end),
				color);
		}


		public static void DrawLine(this SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color,
		                            float thickness)
		{
			DrawLineAngle(spriteBatch, start, Mathf.AngleBetweenVectors(start, end), Vector2.Distance(start, end),
				color, thickness);
		}


		public static void DrawLine(this SpriteBatch spriteBatch, float x1, float y1, float x2, float y2, Color color)
		{
			DrawLine(spriteBatch, new Vector2(x1, y1), new Vector2(x2, y2), color);
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPoints(this SpriteBatch spriteBatch, List<Vector2> points, Color color,
		                              float thickness = 1)
		{
			if (points.Count < 2)
				return;

			for (int i = 1; i < points.Count; i++)
				DrawLine(spriteBatch, points[i - 1], points[i], color, thickness);
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPoints(this SpriteBatch spriteBatch, Vector2[] points, Color color, float thickness = 1)
		{
			if (points.Length < 2)
				return;

			for (int i = 1; i < points.Length; i++)
				DrawLine(spriteBatch, points[i - 1], points[i], color, thickness);
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		/// <param name="closePoly">If set to <c>true</c> the first and last points will be connected.</param>
		public static void DrawPoints(this SpriteBatch spriteBatch, Vector2 position, Vector2[] points, Color color,
		                              bool closePoly = true, float thickness = 1)
		{
			if (points.Length < 2)
				return;

			for (int i = 1; i < points.Length; i++)
				DrawLine(spriteBatch, position + points[i - 1], position + points[i], color, thickness);

			if (closePoly)
				DrawLine(spriteBatch, position + points[points.Length - 1], position + points[0], color, thickness);
		}


		public static void DrawPolygon(this SpriteBatch spriteBatch, Vector2 position, Vector2[] points, Color color,
		                               bool closePoly = true, float thickness = 1)
		{
			if (points.Length < 2)
				return;

			for (int i = 1; i < points.Length; i++)
				DrawLine(spriteBatch, position + points[i - 1], position + points[i], color, thickness);


			if (closePoly)
				DrawLine(spriteBatch, position + points[points.Length - 1], position + points[0], color, thickness);
		}

		#endregion


		#region Line Angle

		public static void DrawLineAngle(this SpriteBatch spriteBatch, Vector2 start, float angle, float length,
		                                 Color color)
		{
			spriteBatch.Draw(Graphics.Instance.PixelTexture, start, Graphics.Instance.PixelTexture.SourceRect, color,
				angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
		}


		public static void DrawLineAngle(this SpriteBatch spriteBatch, Vector2 start, float angle, float length,
		                                 Color color, float thickness)
		{
			spriteBatch.Draw(Graphics.Instance.PixelTexture, start, Graphics.Instance.PixelTexture.SourceRect, color,
				angle, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0);
		}


		public static void DrawLineAngle(this SpriteBatch spriteBatch, float startX, float startY, float angle,
		                                 float length, Color color)
		{
			DrawLineAngle(spriteBatch, new Vector2(startX, startY), angle, length, color);
		}

		#endregion


		#region Circle

		public static void DrawCircle(this SpriteBatch spriteBatch, Vector2 position, float radius, Color color,
		                              float thickness = 1f, int resolution = 12)
		{
			var last = Vector2.UnitX * radius;
			var lastP = Vector2Ext.Perpendicular(last);

			for (int i = 1; i <= resolution; i++)
			{
				var at = Mathf.AngleToVector(i * MathHelper.PiOver2 / resolution, radius);
				var atP = Vector2Ext.Perpendicular(at);

				DrawLine(spriteBatch, position + last, position + at, color, thickness);
				DrawLine(spriteBatch, position - last, position - at, color, thickness);
				DrawLine(spriteBatch, position + lastP, position + atP, color, thickness);
				DrawLine(spriteBatch, position - lastP, position - atP, color, thickness);

				last = at;
				lastP = atP;
			}
		}


		public static void DrawCircle(this SpriteBatch spriteBatch, float x, float y, float radius, Color color,
		                              int resolution)
		{
			DrawCircle(spriteBatch, new Vector2(x, y), radius, color, resolution);
		}

		#endregion


		#region Rect

		public static void DrawRect(this SpriteBatch spriteBatch, float x, float y, float width, float height,
		                            Color color)
		{
			_tempRect.X = (int) x;
			_tempRect.Y = (int) y;
			_tempRect.Width = (int) width;
			_tempRect.Height = (int) height;
			spriteBatch.Draw(Graphics.Instance.PixelTexture, _tempRect, Graphics.Instance.PixelTexture.SourceRect,
				color);
		}


		public static void DrawRect(this SpriteBatch spriteBatch, Vector2 position, float width, float height,
		                            Color color)
		{
			DrawRect(spriteBatch, position.X, position.Y, width, height, color);
		}


		public static void DrawRect(this SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			_tempRect = rect;
			spriteBatch.Draw(Graphics.Instance.PixelTexture, rect, Graphics.Instance.PixelTexture.SourceRect, color);
		}

		#endregion


		#region Hollow Rect

		public static void DrawHollowRect(this SpriteBatch spriteBatch, float x, float y, float width, float height,
		                                  Color color)
		{
			_tempRect.X = (int) x;
			_tempRect.Y = (int) y;
			_tempRect.Width = (int) width;
			_tempRect.Height = 1;

			spriteBatch.Draw(Graphics.Instance.PixelTexture, _tempRect, Graphics.Instance.PixelTexture.SourceRect,
				color);

			_tempRect.Y += (int) height - 1;

			spriteBatch.Draw(Graphics.Instance.PixelTexture, _tempRect, Graphics.Instance.PixelTexture.SourceRect,
				color);

			_tempRect.Y -= (int) height - 1;
			_tempRect.Width = 1;
			_tempRect.Height = (int) height;

			spriteBatch.Draw(Graphics.Instance.PixelTexture, _tempRect, Graphics.Instance.PixelTexture.SourceRect,
				color);

			_tempRect.X += (int) width - 1;

			spriteBatch.Draw(Graphics.Instance.PixelTexture, _tempRect, Graphics.Instance.PixelTexture.SourceRect,
				color);
		}


		public static void DrawHollowRect(this SpriteBatch spriteBatch, Vector2 position, float width, float height,
		                                  Color color)
		{
			DrawHollowRect(spriteBatch, position.X, position.Y, width, height, color);
		}


		public static void DrawHollowRect(this SpriteBatch spriteBatch, Rectangle rect, Color color)
		{
			DrawHollowRect(spriteBatch, rect.X, rect.Y, rect.Width, rect.Height, color);
		}


		public static void DrawHollowRect(this SpriteBatch spriteBatch, RectangleF rect, Color color)
		{
			DrawHollowRect(spriteBatch, rect.X, rect.Y, rect.Width, rect.Height, color);
		}

		#endregion


		#region Pixel

		public static void DrawPixel(this SpriteBatch spriteBatch, float x, float y, Color color)
		{
			DrawPixel(spriteBatch, new Vector2(x, y), color);
		}


		public static void DrawPixel(this SpriteBatch spriteBatch, Vector2 position, Color color, int size = 1)
		{
			var sourceRect = Graphics.Instance.PixelTexture.SourceRect;
			if (size != 1)
			{
				position.X -= size * 0.5f;
				position.Y -= size * 0.5f;
				sourceRect.Width *= size;
				sourceRect.Height *= size;
			}

			spriteBatch.Draw(Graphics.Instance.PixelTexture, position, sourceRect, color);
		}

		#endregion
	}
}