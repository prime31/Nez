using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// an assortment of helper methods to assist with drawing
	/// </summary>
	public static class BatcherDrawingExt
	{
		static Rectangle _tempRect;


		#region Line

		public static void DrawLine(this Batcher batcher, Vector2 start, Vector2 end, Color color)
		{
			DrawLineAngle(batcher, start, Mathf.AngleBetweenVectors(start, end), Vector2.Distance(start, end), color);
		}


		public static void DrawLine(this Batcher batcher, Vector2 start, Vector2 end, Color color, float thickness)
		{
			DrawLineAngle(batcher, start, Mathf.AngleBetweenVectors(start, end), Vector2.Distance(start, end), color,
				thickness);
		}


		public static void DrawLine(this Batcher batcher, float x1, float y1, float x2, float y2, Color color)
		{
			DrawLine(batcher, new Vector2(x1, y1), new Vector2(x2, y2), color);
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPoints(this Batcher batcher, List<Vector2> points, Color color, float thickness = 1)
		{
			if (points.Count < 2)
				return;

			batcher.SetIgnoreRoundingDestinations(true);
			for (int i = 1; i < points.Count; i++)
				DrawLine(batcher, points[i - 1], points[i], color, thickness);
			batcher.SetIgnoreRoundingDestinations(false);
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		public static void DrawPoints(this Batcher batcher, Vector2[] points, Color color, float thickness = 1)
		{
			if (points.Length < 2)
				return;

			batcher.SetIgnoreRoundingDestinations(true);
			for (int i = 1; i < points.Length; i++)
				DrawLine(batcher, points[i - 1], points[i], color, thickness);
			batcher.SetIgnoreRoundingDestinations(false);
		}


		/// <summary>
		/// Draws a list of connected points
		/// </summary>
		/// <param name="points">The points to connect with lines</param>
		/// <param name="color">The color to use</param>
		/// <param name="thickness">The thickness of the lines</param>
		/// <param name="closePoly">If set to <c>true</c> the first and last points will be connected.</param>
		public static void DrawPoints(this Batcher batcher, Vector2 position, Vector2[] points, Color color,
		                              bool closePoly = true, float thickness = 1)
		{
			if (points.Length < 2)
				return;

			batcher.SetIgnoreRoundingDestinations(true);
			for (int i = 1; i < points.Length; i++)
				DrawLine(batcher, position + points[i - 1], position + points[i], color, thickness);

			if (closePoly)
				DrawLine(batcher, position + points[points.Length - 1], position + points[0], color, thickness);
			batcher.SetIgnoreRoundingDestinations(false);
		}


		public static void DrawPolygon(this Batcher batcher, Vector2 position, Vector2[] points, Color color,
		                               bool closePoly = true, float thickness = 1)
		{
			if (points.Length < 2)
				return;

			batcher.SetIgnoreRoundingDestinations(true);
			for (int i = 1; i < points.Length; i++)
				DrawLine(batcher, position + points[i - 1], position + points[i], color, thickness);


			if (closePoly)
				DrawLine(batcher, position + points[points.Length - 1], position + points[0], color, thickness);
			batcher.SetIgnoreRoundingDestinations(false);
		}

		#endregion


		#region Line Angle

		public static void DrawLineAngle(this Batcher batcher, Vector2 start, float radians, float length, Color color)
		{
			batcher.Draw(Graphics.Instance.PixelTexture, start, Graphics.Instance.PixelTexture.SourceRect, color,
				radians, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
		}


		public static void DrawLineAngle(this Batcher batcher, Vector2 start, float radians, float length, Color color,
		                                 float thickness)
		{
			batcher.Draw(Graphics.Instance.PixelTexture, start, Graphics.Instance.PixelTexture.SourceRect, color,
				radians, new Vector2(0f, 0.5f), new Vector2(length, thickness), SpriteEffects.None, 0);
		}


		public static void DrawLineAngle(this Batcher batcher, float startX, float startY, float radians, float length,
		                                 Color color)
		{
			DrawLineAngle(batcher, new Vector2(startX, startY), radians, length, color);
		}

		#endregion


		#region Circle

		public static void DrawCircle(this Batcher batcher, Vector2 position, float radius, Color color,
		                              float thickness = 1f, int resolution = 12)
		{
			var last = Vector2.UnitX * radius;
			var lastP = Vector2Ext.Perpendicular(last);

			batcher.SetIgnoreRoundingDestinations(true);
			for (int i = 1; i <= resolution; i++)
			{
				var at = Mathf.AngleToVector(i * MathHelper.PiOver2 / resolution, radius);
				var atP = Vector2Ext.Perpendicular(at);

				DrawLine(batcher, position + last, position + at, color, thickness);
				DrawLine(batcher, position - last, position - at, color, thickness);
				DrawLine(batcher, position + lastP, position + atP, color, thickness);
				DrawLine(batcher, position - lastP, position - atP, color, thickness);

				last = at;
				lastP = atP;
			}

			batcher.SetIgnoreRoundingDestinations(false);
		}


		public static void DrawCircle(this Batcher batcher, float x, float y, float radius, Color color,
		                              int thickness = 1, int resolution = 12)
		{
			DrawCircle(batcher, new Vector2(x, y), radius, color, thickness, resolution);
		}

		#endregion


		#region Rect

		public static void DrawRect(this Batcher batcher, float x, float y, float width, float height, Color color)
		{
			_tempRect.X = (int) x;
			_tempRect.Y = (int) y;
			_tempRect.Width = (int) width;
			_tempRect.Height = (int) height;
			batcher.Draw(Graphics.Instance.PixelTexture, _tempRect, Graphics.Instance.PixelTexture.SourceRect, color);
		}


		public static void DrawRect(this Batcher batcher, Vector2 position, float width, float height, Color color)
		{
			DrawRect(batcher, position.X, position.Y, width, height, color);
		}


		public static void DrawRect(this Batcher batcher, Rectangle rect, Color color)
		{
			batcher.Draw(Graphics.Instance.PixelTexture, rect, Graphics.Instance.PixelTexture.SourceRect, color);
		}

		public static void DrawHollowRect(this Batcher batcher, float x, float y, float width, float height,
		                                  Color color, float rotation, Vector2 origin, float thickness = 1)
		{
			var rotor = Matrix2D.CreateTranslation(-origin);
			rotor *= Matrix2D.CreateRotation(rotation);
			rotor *= Matrix2D.CreateTranslation(origin);

			var tl = Vector2Ext.Round(new Vector2(x, y));
			var tr = Vector2Ext.Round(Vector2.Transform(new Vector2(x + width, y), rotor));
			var br = Vector2Ext.Round(Vector2.Transform(new Vector2(x + width, y + height), rotor));
			var bl = Vector2Ext.Round(Vector2.Transform(new Vector2(x, y + height), rotor));

			batcher.SetIgnoreRoundingDestinations(true);
			batcher.DrawLine(tl, tr, color, thickness);
			batcher.DrawLine(tr, br, color, thickness);
			batcher.DrawLine(br, bl, color, thickness);
			batcher.DrawLine(bl, tl, color, thickness);
			batcher.SetIgnoreRoundingDestinations(false);
		}

		public static void DrawHollowRect(this Batcher batcher, Vector2 position, float width, float height,
		                                  Color color, float rotation, Vector2 origin, float thickness = 1)
		{
			DrawHollowRect(batcher, position.X, position.Y, width, height, color, rotation, origin, thickness);
		}

		public static void DrawHollowRect(this Batcher batcher, Rectangle rect, Color color, float rotation, Vector2 origin, float thickness = 1)
		{
			DrawHollowRect(batcher, rect.X, rect.Y, rect.Width, rect.Height, color, rotation, origin, thickness);
		}

		public static void DrawHollowRect(this Batcher batcher, RectangleF rect, Color color, float rotation, Vector2 origin, float thickness = 1)
		{
			DrawHollowRect(batcher, rect.X, rect.Y, rect.Width, rect.Height, color, rotation, origin, thickness);
		}

		#endregion


		#region Hollow Rect

		public static void DrawHollowRect(this Batcher batcher, float x, float y, float width, float height,
		                                  Color color, float thickness = 1)
		{
			var tl = Vector2Ext.Round(new Vector2(x, y));
			var tr = Vector2Ext.Round(new Vector2(x + width, y));
			var br = Vector2Ext.Round(new Vector2(x + width, y + height));
			var bl = Vector2Ext.Round(new Vector2(x, y + height));

			batcher.SetIgnoreRoundingDestinations(true);
			batcher.DrawLine(tl, tr, color, thickness);
			batcher.DrawLine(tr, br, color, thickness);
			batcher.DrawLine(br, bl, color, thickness);
			batcher.DrawLine(bl, tl, color, thickness);
			batcher.SetIgnoreRoundingDestinations(false);
		}


		public static void DrawHollowRect(this Batcher batcher, Vector2 position, float width, float height,
		                                  Color color, float thickness = 1)
		{
			DrawHollowRect(batcher, position.X, position.Y, width, height, color, thickness);
		}


		public static void DrawHollowRect(this Batcher batcher, Rectangle rect, Color color, float thickness = 1)
		{
			DrawHollowRect(batcher, rect.X, rect.Y, rect.Width, rect.Height, color, thickness);
		}


		public static void DrawHollowRect(this Batcher batcher, RectangleF rect, Color color, float thickness = 1)
		{
			DrawHollowRect(batcher, rect.X, rect.Y, rect.Width, rect.Height, color, thickness);
		}

		#endregion


		#region Pixel

		public static void DrawPixel(this Batcher batcher, float x, float y, Color color, int size = 1)
		{
			DrawPixel(batcher, new Vector2(x, y), color, size);
		}


		public static void DrawPixel(this Batcher batcher, Vector2 position, Color color, int size = 1)
		{
			var destRect = new Rectangle((int)position.X, (int)position.Y, size, size);
            if (size != 1)
			{
                destRect.X -= (int)(size * 0.5f);
                destRect.Y -= (int)(size * 0.5f);
			}

            batcher.Draw(Graphics.Instance.PixelTexture, destRect, Graphics.Instance.PixelTexture.SourceRect, color);
		}

		#endregion
	}
}