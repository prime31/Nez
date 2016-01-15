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
		/// <summary>
		/// default scene Camera
		/// </summary>
		public Camera camera;

		/// <summary>
		/// clear color that is used in preRender to clear the screen
		/// </summary>
		public Color clearColor = Color.CornflowerBlue;

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

		RenderTexture _sceneRenderTexture;
		RenderTexture _destinationRenderTexture;

		List<Renderer> _renderers = new List<Renderer>();
		Dictionary<int,double> _actualEntityOrderLookup = new Dictionary<int,double>();
		readonly List<PostProcessor> _postProcessors = new List<PostProcessor>();


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
		}


		internal void begin()
		{
			Debug.warnIf( _renderers.Count == 0, "Scene has begun with no renderer. Are you sure you want to run a Scene without a renderer?" );
			Physics.reset();
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
			// the current RenderTarget
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
			Graphics.instance.spriteBatch.Begin( SpriteSortMode.Deferred, BlendState.Opaque );
			Graphics.instance.spriteBatch.Draw( Mathf.isEven( enabledCounter ) ? _sceneRenderTexture : _destinationRenderTexture, Vector2.Zero, Color.White );
			Graphics.instance.spriteBatch.End();
		}


		void onGraphicsDeviceReset()
		{
			_sceneRenderTexture.resizeToFitBackbuffer();

			if( _destinationRenderTexture != null )
				_destinationRenderTexture.resizeToFitBackbuffer();
		}


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

		public void addRenderer( Renderer renderer )
		{
			_renderers.Add( renderer );
			_renderers.Sort( Renderer.compareRenderOrder );
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
			addEntity( entity );

			return entity;
		}


		/// <summary>
		/// adds an Entity to the Scene's Entities list
		/// </summary>
		/// <param name="entity">The Entity to add</param>
		public void addEntity( Entity entity )
		{
			Assert.isFalse( entities.contains( entity ), "You are attempting to add the same entity to a scene twice: {0}", entity );
			entities.add( entity );
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

