using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// concrete implementation of IRenderable. Contains convenience 
	/// 
	/// Subclasses MUST either override width/height or bounds!
	/// </summary>
	public abstract class RenderableComponent : Component, IRenderable, IComparable<RenderableComponent>
	{
		#region properties and fields

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
		/// the AABB that wraps this object. Used for camera culling.
		/// </summary>
		/// <value>The bounds.</value>
		public virtual RectangleF bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds.calculateBounds( entity.transform.position, _localOffset, Vector2.Zero, entity.transform.scale, entity.transform.rotation, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
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
		/// color passed along to the Batcher when rendering
		/// </summary>
		public Color color = Color.White;

		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered
		/// </summary>
		public virtual Material material { get; set; }

		/// <summary>
		/// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
		/// </summary>
		/// <value>The local position.</value>
		public Vector2 localOffset
		{
			get { return _localOffset; }
			set { setLocalOffset( value ); }
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

		public bool debugRenderEnabled = true;

		protected Vector2 _localOffset;
		protected float _layerDepth;
		protected int _renderLayer;
		protected RectangleF _bounds;
		protected bool _isVisible;
		protected bool _areBoundsDirty = true;

		#endregion


		#region Component overrides and IRenderable

		public override void onEntityTransformChanged( Transform.Component comp )
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
			if (!debugRenderEnabled) return;
			// if we have no collider draw our bounds
			if( entity.getComponent<Collider>() == null )
				graphics.batcher.drawHollowRect( bounds, Debug.Colors.renderableBounds );

			// draw a square for our pivot/origin
			graphics.batcher.drawPixel( entity.transform.position + _localOffset, Debug.Colors.renderableCenter, 4 );
		}


		/// <summary>
		/// called when the Renderable enters the camera frame. Note that these methods will not be called if your Renderer does not use
		/// isVisibleFromCamera for its culling check. All default Renderers do.
		/// </summary>
		protected virtual void onBecameVisible()
		{ }


		/// <summary>
		/// called when the renderable exits the camera frame. Note that these methods will not be called if your Renderer does not use
		/// isVisibleFromCamera for its culling check. All default Renderers do.
		/// </summary>
		protected virtual void onBecameInvisible()
		{ }


		public override void onRemovedFromEntity()
		{ }


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
			if( entity != null && entity.scene != null )
				entity.scene.renderableComponents.setRenderLayerNeedsComponentSort( renderLayer );
			return this;
		}


		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// </summary>
		/// <returns>The layer depth.</returns>
		/// <param name="layerDepth">Value.</param>
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
				if( res == 0 )
				{
					// both null or equal
					if( ReferenceEquals( material, other.material ) )
						return 0;

					if( other.material == null )
						return -1;

					return 1;
				}
			}
			return res;
		}


		public override string ToString()
		{
			return string.Format( "[RenderableComponent] {0}, renderLayer: {1}]", this.GetType(), renderLayer );
		}

	}
}

