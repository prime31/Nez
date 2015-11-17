using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nez.Physics;


namespace Nez
{
	public abstract class RenderableComponent : Component
	{
		public abstract float width { get; }
		public abstract float height { get; }

		public Vector2 position;
		public Vector2 origin;
		public Vector2 scale = Vector2.One;
		public float zoom = 1.0f;
		public float rotation;
		/// <summary>
		/// standard SpriteBatch layerdepth
		/// </summary>
		public float layerDepth = 0f;
		public Color color = Color.White;
		public SpriteEffects spriteEffects = SpriteEffects.None;

		int _renderLayer;
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
			get { return entity.position + position; }
		}
			
		public Rectangle bounds
		{
			get
			{
				// TODO: take scale into account as well
				return RectangleExtension.fromFloats( entity.position.X + position.X - origin.X * zoom, entity.position.Y + position.Y - origin.Y * zoom, width * zoom, height * zoom );
			}
		}

		/// <summary>
		/// helper method for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <value>The origin normalized.</value>
		public Vector2 originNormalized
		{
			get { return new Vector2( origin.X / width, origin.Y / height ); }
			set { origin = new Vector2( value.X * width, value.Y * height ); }
		}


		public RenderableComponent()
		{}


		public virtual void render( Graphics graphics, Camera camera )
		{}


		public override void debugRender( Graphics graphics )
		{
			graphics.drawHollowRect( bounds, Color.Yellow );
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
			var originalPosition = position;
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
						position = originalPosition + new Vector2( i * offset, j * offset );
						//render( graphics );
					}
				}
			}

			// restore changed state
			position = originalPosition;
			color = originalColor;
			layerDepth = originalLayerDepth;
		}

	}
}

