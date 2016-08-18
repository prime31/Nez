using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// skewable rectangle sprite for prototyping
	/// </summary>
	public class PrototypeSprite : RenderableComponent
	{
		public override float width { get { return _width; } }
		public override float height { get { return _height; } }

		public float skewTopX { get { return _skewTopX; } }
		public float skewBottomX { get { return _skewBottomX; } }
		public float skewLeftY { get { return _skewLeftY; } }
		public float skewRightY { get { return _skewRightY; } }

		float _width, _height;
		[Inspectable]
		float _skewTopX, _skewBottomX, _skewLeftY, _skewRightY;


		public PrototypeSprite( float width, float height )
		{
			_width = width;
			_height = height;
			originNormalized = new Vector2( 0.5f, 0.5f );
		}


		/// <summary>
		/// sets the width of the sprite
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="width">Width.</param>
		public PrototypeSprite setWidth( float width )
		{
			_width = width;
			return this;
		}


		/// <summary>
		/// sets the height of the sprite
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="height">Height.</param>
		public PrototypeSprite setHeight( float height )
		{
			_height = height;
			return this;
		}


		/// <summary>
		/// sets the skew values for the sprite
		/// </summary>
		/// <returns>The skew.</returns>
		/// <param name="skewTopX">Skew top x.</param>
		/// <param name="skewBottomX">Skew bottom x.</param>
		/// <param name="skewLeftY">Skew left y.</param>
		/// <param name="skewRightY">Skew right y.</param>
		public PrototypeSprite setSkew( float skewTopX, float skewBottomX, float skewLeftY, float skewRightY )
		{
			_skewTopX = skewTopX;
			_skewBottomX = skewBottomX;
			_skewLeftY = skewLeftY;
			_skewRightY = skewRightY;
			return this;
		}


		public override void render( Graphics graphics, Camera camera )
		{
			var destRect = new Rectangle( ( entity.transform.position - ( origin * entity.transform.localScale ) + localOffset ).ToPoint(), new Point( (int)( _width * entity.transform.localScale.X ), (int)( _height * entity.transform.localScale.Y ) ) );
			graphics.batcher.draw( graphics.pixelTexture, destRect, graphics.pixelTexture.sourceRect, color, entity.transform.rotation, SpriteEffects.None, layerDepth, _skewTopX, _skewBottomX, _skewLeftY, _skewRightY );
		}
	}
}

