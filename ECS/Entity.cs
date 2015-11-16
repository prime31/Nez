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

					foreach( var component in components )
					{
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


		public Entity()
		{
			components = new ComponentList( this );
		}


		public Entity( string name )
		{
			this.name = name;
		}


		/// <summary>
		/// Called when this entity is added to a scene
		/// </summary>
		/// <param name="scene">Scene.</param>
		public virtual void onAddedToScene()
		{
			// if we have a collider, we need to let it register with the Physics system when we are added to a scene
			if( collider != null )
				collider.registerColliderWithPhysicsSystem();
		}


		/// <summary>
		/// Called when this entity is removed from a scene
		/// </summary>
		public virtual void onRemovedFromScene()
		{
			if( collider != null )
				collider.unregisterColliderWithPhysicsSystem();
		}


		/// <summary>
		/// called in the same frame as onAddedToScene but after all pending entity changes are committed
		/// </summary>
		public virtual void onAwake()
		{}


		public virtual void update()
		{
			components.updateLists();

			foreach( var component in components )
			{
				if( component.enabled )
					component.update();
			}
		}


		public virtual void debugRender( Graphics graphics )
		{
			foreach( var component in components )
			{
				if( component.enabled )
					component.debugRender( graphics );
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
			foreach( var component in components )
				removeComponent( component );
		}

		#endregion


		#region Movement with Collision Checks

		/// <summary>
		/// attempts to move deltaX, deltaY. if a collision occurs movement will stop at the point of collision
		/// </summary>
		/// <param name="deltaX">Delta x.</param>
		/// <param name="deltaY">Delta y.</param>
		public void move( float deltaX, float deltaY )
		{
			// no collider? just move and forget about it
			if( collider == null )
			{
				position += new Vector2( deltaX, deltaY );
				return;
			}

			// store our pre-move bounds so that we can use it to update ourself in the physics system after moving
			var oldBounds = collider.bounds;

			// fetch anything that we might collide with along the way
			var neighbors = scene.physics.boxcastExcludingSelf( collider, deltaX, deltaY );

			moveX( deltaX, neighbors );
			moveY( deltaY, neighbors );
			position.round();

			// back into the physics system we go
			scene.physics.updateCollider( collider, ref oldBounds );
		}


		void moveX( float amount, HashSet<Collider> neighbors )
		{
			var xRemainder = amount;
			var moveAmount = Mathf.roundToInt( xRemainder );

			if( moveAmount == 0 )
				return;

			var sign = Math.Sign( moveAmount );
			var deltaSinglePixelMovement = new Vector2( sign, 0f );
			while( moveAmount != 0 )
			{
				xRemainder -= moveAmount;
				foreach( var neighbor in neighbors )
				{
					if( collider.collidesWithAtPosition( neighbor, position + deltaSinglePixelMovement ) )
					{
						// TODO: deal with triggers here. we shouldnt bail out and we should let the two colliders know about each other
						return;
					}
				}

				// all clear
				position.X += sign;
				moveAmount -= sign;
			}
		}


		void moveY( float amount, HashSet<Collider> neighbors )
		{
			var yRemainder = amount;
			var moveAmount = Mathf.roundToInt( yRemainder );

			if( moveAmount == 0 )
				return;

			var sign = Math.Sign( moveAmount );
			var deltaSinglePixelMovement = new Vector2( 0f, sign );
			while( moveAmount != 0 )
			{
				yRemainder -= moveAmount;
				var b = collider.bounds.clone();
				b.Y += sign;

				foreach( var neighbor in neighbors )
				{
					if( collider.collidesWithAtPosition( neighbor, position + deltaSinglePixelMovement ) )
					{
						// collision. done here
						return;
					}
				}

				// all clear
				position.Y += sign;
				moveAmount -= sign;
			}
		}
			
		#endregion


		public override string ToString()
		{
			return string.Format(" [Entity: tag: {0}, enabled: {1}, depth: {2}, name: {3}]", tag, enabled, depth, name );
		}

	}
}

