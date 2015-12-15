using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


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

		internal double _actualOrder = 0;
		internal int _order = 0;

		/// <summary>
		/// render order of this Entity. Also used to sort tag lists on scene.entities
		/// </summary>
		/// <value>The order.</value>
		public int order
		{
			get { return _order; }
			set
			{
				if( _order != value )
				{
					_order = value;
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

		Vector2 _movementRemainder = Vector2.Zero;

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


		#region Movement with Collision Checks

		/// <summary>
		/// attempts to move deltaX, deltaY. if a collision occurs movement will stop at the point of collision. always use this method to
		/// move an Entity that has colliders! It handles updating the spatial hash.
		/// </summary>
		/// <param name="deltaX">Delta x.</param>
		/// <param name="deltaY">Delta y.</param>
		public bool moveActor( float deltaX, float deltaY, ICollisionCallback collisionHandler = null, ITriggerCallback triggerHandler = null )
		{
			// no collider? just move and forget about it
			if( collider == null )
			{
				position += new Vector2( deltaX, deltaY );
				return false;
			}

			// remove ourself from the physics system until after we are done moving
			Physics.removeCollider( collider, true );

			// fetch anything that we might collide with along the way
			var neighbors = Physics.boxcastBroadphaseExcludingSelf( collider, deltaX, deltaY );

			var collideX = moveActorX( deltaX, neighbors, collisionHandler, triggerHandler );
			var collideY = moveActorY( deltaY, neighbors, collisionHandler, triggerHandler );

			// set our new position which will trigger child component/collider bounds updates
			position = _position;

			// let Physics know about our new position
			Physics.addCollider( collider );

			return collideX || collideY;
		}


		bool moveActorX( float amount, HashSet<Collider> neighbors, ICollisionCallback collisionHandler = null, ITriggerCallback triggerHandler = null )
		{
			_movementRemainder.X += amount;
			var moveAmount = Mathf.roundToInt( _movementRemainder.X );

			if( moveAmount == 0 )
				return false;
			
			var sign = Math.Sign( moveAmount );
			var deltaSinglePixelMovement = new Vector2( sign, 0f );
			while( moveAmount != 0 )
			{
				_movementRemainder.X -= sign;
				foreach( var neighbor in neighbors )
				{
					if( collider.collidesWithAtPosition( neighbor, _position + deltaSinglePixelMovement ) )
					{
						// if we have a trigger notify the listener but we dont alter movement
						if( neighbor.isTrigger || collider.isTrigger )
						{
							if( triggerHandler != null )
								triggerHandler.onTriggerEnter( neighbor );
						}
						else
						{
							// hit a non-trigger. that's all folks. we bail here
							if( collisionHandler != null )
								collisionHandler.onCollisionEnter( neighbor, sign > 0 ? CollisionDirection.Right : CollisionDirection.Left );
							_movementRemainder.X = 0f;
							return true;
						}
					}
				}

				// all clear
				_position += deltaSinglePixelMovement;
				moveAmount -= sign;
			}

			return false;
		}


		bool moveActorY( float amount, HashSet<Collider> neighbors, ICollisionCallback collisionHandler = null, ITriggerCallback triggerHandler = null )
		{
			_movementRemainder.Y += amount;
			var moveAmount = Mathf.roundToInt( _movementRemainder.Y );

			if( moveAmount == 0 )
				return false;

			var sign = Math.Sign( moveAmount );
			var deltaSinglePixelMovement = new Vector2( 0f, sign );
			while( moveAmount != 0 )
			{
				_movementRemainder.Y -= sign;
				var b = collider.bounds.clone();
				b.Y += sign;

				foreach( var neighbor in neighbors )
				{
					if( collider.collidesWithAtPosition( neighbor, _position + deltaSinglePixelMovement ) )
					{
						// if we have a trigger notify the listener but we dont alter movement
						if( neighbor.isTrigger || collider.isTrigger )
						{
							if( triggerHandler != null )
								triggerHandler.onTriggerEnter( neighbor );
						}
						else
						{
							// hit a non-trigger. that's all folks. we bail here
							if( collisionHandler != null )
								collisionHandler.onCollisionEnter( neighbor, sign > 0 ? CollisionDirection.Below : CollisionDirection.Above );
							_movementRemainder.Y = 0f;
							return true;
						}
					}
				}

				// all clear
				_position += deltaSinglePixelMovement;
				moveAmount -= sign;
			}

			return false;
		}
			

		/// <summary>
		/// solid movement does not care about collisions for the movement itself. A solid will always get to its final destination. It will,
		/// however push any Actors in its way and caryy any actors riding on it.
		/// </summary>
		/// <param name="deltaX">Delta x.</param>
		/// <param name="deltaY">Delta y.</param>
		/// <param name="allActors">All actors.</param>
		/// <param name="ridingActors">Riding actors.</param>
		public void moveSolid( float deltaX, float deltaY, List<Entity> allActors, List<Entity> ridingActors )
		{
			if( deltaX == 0f && deltaY == 0f )
				return;

			// remove ourself from the physics system until after we are done moving
			Physics.removeCollider( collider, true );

			if( deltaX != 0f )
			{
				position += new Vector2( deltaX, 0f );
				moveSolidX( deltaX, allActors, ridingActors );
			}

			if( deltaY != 0f )
			{
				position += new Vector2( 0f, deltaY );
				moveSolidY( deltaY, allActors, ridingActors );
			}

			// let Physics know about our new position
			Physics.addCollider( collider );
		}


		void moveSolidX( float amount, List<Entity> allActors, List<Entity> ridingActors )
		{
			for( var i = 0; i < allActors.Count; i++ )
			{
				var actor = allActors[i];

				if( actor.collider.collidesWith( collider ) )
				{
					// push. deal with moving left/right
					float moveAmount;
					if( amount > 0f )
						moveAmount = collider.bounds.Right - actor.collider.bounds.Left;
					else
						moveAmount = collider.bounds.Left - actor.collider.bounds.Right;

					if( actor.moveActor( moveAmount, 0 ) )
					{
						// TODO: dont do this. we need an event for this
						scene.removeEntity( actor );
					}
				}
				else if( ridingActors.Contains( actor ) )
				{
					// riding
					actor.moveActor( amount, 0 );
				}
			}
		}


		void moveSolidY( float amount, List<Entity> allActors, List<Entity> ridingActors )
		{
			for( var i = 0; i < allActors.Count; i++ )
			{
				var actor = allActors[i];

				if( actor.collider.collidesWith( collider ) )
				{
					// push. deal with moving up/down
					float moveAmount;
					if( amount > 0f )
						moveAmount = collider.bounds.Bottom - actor.collider.bounds.Top;
					else
						moveAmount = collider.bounds.Top - actor.collider.bounds.Bottom;

					if( actor.moveActor( 0, moveAmount ) )
					{
						// TODO: dont do this. we need an event for this
						scene.removeEntity( actor );
					}
				}
				else if( ridingActors.Contains( actor ) )
				{
					// riding
					actor.moveActor( 0, amount );
				}
			}
		}

		#endregion


		public override string ToString()
		{
			return string.Format(" [Entity: tag: {0}, enabled: {1}, depth: {2}, name: {3}]", tag, enabled, order, name );
		}

	}
}

