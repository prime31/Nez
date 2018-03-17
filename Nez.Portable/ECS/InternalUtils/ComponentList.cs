using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace Nez
{
	public class ComponentList
	{
		// global updateOrder sort for the IUpdatable list
		static IUpdatableComparer compareUpdatableOrder = new IUpdatableComparer();

		Entity _entity;

		/// <summary>
		/// list of components added to the entity
		/// </summary>
		FastList<Component> _components = new FastList<Component>();

		/// <summary>
		/// list of all Components that want update called
		/// </summary>
		FastList<IUpdatable> _updatableComponents = new FastList<IUpdatable>();

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
			_entity = entity;
		}


		#region array access

		public int count { get { return _components.length; } }

		public Component this[int index] { get { return _components.buffer[index]; } }

		#endregion


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
			Debug.warnIf( _componentsToRemove.Contains( component ), "You are trying to remove a Component ({0}) that you already removed", component );

			// this may not be a live Component so we have to watch out for if it hasnt been processed yet but it is being removed in the same frame
			if( _componentsToAdd.Contains( component ) )
			{
				_componentsToAdd.Remove( component );
				return;
			}
			
			_componentsToRemove.Add( component );
		}


		/// <summary>
		/// removes all components from the component list immediately
		/// </summary>
		public void removeAllComponents()
		{
			for( var i = 0; i < _components.length; i++ )
				handleRemove( _components.buffer[i] );

			_components.clear();
			_updatableComponents.clear();
			_componentsToAdd.Clear();
			_componentsToRemove.Clear();
		}


		internal void deregisterAllComponents()
		{
			for( var i = 0; i < _components.length; i++ )
			{
				var component = _components.buffer[i];

				// deal with renderLayer list if necessary
				if( component is RenderableComponent )
					_entity.scene.renderableComponents.remove( component as RenderableComponent );

				// deal with IUpdatable
				if( component is IUpdatable )
					_updatableComponents.remove( component as IUpdatable );

				if( Core.entitySystemsEnabled )
				{
					_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ), false );
					_entity.scene.entityProcessors.onComponentRemoved( _entity );
				}
			}
		}


		internal void registerAllComponents()
		{
			for( var i = 0; i < _components.length; i++ )
			{
				var component = _components.buffer[i];
				if( component is RenderableComponent )
					_entity.scene.renderableComponents.add( component as RenderableComponent );

				if( component is IUpdatable )
					_updatableComponents.add( component as IUpdatable );

				if( Core.entitySystemsEnabled )
				{
					_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ) );
					_entity.scene.entityProcessors.onComponentAdded( _entity );
				}
			}
		}


		/// <summary>
		/// handles any Components that need to be removed or added
		/// </summary>
		void updateLists()
		{
			// handle removals
			if( _componentsToRemove.Count > 0 )
			{
				for( int i = 0, count = _componentsToRemove.Count; i < count; i++ )
				{
					handleRemove( _componentsToRemove[i] );
					_components.remove( _componentsToRemove[i] );
				}
				_componentsToRemove.Clear();
			}

			// handle additions
			if( _componentsToAdd.Count > 0 )
			{
				for( int i = 0, count = _componentsToAdd.Count; i < count; i++ )
				{
					var component = _componentsToAdd[i];
					if( component is RenderableComponent )
						_entity.scene.renderableComponents.add( component as RenderableComponent );

					if( component is IUpdatable )
						_updatableComponents.add( component as IUpdatable );

					if( Core.entitySystemsEnabled )
					{
						_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ) );
						_entity.scene.entityProcessors.onComponentAdded( _entity );
					}

					_components.add( component );
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

					// component.enabled checks both the Entity and the Component
					if( component.enabled )
						component.onEnabled();
				}

				_tempBufferList.Clear();
			}

			if( _isComponentListUnsorted )
			{
				_updatableComponents.sort( compareUpdatableOrder );
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
				_updatableComponents.remove( component as IUpdatable );

			if( Core.entitySystemsEnabled )
			{
				_entity.componentBits.set( ComponentTypeManager.getIndexFor( component.GetType() ), false );
				_entity.scene.entityProcessors.onComponentRemoved( _entity );
			}

			component.onRemovedFromEntity();
			component.entity = null;
		}


		/// <summary>
		/// Gets the first component of type T and returns it. Optionally skips checking un-initialized Components (Components who have not yet had their
		/// onAddedToEntity method called). If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public T getComponent<T>( bool onlyReturnInitializedComponents ) where T : Component
		{
			for( var i = 0; i < _components.length; i++ )
			{
				var component = _components.buffer[i];
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public void getComponents<T>( List<T> components ) where T : class
		{
			for( var i = 0; i < _components.length; i++ )
			{
				var component = _components.buffer[i];
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
		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		public List<T> getComponents<T>() where T : class
		{
			var components = ListPool<T>.obtain();
			getComponents( components );

			return components;
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal void update()
		{
			updateLists();
			for( var i = 0; i < _updatableComponents.length; i++ )
			{
				if( _updatableComponents.buffer[i].enabled && ( _updatableComponents.buffer[i] as Component ).enabled )
					_updatableComponents.buffer[i].update();
			}
		}


		[MethodImpl( MethodImplOptions.AggressiveInlining )]
		internal void onEntityTransformChanged( Transform.Component comp )
		{
			for( var i = 0; i < _components.length; i++ )
			{
				if( _components.buffer[i].enabled )
					_components.buffer[i].onEntityTransformChanged( comp );
			}

			for( var i = 0; i < _componentsToAdd.Count; i++ )
			{
				if( _componentsToAdd[i].enabled )
					_componentsToAdd[i].onEntityTransformChanged( comp );
			}
		}


		internal void onEntityEnabled()
		{
			for( var i = 0; i < _components.length; i++ )
				_components.buffer[i].onEnabled();
		}


		internal void onEntityDisabled()
		{
			for( var i = 0; i < _components.length; i++ )
				_components.buffer[i].onDisabled();
		}


		internal void debugRender( Graphics graphics )
		{
			for( var i = 0; i < _components.length; i++ )
			{
				if( _components.buffer[i].enabled )
					_components.buffer[i].debugRender( graphics );
			}
		}

	}
}

