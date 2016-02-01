using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System.Linq;


namespace Nez
{
	public class Entity : IComparable<Entity>
	{
		#region properties and fields

		/// <summary>
		/// the scene this entity belongs to
		/// </summary>
		public Scene scene;

		/// <summary>
		/// entity name. useful for doing scene-wide searches for an entity
		/// </summary>
		public readonly string name;

		/// <summary>
		/// encapsulates the Entity's position/rotation/scale and allows setting up a hieararchy
		/// </summary>
		public readonly Transform transform;

		/// <summary>
		/// list of all the components currently attached to this entity
		/// </summary>
		public ComponentList components;

		/// <summary>
		/// list of all the Colliders currently attached to this entity
		/// </summary>
		public ColliderList colliders;

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
		int _tag = 0;

		/// <summary>
		/// specifies how often this entitys update method should be called. 1 means every frame, 2 is every other, etc
		/// </summary>
		public uint updateInterval = 1;

		/// <summary>
		/// enables/disables the Entity. When disabled colliders are removed from the Physics system and components methods will not be called
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
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

					if( _enabled )
					{
						components.onEntityEnabled();
						colliders.onEntityEnabled();
					}
					else
					{
						components.onEntityDisabled();
						colliders.onEntityDisabled();
					}
				}
			}
		}
		bool _enabled = true;

		internal double _actualUpdateOrder = 0;


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
					{
						scene.entities.markEntityListUnsorted();
						scene.entities.markTagUnsorted( tag );
					}
				}
			}
		}
		internal int _updateOrder = 0;

		internal BitSet componentBits;

		#endregion


		public Entity()
		{
			components = new ComponentList( this );
			colliders = new ColliderList( this );
			transform = new Transform( this );

			if( Core.entitySystemsEnabled )
				componentBits = new BitSet();
		}


		public Entity( string name ) : this()
		{
			this.name = name;
		}


		internal void onTransformChanged()
		{
			// notify our children of our changed position
			components.onEntityPositionChanged();
			colliders.onEntityPositionChanged();
		}


		#region Entity lifecycle methods

		/// <summary>
		/// Called when this entity is added to a scene after all pending entity changes are committed
		/// </summary>
		public virtual void onAddedToScene()
		{
			// if we have a collider, we need to let it register with the Physics system when we are added to a scene
			colliders.onEntityAddedToScene();
		}


		/// <summary>
		/// Called when this entity is removed from a scene
		/// </summary>
		public virtual void onRemovedFromScene()
		{
			colliders.onEntityRemovedFromScene();

			// detach all our components when removed from a scene
			components.removeAllComponents();
		}


		/// <summary>
		/// called each frame as long as the Entity is enabled
		/// </summary>
		public virtual void update()
		{
			components.updateLists();
			components.update();
		}


		/// <summary>
		/// called if Core.debugRenderEnabled is true by the default renderers. Custom renderers can choose to call it or not.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public virtual void debugRender( Graphics graphics )
		{
			components.debugRender( graphics );
			colliders.debugRender( graphics );
		}

		#endregion


		#region Component Management

		/// <summary>
		/// Adds a Component to the components list. Returns this for chaining.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="component">Component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Entity addComponent<T>( T component ) where T : Component
		{
			component.entity = this;
			components.add( component );
			return this;
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

		public bool newMoveActor( Vector2 motion, out CollisionResult collisionResult )
		{
			collisionResult = new CollisionResult();

			// no collider? just move and forget about it
			if( colliders.Count == 0 )
			{
				transform.position += motion;
				return false;
			}

			// remove ourself from the physics system until after we are done moving
			colliders.unregisterAllCollidersWithPhysicsSystem();

			// 1. move all non-trigger entity.colliders and get closest collision
			for( var i = 0; i < colliders.Count; i++ )
			{
				var collider = colliders[i];

				// skip triggers for now. we will revisit them after we move.
				if( collider.isTrigger )
					continue;

				// fetch anything that we might collide with at our new position
				var bounds = collider.bounds;
				bounds.X += (int)motion.X;
				bounds.Y += (int)motion.Y;
				var neighbors = Physics.boxcastBroadphase( ref bounds );

				foreach( var neighbor in neighbors )
				{
					// skip triggers for now. we will revisit them after we move.
					if( neighbor.isTrigger )
						continue;
					
					CollisionResult tempCollisionResult;
					if( collider.collidesWith( neighbor, motion, out tempCollisionResult ) )
					{
						// hit. compare with the previous hit if we have one and choose the one that is closest (smallest MTV)
						if( collisionResult.collider == null ||
							( collisionResult.collider != null && collisionResult.minimumTranslationVector.LengthSquared() > tempCollisionResult.minimumTranslationVector.LengthSquared() ) )
						{
							collisionResult = tempCollisionResult;
						}
					}
				}
			}

			// 2. move entity to its new position
			if( collisionResult.collider != null )
				transform.position += motion - collisionResult.minimumTranslationVector;
			else
				transform.position += motion;

			// 3. do an overlap check of all entity.colliders that are triggers with all broadphase colliders, triggers or not.
			//    Any overlaps result in trigger events.
			for( var i = 0; i < colliders.Count; i++ )
			{
				var collider = colliders[i];

				// fetch anything that we might collide with at our new position
				var neighbors = Physics.boxcastBroadphase( collider.bounds );
				foreach( var neighbor in neighbors )
				{
					// we need at least one of the colliders to be a trigger
					if( !collider.isTrigger && !neighbor.isTrigger )
						continue;
					
					if( collider.overlaps( neighbor ) )
					{
						// trigger event perhaps?
						Debug.log( "trigger between {0} and {1}", collider, neighbor );
					}
				}
			}

			// let Physics know about our new position
			colliders.registerAllCollidersWithPhysicsSystem();

			return collisionResult.collider != null;
		}


		/// <summary>
		/// moves the entity taking collision into account
		/// </summary>
		/// <param name="motion">Motion.</param>
		public void moveActor( Vector2 motion, bool stepXYSeparatelyForMultiCollisions = true )
		{
			// no collider? just move and forget about it
			if( colliders.Count == 0 )
			{
				transform.position += motion;
				return;
			}
				
			// remove ourself from the physics system until after we are done moving
			Physics.removeCollider( colliders.mainCollider, true );

			// fetch anything that we might collide with at our new position
			var bounds = colliders.mainCollider.bounds;
			bounds.X += (int)motion.X;
			bounds.Y += (int)motion.Y;
			var neighbors = Physics.boxcastBroadphase( ref bounds );

			// if we have more than one possible collision we have to break this up into separate x/y movement. Note that this is only necessary
			// for certain types of movement such as gravity based systems due to the SAT collision response being shortest distance
			// and not necessarily in the path of movement. Ex. a platformer on a slope will have an unwanted horizontal response.
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
			transform.position += motion;

			// let Physics know about our new position
			Physics.addCollider( colliders.mainCollider );
		}


		void moveActorCollisionChecks( IEnumerable<Collider> neighbors, ref Vector2 motion )
		{
			foreach( var neighbor in neighbors )
			{
				CollisionResult result;
				if( colliders.mainCollider.collidesWith( neighbor, motion, out result ) )
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


		public int CompareTo( Entity other )
		{
			return _actualUpdateOrder.CompareTo( other._actualUpdateOrder );
		}


		public override string ToString()
		{
			return string.Format(" [Entity: name: {0}, tag: {1}, enabled: {2}, depth: {3}]", name, tag, enabled, updateOrder );
		}

	}
}

