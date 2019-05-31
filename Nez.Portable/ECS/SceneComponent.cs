using System;


namespace Nez
{
	public class SceneComponent : IComparable<SceneComponent>
	{
		/// <summary>
		/// the scene this SceneComponent is attached to
		/// </summary>
		public Scene scene;
	    
		/// <summary>
		/// true if the SceneComponent is enabled. Changes in state result in onEnabled/onDisable being called.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool enabled
		{
			get { return _enabled; }
			set { setEnabled( value ); }
		}

		/// <summary>
		/// update order of the SceneComponents on this Scene
		/// </summary>
		/// <value>The order.</value>
		public int updateOrder { get; private set; } = 0;

		bool _enabled = true;


		#region SceneComponent Lifecycle

		/// <summary>
		/// called when this SceneComponent is enabled
		/// </summary>
		public virtual void onEnabled()
		{ }


		/// <summary>
		/// called when the this SceneComponent is disabled
		/// </summary>
		public virtual void onDisabled()
		{ }


		/// <summary>
		/// called when this SceneComponent is removed from the Scene
		/// </summary>
		public virtual void onRemovedFromScene()
		{}


		/// <summary>
		/// called each frame before the Entities are updated
		/// </summary>
		public virtual void update()
		{}

		#endregion


		#region Fluent setters

		/// <summary>
		/// enables/disables this SceneComponent
		/// </summary>
		/// <returns>The enabled.</returns>
		/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
		public SceneComponent setEnabled( bool isEnabled )
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


		/// <summary>
		/// sets the updateOrder for the SceneComponent and triggers a sort of the SceneComponents
		/// </summary>
		/// <returns>The update order.</returns>
		/// <param name="updateOrder">Update order.</param>
		public SceneComponent setUpdateOrder( int updateOrder )
		{
			if( this.updateOrder != updateOrder )
			{
				this.updateOrder = updateOrder;
				Core.scene._sceneComponents.sort();
			}
			return this;
		}

		#endregion


		int IComparable<SceneComponent>.CompareTo( SceneComponent other )
		{
			return updateOrder.CompareTo( other.updateOrder );
		}

	}
}
