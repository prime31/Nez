using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;


namespace Nez
{
	public class Entity
	{
		/// <summary>
		/// the scene this entity belongs to
		/// </summary>
		public Scene scene;

		/// <summary>
		/// entity name. useful for doing scene-wide searches for an entity
		/// </summary>
		public string name;

		protected Vector2 _position;
		public Vector2 position
		{
			get { return _position; }
			set
			{
				_position = value;

				// notify our children of our changed position
				if( collider != null )
					collider.onEntityPositionChanged();

				for( var i = 0; i < components.Count; i++ )
				{
					if( components[i].enabled )
						components[i].onEntityPositionChanged();
				}
			}
		}

		/// <summary>
		/// list of all the components currently attached to this entity
		/// </summary>
		public ComponentList components;

		int _tag = 0;
		/// <summary>
		/// use this however you want to. It can later be used to query the scene for all Entities with a specific tag
		/// </summary>
		public int tag
		{
			get { return _tag; }
			set
			{
				if( _tag != value )
				{
					// we only call through to the entityTagList if we already have a scene. if we dont have a scene yet we will be
					// added to the entityTagList when we do
					if( scene != null )
						scene.entities.removeFromTagList( this );
					_tag = value;
					if( scene != null )
						scene.entities.addToTagList( this );
				}
			}
		}

		/// <summary>
		/// specifies how often this entitys update method should be called. 1 means every frame, 2 is every other, etc
		/// </summary>
		public uint updateInterval = 1;

		bool _enabled = true;
		public bool enabled
		{
			get
			{
				return _enabled;
			}
			set
			{
				if( _enabled != value )
				{
					_enabled = value;

					for( var i = 0; i < components.Count; i++ )
					{
						var component = components[i];

						if( _enabled )
							component.onEnabled();
						else
							component.onDisabled();
					}

					if( _collider != null )
					{
						if( enabled )
							_collider.registerColliderWithPhysicsSystem();
						else
							_collider.unregisterColliderWithPhysicsSystem();
					}
				}
			}
		}

		internal double _actualUpdateOrder = 0;
		internal int _updateOrder = 0;

		/// <summary>
		/// update order of this Entity. Also used to sort tag lists on scene.entities
		/// </summary>
		/// <value>The order.</value>
		public int updateOrder
		{
			get { return _updateOrder; }
			set
			{
				if( _updateOrder != value )
				{
					_updateOrder = value;
					if( scene != null )
						scene.setActualOrder( this );
				}
			}
		}

		Collider _collider;
		/// <summary>
		/// the Collider managed by this Entity. Setting this property automatically registers the Collider with the Physics system.
		/// </summary>
		/// <value>The collider.</value>
		public Collider collider
		{
			get { return _collider; }
			set
			{
				if( value == _collider )
					return;
				
				if( _collider != null )
				{
					_collider.unregisterColliderWithPhysicsSystem();
					_collider.entity = null;
				}

				_collider = value;

				if( _collider != null )
				{
					_collider.entity = this;

					// if we dont have a scene yet onAddedToEntity will be called when this Entity is added to the scene
					if( scene != null )
						_collider.registerColliderWithPhysicsSystem();
				}
			}
		}

		private BitSet _componentBits = new BitSet();
		public BitSet componentBits
		{
			get { return _componentBits; }
		}

		public Entity()
		{
			components = new ComponentList( this );
		}


		public Entity( string name ) : this()
		{
			this.name = name;
		}


		/// <summary>
		/// Called when this entity is added to a scene
		/// </summary>
		public virtual void onAddedToScene()
		{
			// if we have a collider, we need to let it register with the Physics system when we are added to a scene
			if( collider != null )
				collider.onEntityAddedToScene();
		}


		/// <summary>
		/// Called when this entity is removed from a scene
		/// </summary>
		public virtual void onRemovedFromScene()
		{
			if( collider != null )
				collider.onEntityRemovedFromScene();

			// detach all our components when removed from a scene
			components.removeAllComponents();
		}


		/// <summary>
		/// called in the same frame as onAddedToScene but after all pending entity changes are committed
		/// </summary>
		public virtual void onAwake()
		{}


		/// <summary>
		/// called each frame as long as the Entity is enabled
		/// </summary>
		public virtual void update()
		{
			components.updateLists();

			for( var i = 0; i < components.Count; i++ )
			{
				if( components[i].enabled )
					components[i].update();
			}
		}


		/// <summary>
		/// called if Core.debugRenderEnabled is true by the default renderers. Custom renderers can choose to call it or not.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public virtual void debugRender( Graphics graphics )
		{
			for( var i = 0; i < components.Count; i++ )
			{
				if( components[i].enabled )
					components[i].debugRender( graphics );
			}

			if( _collider != null )
				_collider.debugRender( graphics );
		}


		#region Component Management

		/// <summary>
		/// Adds a Component to the components list
		/// </summary>
		public void addComponent( Component component )
		{
			component.entity = this;
			components.add( component );
		}


		/// <summary>
		/// Gets the first component of type T and returns it. If no components are found returns null
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getComponent<T>() where T : Component
		{
			return components.getComponent<T>();
		}


		/// <summary>
		/// Gets all the components of type T
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> getComponents<T>() where T : Component
		{
			return components.getComponents<T>();
		}


		/// <summary>
		/// removes a Component from the components list
		/// </summary>
		/// <param name="component">The Component to remove</param>
		public void removeComponent( Component component )
		{
			components.remove( component );
		}


		/// <summary>
		/// removes all Components from the Entity
		/// </summary>
		public void removeAllComponents()
		{
			for( var i = 0; i < components.Count; i++ )
				removeComponent( components[i] );
		}

		#endregion


		#region Movement helpers

		/// <summary>
		/// moves the entity taking collision into account
		/// </summary>
		/// <param name="motion">Motion.</param>
		public void moveActor( Vector2 motion, bool stepXYSeparatelyForMultiCollisions = true )
		{
			// no collider? just move and forget about it
			if( collider == null )
			{
				position += motion;
				return;
			}
				
			// remove ourself from the physics system until after we are done moving
			Physics.removeCollider( collider, true );

			// fetch anything that we might collide with along the way
			var neighbors = Physics.boxcastBroadphaseExcludingSelf( collider, motion.X, motion.Y );

			// if we have more than once possible collision we have to break this up into separate x/y movement
			if( stepXYSeparatelyForMultiCollisions && neighbors.Count() > 1 )
			{
				if( motion.X != 0f )
				{
					var xMotion = new Vector2( motion.X, 0f );
					moveActorCollisionChecks( neighbors, ref xMotion );
					motion.X = xMotion.X;
				}

				if( motion.Y != 0f )
				{
					var yMotion = new Vector2( 0f, motion.Y );
					moveActorCollisionChecks( neighbors, ref yMotion );
					motion.Y = yMotion.Y;
				}
			}
			else
			{
				moveActorCollisionChecks( neighbors, ref motion );
			}

			// set our new position which will trigger child component/collider bounds updates
			position += motion;

			// let Physics know about our new position
			Physics.addCollider( collider );
		}


		void moveActorCollisionChecks( IEnumerable<Collider> neighbors, ref Vector2 motion )
		{
			foreach( var neighbor in neighbors )
			{
				ShapeCollisionResult result;
				if( collider.collidesWith( neighbor, motion, out result ) )
				{
					// if we have a trigger notify the listener but we dont alter movement
					if( neighbor.isTrigger )
					{
						// TODO: notifiy listener
						Debug.log( "hit trigger: {0}", neighbor.entity );
						continue;
					}

					// hit. alter our motion by the MTV and continue looping in case there are other collisions
					motion -= result.minimumTranslationVector;
				}
			}
		}

		#endregion


		public override string ToString()
		{
			return string.Format(" [Entity: name: {0}, tag: {1}, enabled: {2}, depth: {3}]", name, tag, enabled, updateOrder );
		}

	}
}

