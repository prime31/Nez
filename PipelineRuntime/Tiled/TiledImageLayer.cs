using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez.Tiled
{
	public class TiledImageLayer : TiledLayer
	{
		public Vector2 position;
		public readonly Texture2D texture;


		public TiledImageLayer( string name, Texture2D texture, Vector2 position ) : base( name )
		{
			this.position = position;
			this.texture = texture;
		}


		public override void draw( SpriteBatch spriteBatch )
		{
			spriteBatch.Draw( texture, position, Color.White );
		}


		public override void draw( SpriteBatch spriteBatch, Vector2 parentPosition, float layerDepth, Rectangle cameraClipBounds )
		{
			var bounds = new Rectangle( (int)( parentPosition.X + position.X ), (int)( parentPosition.Y + position.Y ), (int)texture.Width, (int)texture.Height );
			if( cameraClipBounds.Intersects( bounds ) )
				spriteBatch.Draw( texture, parentPosition + position, null, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, layerDepth );
		}

	}
}