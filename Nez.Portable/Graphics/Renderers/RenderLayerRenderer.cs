using System;


namespace Nez
{
	/// <summary>
	/// Renderer that only renders the specified renderLayers. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with other RenderLayerRenderers rendering different renderLayers.
	/// </summary>
	public class RenderLayerRenderer : Renderer
	{
		/// <summary>
		/// the renderLayers this Renderer will render
		/// </summary>
		public int[] RenderLayers;


		public RenderLayerRenderer(int renderOrder, params int[] renderLayers) : base(renderOrder, null)
		{
			Array.Sort(renderLayers);
			Array.Reverse(renderLayers);
			RenderLayers = renderLayers;
		}

		public override void Render(Scene scene)
		{
			var cam = Camera ?? scene.Camera;
			BeginRender(cam);

			for (var i = 0; i < RenderLayers.Length; i++)
			{
				var renderables = scene.RenderableComponents.ComponentsWithRenderLayer(RenderLayers[i]);
				for (var j = 0; j < renderables.Length; j++)
				{
					var renderable = renderables.Buffer[j];
					if (renderable.Enabled && renderable.IsVisibleFromCamera(cam))
						RenderAfterStateCheck(renderable, cam);
				}
			}

			if (ShouldDebugRender && Core.DebugRenderEnabled)
				DebugRender(scene, cam);

			EndRender();
		}

		protected override void DebugRender(Scene scene, Camera cam)
		{
			Graphics.Instance.Batcher.End();
			Graphics.Instance.Batcher.Begin(cam.TransformMatrix);

			for (var i = 0; i < RenderLayers.Length; i++)
			{
				var renderables = scene.RenderableComponents.ComponentsWithRenderLayer(RenderLayers[i]);
				for (var j = 0; j < renderables.Length; j++)
				{
					var renderable = renderables.Buffer[j];
					if (renderable.Enabled && renderable.IsVisibleFromCamera(cam))
						renderable.DebugRender(Graphics.Instance.Batcher);
				}
			}

			base.DebugRender(scene, cam);
		}
	}
}