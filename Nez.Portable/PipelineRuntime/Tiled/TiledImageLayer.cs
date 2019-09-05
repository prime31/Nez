using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledImageLayer : TiledLayer
	{
		public readonly Texture2D Texture;

		RectangleF _bounds;


		public TiledImageLayer(string name, Texture2D texture) : base(name)
		{
			this.Texture = texture;
			_bounds.Width = texture.Width;
			_bounds.Height = texture.Height;
		}


		public void Draw(Batcher batcher)
		{
			batcher.Draw(Texture, Offset, Color.White);
		}


		public override void Draw(Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds)
		{
			Draw(batcher, position, Vector2.One, layerDepth, cameraClipBounds);
		}


		public override void Draw(Batcher batcher, Vector2 position, Vector2 scale, float layerDepth,
		                          RectangleF cameraClipBounds)
		{
			batcher.Draw(Texture, position + Offset, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None,
				layerDepth);
		}
	}
}