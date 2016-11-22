using System;
using System.Runtime.CompilerServices;


namespace Nez
{
	/// <summary>
	/// Execution order:
	/// - onAddedToEntity
	/// - onAwake (all other components added this frame will have been added to the Entity at this point
	/// - onEnabled
	/// 
	/// Removal:
	/// - onRemovedFromEntity
	/// 
	/// </summary>
	public class Component : IComparable<Component>
	{
		/// <summary>
		/// the Entity this Component is attached to
		/// </summary>
		public Entity entity;

		/// <summary>
		/// shortcut to entity.transform
		/// </summary>
		/// <value>The transform.</value>
		public Transform transform
		{
			[MethodImpl( MethodImplOptions.AggressiveInlining )]
			get { return entity.transform; }
		}

		/// <summary>
		/// true if the Component is enabled and the Entity is enabled. When enabled this Components lifecycle methods will be called.
		/// Changes in state result in onEnabled/onDisable being called.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool enabled
		{
			get { return entity != null ? entity.enabled && _enabled : _enabled; }
			set { setEnabled( value ); }
		}

		/// <summary>
		/// update order of the Components on this Entity
		/// </summary>
		/// <value>The order.</value>
		public int updateOrder { get { return _updateOrder; } }

		bool _enabled = true;
		internal int _updateOrder = 0;


		#region Component Lifecycle

		/// <summary>
		/// called when this Component has had its Entity assigned but it is NOT yet added to the live Components list of the Entity yet. Useful
		/// for things like physics Components that need to access the Transform to modify collision body properties.
		/// </summary>
		public virtual void initialize()
		{}


		/// <summary>
		/// Called when this component is added to a scene after all pending component changes are committed. At this point, the entity field
		/// is set and the entity.scene is also set.
		/// </summary>
		public virtual void onAddedToEntity()
		{}


		/// <summary>
		/// Called when this component is removed from its entity. Do all cleanup here.
		/// </summary>
		public virtual void onRemovedFromEntity()
		{}


		/// <summary>
		/// called when the entity's position changes. This allows components to be aware that they have moved due to the parent
		/// entity moving.
		/// </summary>
		public virtual void onEntityTransformChanged( Transform.Component comp )
		{}


		public virtual void debugRender( Graphics graphics )
		{}


		/// <summary>
		/// called when the parent Entity or this Component is enabled
		/// </summary>
		public virtual void onEnabled()
		{}


		/// <summary>
		/// called when the parent Entity or this Component is disabled
		/// </summary>
		public virtual void onDisabled()
		{}

		#endregion


		#region Fluent setters

		public Component setEnabled( bool isEnabled )
		{
			if( _enabled != isEnabled )
			{
				_enabled = isEnabled;

				if( _enabled )
					onEnabled();
				else
					onDisabled();
			}
			return this;
		}


		public Component setUpdateOrder( int updateOrder )
		{
			if( _updateOrder != updateOrder )
			{
				_updateOrder = updateOrder;
				if( entity != null )
					entity.components.markEntityListUnsorted();
			}
			return this;
		}

		#endregion


		/// <summary>
		/// creates a clone of this Component. The default implementation is just a MemberwiseClone so if a Component has object references
		/// that need to be cloned this method should be overriden.
		/// </summary>
		public virtual Component clone()
		{
			var component = MemberwiseClone() as Component;
			component.entity = null;

			return component;
		}


		public int CompareTo( Component other )
		{
			return _updateOrder.CompareTo( other._updateOrder );
		}


		public override string ToString()
		{
			return string.Format( "[Component: type: {0}, updateOrder: {1}]", this.GetType(), updateOrder );
		}

	}
}

