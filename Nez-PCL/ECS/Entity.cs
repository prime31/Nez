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
		// TODO: should this be a property that fires a onEntityPositionChangedevent?
		public Vector2 position;
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

		internal double _actualDepth = 0;
		internal int _depth = 0;
		public int depth
		{
			get { return _depth; }
			set
			{
				if( _depth != value )
				{
					_depth = value;
					if( scene != null )
						scene.setActualDepth( this );
				}
			}
		}

		Collider _collider;
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


		public virtual void update()
		{
			components.updateLists();

			for( var i = 0; i < components.Count; i++ )
			{
				if( components[i].enabled )
					components[i].update();
			}
		}


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
		public bool move( float deltaX, float deltaY, ICollisionCallback collisionHandler = null, ITriggerCallback triggerHandler = null )
		{
			// no collider? just move and forget about it
			if( collider == null )
			{
				position += new Vector2( deltaX, deltaY );
				return false;
			}

			// store our pre-move bounds so that we can use it to update ourself in the physics system after moving
			var oldBounds = collider.bounds;

			// fetch anything that we might collide with along the way
			var neighbors = Physics.boxcastExcludingSelf( collider, deltaX, deltaY );

			var collideX = moveX( deltaX, neighbors, collisionHandler, triggerHandler );
			var collideY = moveY( deltaY, neighbors, collisionHandler, triggerHandler );
			Mathf.round( ref position );

			// let Physics know about our new position
			Physics.updateCollider( collider, ref oldBounds );

			return collideX || collideY;
		}


		bool moveX( float amount, HashSet<Collider> neighbors, ICollisionCallback collisionHandler = null, ITriggerCallback triggerHandler = null )
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
					if( collider.collidesWithAtPosition( neighbor, position + deltaSinglePixelMovement ) )
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
				position.X += sign;
				moveAmount -= sign;
			}

			return false;
		}


		bool moveY( float amount, HashSet<Collider> neighbors, ICollisionCallback collisionHandler = null, ITriggerCallback triggerHandler = null )
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
					if( collider.collidesWithAtPosition( neighbor, position + deltaSinglePixelMovement ) )
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
				position.Y += sign;
				moveAmount -= sign;
			}

			return false;
		}
			
		#endregion


		public override string ToString()
		{
			return string.Format(" [Entity: tag: {0}, enabled: {1}, depth: {2}, name: {3}]", tag, enabled, depth, name );
		}

	}
}

