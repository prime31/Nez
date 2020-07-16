using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// interface that when applied to a Component will register it to be rendered by the Scene Renderers. Implement this very carefully! Changing
	/// things like layerDepth/renderLayer/material need to update the Scene RenderableComponentList
	/// </summary>
	public interface IRenderable
	{
		/// <summary>
		/// the AABB that wraps this object. Used for camera culling.
		/// </summary>
		/// <value>The bounds.</value>
		RectangleF Bounds { get; }

		/// <summary>
		/// whether this IRenderable should be rendered or not
		/// </summary>
		bool Enabled { get; set; }

		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// list on the scene.
		/// </summary>
		float LayerDepth { get; set; }

		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
		/// </summary>
		int RenderLayer { get; set; }

		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered. If non-null, it is automatically disposed of when the Component
		/// is removed from the Entity.
		/// </summary>
		Material Material { get; set; }

		/// <summary>
		/// the visibility of this Renderable. Changes in state end up calling the onBecameVisible/onBecameInvisible methods.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		bool IsVisible { get; }


		/// <summary>
		/// helper for retrieving a Material subclass already casted
		/// </summary>
		T GetMaterial<T>() where T : Material;

		/// <summary>
		/// returns true if the Renderables bounds intersects the Camera.bounds. Handles state switches for the isVisible flag. Use this method
		/// in your render method to see decide if you should render or not.
		/// </summary>
		/// <returns><c>true</c>, if visible from camera was ised, <c>false</c> otherwise.</returns>
		bool IsVisibleFromCamera(Camera camera);

		/// <summary>
		/// called by a Renderer. The Camera can be used for culling and the Batcher instance to draw with.
		/// </summary>
		void Render(Batcher batcher, Camera camera);

		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		void DebugRender(Batcher batcher);
	}


	/// <summary>
	/// Comparer for sorting IRenderables. Sorts first by RenderLayer, then LayerDepth. If there is a tie Materials
	/// are used for the tie-breaker to avoid render state changes.
	/// </summary>
	public class RenderableComparer : IComparer<IRenderable>
	{
		public int Compare(IRenderable self, IRenderable other)
		{
			var res = other.RenderLayer.CompareTo(self.RenderLayer);
			if (res == 0)
			{
				res = other.LayerDepth.CompareTo(self.LayerDepth);
				if (res == 0)
				{
					// both null or equal
					if (ReferenceEquals(self.Material, other.Material))
						return 0;

					if (other.Material == null)
						return -1;

					return 1;
				}
			}

			return res;
		}
	}
}