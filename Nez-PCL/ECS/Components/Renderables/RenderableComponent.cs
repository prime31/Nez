using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// by default, a RenderableComponent faces up/right. You can use the flipX/Y or face* method to adjust that to suit your needs.
	/// </summary>
	public abstract class RenderableComponent : Component
	{
		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered.
		/// </summary>
		public RenderState renderState;
		
		// TODO: should width/height be multiplied by scale when they are returned?
		public abstract float width { get; }
		public abstract float height { get; }

		protected Vector2 _localPosition;
		public Vector2 localPosition
		{
			get { return _localPosition; }
			set
			{
				if( _localPosition != value )
				{
					_localPosition = value;
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
		/// standard SpriteBatch layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// list on the scene.
		/// </summary>
		public float layerDepth
		{
			get { return _layerDepth; }
			set
			{
				_layerDepth = value;

				if( entity != null && entity.scene != null )
					entity.scene.renderableComponents.setNeedsComponentSort();
			}
		}
		protected float _layerDepth;

		public Color color = Color.White;
		public SpriteEffects spriteEffects = SpriteEffects.None;

		protected int _renderLayer;
		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1
		/// </summary>
		/// <value>The render layer.</value>
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
					if( entity != null && entity.scene != null )
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
			get { return entity.position + _localPosition; }
		}
			
		protected Rectangle _bounds;
		public virtual Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					RectangleExt.calculateBounds( ref _bounds, entity.position, _localPosition, _origin, _scale, _rotation, width, height );
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


		/// <summary>
		/// called by a Renderer. The Camera can be used for culling and the Graphics instance to draw with.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="camera">Camera.</param>
		public abstract void render( Graphics graphics, Camera camera );


		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public override void debugRender( Graphics graphics )
		{
			// if we have no collider draw our bounds
			if( entity.collider == null )
				graphics.spriteBatch.drawHollowRect( bounds, Color.Yellow );

			// draw a square for our pivot/origin
			graphics.spriteBatch.drawPixel( renderPosition, Color.DarkOrchid, 4 );
		}


		public void faceLeft()
		{
			spriteEffects = spriteEffects | SpriteEffects.FlipHorizontally;
		}


		public void faceRight()
		{
			spriteEffects = spriteEffects & ~SpriteEffects.FlipHorizontally;
		}


		public void faceUp()
		{
			spriteEffects = spriteEffects & ~SpriteEffects.FlipVertically;
		}


		public void faceDown()
		{
			spriteEffects = spriteEffects | SpriteEffects.FlipVertically;
		}


		/// <summary>
		/// Draws the Renderable with an outline. Note that this should be called on disabled Renderables since they shouldnt take part in default
		/// rendering if they need an ouline.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="camera">Camera.</param>
		/// <param name="offset">Offset.</param>
		public void drawOutline( Graphics graphics, Camera camera, int offset = 1 )
		{
			drawOutline( graphics, camera, Color.Black, offset );
		}


		public void drawOutline( Graphics graphics, Camera camera, Color outlineColor, int offset = 1 )
		{
			// save the stuff we are going to modify so we can restore it later
			var originalPosition = _localPosition;
			var originalColor = color;
			var originalLayerDepth = _layerDepth;

			// set our new values
			color = outlineColor;
			_layerDepth += 0.01f;

			for( var i = -1; i < 2; i++ )
			{
				for( var j = -1; j < 2; j++ )
				{
					if( i != 0 || j != 0 )
					{
						_localPosition = originalPosition + new Vector2( i * offset, j * offset );
						render( graphics, camera );
					}
				}
			}

			// restore changed state
			_localPosition = originalPosition;
			color = originalColor;
			_layerDepth = originalLayerDepth;
		}

	}
}

