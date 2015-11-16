using System;
using System.Collections.Generic;
using Nez.Physics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Scene
	{
		// cache used across all scenes so it is static
		internal static EntityCache _entityCache;

		public Camera2D camera;
		public Color clearColor = Color.CornflowerBlue;
		public float sceneActiveDuration;
		public Physics.Physics physics;

		public bool enablePostProcessing;
		readonly List<PostProcessingStep> _postProcessingSteps = new List<PostProcessingStep>();

		/// <summary>
		/// The list of entities within this Scene
		/// </summary>
		public EntityList entities;

		public RenderableComponentList renderableComponents;

		List<Renderer> _renderers = new List<Renderer>();
		Dictionary<int,double> _actualDepthLookup = new Dictionary<int,double>();


		static Scene()
		{
			_entityCache = new EntityCache();
		}


		public Scene( int physicsSystemCellSize = 100 )
		{
			camera = new Camera2D( Graphics.defaultGraphics.graphicsDevice );
			physics = new Physics.Physics( physicsSystemCellSize );
			entities = new EntityList( this );
			renderableComponents = new RenderableComponentList();
		}


		internal void begin()
		{
			Debug.warnIf( _renderers.Count == 0, "Scene has begun with no renderer. Are you sure you want to run a Scene without a renderer?" );
		}


		internal void end()
		{}


		internal void update()
		{
			sceneActiveDuration += Core.deltaTime;
			entities.updateLists();

			foreach( var entity in entities )
			{
				if( entity.enabled && ( entity.updateInterval == 1 || Core.frameCount % entity.updateInterval == 0 ) )
					entity.update();
			}
		}


		internal void preRender()
		{
			// TODO: if enablePostProcessing is true set a proper renderTarget
			Graphics.defaultGraphics.graphicsDevice.SetRenderTarget( null );
			Graphics.defaultGraphics.graphicsDevice.Clear( clearColor );
		}


		internal void render()
		{
			foreach( var renderer in _renderers )
				renderer.render( this );
		}


		internal void debugRender()
		{
			foreach( var renderer in _renderers )
				renderer.debugRender( this );
		}


		internal void postRender()
		{
			if( !enablePostProcessing )
				return;

			foreach( var step in _postProcessingSteps )
			{
				if( !step.enabled )
					continue;
				
				// TODO: support post processing
				// _activeRenderTarget: RT that the scene was drawn into
				// _activeInputSource: RT used for post effet
//				var temp = _activeRenderTarget;
//				_activeRenderTarget = _activeInputSource;
//				_activeInputSource = temp;
//				_device.SetRenderTarget(_activeRenderTarget);
//
//				step.process( _activeInputSource, _activeRenderTarget );
			}

//			_device.SetRenderTargets( _oldBindings );
//			Graphics.defaultGraphics.spriteBatch.Begin();
//			Graphics.defaultGraphics.spriteBatch.Draw(_activeRenderTarget, Vector2.Zero, Color.White);
//			Graphics.defaultGraphics.spriteBatch.End();
		}


		#region Utils

		/// <summary>
		/// Returns whether the Scene timer has passed the given time interval since the last frame. Ex: given 2.0f, this will return true once every 2 seconds
		/// </summary>
		/// <param name="interval">The time interval to check for</param>
		/// <returns></returns>
		public bool onInterval( float interval )
		{
			return (int)(( sceneActiveDuration - Core.deltaTime ) / interval ) < (int)( sceneActiveDuration / interval );
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


		#region Renderer Management

		public void addRenderer( Renderer renderer )
		{
			_renderers.Add( renderer );
			_renderers.Sort( Renderer.compareRenderOrder );
		}


		public void removeRenderer( Renderer renderer )
		{
			_renderers.Remove( renderer );
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
			foreach( var entity in entities )
				removeEntity( entity );
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

