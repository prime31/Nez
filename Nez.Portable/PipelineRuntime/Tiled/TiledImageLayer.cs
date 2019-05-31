using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledImageLayer : TiledLayer
	{
		public readonly Texture2D texture;

		RectangleF _bounds;


		public TiledImageLayer( string name, Texture2D texture ) : base( name )
		{
			this.texture = texture;
			_bounds.width = texture.Width;
			_bounds.height = texture.Height;
		}


		public void draw( Batcher batcher )
		{
			batcher.draw( texture, offset, Color.White );
		}


		public override void draw( Batcher batcher, Vector2 position, float layerDepth, RectangleF cameraClipBounds )
		{
			draw( batcher, position, Vector2.One, layerDepth, cameraClipBounds );
		}


		public override void draw( Batcher batcher, Vector2 position, Vector2 scale, float layerDepth, RectangleF cameraClipBounds )
		{
			batcher.draw( texture, position + offset, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, layerDepth );
		}

	}
}