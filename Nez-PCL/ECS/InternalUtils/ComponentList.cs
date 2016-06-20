using System;
using System.Collections.Generic;
using System.Collections;


namespace Nez
{
	public class ComponentList : IEnumerable<Component>, IEnumerable
	{
		// global updateOrder sort for the IUpdatable list
		static Comparison<IUpdatable> compareUpdatableOrder = ( a, b ) => { return a.updateOrder.CompareTo( b.updateOrder ); };


		Entity _entity;

		/// <summary>
		/// list of components added to the entity
		/// </summary>
		List<Component> _components = new List<Component>();

		/// <summary>
		/// list of all Components that want update called
		/// </summary>
		List<IUpdatable> _updatableComponents = new List<IUpdatable>();

		/// <summary>
		/// The list of components that were added this frame. Used to group the components so we can process them simultaneously
		/// </summary>
		internal List<Component> _componentsToAdd = new List<Component>();

		/// <summary>
		/// The list of components that were marked for removal this frame. Used to group the components so we can process them simultaneously
		/// </summary>
		List<Component> _componentsToRemove = new List<Component>();

		List<Component> _tempBufferList = new List<Component>();

		/// <summary>
		/// flag used to determine if we need to sort our Components this frame
		/// </summary>
		bool _isComponentListUnsorted;


		public ComponentList( Entity entity )
		{
			this._entity = entity;
		}


		public void markEntityListUnsorted()
		{
			_isComponentListUnsorted = true;
		}


		public void add( Component component )
		{
			_componentsToAdd.Add( component );
		}


		public void remove( Component component )
		{
			_componentsToRemove.Add( component );
		}


		/// <summary>
		/// removes all components from the component list immediately
		/// </summary>
		public void removeAllComponents()
		{
			for( var i = 0; i < _components.Count; i++ )
				handleRemove( _components[i] );

			_components.Clear();
			_updatableComponents.Clear();
			_componentsToAdd.Clear();
			_componentsToRemove.Clear();
		}


		internal void deregisterAllComponents()
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				var component = _components[i];

				// deal with renderLayer list if necessary
				if( component is RenderableComponent )
					_entity.scene.renderableComponents.remove( component as RenderableComponent );

				// deal with IUpdatable
				if( component is IUpdatable )
					_updatableComponents.Remove( component as IUpdatable );

				if( Core.entitySystemsEnabled )
				{
					_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ), false );
					_entity.scene.entityProcessors.onComponentRemoved( _entity );
				}
			}
		}


		internal void registerAllComponents()
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				var component = _components[i];
				if( component is RenderableComponent )
					_entity.scene.renderableComponents.add( component as RenderableComponent );

				if( component is IUpdatable )
					_updatableComponents.Add( component as IUpdatable );

				if( Core.entitySystemsEnabled )
				{
					_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ) );
					_entity.scene.entityProcessors.onComponentAdded( _entity );
				}
			}
		}


		public void updateLists()
		{
			// handle removals
			if( _componentsToRemove.Count > 0 )
			{
				for( var i = 0; i < _componentsToRemove.Count; i++ )
				{
					handleRemove( _componentsToRemove[i] );
					_components.Remove( _componentsToRemove[i] );
				}
				_componentsToRemove.Clear();
			}

			// handle additions
			if( _componentsToAdd.Count > 0 )
			{
				for( var i = 0; i < _componentsToAdd.Count; i++ )
				{
					var component = _componentsToAdd[i];
					if( component is RenderableComponent )
						_entity.scene.renderableComponents.add( component as RenderableComponent );

					if( component is IUpdatable )
						_updatableComponents.Add( component as IUpdatable );

					if( Core.entitySystemsEnabled )
					{
						_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ) );
						_entity.scene.entityProcessors.onComponentAdded( _entity );
					}

					_components.Add( component );
					_tempBufferList.Add( component );
				}

				// clear before calling onAddedToEntity in case more Components are added then
				_componentsToAdd.Clear();
				_isComponentListUnsorted = true;

				// now that all components are added to the scene, we loop through again and call onAddedToEntity/onEnabled
				for( var i = 0; i < _tempBufferList.Count; i++ )
				{
					var component = _tempBufferList[i];
					component.onAddedToEntity();

					// component.enabled check both the Entity and the Component
					if( component.enabled )
						component.onEnabled();
				}

				_tempBufferList.Clear();
			}

			if( _isComponentListUnsorted )
			{
				_updatableComponents.Sort( compareUpdatableOrder );
				_isComponentListUnsorted = false;
			}
		}


		void handleRemove( Component component )
		{
			// deal with renderLayer list if necessary
			if( component is RenderableComponent )
				_entity.scene.renderableComponents.remove( component as RenderableComponent );

			// deal with IUpdatable
			if( component is IUpdatable )
				_updatableComponents.Remove( component as IUpdatable );

			if( Core.entitySystemsEnabled )
			{
				_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ), false );
				_entity.scene.entityProcessors.onComponentRemoved( _entity );
			}

			component.onRemovedFromEntity();
			component.entity = null;
		}


		public int Count
		{
			get { return _components.Count; }
		}


		/// <summary>
		/// Gets the first component of type T and returns it. Optionally skips checking un-initialized Components (Components who have not yet had their
		/// onAddedToEntity method called). If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T getComponent<T>( bool onlyReturnInitializedComponents ) where T : Component
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				var component = _components[i];
				if( component is T )
					return component as T;
			}

			// we optionally check the pending components just in case addComponent and getComponent are called in the same frame
			if( !onlyReturnInitializedComponents )
			{
				for( var i = 0; i < _componentsToAdd.Count; i++ )
				{
					var component = _componentsToAdd[i];
					if( component is T )
						return component as T;
				}
			}

			return null;
		}


		/// <summary>
		/// Gets all the components of type T without a List allocation
		/// </summary>
		/// <param name="components">Components.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void getComponents<T>( List<T> components ) where T : class
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				var component = _components[i];
				if( component is T )
					components.Add( component as T );
			}

			// we also check the pending components just in case addComponent and getComponent are called in the same frame
			for( var i = 0; i < _componentsToAdd.Count; i++ )
			{
				var component = _componentsToAdd[i];
				if( component is T )
					components.Add( component as T );
			}
		}


		/// <summary>
		/// Gets all the components of type T. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The components.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> getComponents<T>() where T : class
		{
			var components = ListPool<T>.obtain();
			getComponents( components );

			return components;
		}


		internal void update()
		{
			for( var i = 0; i < _updatableComponents.Count; i++ )
			{
				if( _updatableComponents[i].enabled )
					_updatableComponents[i].update();
			}
		}


		internal void onEntityTransformChanged()
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				if( _components[i].enabled )
					_components[i].onEntityTransformChanged();
			}
		}


		internal void onEntityEnabled()
		{
			for( var i = 0; i < _components.Count; i++ )
				_components[i].onEnabled();
		}


		internal void onEntityDisabled()
		{
			for( var i = 0; i < _components.Count; i++ )
				_components[i].onDisabled();
		}


		internal void debugRender( Graphics graphics )
		{
			for( var i = 0; i < _components.Count; i++ )
			{
				if( _components[i].enabled )
					_components[i].debugRender( graphics );
			}
		}


		#region IEnumerable and array access

		public Component this[int index]
		{
			get { return _components[index]; }
		}


		public IEnumerator<Component> GetEnumerator()
		{
			return _components.GetEnumerator();
		}


		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _components.GetEnumerator();
		}

		#endregion

	}
}

