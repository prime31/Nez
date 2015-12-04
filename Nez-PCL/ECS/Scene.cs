using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Nez.Systems;


namespace Nez
{
	public class Scene
	{
		// cache used across all scenes so it is static
		internal static EntityCache _entityCache;

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

		List<Renderer> _renderers = new List<Renderer>();
		Dictionary<int,double> _actualDepthLookup = new Dictionary<int,double>();
		readonly List<PostProcessor> _postProcessors = new List<PostProcessor>();


		static Scene()
		{
			_entityCache = new EntityCache();
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
			camera = new Camera( Graphics.instance.graphicsDevice );
			entities = new EntityList( this );
			renderableComponents = new RenderableComponentList();
			contentManager = new NezContentManager();
		}


		internal void begin()
		{
			Debug.warnIf( _renderers.Count == 0, "Scene has begun with no renderer. Are you sure you want to run a Scene without a renderer?" );
			Physics.reset();
		}


		internal void end()
		{
			for( var i = 0; i < _renderers.Count; i++ )
				_renderers[i].unload();

			entities.removeAllEntities();
			camera.unload();
			camera = null;
			contentManager.Dispose();
		}


		internal void update()
		{
			entities.updateLists();

			for( var i = 0; i < entities.Count; i++ )
			{
				var entity = entities[i];
				if( entity.enabled && ( entity.updateInterval == 1 || Time.frameCount % entity.updateInterval == 0 ) )
					entity.update();
			}
		}


		internal void preRender()
		{
			Graphics.instance.graphicsDevice.SetRenderTarget( null );
			Graphics.instance.graphicsDevice.Clear( clearColor );
		}


		internal void render( bool enableDebugRender )
		{
			var lastRendererHadRenderTexture = false;
			for( var i = 0; i < _renderers.Count; i++ )
			{
				// MonoGame follows the XNA bullshit implementation so it will clear the entire buffer if we change the render target even if null.
				// Because of that, we track when we are done with our RenderTextures and clear the scene at that time.
				if( lastRendererHadRenderTexture )
					Graphics.instance.graphicsDevice.Clear( clearColor );
				
				_renderers[i].render( this, enableDebugRender );
				lastRendererHadRenderTexture = _renderers[i].renderTexture != null;
			}
		}


		internal void postRender()
		{
			if( !enablePostProcessing )
				return;

			for( var i = 0; i < _postProcessors.Count; i++ )
			{
				if( _postProcessors[i].enabled )
					_postProcessors[i].process();
			}
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
		/// handles fine grained depth sorting for entities. When an entity sets it's depth this method will be called which will
		/// set the actualDepth of the entity.
		/// </summary>
		/// <param name="entity">Entity.</param>
		internal void setActualDepth( Entity entity )
		{
			const double theta = .000001f;

			// if an entity is already at the requested depth we increment the depth by theta and set the entities actual depth based on
			// the already present depth value from the previous entity
			double add = 0;
			if( _actualDepthLookup.TryGetValue( entity._depth, out add ) )
				_actualDepthLookup[entity._depth] += theta;
			else
				_actualDepthLookup.Add( entity._depth, theta );
			entity._actualDepth = entity._depth - add;

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


		public void addPostProcessStep( PostProcessor step )
		{
			_postProcessors.Add( step );
		}


		public void removePostProcessingStep( PostProcessor step )
		{
			_postProcessors.Remove( step );
		}

		#endregion


		#region Entity Management

		/// <summary>
		/// add the Entity to this Scene, and return it. Entity type must have the PooledEntityAttribute for a cached Entity to be used
		/// </summary>
		/// <typeparam name="T">Pooled Entity type to create</typeparam>
		/// <returns></returns>
		public T createAndAddEntity<T>( string name ) where T : Entity, new()
		{
			var entity = _entityCache.create<T>();
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
			Debug.assertIsFalse( entities.contains( entity ), "You are attempting to add the same entity to a scene twice: {0}", entity );
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

