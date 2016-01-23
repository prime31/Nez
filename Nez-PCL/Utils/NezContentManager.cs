using System;
using System.Threading;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;


namespace Nez.Systems
{
	/// <summary>
	/// ContentManager subclass that also manages Effects from ogl files. Adds asynchronous loading of assets as well.
	/// </summary>
	public class NezContentManager : Microsoft.Xna.Framework.Content.ContentManager
	{
		Dictionary<string,Effect> _loadedEffects = new Dictionary<string,Effect>();

		
		public NezContentManager( IServiceProvider serviceProvider, string rootDirectory ) : base( serviceProvider, rootDirectory )
		{}


		public NezContentManager( IServiceProvider serviceProvider ) : base( serviceProvider )
		{}


		public NezContentManager() : base( Core._instance.Services, Core._instance.Content.RootDirectory )
		{}


		/// <summary>
		/// loads an ogl effect directly from file and handles disposing of it when the ContentManager is disposed. Name should the the path
		/// relative to the Content folder.
		/// </summary>
		/// <returns>The effect.</returns>
		/// <param name="name">Name.</param>
		public Effect LoadEffect( string name )
		{
			// make sure the effect 
			if( !name.StartsWith( RootDirectory ) )
				name = RootDirectory + "/" + name;

			// check the cache first
			if( _loadedEffects.ContainsKey( name ) )
				return _loadedEffects[name];
			
			var graphicsDeviceService = (IGraphicsDeviceService)ServiceProvider.GetService( typeof( IGraphicsDeviceService ) );
			var graphicsDevice = graphicsDeviceService.GraphicsDevice;

			byte[] bytes;
			using( var stream = TitleContainer.OpenStream( name ) )
			{
				bytes = new byte[stream.Length];
				stream.Read( bytes, 0, bytes.Length );
			}

			var effect = new Effect( graphicsDevice, bytes );
			_loadedEffects[name] = effect;

			return effect;
		}


		/// <summary>
		/// loads an asset on a background thread with optional callback for when it is loaded
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void LoadAsync<T>( string assetName, Action<T> onLoaded = null )
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
		public void LoadAsync<T>( string assetName, Action<object,T> onLoaded = null, object context = null )
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
		public void LoadAsync<T>( string[] assetNames, Action onLoaded = null )
		{
			ThreadPool.QueueUserWorkItem( s =>
			{
				for( var i = 0; i < assetNames.Length; i++ )
					Load<T>( assetNames[i] );

				if( onLoaded != null )
					onLoaded();
			} );
		}


		/// <summary>
		/// checks to see if an asset with assetName is loaded
		/// </summary>
		/// <returns><c>true</c> if this instance is asset loaded the specified assetName; otherwise, <c>false</c>.</returns>
		/// <param name="assetName">Asset name.</param>
		public bool IsAssetLoaded( string assetName )
		{
			return LoadedAssets.ContainsKey( assetName );
		}


		/// <summary>
		/// provides a string suitable for logging with all the currently loaded assets and effects
		/// </summary>
		/// <returns>The loaded assets.</returns>
		internal string logLoadedAssets()
		{
			var builder = new StringBuilder();
			foreach( var asset in LoadedAssets.Keys )
			{
				builder.AppendFormat( "{0}: ({1})\n", asset, LoadedAssets[asset].GetType().Name );
			}

			foreach( var asset in _loadedEffects.Keys )
			{
				builder.AppendFormat( "{0}: ({1})\n", asset, _loadedEffects[asset].GetType().Name );
			}

			return builder.ToString();
		}


		/// <summary>
		/// override that disposes of all loaded Effects
		/// </summary>
		public override void Unload()
		{
			base.Unload();

			foreach( var key in _loadedEffects.Keys )
				_loadedEffects[key].Dispose();

			_loadedEffects.Clear();
		}

	}
}

