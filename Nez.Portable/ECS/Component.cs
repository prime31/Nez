using System;
using System.Runtime.CompilerServices;


namespace Nez
{
	/// <summary>
	/// Execution order:
	/// - OnAddedToEntity
	/// - OnEnabled
	///
	/// Removal:
	/// - OnRemovedFromEntity
	///
	/// </summary>
	public class Component : IComparable<Component>
	{
		/// <summary>
		/// the Entity this Component is attached to
		/// </summary>
		public Entity Entity;

		/// <summary>
		/// shortcut to entity.transform
		/// </summary>
		/// <value>The transform.</value>
		public Transform Transform
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Entity.Transform;
		}

		/// <summary>
		/// true if the Component is enabled and the Entity is enabled. When enabled this Components lifecycle methods will be called.
		/// Changes in state result in onEnabled/onDisable being called.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled
		{
			get => Entity != null ? Entity.Enabled && _enabled : _enabled;
			set => SetEnabled(value);
		}

		/// <summary>
		/// update order of the Components on this Entity
		/// </summary>
		/// <value>The order.</value>
		public int UpdateOrder
		{
			get => _updateOrder;
			set => SetUpdateOrder(value);
		}

		bool _enabled = true;
		internal int _updateOrder = 0;


		#region Component Lifecycle

		/// <summary>
		/// called when this Component has had its Entity assigned but it is NOT yet added to the live Components list of the Entity yet. Useful
		/// for things like physics Components that need to access the Transform to modify collision body properties.
		/// </summary>
		public virtual void Initialize()
		{ }

		/// <summary>
		/// Called when this component is added to a scene after all pending component changes are committed. At this point, the Entity field
		/// is set and the Entity.Scene is also set.
		/// </summary>
		public virtual void OnAddedToEntity()
		{ }

		/// <summary>
		/// Called when this component is removed from its entity. Do all cleanup here.
		/// </summary>
		public virtual void OnRemovedFromEntity()
		{ }

		/// <summary>
		/// called when the entity's position changes. This allows components to be aware that they have moved due to the parent
		/// entity moving.
		/// </summary>
		public virtual void OnEntityTransformChanged(Transform.Component comp)
		{ }

		public virtual void DebugRender(Batcher batcher)
		{ }

		/// <summary>
		/// called when the parent Entity or this Component is enabled
		/// </summary>
		public virtual void OnEnabled()
		{ }

		/// <summary>
		/// called when the parent Entity or this Component is disabled
		/// </summary>
		public virtual void OnDisabled()
		{ }

		#endregion

		#region Fluent setters

		public Component SetEnabled(bool isEnabled)
		{
			if (_enabled != isEnabled)
			{
				_enabled = isEnabled;

				if (_enabled)
					OnEnabled();
				else
					OnDisabled();
			}

			return this;
		}

		public Component SetUpdateOrder(int updateOrder)
		{
			if (_updateOrder != updateOrder)
			{
				_updateOrder = updateOrder;
				if (Entity != null)
					Entity.Components.MarkEntityListUnsorted();
			}

			return this;
		}

		#endregion

		/// <summary>
		/// creates a clone of this Component. The default implementation is just a MemberwiseClone so if a Component has object references
		/// that need to be cloned this method should be overriden.
		/// </summary>
		public virtual Component Clone()
		{
			var component = MemberwiseClone() as Component;
			component.Entity = null;

			return component;
		}

		public int CompareTo(Component other) => _updateOrder.CompareTo(other._updateOrder);


		public override string ToString() => $"[Component: type: {GetType()}, updateOrder: {UpdateOrder}]";
	}
}