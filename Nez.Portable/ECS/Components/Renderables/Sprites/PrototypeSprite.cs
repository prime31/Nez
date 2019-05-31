using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Sprites;


namespace Nez
{
	/// <summary>
	/// skewable rectangle sprite for prototyping
	/// </summary>
	public class PrototypeSprite : Sprite
	{
		public override float width => _width;
		public override float height => _height;

		public override RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( entity.transform.position, _localOffset, _origin, entity.transform.scale, entity.transform.rotation, _width, _height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		public float skewTopX;
		public float skewBottomX;
		public float skewLeftY;
		public float skewRightY;

		float _width, _height;


		public PrototypeSprite() : this( 50, 50 )
		{}

		public PrototypeSprite( float width, float height ) : base( Graphics.instance.pixelTexture )
		{
			_width = width;
			_height = height;
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
			this.skewTopX = skewTopX;
			this.skewBottomX = skewBottomX;
			this.skewLeftY = skewLeftY;
			this.skewRightY = skewRightY;
			return this;
		}
       
	    public override void onAddedToEntity()
        {
            originNormalized = Vector2Ext.halfVector();
        }

        public override void render( Graphics graphics, Camera camera )
		{
			var pos = ( entity.transform.position - ( origin * entity.transform.scale ) + localOffset );
			var size = new Point( (int)( _width * entity.transform.scale.X ), (int)( _height * entity.transform.scale.Y ) );
			var destRect = new Rectangle( (int)pos.X, (int)pos.Y, size.X, size.Y );
			graphics.batcher.draw( subtexture, destRect, subtexture.sourceRect, color, entity.transform.rotation, SpriteEffects.None, layerDepth, skewTopX, skewBottomX, skewLeftY, skewRightY );
		}

	}
}

