using System;


namespace Nez
{
	public class SceneComponent : IComparable<SceneComponent>
	{
		/// <summary>
		/// the scene this SceneComponent is attached to
		/// </summary>
		public Scene Scene;

		/// <summary>
		/// true if the SceneComponent is enabled. Changes in state result in onEnabled/onDisable being called.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool Enabled
		{
			get => _enabled;
			set => SetEnabled(value);
		}

		/// <summary>
		/// update order of the SceneComponents on this Scene
		/// </summary>
		/// <value>The order.</value>
		public int UpdateOrder { get; private set; } = 0;

		bool _enabled = true;


		#region SceneComponent Lifecycle

		/// <summary>
		/// called when this SceneComponent is enabled
		/// </summary>
		public virtual void OnEnabled()
		{
		}


		/// <summary>
		/// called when the this SceneComponent is disabled
		/// </summary>
		public virtual void OnDisabled()
		{
		}


		/// <summary>
		/// called when this SceneComponent is removed from the Scene
		/// </summary>
		public virtual void OnRemovedFromScene()
		{
		}


		/// <summary>
		/// called each frame before the Entities are updated
		/// </summary>
		public virtual void Update()
		{
		}

		#endregion


		#region Fluent setters

		/// <summary>
		/// enables/disables this SceneComponent
		/// </summary>
		/// <returns>The enabled.</returns>
		/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
		public SceneComponent SetEnabled(bool isEnabled)
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


		/// <summary>
		/// sets the updateOrder for the SceneComponent and triggers a sort of the SceneComponents
		/// </summary>
		/// <returns>The update order.</returns>
		/// <param name="updateOrder">Update order.</param>
		public SceneComponent SetUpdateOrder(int updateOrder)
		{
			if (UpdateOrder != updateOrder)
			{
				UpdateOrder = updateOrder;
				Core.Scene._sceneComponents.Sort();
			}

			return this;
		}

		#endregion


		int IComparable<SceneComponent>.CompareTo(SceneComponent other)
		{
			return UpdateOrder.CompareTo(other.UpdateOrder);
		}
	}
}