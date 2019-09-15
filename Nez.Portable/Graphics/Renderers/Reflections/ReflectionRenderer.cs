using Nez.Textures;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// assists in creating a mirror effect. To use the ReflectionRenderer do the following:
	/// - call createAndSetupScene being sure to pass in a renderOrder BEFORE the renderer that contains your reflective surface.
	/// - reflectableObjectRenderLayers should contain all the renderLayers that contain objects that you want reflected
	/// - create a ReflectionMaterial which you will use to render your reflective surfaces. Note that your reflective
	///     surfaces should NOT be rendered by the ReflectionRenderer! It needs to create a RenderTexture with just the objects to reflect.
	/// - you can optionally set a normal map on the ReflectionEffect for a refraction effect
	/// - move the ReflectionRenderer.camera around to get the desired offset for your reflections. You can also change the zoom of the Camera.
	/// </summary>
	public class ReflectionRenderer : RenderLayerRenderer
	{
		ReflectionRenderer(int renderOrder, params int[] reflectableObjectRenderLayers) : base(renderOrder,
			reflectableObjectRenderLayers)
		{
		}

		public static ReflectionRenderer CreateAndSetupScene(Scene scene, int renderOrder, params int[] renderLayers)
		{
			var reflectionRenderer = scene.AddRenderer(new ReflectionRenderer(-1, renderLayers));
			reflectionRenderer.RenderTargetClearColor = Color.Transparent;
			reflectionRenderer.RenderTexture = new RenderTexture(1, 1);

			// create a Camera and parent it to the Scene's Camera
			var cameraEntity = scene.CreateEntity("reflection-camera");
			cameraEntity.Transform.SetParent(scene.Camera.Entity.Transform);
			reflectionRenderer.Camera = cameraEntity.AddComponent<Camera>();

			return reflectionRenderer;
		}

		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			base.OnSceneBackBufferSizeChanged(newWidth, newHeight);

			// keep our Camera in sync with the normal Scene Camera. This will ensure the origin is updated with screen size changes.
			Camera.Origin = new Vector2(newWidth / 2f, newHeight / 2f);
		}
	}
}