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


		public override void draw( SpriteBatch spriteBatch )
		{
			spriteBatch.Draw( texture, offset, Color.White );
		}


		public override void draw( SpriteBatch spriteBatch, Vector2 parentPosition, float layerDepth, RectangleF cameraClipBounds )
		{
			if( cameraClipBounds.intersects( _bounds ) )
				spriteBatch.Draw( texture, parentPosition + offset, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, layerDepth );
		}

	}
}