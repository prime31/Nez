using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Nez.Systems;
using Nez.Textures;
using Microsoft.Xna.Framework.Graphics;


namespace Nez
{
	public class Scene
	{
		public enum SceneResolutionPolicy
		{
			/// <summary>
			/// Default. RenderTexture matches the sceen size
			/// </summary>
			None,
			/// <summary>
			/// The entire application is visible in the specified area without trying to preserve the original aspect ratio. 
			/// Distortion can occur, and the application may appear stretched or compressed.
			/// </summary>
			ExactFit,
			/// <summary>
			/// The entire application fills the specified area, without distortion but possibly with some cropping, 
			/// while maintaining the original aspect ratio of the application.
			/// </summary>
			NoBorder,
			/// <summary>
			/// Pixel perfect version of NoBorder. Scaling is limited to integer values.
			/// </summary>
			NoBorderPixelPerfect,
			/// <summary>
			/// The entire application is visible in the specified area without distortion while maintaining the original 
			/// aspect ratio of the application. Borders can appear on two sides of the application.
			/// </summary>
			ShowAll,
			/// <summary>
			/// Pixel perfect version of ShowAll. Scaling is limited to integer values.
			/// </summary>
			ShowAllPixelPerfect,
			/// <summary>
			/// The application takes the height of the design resolution size and modifies the width of the internal
			/// canvas so that it fits the aspect ratio of the device.
			/// no distortion will occur however you must make sure your application works on different
			/// aspect ratios
			/// </summary>
			FixedHeight,
			/// <summary>
			/// Pixel perfect version of FixedHeight. Scaling is limited to integer values.
			/// </summary>
			FixedHeightPixelPerfect,
			/// <summary>
			/// The application takes the width of the design resolution size and modifies the height of the internal
			/// canvas so that it fits the aspect ratio of the device.
			/// no distortion will occur however you must make sure your application works on different
			/// aspect ratios
			/// </summary>
			FixedWidth,
			/// <summary>
			/// Pixel perfect version of FixedWidth. Scaling is limited to integer values.
			/// </summary>
			FixedWidthPixelPerfect
		}


		/// <summary>
		/// default scene Camera
		/// </summary>
		public Camera camera;

		/// <summary>
		/// clear color that is used in preRender to clear the screen
		/// </summary>
		public Color clearColor = Color.CornflowerBlue;

		/// <summary>
		/// clear color for the final render of the RenderTexture to the framebuffer
		/// </summary>
		public Color letterboxColor = Color.Black;

		/// <summary>
		/// SamplerState used for the final draw of the RenderTexture to the framebuffer
		/// </summary>
		public SamplerState samplerState = SamplerState.PointClamp;

		/// <summary>
		/// Scene-specific ContentManager. Use it to load up any resources that are needed only by this scene. If you have global/multi-scene
		/// resources you can use Core.contentManager to load them since Nez will not ever unload them.
		/// </summary>
		public readonly NezContentManager contentManager;

		/// <summary>
		/// global toggle for PostProcessors
		/// </summary>
		public bool enablePostProcessing;

		/// <summary>
		/// The list of entities within this Scene
		/// </summary>
		public EntityList entities;

		/// <summary>
		/// Manages a list of all the RenderableComponents that are currently on scene Entitys
		/// </summary>
		public readonly RenderableComponentList renderableComponents;

		/// <summary>
		/// gets the size of the sceneRenderTexture
		/// </summary>
		/// <value>The size of the scene render texture.</value>
		public Vector2 sceneRenderTextureSize
		{
			get { return new Vector2( _sceneRenderTexture.renderTarget2D.Width, _sceneRenderTexture.renderTarget2D.Height ); }
		}

		/// <summary>
		/// default resolution size used for all scenes
		/// </summary>
		static Point defaultDesignResolutionSize;

		/// <summary>
		/// default resolution policy used for all scenes
		/// </summary>
		static SceneResolutionPolicy defaultSceneResolutionPolicy = SceneResolutionPolicy.None;

		/// <summary>
		/// resolution policy used by the scene
		/// </summary>
		SceneResolutionPolicy _resolutionPolicy;

		/// <summary>
		/// design resolution size used by the scene
		/// </summary>
		Point _designResolutionSize;

		/// <summary>
		/// this gets setup based on the resolution policy and is used for the final blit of the RenderTexture
		/// </summary>
		Rectangle _finalRenderDestinationRect;

		RenderTexture _sceneRenderTexture;
		RenderTexture _destinationRenderTexture;

		List<Renderer> _renderers = new List<Renderer>();
		Dictionary<int,double> _actualEntityOrderLookup = new Dictionary<int,double>();
		readonly List<PostProcessor> _postProcessors = new List<PostProcessor>();


		/// <summary>
		/// sets the default design size and resolution policy that new scenes will use
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="sceneResolutionPolicy">Scene resolution policy.</param>
		public static void setDefaultDesignResolution( int width, int height, SceneResolutionPolicy sceneResolutionPolicy )
		{
			defaultDesignResolutionSize = new Point( width, height );
			defaultSceneResolutionPolicy = sceneResolutionPolicy;
		}


		/// <summary>
		/// helper that creates a scene with the DefaultRenderer attached and ready for use
		/// </summary>
		/// <returns>The with default renderer.</returns>
		public static Scene createWithDefaultRenderer( Color? clearColor = null )
		{
			var scene = new Scene();

			if( clearColor.HasValue )
				scene.clearColor = clearColor.Value;
			scene.addRenderer( new DefaultRenderer( scene.camera ) );
			return scene;
		}


		public Scene()
		{
			camera = new Camera();
			entities = new EntityList( this );
			renderableComponents = new RenderableComponentList();
			contentManager = new NezContentManager();
			_sceneRenderTexture = new RenderTexture();

			// setup our resolution policy. we'll commit it in begin
			_resolutionPolicy = defaultSceneResolutionPolicy;
			_designResolutionSize = defaultDesignResolutionSize;
		}


		internal void begin()
		{
			Assert.isFalse( _renderers.Count == 0, "Scene has begun with no renderer. At least one renderer must be present before beginning a scene." );
			Physics.reset();
			updateResolutionScaler();
			Core.emitter.addObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
		}


		internal void end()
		{
			for( var i = 0; i < _renderers.Count; i++ )
				_renderers[i].unload();

			for( var i = 0; i < _postProcessors.Count; i++ )
				_postProcessors[i].unload();

			Core.emitter.removeObserver( CoreEvents.GraphicsDeviceReset, onGraphicsDeviceReset );
			entities.removeAllEntities();
			camera.unload();
			camera = null;
			contentManager.Dispose();
			_sceneRenderTexture.unload();

			if( _destinationRenderTexture != null )
				_destinationRenderTexture.unload();
		}


		internal void update()
		{
			// we set the RenderTarget here so that the Viewport will match the RenderTarget properly
			Core.graphicsDevice.SetRenderTarget( _sceneRenderTexture );

			// update our lists in case they have any changes
			entities.updateLists();
			renderableComponents.updateLists();

			for( var i = 0; i < entities.Count; i++ )
			{
				var entity = entities[i];
				if( entity.enabled && ( entity.updateInterval == 1 || Time.frameCount % entity.updateInterval == 0 ) )
					entity.update();
			}
		}


		internal void preRender()
		{
			// Renderers should always have those that require RenderTextures first. They clear themselves and set themselves as
			// the current RenderTarget when they render
			if( _renderers[0].renderTexture == null )
			{
				Core.graphicsDevice.SetRenderTarget( _sceneRenderTexture );
				Core.graphicsDevice.Clear( clearColor );
			}
		}


		internal void render( bool debugRenderEnabled )
		{
			var lastRendererHadRenderTexture = false;
			for( var i = 0; i < _renderers.Count; i++ )
			{
				// MonoGame follows the XNA bullshit implementation so it will clear the entire buffer if we change the render target even if null.
				// Because of that, we track when we are done with our RenderTextures and clear the scene at that time.
				if( lastRendererHadRenderTexture )
				{
					Core.graphicsDevice.SetRenderTarget( _sceneRenderTexture );
					Core.graphicsDevice.Clear( clearColor );

					// force a Camera matrix update to account for the new Viewport size
					if( _renderers[i].camera != null )
						_renderers[i].camera.forceMatrixUpdate();
					camera.forceMatrixUpdate();
				}

				_renderers[i].render( this, debugRenderEnabled );
				lastRendererHadRenderTexture = _renderers[i].renderTexture != null;
			}
		}


		internal void postRender()
		{
			var enabledCounter = 0;
			if( enablePostProcessing )
			{
				for( var i = 0; i < _postProcessors.Count; i++ )
				{
					if( _postProcessors[i].enabled )
					{
						var isEven = Mathf.isEven( enabledCounter );
						enabledCounter++;
						_postProcessors[i].process( isEven ? _sceneRenderTexture : _destinationRenderTexture, isEven ? _destinationRenderTexture : _sceneRenderTexture );
					}
				}
			}

			// render our final result to the backbuffer
			Core.graphicsDevice.SetRenderTarget( null );
			Core.graphicsDevice.Clear( letterboxColor );
			Graphics.instance.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.Opaque, samplerState );
			Graphics.instance.spriteBatch.Draw( Mathf.isEven( enabledCounter ) ? _sceneRenderTexture : _destinationRenderTexture, _finalRenderDestinationRect, Color.White );
			Graphics.instance.spriteBatch.End();
		}


		void onGraphicsDeviceReset()
		{
			updateResolutionScaler();
		}


		#region Resolution Policy

		/// <summary>
		/// sets the design size and resolution policy then updates the render textures
		/// </summary>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		/// <param name="sceneResolutionPolicy">Scene resolution policy.</param>
		public void setDesignResolution( int width, int height, SceneResolutionPolicy sceneResolutionPolicy )
		{
			_designResolutionSize = new Point( width, height );
			_resolutionPolicy = sceneResolutionPolicy;
			updateResolutionScaler();
		}


		void updateResolutionScaler()
		{
			var designSize = _designResolutionSize;
			var screenSize = new Point( Screen.backBufferWidth, Screen.backBufferHeight );
			var screenAspectRatio = (float)screenSize.X / (float)screenSize.Y;

			var renderTextureWidth = screenSize.X;
			var renderTextureHeight = screenSize.Y;

			var resolutionScaleX = (float)screenSize.X / (float)designSize.X;
			var resolutionScaleY = (float)screenSize.Y / (float)designSize.Y;

			var rectCalculated = false;

			// calculate the scale used by the PixelPerfect variants
			var scale = 1;
			if( _resolutionPolicy != SceneResolutionPolicy.None )
			{
				if( (float)designSize.X / (float)designSize.Y > screenAspectRatio )
					scale = screenSize.X / designSize.X;
				else
					scale = screenSize.Y / designSize.Y;

				if( scale == 0 )
					scale = 1;
			}

			switch( _resolutionPolicy )
			{
				case SceneResolutionPolicy.None:
					_finalRenderDestinationRect.X = _finalRenderDestinationRect.Y = 0;
					_finalRenderDestinationRect.Width = screenSize.X;
					_finalRenderDestinationRect.Height = screenSize.Y;
					rectCalculated = true;
					break;
				case SceneResolutionPolicy.ExactFit:
					// exact design size render texture
					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.NoBorder:
					// exact design size render texture
					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;

					resolutionScaleX = resolutionScaleY = Math.Max( resolutionScaleX, resolutionScaleY );
					break;
				case SceneResolutionPolicy.NoBorderPixelPerfect:
					// exact design size render texture
					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;

					// we are going to do some cropping so we need to use floats for the scale then round up
					scale = 1;
					if( (float)designSize.X / (float)designSize.Y < screenAspectRatio )
					{
						var floatScale = (float)screenSize.X / (float)designSize.X;
						scale = Mathf.ceilToInt( floatScale );
					}
					else
					{
						var floatScale = (float)screenSize.Y / (float)designSize.Y;
						scale = Mathf.ceilToInt( floatScale );
					}

					if( scale == 0 )
						scale = 1;

					_finalRenderDestinationRect.Width = Mathf.ceilToInt( designSize.X * scale );
					_finalRenderDestinationRect.Height = Mathf.ceilToInt( designSize.Y * scale );
					_finalRenderDestinationRect.X = ( screenSize.X - _finalRenderDestinationRect.Width ) / 2;
					_finalRenderDestinationRect.Y = ( screenSize.Y - _finalRenderDestinationRect.Height ) / 2;
					rectCalculated = true;

					break;
				case SceneResolutionPolicy.ShowAll:
					resolutionScaleX = resolutionScaleY = Math.Min( resolutionScaleX, resolutionScaleY );

					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.ShowAllPixelPerfect:
					// exact design size render texture
					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;

					_finalRenderDestinationRect.Width = Mathf.ceilToInt( designSize.X * scale );
					_finalRenderDestinationRect.Height = Mathf.ceilToInt( designSize.Y * scale );
					_finalRenderDestinationRect.X = ( screenSize.X - _finalRenderDestinationRect.Width ) / 2;
					_finalRenderDestinationRect.Y = ( screenSize.Y - _finalRenderDestinationRect.Height ) / 2;
					rectCalculated = true;

					break;
				case SceneResolutionPolicy.FixedHeight:
					resolutionScaleX = resolutionScaleY;
					designSize.X = Mathf.ceilToInt( screenSize.X / resolutionScaleX );

					// exact design size render texture for height but not width
					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.FixedHeightPixelPerfect:
					// start with exact design size render texture height. the width may change
					renderTextureHeight = designSize.Y;

					_finalRenderDestinationRect.Width = Mathf.ceilToInt( designSize.X * resolutionScaleX );
					_finalRenderDestinationRect.Height = Mathf.ceilToInt( designSize.Y * scale );
					_finalRenderDestinationRect.X = ( screenSize.X - _finalRenderDestinationRect.Width ) / 2;
					_finalRenderDestinationRect.Y = ( screenSize.Y - _finalRenderDestinationRect.Height ) / 2;
					rectCalculated = true;

					renderTextureWidth = (int)( designSize.X * resolutionScaleX / scale );
					break;
				case SceneResolutionPolicy.FixedWidth:
					resolutionScaleY = resolutionScaleX;
					designSize.Y = Mathf.ceilToInt( screenSize.Y / resolutionScaleY );

					// exact design size render texture for width but not height
					renderTextureWidth = designSize.X;
					renderTextureHeight = designSize.Y;
					break;
				case SceneResolutionPolicy.FixedWidthPixelPerfect:
					// start with exact design size render texture width. the height may change
					renderTextureWidth = designSize.X;

					_finalRenderDestinationRect.Width = Mathf.ceilToInt( designSize.X * scale );
					_finalRenderDestinationRect.Height = Mathf.ceilToInt( designSize.Y * resolutionScaleY );
					_finalRenderDestinationRect.X = ( screenSize.X - _finalRenderDestinationRect.Width ) / 2;
					_finalRenderDestinationRect.Y = ( screenSize.Y - _finalRenderDestinationRect.Height ) / 2;
					rectCalculated = true;

					renderTextureHeight = (int)( designSize.Y * resolutionScaleY / scale );

					break;
			}

			// if we didnt already calculate a rect (None and all pixel perfect variants calculate it themselves) calculate it now
			if( !rectCalculated )
			{
				// calculate the display rect of the RenderTexture
				var renderWidth = designSize.X * resolutionScaleX;
				var renderHeight = designSize.Y * resolutionScaleY;

				_finalRenderDestinationRect = RectangleExt.fromFloats( ( screenSize.X - renderWidth ) / 2, ( screenSize.Y - renderHeight ) / 2, renderWidth, renderHeight );
			}


			// set some values in the Input class to translate mouse position to our scaled resolution
			var scaleX = renderTextureWidth / (float)_finalRenderDestinationRect.Width;
			var scaleY = renderTextureHeight / (float)_finalRenderDestinationRect.Height;

			Input._resolutionScale = new Vector2( scaleX, scaleY );
			Input._resolutionOffset = _finalRenderDestinationRect.Location;

			// resize our RenderTextures
			_sceneRenderTexture.resize( renderTextureWidth, renderTextureHeight );

			if( _destinationRenderTexture != null )
				_destinationRenderTexture.resize( renderTextureWidth, renderTextureHeight );

			// notify the PostProcessors and Renderers of the change in render texture size
			for( var i = 0; i < _postProcessors.Count; i++ )
				_postProcessors[i].onSceneBackBufferSizeChanged( renderTextureWidth, renderTextureHeight );

			for( var i = 0; i < _renderers.Count; i++ )
				_renderers[i].onSceneBackBufferSizeChanged( renderTextureWidth, renderTextureHeight );
		}

		#endregion


		#region Utils

		/// <summary>
		/// Returns whether the timeSinceSceneLoad has passed the given time interval since the last frame. Ex: given 2.0f, this will return true once every 2 seconds
		/// </summary>
		/// <param name="interval">The time interval to check for</param>
		/// <returns></returns>
		public bool onInterval( float interval )
		{
			return (int)( ( Time.timeSinceSceneLoad - Time.deltaTime ) / interval ) < (int)( Time.timeSinceSceneLoad / interval );
		}


		/// <summary>
		/// handles fine grained ordering of entities. When an entity sets it's order this method will be called which will
		/// set the actualOrder of the entity.
		/// </summary>
		/// <param name="entity">Entity.</param>
		internal void setActualOrder( Entity entity )
		{
			const double theta = .000001f;

			// if an entity is already at the requested depth we increment the depth by theta and set the entities actual depth based on
			// the already present depth value from the previous entity
			double add = 0;
			if( _actualEntityOrderLookup.TryGetValue( entity._updateOrder, out add ) )
				_actualEntityOrderLookup[entity._updateOrder] += theta;
			else
				_actualEntityOrderLookup.Add( entity._updateOrder, theta );
			entity._actualUpdateOrder = entity._updateOrder - add;

			// mark lists unsorted
			entities.markTagUnsorted();
			entities.markTagUnsorted( entity.tag );
		}

		#endregion


		#region Renderer/PostProcessor Management

		public Renderer addRenderer( Renderer renderer )
		{
			_renderers.Add( renderer );
			_renderers.Sort( Renderer.compareRenderOrder );
			return renderer;
		}


		public void removeRenderer( Renderer renderer )
		{
			_renderers.Remove( renderer );
		}


		public void addPostProcessor( PostProcessor step )
		{
			_postProcessors.Add( step );
			_postProcessors.Sort( PostProcessor.comparePostProcessorOrder );

			// lazily create the 2nd RenderTexture for post processing only when a PostProcessor is added
			if( _destinationRenderTexture == null )
				_destinationRenderTexture = new RenderTexture();
		}


		public void removePostProcessor( PostProcessor step )
		{
			_postProcessors.Remove( step );
		}

		#endregion


		#region Entity Management

		/// <summary>
		/// add the Entity to this Scene, and return it
		/// </summary>
		/// <typeparam name="T">entity type</typeparam>
		/// <returns></returns>
		public T createAndAddEntity<T>( string name ) where T : Entity, new()
		{
			var entity = new T();
			entity.name = name;
			return addEntity( entity );
		}


		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public Entity addEntity( Entity entity )
		{
			Assert.isFalse( entities.contains( entity ), "You are attempting to add the same entity to a scene twice: {0}", entity );
			entities.add( entity );
			return entity;
		}



		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public T addEntity<T>( T entity ) where T : Entity 
		{
			Assert.isFalse( entities.contains( entity ), "You are attempting to add the same entity to a scene twice: {0}", entity );
			entities.add( entity );
			return entity;
		}



		/// <summary>
		/// removes an Entity from the Scene's entities list
		/// </summary>
		/// <param name="entity">The Entity to remove</param>
		public void removeEntity( Entity entity )
		{
			entities.remove( entity );
		}


		/// <summary>
		/// removes all entities from the scene
		/// </summary>
		public void removeAllEntities()
		{
			for( var i = 0; i < entities.Count; i++ )
				removeEntity( entities[i] );
		}


		public Entity findEntity( string name )
		{
			return entities.findEntity( name );
		}


		/// <summary>
		/// returns all entities with the given tag
		/// </summary>
		/// <returns>The entities by tag.</returns>
		/// <param name="tag">Tag.</param>
		public List<Entity> findEntitiesByTag( int tag )
		{
			return entities.entitiesWithTag( tag );
		}

		#endregion

	}
}

