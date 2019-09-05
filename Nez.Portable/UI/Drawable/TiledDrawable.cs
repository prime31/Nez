using System;
using Microsoft.Xna.Framework;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	/// <summary>
	/// Draws a {@link Subtexture} repeatedly to fill the area, instead of stretching it
	/// </summary>
	public class TiledDrawable : SubtextureDrawable
	{
		public TiledDrawable(Subtexture subtexture) : base(subtexture)
		{
		}


		public TiledDrawable(Texture2D texture) : base(new Subtexture(texture))
		{
		}


		public override void Draw(Graphics graphics, float x, float y, float width, float height, Color color)
		{
			float regionWidth = _subtexture.SourceRect.Width, regionHeight = _subtexture.SourceRect.Height;
			int fullX = (int) (width / regionWidth), fullY = (int) (height / regionHeight);
			float remainingX = width - regionWidth * fullX, remainingY = height - regionHeight * fullY;
			float startX = x, startY = y;

			// draw all full, unclipped first
			for (var i = 0; i < fullX; i++)
			{
				y = startY;
				for (var j = 0; j < fullY; j++)
				{
					graphics.Batcher.Draw(_subtexture, new Vector2(x, y), _subtexture.SourceRect, color);
					y += regionHeight;
				}

				x += regionWidth;
			}

			var tempSourceRect = _subtexture.SourceRect;
			if (remainingX > 0)
			{
				// right edge
				tempSourceRect.Width = (int) remainingX;
				y = startY;
				for (var ii = 0; ii < fullY; ii++)
				{
					graphics.Batcher.Draw(_subtexture, new Vector2(x, y), tempSourceRect, color);
					y += regionHeight;
				}

				// lower right corner.
				tempSourceRect.Height = (int) remainingY;
				if (remainingY > 0)
					graphics.Batcher.Draw(_subtexture, new Vector2(x, y), tempSourceRect, color);
			}

			tempSourceRect.Width = _subtexture.SourceRect.Width;
			if (remainingY > 0)
			{
				// bottom edge
				tempSourceRect.Height = (int) remainingY;
				x = startX;
				for (var i = 0; i < fullX; i++)
				{
					graphics.Batcher.Draw(_subtexture, new Vector2(x, y), tempSourceRect, color);
					x += regionWidth;
				}
			}
		}
	}
}