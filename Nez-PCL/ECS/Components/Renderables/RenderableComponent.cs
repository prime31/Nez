using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	/// <summary>
	/// by default, a RenderableComponent faces up/right. You can use the flipX/Y or face* method to adjust that to suit your needs.
	/// </summary>
	public abstract class RenderableComponent : Component, IComparable<RenderableComponent>
	{
		#region properties and fields

		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered. If non-null, it is automatically disposed of when the Component
		/// is removed from the Entity.
		/// </summary>
		public RenderState renderState;

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
		/// offset from the parent entity
		/// </summary>
		/// <value>The local position.</value>
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
		protected Vector2 _localPosition;

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
		protected Vector2 _origin;

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

		/// <summary>
		/// color passed along to the SpriteBatch when rendering
		/// </summary>
		public Color color = Color.White;

		/// <summary>
		/// SpriteEffects passed along to the SpriteBatch when rendering. flipX/flipY are helpers for setting this.
		/// </summary>
		public SpriteEffects spriteEffects = SpriteEffects.None;

		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the SpriteBatch first. An important fact when using the stencil buffer.
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
		protected int _renderLayer;

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
					_bounds.calculateBounds( entity.transform.position, _localPosition, _origin, entity.transform.scale, entity.transform.rotation, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}
		protected RectangleF _bounds;

		/// <summary>
		/// helper property for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <value>The origin normalized.</value>
		public Vector2 originNormalized
		{
			get { return new Vector2( _origin.X / width, _origin.Y / height ); }
			set { origin = new Vector2( value.X * width, value.Y * height ); }
		}
			
		/// <summary>
		/// the visibility of this Renderable. Changes in state end up calling the onBecameVisible/onBecameInvisible methods.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		public bool isVisible
		{
			get { return _isVisible; }
			protected set
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
		protected bool _isVisible;

		protected bool _areBoundsDirty = true;

		#endregion


		public RenderableComponent()
		{}


		#region Component overrides and RenderableComponent

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
			if( entity.colliders.Count == 0 )
				graphics.spriteBatch.drawHollowRect( bounds, Color.Yellow );

			// draw a square for our pivot/origin
			graphics.spriteBatch.drawPixel( entity.transform.position + _localPosition, Color.DarkOrchid, 4 );
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
		{
			if( renderState != null )
			{
				renderState.unload();
				renderState = null;
			}
		}


		/// <summary>
		/// returns true if the Renderables bounds intersects the Camera.bounds. Handles state switches for the isVisible flag. Use this method
		/// in your render method to see decide if you should render or not.
		/// </summary>
		/// <returns><c>true</c>, if visible from camera was ised, <c>false</c> otherwise.</returns>
		/// <param name="camera">Camera.</param>
		protected bool isVisibleFromCamera( Camera camera )
		{
			if( camera.bounds.intersects( bounds ) )
			{
				isVisible = true;
				return true;
			}
			else
			{
				isVisible = false;
				return false;
			}
		}

		#endregion


		#region public API

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

		#endregion


		/// <Docs>To be added.</Docs>
		/// <para>Returns the sort order of the current instance compared to the specified object.</para>
		/// <summary>
		/// sorted first by renderLayer, then layerDepth and finally renderState
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo( RenderableComponent other )
		{
			var res = other.renderLayer.CompareTo( renderLayer );
			if( res == 0 )
			{
				var layerDepthRes = other.layerDepth.CompareTo( layerDepth );
				if( layerDepthRes == 0 && other.renderState != null )
					return other.renderState.CompareTo( renderState );
			}

			return res;
		}

	}
}

