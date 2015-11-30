using System;
using System.Threading;
using System.Text;


namespace Nez.Systems
{
	public class NezContentManager : Microsoft.Xna.Framework.Content.ContentManager
	{
		public NezContentManager( IServiceProvider serviceProvider, string rootDirectory ) : base( serviceProvider, rootDirectory )
		{}


		public NezContentManager( IServiceProvider serviceProvider ) : base( serviceProvider )
		{}


		public NezContentManager() : base( Core._instance.Services, Core._instance.Content.RootDirectory )
		{}


		/// <summary>
		/// loads an asset on a background thread with optional callback for when it is loaded
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual void LoadAsync<T>( string assetName, Action<T> onLoaded = null )
		{
			ThreadPool.QueueUserWorkItem( s =>
			{
				var asset = Load<T>( assetName );

				if( onLoaded != null )
					onLoaded( asset );
			} );
		}


		/// <summary>
		/// loads an asset on a background thread with optional callback that includes a context parameter for when it is loaded
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <param name="context">Context.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual void LoadAsync<T>( string assetName, Action<object,T> onLoaded = null, object context = null )
		{
			ThreadPool.QueueUserWorkItem( state =>
			{
				var asset = Load<T>( assetName );

				if( onLoaded != null )
					onLoaded( state, asset );
			}, context );
		}


		/// <summary>
		/// loads a group of assets on a background thread with optional callback for when it is loaded
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public virtual void LoadAsync<T>( string[] assetNames, Action onLoaded = null )
		{
			ThreadPool.QueueUserWorkItem( s =>
			{
				for( var i = 0; i < assetNames.Length; i++ )
					Load<T>( assetNames[i] );

				if( onLoaded != null )
					onLoaded();
			} );
		}


		public string logLoadedAssets()
		{
			var builder = new StringBuilder();

			foreach( var asset in LoadedAssets.Keys )
			{
				builder.AppendFormat( "{0}: ({1})\n", asset, LoadedAssets[asset].GetType().Name );
			}

			return builder.ToString();
		}

	}
}

