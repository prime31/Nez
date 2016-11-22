using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


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
		public string name;

		/// <summary>
		/// encapsulates the Entity's position/rotation/scale and allows setting up a hieararchy
		/// </summary>
		public readonly Transform transform;

		/// <summary>
		/// list of all the components currently attached to this entity
		/// </summary>
		public readonly ComponentList components;

		/// <summary>
		/// use this however you want to. It can later be used to query the scene for all Entities with a specific tag
		/// </summary>
		public int tag
		{
			get { return _tag; }
			set { setTag( value ); }
		}

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
			get { return _enabled; }
			set { setEnabled( value ); }
		}

		/// <summary>
		/// update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
		/// </summary>
		/// <value>The order.</value>
		public int updateOrder
		{
			get { return _updateOrder; }
			set { setUpdateOrder( value ); }
		}

		internal BitSet componentBits;

		/// <summary>
		/// flag indicating if destroy was called on this Entity
		/// </summary>
		internal bool _isDestroyed;

		int _tag = 0;
		bool _enabled = true;
		internal int _updateOrder = 0;

		#endregion


		#region Transform passthroughs

		public Transform parent
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.parent; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setParent( value ); }
		}

		public int childCount
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.childCount; }
		}

		public Vector2 position
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.position; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setPosition( value ); }
		}

		public Vector2 localPosition
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.localPosition; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setLocalPosition( value ); }
		}

		public float rotation
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.rotation; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setRotation( value ); }
		}

		public float rotationDegrees
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.rotationDegrees; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setRotationDegrees( value ); }
		}

		public float localRotation
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.localRotation; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setLocalRotation( value ); }
		}

		public float localRotationDegrees
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.localRotationDegrees; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setLocalRotationDegrees( value ); }
		}

		public Vector2 scale
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.scale; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setScale( value ); }
		}

		public Vector2 localScale
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.localScale; }
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			set { transform.setLocalScale( value ); }
		}


		public Matrix2D worldInverseTransform
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.worldInverseTransform; }
		}


		public Matrix2D localToWorldTransform
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.localToWorldTransform; }
		}


		public Matrix2D worldToLocalTransform
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return transform.worldToLocalTransform; }
		}

		#endregion


		public Entity()
		{
			components = new ComponentList( this );
			transform = new Transform( this );

			if( Core.entitySystemsEnabled )
				componentBits = new BitSet();
		}


		public Entity( string name ) : this()
		{
			this.name = name;
		}


		internal void onTransformChanged( Transform.Component comp )
		{
			// notify our children of our changed position
			components.onEntityTransformChanged( comp );
		}


		#region Fluent setters

		/// <summary>
		/// sets the tag for the Entity
		/// </summary>
		/// <returns>The tag.</returns>
		/// <param name="tag">Tag.</param>
		public Entity setTag( int tag )
		{
			if( _tag != tag )
			{
				// we only call through to the entityTagList if we already have a scene. if we dont have a scene yet we will be
				// added to the entityTagList when we do
				if( scene != null )
					scene.entities.removeFromTagList( this );
				_tag = tag;
				if( scene != null )
					scene.entities.addToTagList( this );
			}

			return this;
		}


		/// <summary>
		/// sets the enabled state of the Entity. When disabled colliders are removed from the Physics system and components methods will not be called
		/// </summary>
		/// <returns>The enabled.</returns>
		/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
		public Entity setEnabled( bool isEnabled )
		{
			if( _enabled != isEnabled )
			{
				_enabled = isEnabled;

				if( _enabled )
					components.onEntityEnabled();
				else
					components.onEntityDisabled();
			}

			return this;
		}


		/// <summary>
		/// sets the update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
		/// </summary>
		/// <returns>The update order.</returns>
		/// <param name="updateOrder">Update order.</param>
		public Entity setUpdateOrder( int updateOrder )
		{
			if( _updateOrder != updateOrder )
			{
				_updateOrder = updateOrder;
				if( scene != null )
				{
					scene.entities.markEntityListUnsorted();
					scene.entities.markTagUnsorted( tag );
				}
			}

			return this;
		}

		#endregion


		/// <summary>
		/// removes the Entity from the scene and destroys all children
		/// </summary>
		public void destroy()
		{
			_isDestroyed = true;
			scene.entities.remove( this );
			transform.parent = null;

			// destroy any children we have
			for( var i = transform.childCount - 1; i >= 0; i-- )
			{
				var child = transform.getChild( i );
				child.entity.destroy();
			}
		}


		/// <summary>
		/// detaches the Entity from the scene.
		/// the following lifecycle method will be called on the Entity: onRemovedFromScene
		/// the following lifecycle method will be called on the Components: onRemovedFromEntity
		/// </summary>
		public void detachFromScene()
		{
			scene.entities.remove( this );
			components.deregisterAllComponents();

			for( var i = 0; i < transform.childCount; i++ )
				transform.getChild( i ).entity.detachFromScene();
		}


		/// <summary>
		/// attaches an Entity that was previously detached to a new scene
		/// </summary>
		/// <param name="newScene">New scene.</param>
		public void attachToScene( Scene newScene )
		{
			scene = newScene;
			newScene.entities.add( this );
			components.registerAllComponents();

			for( var i = 0; i < transform.childCount; i++ )
				transform.getChild( i ).entity.attachToScene( newScene );
		}


		/// <summary>
		/// creates a deep clone of this Entity. Subclasses can override this method to copy any custom fields. When overriding,
		/// the copyFrom method should be called which will clone all Components, Colliders and Transform children for you. Note that cloned
		/// objects will not be added to any Scene! You must add them yourself!
		/// </summary>
		public Entity clone( Vector2 position = default( Vector2 ) )
		{
			var entity = Activator.CreateInstance( GetType() ) as Entity;
			entity.name = name + "(clone)";
			entity.copyFrom( this );
			entity.transform.position = position;

			return entity;
		}


		/// <summary>
		/// copies the properties, components and colliders of Entity to this instance
		/// </summary>
		/// <param name="entity">Entity.</param>
		protected void copyFrom( Entity entity )
		{
			// Entity fields
			tag = entity.tag;
			updateInterval = entity.updateInterval;
			updateOrder = entity.updateOrder;
			enabled = entity.enabled;

			transform.scale = entity.transform.scale;
			transform.rotation = entity.transform.rotation;

			// clone Components
			for( var i = 0; i < entity.components.count; i++ )
				addComponent( entity.components[i].clone() );
			for( var i = 0; i < entity.components._componentsToAdd.Count; i++ )
				addComponent( entity.components._componentsToAdd[i].clone() );

			// clone any children of the Entity.transform
			for( var i = 0; i < entity.transform.childCount; i++ )
			{
				var child = entity.transform.getChild( i ).entity;

				var childClone = child.clone();
				childClone.transform.copyFrom( child.transform );
				childClone.transform.parent = transform;
			}
		}


		#region Entity lifecycle methods

		/// <summary>
		/// Called when this entity is added to a scene after all pending entity changes are committed
		/// </summary>
		public virtual void onAddedToScene()
		{}


		/// <summary>
		/// Called when this entity is removed from a scene
		/// </summary>
		public virtual void onRemovedFromScene()
		{
			// if we were destroyed, remove our components. If we were just detached we need to keep our components on the Entity.
			if( _isDestroyed )
				components.removeAllComponents();
		}


		/// <summary>
		/// called each frame as long as the Entity is enabled
		/// </summary>
		public virtual void update()
		{
			components.update();
		}


		/// <summary>
		/// called if Core.debugRenderEnabled is true by the default renderers. Custom renderers can choose to call it or not.
		/// </summary>
		/// <param name="graphics">Graphics.</param>
		public virtual void debugRender( Graphics graphics )
		{
			components.debugRender( graphics );
		}

		#endregion


		#region Component Management

		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <param name="component">Component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T addComponent<T>( T component ) where T : Component
		{
			component.entity = this;
			components.add( component );
			component.initialize();
			return component;
		}


		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T addComponent<T>() where T : Component, new()
		{
			var component = new T();
			component.entity = this;
			components.add( component );
			component.initialize();
			return component;
		}


		/// <summary>
		/// Gets the first component of type T and returns it. If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getComponent<T>() where T : Component
		{
			return components.getComponent<T>( false );
		}


		/// <summary>
		/// Gets the first Component of type T and returns it. If no Component is found the Component will be created.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getOrCreateComponent<T>() where T : Component, new()
		{
			var comp = components.getComponent<T>( true );
			if( comp == null )
				comp = addComponent<T>();

			return comp;
		}


		/// <summary>
		/// Gets the first component of type T and returns it optionally skips checking un-initialized Components (Components who have not yet had their
		/// onAddedToEntity method called). If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getComponent<T>( bool onlyReturnInitializedComponents ) where T : Component
		{
			return components.getComponent<T>( onlyReturnInitializedComponents );
		}


		/// <summary>
		/// Gets all the components of type T without a List allocation
		/// </summary>
		/// <param name="componentList">Component list.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void getComponents<T>( List<T> componentList ) where T : class
		{
			components.getComponents( componentList );
		}


		/// <summary>
		/// Gets all the components of type T. The returned List can be put back in the pool via ListPool.free.
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
			for( var i = 0; i < components.count; i++ )
				removeComponent( components[i] );
		}

		#endregion


		#region Collider management

		[Obsolete( "Colliders are now Components. Use addComponent instead." )]
		public T addCollider<T>( T collider ) where T : Collider
		{
			return addComponent( collider );
		}


		[Obsolete( "Colliders are now Components. Use removeComponent instead." )]
		public void removeCollider( Collider collider )
		{
			removeComponent( collider );
		}


		[Obsolete( "Colliders are now Components. Use the normal Component methods to manage Colliders." )]
		public void removeAllColliders()
		{
			throw new NotImplementedException();
		}


		[Obsolete( "Colliders are now Components. Use the normal Component methods to manage Colliders." )]
		public T getCollider<T>( bool onlyReturnInitializedColliders = false ) where T : Collider
		{
			return getComponent<T>( onlyReturnInitializedColliders );
		}


		[Obsolete( "Colliders are now Components. Use the normal Component methods to manage Colliders." )]
		public void getColliders( List<Collider> colliders )
		{
			getComponents( colliders );
		}


		[Obsolete( "Colliders are now Components. Use the normal Component methods to manage Colliders." )]
		public List<Collider> getColliders()
		{
			return getComponents<Collider>();
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

