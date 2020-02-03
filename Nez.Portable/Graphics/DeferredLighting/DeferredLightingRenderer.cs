using System;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;


namespace Nez.DeferredLighting
{
	/// <summary>
	/// handles deferred lighting. This Renderer should be ordered after any of your Renderers that render to a RenderTexture. Any renderLayers
	/// rendered by this Renderer should have Renderables with DeferredSpriteMaterials (or null Material to use the default, diffuse only Material).
	/// </summary>
	public class DeferredLightingRenderer : Renderer
	{
		/// <summary>
		/// we do not want to render into the Scene render texture
		/// </summary>
		/// <value>true</value>
		/// <c>false</c>
		public override bool WantsToRenderToSceneRenderTarget => false;

		/// <summary>
		/// the renderLayers this Renderer will render
		/// </summary>
		public int[] RenderLayers;

		/// <summary>
		/// ambient lighting color. Alpha is ignored
		/// </summary>
		/// <value>The color of the ambient.</value>
		public Color AmbientColor
		{
			get => _ambientColor;
			set => SetAmbientColor(value);
		}

		/// <summary>
		/// clear color for the diffuse portion of the gbuffer
		/// </summary>
		/// <value>The color of the clear.</value>
		public Color ClearColor
		{
			get => _clearColor;
			set => SetClearColor(value);
		}

		/// <summary>
		/// single pixel texture of a neutral normal map. This will effectively make the object have only diffuse lighting if applied as the normal map.
		/// </summary>
		/// <value>The null normal map texture.</value>
		public Texture2D NullNormalMapTexture
		{
			get
			{
				if (_nullNormalMapTexture == null)
					_nullNormalMapTexture = Graphics.CreateSingleColorTexture(1, 1, new Color(0.5f, 0.5f, 1f, 0f));
				return _nullNormalMapTexture;
			}
		}

		/// <summary>
		/// if true, all stages of the deferred pipeline are rendered after the final combine
		/// </summary>
		public bool EnableDebugBufferRender;


		int _lightLayer;
		Color _ambientColor;
		Color _clearColor;
		Texture2D _nullNormalMapTexture;

		public RenderTexture DiffuseRT;
		public RenderTexture NormalRT;
		public RenderTexture LightRT;

		DeferredLightEffect _lightEffect;

		// light volumes. quad for directional/area and polygon for others
		QuadMesh _quadMesh;
		PolygonMesh _polygonMesh;
		PolygonMesh _quadPolygonMesh;


		public DeferredLightingRenderer(int renderOrder, int lightLayer, params int[] renderLayers) : base(renderOrder)
		{
			// make sure we have a workable Material for our lighting system
			Material = new DeferredSpriteMaterial(NullNormalMapTexture);

			_lightLayer = lightLayer;
			Array.Sort(renderLayers);
			Array.Reverse(renderLayers);
			RenderLayers = renderLayers;

			_lightEffect = new DeferredLightEffect();

			// meshes used for light volumes
			_quadMesh = new QuadMesh(Core.GraphicsDevice);
			_polygonMesh = PolygonMesh.CreateSymmetricalPolygon(10);
			_quadPolygonMesh = PolygonMesh.CreateRectangle();

			// set some sensible defaults
			SetAmbientColor(new Color(0.2f, 0.2f, 0.2f))
				.SetClearColor(Color.CornflowerBlue);
		}

		/// <summary>
		/// we override render completely here so we can do our thing with multiple render targets
		/// </summary>
		/// <param name="scene">scene.</param>
		public override void Render(Scene scene)
		{
			ClearRenderTargets();
			RenderSprites(scene);
			RenderLights(scene);
			RenderFinalCombine(scene);

			if (EnableDebugBufferRender)
				RenderAllBuffers(scene);
		}

		protected override void DebugRender(Scene scene, Camera cam)
		{
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

			var lightRenderables = scene.RenderableComponents.ComponentsWithRenderLayer(_lightLayer);
			for (var j = 0; j < lightRenderables.Length; j++)
			{
				var renderable = lightRenderables.Buffer[j];
				if (renderable.Enabled && renderable.IsVisibleFromCamera(cam))
					renderable.DebugRender(Graphics.Instance.Batcher);
			}
		}


		#region Configuration

		/// <summary>
		/// ambient lighting color. Alpha is ignored
		/// </summary>
		/// <returns>The ambient color.</returns>
		/// <param name="color">Color.</param>
		public DeferredLightingRenderer SetAmbientColor(Color color)
		{
			if (_ambientColor != color)
			{
				_ambientColor = color;
				_lightEffect.SetAmbientColor(color);
			}

			return this;
		}

		/// <summary>
		/// clear color for the diffuse portion of the gbuffer
		/// </summary>
		/// <returns>The clear color.</returns>
		/// <param name="color">Color.</param>
		public DeferredLightingRenderer SetClearColor(Color color)
		{
			if (_clearColor != color)
			{
				_clearColor = color;
				_lightEffect.SetClearColor(color);
			}

			return this;
		}

		#endregion


		#region Rendering

		void ClearRenderTargets()
		{
			Core.GraphicsDevice.SetRenderTargets(DiffuseRT.RenderTarget, NormalRT.RenderTarget);
			_lightEffect.PrepareClearGBuffer();
			_quadMesh.Render();
		}

		void RenderSprites(Scene scene)
		{
			BeginRender(scene.Camera);

			for (var i = 0; i < RenderLayers.Length; i++)
			{
				var renderables = scene.RenderableComponents.ComponentsWithRenderLayer(RenderLayers[i]);
				for (var j = 0; j < renderables.Length; j++)
				{
					var renderable = renderables.Buffer[j];
					if (renderable.Enabled && renderable.IsVisibleFromCamera(scene.Camera))
						RenderAfterStateCheck(renderable, scene.Camera);
				}
			}

			if (ShouldDebugRender && Core.DebugRenderEnabled)
				DebugRender(scene, scene.Camera);

			EndRender();
		}

		void RenderLights(Scene scene)
		{
			// bind the normalMap and update the Effect with our camera
			_lightEffect.SetNormalMap(NormalRT);
			_lightEffect.UpdateForCamera(scene.Camera);

			Core.GraphicsDevice.SetRenderTarget(LightRT);
			Core.GraphicsDevice.Clear(Color.Transparent);
			Core.GraphicsDevice.BlendState = BlendState.Additive;
			Core.GraphicsDevice.DepthStencilState = DepthStencilState.None;
			Core.GraphicsDevice.RasterizerState = RasterizerState.CullNone;

			var renderables = scene.RenderableComponents.ComponentsWithRenderLayer(_lightLayer);
			for (var i = 0; i < renderables.Length; i++)
			{
				Insist.IsTrue(renderables.Buffer[i] is DeferredLight, "Found a Renderable in the lightLayer that is not a DeferredLight!");
				var renderable = renderables.Buffer[i];
				if (renderable.Enabled)
				{
					var light = renderable as DeferredLight;
					if (light is DirLight || light.IsVisibleFromCamera(scene.Camera))
						RenderLight(light);
				}
			}
		}

		void RenderFinalCombine(Scene scene)
		{
			Core.GraphicsDevice.SetRenderTarget(scene.SceneRenderTarget);
			Core.GraphicsDevice.BlendState = BlendState.Opaque;
			Core.GraphicsDevice.DepthStencilState = DepthStencilState.None;

			// combine everything. ambient color is set in the shader when the property is set so no need to reset it
			_lightEffect.PrepareForFinalCombine(DiffuseRT, LightRT, NormalRT);
			_quadMesh.Render();
		}

		void RenderAllBuffers(Scene scene)
		{
			var tempRT = RenderTarget.GetTemporary(scene.SceneRenderTarget.Width, scene.SceneRenderTarget.Height);

			Core.GraphicsDevice.SetRenderTarget(tempRT);

			var halfWidth = tempRT.Width / 2;
			var halfHeight = tempRT.Height / 2;

			Graphics.Instance.Batcher.Begin(BlendState.Opaque);
			Graphics.Instance.Batcher.Draw(LightRT, new Rectangle(0, 0, halfWidth, halfHeight));
			Graphics.Instance.Batcher.Draw(DiffuseRT, new Rectangle(halfWidth, 0, halfWidth, halfHeight));
			Graphics.Instance.Batcher.Draw(NormalRT, new Rectangle(0, halfHeight, halfWidth, halfHeight));
			Graphics.Instance.Batcher.Draw(scene.SceneRenderTarget, new Rectangle(halfWidth, halfHeight, halfWidth, halfHeight));
			Graphics.Instance.Batcher.End();

			Core.GraphicsDevice.SetRenderTarget(scene.SceneRenderTarget);
			Graphics.Instance.Batcher.Begin(BlendState.Opaque);
			Graphics.Instance.Batcher.Draw(tempRT, Vector2.Zero);
			Graphics.Instance.Batcher.End();

			RenderTarget.ReleaseTemporary(tempRT);
		}

		#endregion


		#region Light rendering

		void RenderLight(DeferredLight light)
		{
			// check SpotLight first because it is a subclass of PointLight!
			if (light is SpotLight)
				RenderLight(light as SpotLight);
			else if (light is PointLight)
				RenderLight(light as PointLight);
			else if (light is AreaLight)
				RenderLight(light as AreaLight);
			else if (light is DirLight)
				RenderLight(light as DirLight);
		}

		void RenderLight(DirLight light)
		{
			_lightEffect.UpdateForLight(light);
			_quadMesh.Render();
		}

		void RenderLight(PointLight light)
		{
			_lightEffect.UpdateForLight(light);
			_polygonMesh.Render();
		}

		void RenderLight(SpotLight light)
		{
			_lightEffect.UpdateForLight(light);
			_polygonMesh.Render();
		}

		void RenderLight(AreaLight light)
		{
			_lightEffect.UpdateForLight(light);
			_quadPolygonMesh.Render();
		}

		#endregion


		public override void OnSceneBackBufferSizeChanged(int newWidth, int newHeight)
		{
			// create our RenderTextures if we havent and resize them if we have
			if (DiffuseRT == null)
			{
				DiffuseRT = new RenderTexture(newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None);
				NormalRT = new RenderTexture(newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None);
				LightRT = new RenderTexture(newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None);
			}
			else
			{
				DiffuseRT.OnSceneBackBufferSizeChanged(newWidth, newHeight);
				NormalRT.OnSceneBackBufferSizeChanged(newWidth, newHeight);
				LightRT.OnSceneBackBufferSizeChanged(newWidth, newHeight);
			}
		}

		public override void Unload()
		{
			_lightEffect.Dispose();

			DiffuseRT.Dispose();
			NormalRT.Dispose();
			LightRT.Dispose();

			if (_nullNormalMapTexture != null)
				_nullNormalMapTexture.Dispose();

			base.Unload();
		}
	}
}