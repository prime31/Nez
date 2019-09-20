using Microsoft.Xna.Framework;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.UI
{
	/// <summary>
	/// Draws a {@link Sprite} repeatedly to fill the area, instead of stretching it
	/// </summary>
	public class TiledDrawable : SpriteDrawable
	{
		public TiledDrawable(Sprite sprite) : base(sprite)
		{ }


		public TiledDrawable(Texture2D texture) : base(new Sprite(texture))
		{ }


		public override void Draw(Batcher batcher, float x, float y, float width, float height, Color color)
		{
			float regionWidth = Sprite.SourceRect.Width, regionHeight = Sprite.SourceRect.Height;
			int fullX = (int) (width / regionWidth), fullY = (int) (height / regionHeight);
			float remainingX = width - regionWidth * fullX, remainingY = height - regionHeight * fullY;
			float startX = x, startY = y;

			// draw all full, unclipped first
			for (var i = 0; i < fullX; i++)
			{
				y = startY;
				for (var j = 0; j < fullY; j++)
				{
					batcher.Draw(Sprite, new Vector2(x, y), Sprite.SourceRect, color);
					y += regionHeight;
				}

				x += regionWidth;
			}

			var tempSourceRect = Sprite.SourceRect;
			if (remainingX > 0)
			{
				// right edge
				tempSourceRect.Width = (int) remainingX;
				y = startY;
				for (var ii = 0; ii < fullY; ii++)
				{
					batcher.Draw(Sprite, new Vector2(x, y), tempSourceRect, color);
					y += regionHeight;
				}

				// lower right corner.
				tempSourceRect.Height = (int) remainingY;
				if (remainingY > 0)
					batcher.Draw(Sprite, new Vector2(x, y), tempSourceRect, color);
			}

			tempSourceRect.Width = Sprite.SourceRect.Width;
			if (remainingY > 0)
			{
				// bottom edge
				tempSourceRect.Height = (int) remainingY;
				x = startX;
				for (var i = 0; i < fullX; i++)
				{
					batcher.Draw(Sprite, new Vector2(x, y), tempSourceRect, color);
					x += regionWidth;
				}
			}
		}
	}
}