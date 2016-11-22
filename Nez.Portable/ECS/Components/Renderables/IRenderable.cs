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
		RectangleF bounds { get; }

		/// <summary>
		/// whether this IRenderable should be rendered or not
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		bool enabled { get; set; }

		/// <summary>
		/// standard Batcher layerdepth. 0 is in front and 1 is in back. Changing this value will trigger a sort of the renderableComponents
		/// list on the scene. 
		/// </summary>
		/// <value>The layer depth.</value>
		float layerDepth { get; set; }

		/// <summary>
		/// lower renderLayers are in the front and higher are in the back, just like layerDepth but not clamped to 0-1. Note that this means
		/// higher renderLayers are sent to the Batcher first. An important fact when using the stencil buffer.
		/// </summary>
		/// <value>The render layer.</value>
		int renderLayer { get; set; }

		/// <summary>
		/// used by Renderers to specify how this sprite should be rendered. If non-null, it is automatically disposed of when the Component
		/// is removed from the Entity.
		/// </summary>
		/// <value>The material.</value>
		Material material { get; set; }

		/// <summary>
		/// the visibility of this Renderable. Changes in state end up calling the onBecameVisible/onBecameInvisible methods.
		/// </summary>
		/// <value><c>true</c> if is visible; otherwise, <c>false</c>.</value>
		bool isVisible { get; }


		/// <summary>
		/// helper for retrieving a Material subclass already casted
		/// </summary>
		/// <returns>The material.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		T getMaterial<T>() where T : Material;

		/// <summary>
		/// returns true if the Renderables bounds intersects the Camera.bounds. Handles state switches for the isVisible flag. Use this method
		/// in your render method to see decide if you should render or not.
		/// </summary>
		/// <returns><c>true</c>, if visible from camera was ised, <c>false</c> otherwise.</returns>
		/// <param name="camera">Camera.</param>
		bool isVisibleFromCamera( Camera camera );

		/// <summary>
		/// called by a Renderer. The Camera can be used for culling and the Graphics instance to draw with.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		/// <param name="camera">Camera.</param>
		void render( Graphics graphics, Camera camera );

		/// <summary>
		/// renders the bounds only if there is no collider. Always renders a square on the origin.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		void debugRender( Graphics graphics );
	}


	/// <summary>
	/// Comparer for sorting IRenderables
	/// </summary>
	public class IRenderableComparer : IComparer<IRenderable>
	{
		public int Compare( IRenderable self, IRenderable other )
		{
			var res = other.renderLayer.CompareTo( self.renderLayer );
			if( res == 0 )
			{
				res = other.layerDepth.CompareTo( self.layerDepth );
				if( res == 0 )
				{
					// both null or equal
					if( ReferenceEquals( self.material, other.material ) )
						return 0;

					if( other.material == null )
						return -1;

					return 1;
				}
			}
			return res;
		}
	}

}
