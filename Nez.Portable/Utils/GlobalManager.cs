namespace Nez
{
	public class GlobalManager
	{
		/// <summary>
		/// true if the GlobalManager is enabled. Changes in state result in onEnabled/onDisable being called.
		/// </summary>
		/// <value><c>true</c> if enabled; otherwise, <c>false</c>.</value>
		public bool enabled
		{
			get => _enabled;
			set => setEnabled( value );
		}

		/// <summary>
		/// enables/disables this GlobalManager
		/// </summary>
		/// <returns>The enabled.</returns>
		/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
		public void setEnabled( bool isEnabled )
		{
			if( _enabled != isEnabled )
			{
				_enabled = isEnabled;

				if( _enabled )
					onEnabled();
				else
					onDisabled();
			}
		}

		bool _enabled;


		#region GlobalManager Lifecycle

		/// <summary>
		/// called when this GlobalManager is enabled
		/// </summary>
		public virtual void onEnabled()
		{ }

		/// <summary>
		/// called when the this GlobalManager is disabled
		/// </summary>
		public virtual void onDisabled()
		{ }

		/// <summary>
		/// called each frame before Scene.update
		/// </summary>
		public virtual void update()
		{ }

		#endregion


	}
}
