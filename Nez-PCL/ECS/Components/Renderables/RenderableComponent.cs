using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// by default, a RenderableComponent faces up/right. You can use the flipX/Y or face* method to adjust that to suit your needs.
	/// Subclasses MUST either override width/height or bounds!
	/// </summary>
	public abstract class RenderableComponent : Component, IComparable<RenderableComponent>
	{
		#region properties and fields

		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered. If non-null, it is automatically disposed of when the Component
		/// is removed from the Entity.
		/// </summary>
		public virtual Material material { get; set; }

		/// <summary>
		/// width of the RenderableComponent. subclasses that do not override the bounds property must implement this!
		/// </summary>
		/// <value>The width.</value>
		public virtual float width { get { return bounds.width; } }

		/// <summary>
		/// height of the RenderableComponent. subclasses that do not override the bounds property must implement this!
		/// </summary>
		/// <value>The height.</value>
		public virtual float height { get { return bounds.height; } }

		/// <summary>
		/// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
		/// </summary>
		/// <value>The local position.</value>
		public Vector2 localOffset
		{
			get { return _localOffset; }
			set { setLocalOffset( value ); }
		}

		public Vector2 origin
		{
			get { return _origin; }
			set { setOrigin( value ); }
		}

		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// list on the scene.
		/// </summary>
		public float layerDepth
		{
			get { return _layerDepth; }
			set { setLayerDepth( value ); }
		}

		/// <summary>
		/// color passed along to the Batcher when rendering
		/// </summary>
		public Color color = Color.White;

		/// <summary>
		/// Batchers passed along to the Batcher when rendering. flipX/flipY are helpers for setting this.
		/// </summary>
		public SpriteEffects spriteEffects = SpriteEffects.None;

		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
		/// </summary>
		/// <value>The render layer.</value>
		public int renderLayer
		{
			get { return _renderLayer; }
			set { setRenderLayer( value ); }
		}

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped horizontally
		/// </summary>
		/// <value><c>true</c> if flip x; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// determines if the sprite should be rendered normally or flipped vertically
		/// </summary>
		/// <value><c>true</c> if flip y; otherwise, <c>false</c>.</value>
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

		/// <summary>
		/// the AABB that wraps this object
		/// </summary>
		/// <value>The bounds.</value>
		public virtual RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( entity.transform.position, _localOffset, _origin, entity.transform.scale, entity.transform.rotation, width, height );
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
			set { setOrigin( new Vector2( value.X * width, value.Y * height ) ); }
		}

		/// <summary>
		/// the visibility of this Renderable. Changes in state end up calling the onBecameVisible/onBecameInvisible methods.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		public bool isVisible
		{
			get { return _isVisible; }
			private set
			{
				if( _isVisible != value )
				{
					_isVisible = value;

					if( _isVisible )
						onBecameVisible();
					else
						onBecameInvisible();
				}
			}
		}

		protected Vector2 _localOffset;
		protected Vector2 _origin;
		protected float _layerDepth;
		protected int _renderLayer;
		protected RectangleF _bounds;
		protected bool _isVisible;
		protected bool _areBoundsDirty = true;

		#endregion


		#region Component overrides and RenderableComponent

		public override void onEntityTransformChanged()
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
			if( entity.colliders.Count == 0 )
				graphics.batcher.drawHollowRect( bounds, Color.Yellow );

			// draw a square for our pivot/origin
			graphics.batcher.drawPixel( entity.transform.position + _localOffset, Color.DarkOrchid, 4 );
		}


		/// <summary>
		/// called when the Renderable enters the camera frame. Note that these methods will not be called if your render method does not use
		/// isVisibleFromCamera for its culling check.
		/// </summary>
		protected virtual void onBecameVisible()
		{}


		/// <summary>
		/// called when the renderable exits the camera frame. Note that these methods will not be called if your render method does not use
		/// isVisibleFromCamera for its culling check.
		/// </summary>
		protected virtual void onBecameInvisible()
		{}


		public override void onRemovedFromEntity()
		{}


		/// <summary>
		/// returns true if the Renderables bounds intersects the Camera.bounds. Handles state switches for the isVisible flag. Use this method
		/// in your render method to see decide if you should render or not.
		/// </summary>
		/// <returns><c>true</c>, if visible from camera was ised, <c>false</c> otherwise.</returns>
		/// <param name="camera">Camera.</param>
		public virtual bool isVisibleFromCamera( Camera camera )
		{
			isVisible = camera.bounds.intersects( bounds );
			return isVisible;
		}

		#endregion


		#region Fluent setters

		public RenderableComponent setMaterial( Material material )
		{
			this.material = material;
			return this;
		}


		/// <summary>
		/// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
		/// </summary>
		/// <returns>The local offset.</returns>
		/// <param name="offset">Offset.</param>
		public RenderableComponent setLocalOffset( Vector2 offset )
		{
			if( _localOffset != offset )
			{
				_localOffset = offset;
				_areBoundsDirty = true;
			}
			return this;
		}


		/// <summary>
		/// sets the origin for the Renderable
		/// </summary>
		/// <returns>The origin.</returns>
		/// <param name="origin">Origin.</param>
		public RenderableComponent setOrigin( Vector2 origin )
		{
			if( _origin != origin )
			{
				_origin = origin;
				_areBoundsDirty = true;
			}
			return this;
		}


		/// <summary>
		/// helper for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <returns>The origin normalized.</returns>
		/// <param name="origin">Origin.</param>
		public RenderableComponent setOriginNormalized( Vector2 value )
		{
			setOrigin( new Vector2( value.X * width, value.Y * height ) );
			return this;
		}


		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// </summary>
		/// <returns>The layer depth.</returns>
		/// <param name="value">Value.</param>
		public RenderableComponent setLayerDepth( float layerDepth )
		{
			_layerDepth = Mathf.clamp01( layerDepth );

			if( entity != null && entity.scene != null )
				entity.scene.renderableComponents.setRenderLayerNeedsComponentSort( renderLayer );
			return this;
		}


		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
		/// </summary>
		/// <returns>The render layer.</returns>
		/// <param name="renderLayer">Render layer.</param>
		public RenderableComponent setRenderLayer( int renderLayer )
		{
			if( renderLayer != _renderLayer )
			{
				var oldRenderLayer = _renderLayer;
				_renderLayer = renderLayer;

				// if we have an entity then we are being managed by a ComponentList so we need to let it know that we changed renderLayers
				if( entity != null && entity.scene != null )
					entity.scene.renderableComponents.updateRenderableRenderLayer( this, oldRenderLayer, _renderLayer );
			}
			return this;
		}


		/// <summary>
		/// color passed along to the Batcher when rendering
		/// </summary>
		/// <returns>The color.</returns>
		/// <param name="color">Color.</param>
		public RenderableComponent setColor( Color color )
		{
			this.color = color;
			return this;
		}

		#endregion


		#region public API

		/// <summary>
		/// helper for retrieving a Material subclass already casted
		/// </summary>
		/// <returns>The material.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getMaterial<T>() where T : Material
		{
			return material as T;
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
			var originalPosition = _localOffset;
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
						_localOffset = originalPosition + new Vector2( i * offset, j * offset );
						render( graphics, camera );
					}
				}
			}

			// restore changed state
			_localOffset = originalPosition;
			color = originalColor;
			_layerDepth = originalLayerDepth;
		}

		#endregion


		/// <Docs>To be added.</Docs>
		/// <para>Returns the sort order of the current instance compared to the specified object.</para>
		/// <summary>
		/// sorted first by renderLayer, then layerDepth and finally material
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo( RenderableComponent other )
		{
			var res = other.renderLayer.CompareTo( renderLayer );
			if( res == 0 )
			{
				res = other.layerDepth.CompareTo( layerDepth );
				if( res == 0 && other.material != null )
					return other.material.CompareTo( material );
			}

			return res;
		}


		public override string ToString()
		{
			return string.Format( "[RenderableComponent] {0}, renderLayer: {1}]", this.GetType(), renderLayer );
		}

	}
}

