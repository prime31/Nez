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
		public override bool wantsToRenderToSceneRenderTarget { get { return false; } }

		/// <summary>
		/// the renderLayers this Renderer will render
		/// </summary>
		public int[] renderLayers;

		/// <summary>
		/// ambient lighting color. Alpha is ignored
		/// </summary>
		/// <value>The color of the ambient.</value>
		public Color ambientColor { get { return _ambientColor; } }

		/// <summary>
		/// clear color for the diffuse portion of the gbuffer
		/// </summary>
		/// <value>The color of the clear.</value>
		public Color clearColor { get { return _clearColor; } }

		/// <summary>
		/// single pixel texture of a neutral normal map. This will effectively make the object have only diffuse lighting if applied as the normal map.
		/// </summary>
		/// <value>The null normal map texture.</value>
		public Texture2D nullNormalMapTexture
		{
			get
			{
				if( _nullNormalMapTexture == null )
					_nullNormalMapTexture = Graphics.createSingleColorTexture( 1, 1, new Color( 0.5f, 0.5f, 1f, 0f ) );
				return _nullNormalMapTexture;
			}
		}

		/// <summary>
		/// if true, all stages of the deferred pipeline are rendered after the final combine
		/// </summary>
		public bool enableDebugBufferRender = false;


		int _lightLayer;
		Color _ambientColor;
		Color _clearColor;
		Texture2D _nullNormalMapTexture;

		public RenderTexture diffuseRT;
		public RenderTexture normalRT;
		public RenderTexture lightRT;

		DeferredLightEffect _lightEffect;

		// light volumes. quad for directional/area and polygon for others
		QuadMesh _quadMesh;
		PolygonMesh _polygonMesh;
		PolygonMesh _quadPolygonMesh;


		public DeferredLightingRenderer( int renderOrder, int lightLayer, params int[] renderLayers ) : base( renderOrder )
		{
			// make sure we have a workable Material for our lighting system
			material = new DeferredSpriteMaterial( nullNormalMapTexture );

			_lightLayer = lightLayer;
			Array.Sort( renderLayers );
			this.renderLayers = renderLayers;

			_lightEffect = new DeferredLightEffect();

			// meshes used for light volumes
			_quadMesh = new QuadMesh( Core.graphicsDevice );
			_polygonMesh = PolygonMesh.createSymmetricalPolygon( 10 );
			_quadPolygonMesh = PolygonMesh.createRectangle();

			// set some sensible defaults
			setAmbientColor( new Color( 0.2f, 0.2f, 0.2f ) )
				.setClearColor( Color.CornflowerBlue );
		}


		/// <summary>
		/// we override render completely here so we can do our thing with multiple render targets
		/// </summary>
		/// <param name="cam">Cam.</param>
		public override void render( Scene scene )
		{
			clearRenderTargets();
			renderSprites( scene );
			renderLights( scene );
			renderFinalCombine( scene );

			if( enableDebugBufferRender )
				renderAllBuffers( scene );
		}


		protected override void debugRender( Scene scene, Camera cam )
		{
			for( var i = 0; i < renderLayers.Length; i++ )
			{
				var renderables = scene.renderableComponents.componentsWithRenderLayer( renderLayers[i] );
				for( var j = 0; j < renderables.Count; j++ )
				{
					var renderable = renderables[j];
					if( renderable.enabled && renderable.isVisibleFromCamera( cam ) )
						renderable.debugRender( Graphics.instance );
				}
			}

			var lightRenderables = scene.renderableComponents.componentsWithRenderLayer( _lightLayer );
			for( var j = 0; j < lightRenderables.Count; j++ )
			{
				var renderable = lightRenderables[j];
				if( renderable.enabled && renderable.isVisibleFromCamera( cam ) )
					renderable.debugRender( Graphics.instance );
			}
		}


		#region Configuration

		/// <summary>
		/// ambient lighting color. Alpha is ignored
		/// </summary>
		/// <returns>The ambient color.</returns>
		/// <param name="color">Color.</param>
		public DeferredLightingRenderer setAmbientColor( Color color )
		{
			if( _ambientColor != color )
			{
				_ambientColor = color;
				_lightEffect.setAmbientColor( color );
			}
			return this;
		}


		/// <summary>
		/// clear color for the diffuse portion of the gbuffer
		/// </summary>
		/// <returns>The clear color.</returns>
		/// <param name="color">Color.</param>
		public DeferredLightingRenderer setClearColor( Color color )
		{
			if( _clearColor != color )
			{
				_clearColor = color;
				_lightEffect.setClearColor( color );
			}
			return this;
		}

		#endregion


		#region Rendering

		void clearRenderTargets()
		{
			Core.graphicsDevice.SetRenderTargets( diffuseRT.renderTarget, normalRT.renderTarget );
			_lightEffect.prepareClearGBuffer();
			_quadMesh.render();
		}


		void renderSprites( Scene scene )
		{
			beginRender( scene.camera );

			for( var i = 0; i < renderLayers.Length; i++ )
			{
				var renderables = scene.renderableComponents.componentsWithRenderLayer( renderLayers[i] );
				for( var j = 0; j < renderables.Count; j++ )
				{
					var renderable = renderables[j];
					if( renderable.enabled && renderable.isVisibleFromCamera( scene.camera ) )
						renderAfterStateCheck( renderable, scene.camera );
				}
			}

			if( shouldDebugRender && Core.debugRenderEnabled )
				debugRender( scene, scene.camera );

			endRender();
		}


		void renderLights( Scene scene )
		{
			// bind the normalMap and update the Effect with our camera
			_lightEffect.setNormalMap( normalRT );
			_lightEffect.updateForCamera( scene.camera );

			Core.graphicsDevice.setRenderTarget( lightRT );
			Core.graphicsDevice.Clear( Color.Transparent );
			Core.graphicsDevice.BlendState = BlendState.Additive;
			Core.graphicsDevice.DepthStencilState = DepthStencilState.None;

			var renderables = scene.renderableComponents.componentsWithRenderLayer( _lightLayer );
			for( var i = 0; i < renderables.Count; i++ )
			{
				Assert.isTrue( renderables[i] is DeferredLight, "Found a Renderable in the lightLayer that is not a DeferredLight!" );
				var renderable = renderables[i];
				if( renderable.enabled )
				{
					var light = renderable as DeferredLight;
					if( light is DirLight || light.isVisibleFromCamera( scene.camera ) )
						renderLight( light );
				}
			}
		}


		void renderFinalCombine( Scene scene )
		{
			Core.graphicsDevice.setRenderTarget( scene.sceneRenderTarget );
			Core.graphicsDevice.BlendState = BlendState.Opaque;
			Core.graphicsDevice.DepthStencilState = DepthStencilState.None;

			// combine everything. ambient color is set in the shader when the property is set so no need to reset it
			_lightEffect.prepareForFinalCombine( diffuseRT, lightRT, normalRT );
			_quadMesh.render();
		}


		void renderAllBuffers( Scene scene )
		{
			var tempRT = RenderTarget.getTemporary( scene.sceneRenderTarget.Width, scene.sceneRenderTarget.Height );

			Core.graphicsDevice.setRenderTarget( tempRT );

			var halfWidth = tempRT.Width / 2;
			var halfHeight = tempRT.Height / 2;

			Graphics.instance.batcher.begin( BlendState.Opaque );
			Graphics.instance.batcher.draw( lightRT, new Rectangle( 0, 0, halfWidth, halfHeight ) );
			Graphics.instance.batcher.draw( diffuseRT, new Rectangle( halfWidth, 0, halfWidth, halfHeight ) );
			Graphics.instance.batcher.draw( normalRT, new Rectangle( 0, halfHeight, halfWidth, halfHeight ) );
			Graphics.instance.batcher.draw( scene.sceneRenderTarget, new Rectangle( halfWidth, halfHeight, halfWidth, halfHeight ) );
			Graphics.instance.batcher.end();

			Core.graphicsDevice.setRenderTarget( scene.sceneRenderTarget );
			Graphics.instance.batcher.begin( BlendState.Opaque );
			Graphics.instance.batcher.draw( tempRT, Vector2.Zero );
			Graphics.instance.batcher.end();

			RenderTarget.releaseTemporary( tempRT );
		}

		#endregion


		#region Light rendering

		void renderLight( DeferredLight light )
		{
			// check SpotLight first because it is a subclass of PointLight!
			if( light is SpotLight )
				renderLight( light as SpotLight );
			else if( light is PointLight )
				renderLight( light as PointLight );
			else if( light is AreaLight )
				renderLight( light as AreaLight );
			else if( light is DirLight )
				renderLight( light as DirLight );
		}


		void renderLight( DirLight light )
		{
			_lightEffect.updateForLight( light );
			_quadMesh.render();
		}


		void renderLight( PointLight light )
		{
			_lightEffect.updateForLight( light );
			_polygonMesh.render();
		}


		void renderLight( SpotLight light )
		{
			_lightEffect.updateForLight( light );
			_polygonMesh.render();
		}


		void renderLight( AreaLight light )
		{
			_lightEffect.updateForLight( light );
			_quadPolygonMesh.render();
		}

		#endregion


		public override void onSceneBackBufferSizeChanged( int newWidth, int newHeight )
		{
			// create our RenderTextures if we havent and resize them if we have
			if( diffuseRT == null )
			{
				diffuseRT = new RenderTexture( newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None );
				normalRT = new RenderTexture( newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None );
				lightRT = new RenderTexture( newWidth, newHeight, SurfaceFormat.Color, DepthFormat.None );
			}
			else
			{
				diffuseRT.onSceneBackBufferSizeChanged( newWidth, newHeight );
				normalRT.onSceneBackBufferSizeChanged( newWidth, newHeight );
				lightRT.onSceneBackBufferSizeChanged( newWidth, newHeight );
			}
		}


		public override void unload()
		{
			_lightEffect.Dispose();

			diffuseRT.Dispose();
			normalRT.Dispose();
			lightRT.Dispose();
		}

	}
}

