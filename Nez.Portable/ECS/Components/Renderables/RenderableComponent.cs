using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// concrete implementation of IRenderable. Contains convenience methods.
	///
	/// VERY IMPORTANT! Subclasses MUST either override width/height or bounds!
	/// </summary>
	public abstract class RenderableComponent : Component, IRenderable, IComparable<RenderableComponent>
	{
		#region Properties and fields

		/// <summary>
		/// width of the RenderableComponent. subclasses that do not override the bounds property must implement this!
		/// </summary>
		/// <value>The width.</value>
		public virtual float Width => Bounds.Width;

		/// <summary>
		/// height of the RenderableComponent. subclasses that do not override the bounds property must implement this!
		/// </summary>
		/// <value>The height.</value>
		public virtual float Height => Bounds.Height;

		/// <summary>
		/// the AABB that wraps this object. Used for camera culling.
		/// </summary>
		/// <value>The bounds.</value>
		public virtual RectangleF Bounds
		{
			get
			{
				if (_areBoundsDirty)
				{
					_bounds.CalculateBounds(Entity.Transform.Position, _localOffset, Vector2.Zero,
						Entity.Transform.Scale, Entity.Transform.Rotation, Width, Height);
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// list on the scene.
		/// </summary>
		[Range(0, 1)]
		public float LayerDepth
		{
			get => _layerDepth;
			set => SetLayerDepth(value);
		}

		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
		/// </summary>
		/// <value>The render layer.</value>
		public int RenderLayer
		{
			get => _renderLayer;
			set => SetRenderLayer(value);
		}

		/// <summary>
		/// color passed along to the Batcher when rendering
		/// </summary>
		public Color Color = Color.White;

		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered
		/// </summary>
		public virtual Material Material { get; set; }

		/// <summary>
		/// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
		/// </summary>
		/// <value>The local position.</value>
		public Vector2 LocalOffset
		{
			get => _localOffset;
			set => SetLocalOffset(value);
		}

		/// <summary>
		/// the visibility of this Renderable. Changes in state end up calling the onBecameVisible/onBecameInvisible methods.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		public bool IsVisible
		{
			get => _isVisible;
			private set
			{
				if (_isVisible != value)
				{
					_isVisible = value;

					if (_isVisible)
						OnBecameVisible();
					else
						OnBecameInvisible();
				}
			}
		}

		public bool DebugRenderEnabled = true;

		protected Vector2 _localOffset;
		protected float _layerDepth;
		protected int _renderLayer;
		protected RectangleF _bounds;
		protected bool _isVisible;
		protected bool _areBoundsDirty = true;

		#endregion

		#region Component overrides and IRenderable

		public override void OnEntityTransformChanged(Transform.Component comp)
		{
			_areBoundsDirty = true;
		}

		/// <summary>
		/// called by a Renderer. The Camera can be used for culling and the Batcher instance to draw with.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		/// <param name="camera">Camera.</param>
		public abstract void Render(Batcher batcher, Camera camera);

		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		public override void DebugRender(Batcher batcher)
		{
			if (!DebugRenderEnabled)
				return;

			// if we have no collider draw our bounds
			if (Entity.GetComponent<Collider>() == null)
				batcher.DrawHollowRect(Bounds, Debug.Colors.RenderableBounds);

			// draw a square for our pivot/origin
			batcher.DrawPixel(Entity.Transform.Position + _localOffset, Debug.Colors.RenderableCenter, 4);
		}

		/// <summary>
		/// called when the Renderable enters the camera frame. Note that these methods will not be called if your Renderer does not use
		/// isVisibleFromCamera for its culling check. All default Renderers do.
		/// </summary>
		protected virtual void OnBecameVisible()
		{
		}

		/// <summary>
		/// called when the renderable exits the camera frame. Note that these methods will not be called if your Renderer does not use
		/// isVisibleFromCamera for its culling check. All default Renderers do.
		/// </summary>
		protected virtual void OnBecameInvisible()
		{
		}

		public override void OnRemovedFromEntity()
		{
		}

		/// <summary>
		/// returns true if the Renderables bounds intersects the Camera.bounds. Handles state switches for the isVisible flag. Use this method
		/// in your render method to see decide if you should render or not.
		/// </summary>
		/// <returns><c>true</c>, if visible from camera was ised, <c>false</c> otherwise.</returns>
		/// <param name="camera">Camera.</param>
		public virtual bool IsVisibleFromCamera(Camera camera)
		{
			IsVisible = camera.Bounds.Intersects(Bounds);
			return IsVisible;
		}

		#endregion

		#region Fluent setters

		public RenderableComponent SetMaterial(Material material)
		{
			Material = material;
			if (Entity != null && Entity.Scene != null)
				Entity.Scene.RenderableComponents.SetRenderLayerNeedsComponentSort(RenderLayer);
			return this;
		}

		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// </summary>
		/// <returns>The layer depth.</returns>
		/// <param name="layerDepth">Value.</param>
		public RenderableComponent SetLayerDepth(float layerDepth)
		{
			_layerDepth = Mathf.Clamp01(layerDepth);

			if (Entity != null && Entity.Scene != null)
				Entity.Scene.RenderableComponents.SetRenderLayerNeedsComponentSort(RenderLayer);
			return this;
		}

		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
		/// </summary>
		/// <returns>The render layer.</returns>
		/// <param name="renderLayer">Render layer.</param>
		public RenderableComponent SetRenderLayer(int renderLayer)
		{
			if (renderLayer != _renderLayer)
			{
				var oldRenderLayer = _renderLayer;
				_renderLayer = renderLayer;

				// if we have an entity then we are being managed by a ComponentList so we need to let it know that we changed renderLayers
				if (Entity != null && Entity.Scene != null)
					Entity.Scene.RenderableComponents.UpdateRenderableRenderLayer(this, oldRenderLayer, _renderLayer);
			}

			return this;
		}

		/// <summary>
		/// color passed along to the Batcher when rendering
		/// </summary>
		/// <returns>The color.</returns>
		/// <param name="color">Color.</param>
		public RenderableComponent SetColor(Color color)
		{
			Color = color;
			return this;
		}

		/// <summary>
		/// offset from the parent entity. Useful for adding multiple Renderables to an Entity that need specific positioning.
		/// </summary>
		/// <returns>The local offset.</returns>
		/// <param name="offset">Offset.</param>
		public RenderableComponent SetLocalOffset(Vector2 offset)
		{
			if (_localOffset != offset)
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
		public T GetMaterial<T>() where T : Material
		{
			return Material as T;
		}

		#endregion

		/// <Docs>To be added.</Docs>
		/// <para>Returns the sort order of the current instance compared to the specified object.</para>
		/// <summary>
		/// sorted first by renderLayer, then layerDepth and finally material
		/// </summary>
		/// <returns>The to.</returns>
		/// <param name="other">Other.</param>
		public int CompareTo(RenderableComponent other)
		{
			var res = other.RenderLayer.CompareTo(RenderLayer);
			if (res == 0)
			{
				res = other.LayerDepth.CompareTo(LayerDepth);
				if (res == 0)
				{
					// both null or equal
					if (ReferenceEquals(Material, other.Material))
						return 0;

					if (other.Material == null)
						return -1;

					return 1;
				}
			}

			return res;
		}

		public override string ToString()
		{
			return $"[RenderableComponent] {GetType()}, renderLayer: {RenderLayer}]";
		}
	}
}