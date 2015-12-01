using System;


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
	public class Component
	{
		public Entity entity;

		bool _enabled = true;
		public bool enabled
		{
			get { return entity != null ? entity.enabled && _enabled : _enabled; }
			set
			{
				if( _enabled != value )
				{
					_enabled = value;

					if( _enabled )
						onEnabled();
					else
						onDisabled();
				}
			}
		}


		public Component()
		{}


		/// <summary>
		/// Called when this entity is added to a scene
		/// </summary>
		/// <param name="entity">Entity.</param>
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
		public virtual void onEntityPositionChanged()
		{}


		/// <summary>
		/// called in the same frame as onAddedToEntity but after all pending component changes are committed
		/// </summary>
		public virtual void onAwake()
		{}


		public virtual void update()
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

	}
}

