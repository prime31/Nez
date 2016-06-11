using System;
using System.Threading;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;


namespace Nez.Systems
{
	/// <summary>
	/// ContentManager subclass that also manages Effects from ogl files. Adds asynchronous loading of assets as well.
	/// </summary>
	public class NezContentManager : ContentManager
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
		/// relative to the Content folder or including the Content folder.
		/// </summary>
		/// <returns>The effect.</returns>
		/// <param name="name">Name.</param>
		public Effect loadEffect( string name )
		{
			return loadEffect<Effect>( name );
		}


		/// <summary>
		/// loads an embedded Nez effect. These are any of the Effect subclasses in the Nez/Graphics/Effects folder.
		/// Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect duplication.
		/// </summary>
		/// <returns>The nez effect.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T loadNezEffect<T>() where T : Effect, new()
		{
			var cacheKey = typeof( T ).Name + "-" + Utils.randomString( 5 );
			var effect = new T();
			_loadedEffects[cacheKey] = effect;

			return effect;
		}


		/// <summary>
		/// loads an ogl effect directly from file and handles disposing of it when the ContentManager is disposed. Name should the the path
		/// relative to the Content folder or including the Content folder. Effects must have a constructor that accepts GraphicsDevice and
		/// byte[]. Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect duplication.
		/// </summary>
		/// <returns>The effect.</returns>
		/// <param name="name">Name.</param>
		public T loadEffect<T>( string name ) where T : Effect
		{
			// make sure the effect has the proper root directory
			if( !name.StartsWith( RootDirectory ) )
				name = RootDirectory + "/" + name;

			var bytes = EffectResource.getFileResourceBytes( name );

			return loadEffect<T>( name, bytes );
		}


		/// <summary>
		/// loads an ogl effect directly from its bytes and handles disposing of it when the ContentManager is disposed. Name should the the path
		/// relative to the Content folder or including the Content folder. Effects must have a constructor that accepts GraphicsDevice and
		/// byte[]. Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect duplication.
		/// </summary>
		/// <returns>The effect.</returns>
		/// <param name="name">Name.</param>
		internal T loadEffect<T>( string name, byte[] effectCode ) where T : Effect
		{
			var graphicsDeviceService = (IGraphicsDeviceService)ServiceProvider.GetService( typeof( IGraphicsDeviceService ) );
			var graphicsDevice = graphicsDeviceService.GraphicsDevice;

			var effect = Activator.CreateInstance( typeof( T ), graphicsDevice, effectCode ) as T;
			effect.Name = typeof( T ).Name + "-" + Utils.randomString( 5 );
			_loadedEffects[effect.Name] = effect;

			return effect;
		}


		/// <summary>
		/// loads and manages any Effect that is built-in to MonoGame such as BasicEffect, AlphaTestEffect, etc. Note that this will
		/// return a unique instance if you attempt to load the same Effect twice. If you intend to use the same Effect in multiple locations
		/// keep a reference to it and use it directly.
		/// </summary>
		/// <returns>The mono game effect.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T loadMonoGameEffect<T>() where T : Effect
		{
			var graphicsDeviceService = (IGraphicsDeviceService)ServiceProvider.GetService( typeof( IGraphicsDeviceService ) );
			var graphicsDevice = graphicsDeviceService.GraphicsDevice;

			var effect = Activator.CreateInstance( typeof( T ), graphicsDevice ) as T;
			effect.Name = typeof( T ).Name + "-" + Utils.randomString( 5 );
			_loadedEffects[effect.Name] = effect;

			return effect;
		}


		/// <summary>
		/// loads an asset on a background thread with optional callback for when it is loaded. The callback will occur on the main thread.
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void loadAsync<T>( string assetName, Action<T> onLoaded = null )
		{
			var syncContext = SynchronizationContext.Current;
			Task.Run( () =>
			{
				var asset = Load<T>( assetName );

				// if we have a callback do it on the main thread
				if( onLoaded != null )
				{
					syncContext.Post( d =>
					{
						onLoaded( asset );
					}, null );
				}
			} );
		}


		/// <summary>
		/// loads an asset on a background thread with optional callback that includes a context parameter for when it is loaded.
		/// The callback will occur on the main thread.
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <param name="context">Context.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void loadAsync<T>( string assetName, Action<object,T> onLoaded = null, object context = null )
		{
			var syncContext = SynchronizationContext.Current;
			Task.Run( () =>
			{
				var asset = Load<T>( assetName );

				if( onLoaded != null )
				{
					syncContext.Post( d =>
					{
						onLoaded( context, asset );
					}, null );
				}
			} );
		}


		/// <summary>
		/// loads a group of assets on a background thread with optional callback for when it is loaded
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void loadAsync<T>( string[] assetNames, Action onLoaded = null )
		{
			var syncContext = SynchronizationContext.Current;
			Task.Run( () =>
			{
				for( var i = 0; i < assetNames.Length; i++ )
					Load<T>( assetNames[i] );

				// if we have a callback do it on the main thread
				if( onLoaded != null )
				{
					syncContext.Post( d =>
					{
						onLoaded();
					}, null );
				}
			} );
		}


		/// <summary>
		/// removes assetName from LoadedAssets and Disposes of it. Note that this method uses reflection to get at the private ContentManager
		/// disposeableAssets List.
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void unloadAsset<T>( string assetName ) where T : class, IDisposable
		{
			if( isAssetLoaded( assetName ) )
			{
				try
				{
					var fieldInfo = typeof( ContentManager ).GetRuntimeField( "disposableAssets" );
					var assets = fieldInfo.GetValue( this ) as List<IDisposable>;

					for( var i = 0; i < assets.Count; i++ )
					{
						var typedAsset = assets[i] as T;
						if( typedAsset != null )
						{
							typedAsset.Dispose();
							assets.RemoveAt( i );

							#if FNA
							fieldInfo = typeof( ContentManager ).GetRuntimeField( "loadedAssets" );
							var LoadedAssets = fieldInfo.GetValue( this ) as Dictionary<string, object>;
							#endif

							LoadedAssets.Remove( assetName );
							break;
						}
					}
				}
				catch( Exception e )
				{
					Debug.error( "Could not unload asset {0}. {1}", assetName, e );
				}
			}
		}


		/// <summary>
		/// unloads an Effect that was loaded via loadEffect, loadNezEffect or loadMonoGameEffect
		/// </summary>
		/// <param name="effectName">Effect.name</param>
		public void unloadEffect( string effectName )
		{
			if( _loadedEffects.ContainsKey( effectName ) )
			{
				_loadedEffects[effectName].Dispose();
				_loadedEffects.Remove( effectName );
			}
		}


		/// <summary>
		/// unloads an Effect that was loaded via loadEffect, loadNezEffect or loadMonoGameEffect
		/// </summary>
		/// <param name="effectName">Effect.name</param>
		public void unloadEffect( Effect effect )
		{
			unloadEffect( effect.Name );
		}


		/// <summary>
		/// checks to see if an asset with assetName is loaded
		/// </summary>
		/// <returns><c>true</c> if this instance is asset loaded the specified assetName; otherwise, <c>false</c>.</returns>
		/// <param name="assetName">Asset name.</param>
		public bool isAssetLoaded( string assetName )
		{
			#if FNA
			var fieldInfo = ReflectionUtils.getFieldInfo( this, "loadedAssets" );
			var LoadedAssets = fieldInfo.GetValue( this ) as Dictionary<string, object>;
			#endif

			return LoadedAssets.ContainsKey( assetName );
		}


		/// <summary>
		/// provides a string suitable for logging with all the currently loaded assets and effects
		/// </summary>
		/// <returns>The loaded assets.</returns>
		internal string logLoadedAssets()
		{
			#if FNA
			var fieldInfo = ReflectionUtils.getFieldInfo( this, "loadedAssets" );
			var LoadedAssets = fieldInfo.GetValue( this ) as Dictionary<string, object>;
			#endif

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

