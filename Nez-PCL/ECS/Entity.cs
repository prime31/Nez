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
			components.onEntityTransformChanged();
			colliders.onEntityTransformChanged();
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
		/// Adds a Component to the components list. Returns Scene for chaining.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <param name="component">Component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Entity addComponent<T>( T component ) where T : Component
		{
			component.entity = this;
			components.add( component );
			return this;
		}


		/// <summary>
		/// Adds a Component to the components list. Returns Scene for chaining.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public Entity addComponent<T>() where T : Component, new()
		{
			var component = new T();
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
		/// removes the first Component of type T from the components list
		/// </summary>
		/// <param name="component">The Component to remove</param>
		public bool removeComponent<T>() where T : Component
		{
			var comp = getComponent<T>();
			if( comp != null )
			{
				removeComponent( comp );
				return true;
			}

			return false;
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
		/// simple movement helper that should be used if you have Colliders attached to this Entity. It handles keeping the Colliders
		/// in sync in the Physics system.
		/// </summary>
		/// <param name="motion">Motion.</param>
		public void move( Vector2 motion )
		{
			colliders.unregisterAllCollidersWithPhysicsSystem();
			transform.position += motion;
			colliders.registerAllCollidersWithPhysicsSystem();
		}

		#endregion


		public int CompareTo( Entity other )
		{
			return _updateOrder.CompareTo( other._updateOrder );
		}


		public override string ToString()
		{
			return string.Format( "[Entity: name: {0}, tag: {1}, enabled: {2}, depth: {3}]", name, tag, enabled, updateOrder );
		}

	}
}

