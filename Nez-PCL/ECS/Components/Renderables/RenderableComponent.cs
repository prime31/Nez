using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public abstract class RenderableComponent : Component
	{
		// TODO: should width/height be multiplied by scale when they are returned?
		public abstract float width { get; }
		public abstract float height { get; }

		protected Vector2 _position;
		public Vector2 position
		{
			get { return _position; }
			set
			{
				if( _position != value )
				{
					_position = value;
					_areBoundsDirty = true;
				}
			}
		}

		protected Vector2 _origin;
		public Vector2 origin
		{
			get { return _origin; }
			set
			{
				if( _origin != value )
				{
					_origin = value;
					_areBoundsDirty = true;
				}
			}
		}

		protected float _rotation;
		public float rotation
		{
			get { return _rotation; }
			set
			{
				if( _rotation != value )
				{
					_rotation = value;
					_areBoundsDirty = true;
				}
			}
		}

		protected Vector2 _scale = Vector2.One;
		public Vector2 scale
		{
			get { return _scale; }
			set
			{
				if( _scale != value )
				{
					_scale = value;
					_areBoundsDirty = true;
				}
			}
		}

		/// <summary>
		/// shortcut for setting uniform scale
		/// </summary>
		public float zoom
		{
			set { scale = new Vector2( value, value ); }
		}


		/// <summary>
		/// standard SpriteBatch layerdepth
		/// </summary>
		public float layerDepth = 0f;
		public Color color = Color.White;
		public SpriteEffects spriteEffects = SpriteEffects.None;

		protected int _renderLayer;
		public int renderLayer
		{
			get { return _renderLayer; }
			set
			{
				if( value != _renderLayer )
				{
					var oldRenderLayer = _renderLayer;
					_renderLayer = value;

					// if we have an entity then we are being managed by a ComponentList so we need to let it know that we changed renderLayers
					if( entity != null )
						entity.scene.renderableComponents.updateRenderableRenderLayer( this, oldRenderLayer, _renderLayer );
				}
			}
		}

		public bool flipX
		{
			get
			{
				return ( spriteEffects & SpriteEffects.FlipHorizontally ) == SpriteEffects.FlipHorizontally;
			}
			set
			{
				spriteEffects = value ? ( spriteEffects | SpriteEffects.FlipHorizontally ) : ( spriteEffects & ~SpriteEffects.FlipHorizontally );
			}
		}

		public bool flipY
		{
			get
			{
				return ( spriteEffects & SpriteEffects.FlipVertically ) == SpriteEffects.FlipVertically;
			}
			set
			{
				spriteEffects = value ? ( spriteEffects | SpriteEffects.FlipVertically ) : ( spriteEffects & ~SpriteEffects.FlipVertically );
			}
		}

		public Vector2 renderPosition
		{
			get { return entity.position + _position; }
		}
			
		protected Rectangle _bounds;
		public Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					RectangleExtension.calculateBounds( ref _bounds, entity.position, _position, _origin, _scale, _rotation, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// helper property for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <value>The origin normalized.</value>
		public Vector2 originNormalized
		{
			get { return new Vector2( _origin.X / width, _origin.Y / height ); }
			set { origin = new Vector2( value.X * width, value.Y * height ); }
		}

		protected bool _areBoundsDirty = true;


		public RenderableComponent()
		{}


		public override void onEntityPositionChanged()
		{
			_areBoundsDirty = true;
		}


		public virtual void render( Graphics graphics, Camera camera )
		{}


		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public override void debugRender( Graphics graphics )
		{
			if( entity.collider == null )
				graphics.drawHollowRect( bounds, Color.Yellow );
			graphics.drawPixel( entity.position, Color.DarkOrchid, 4 );
		}


		public void faceLeft()
		{
			spriteEffects = spriteEffects & SpriteEffects.FlipHorizontally;
		}


		public void faceRight()
		{
			spriteEffects = spriteEffects & ~SpriteEffects.FlipHorizontally;
		}


		public void faceUp()
		{
			spriteEffects = spriteEffects & SpriteEffects.FlipVertically;
		}


		public void faceDown()
		{
			spriteEffects = spriteEffects & ~SpriteEffects.FlipVertically;
		}


		// TODO: these make no sense here. if they are called in a Components render method it will create an infinite loop
		void drawOutline( Graphics graphics, int offset = 1 )
		{
			drawOutline( graphics, Color.Black, offset );
		}


		void drawOutline( Graphics graphics, Color outlineColor, int offset = 1 )
		{
			// save the stuff we are going to modify so we can restore it later
			var originalPosition = _position;
			var originalColor = color;
			var originalLayerDepth = layerDepth;

			// set our new values
			color = outlineColor;
			layerDepth++;

			for( var i = -1; i < 2; i++ )
			{
				for( var j = -1; j < 2; j++ )
				{
					if( i != 0 || j != 0 )
					{
						_position = originalPosition + new Vector2( i * offset, j * offset );
						//render( graphics );
					}
				}
			}

			// restore changed state
			_position = originalPosition;
			color = originalColor;
			layerDepth = originalLayerDepth;
		}

	}
}

