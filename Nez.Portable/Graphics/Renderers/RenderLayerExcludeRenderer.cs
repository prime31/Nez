namespace Nez
{
	/// <summary>
	/// Renderer that only renders all but one renderLayer. Useful to keep UI rendering separate from the rest of the game when used in conjunction
	/// with a RenderLayerRenderer. Note that UI would most likely want to be rendered in screen space so the camera matrix shouldn't be passed to
	/// Batcher.Begin.
	/// </summary>
	public class RenderLayerExcludeRenderer : Renderer
	{
		public int[] ExcludedRenderLayers;


		public RenderLayerExcludeRenderer(int renderOrder, params int[] excludedRenderLayers) : base(renderOrder, null)
		{
			ExcludedRenderLayers = excludedRenderLayers;
		}

		public override void Render(Scene scene)
		{
			var cam = Camera ?? scene.Camera;
			BeginRender(cam);

			for (var i = 0; i < scene.RenderableComponents.Count; i++)
			{
				var renderable = scene.RenderableComponents[i];
				if (!ExcludedRenderLayers.Contains(renderable.RenderLayer) && renderable.Enabled &&
				    renderable.IsVisibleFromCamera(cam))
					RenderAfterStateCheck(renderable, cam);
			}

			if (ShouldDebugRender && Core.DebugRenderEnabled)
				DebugRender(scene, cam);

			EndRender();
		}

		protected override void DebugRender(Scene scene, Camera cam)
		{
			Graphics.Instance.Batcher.End();
			Graphics.Instance.Batcher.Begin(cam.TransformMatrix);

			for (var i = 0; i < scene.RenderableComponents.Count; i++)
			{
				var renderable = scene.RenderableComponents[i];
				if (!ExcludedRenderLayers.Contains(renderable.RenderLayer) && renderable.Enabled &&
				    renderable.IsVisibleFromCamera(cam))
					renderable.DebugRender(Graphics.Instance.Batcher);
			}

			base.DebugRender(scene, cam);
		}
	}
}