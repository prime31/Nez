﻿using System;
using System.Threading;
using System.Reflection;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

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

		public NezContentManager() : base( ((Game)Core._instance).Services, ((Game)Core._instance).Content.RootDirectory )
		{}


		/// <summary>
		/// loads an ogl effect directly from file and handles disposing of it when the ContentManager is disposed. Name should be the path
		/// relative to the Content folder or including the Content folder.
		/// </summary>
		/// <returns>The effect.</returns>
		/// <param name="name">Name.</param>
		public Effect LoadEffect( string name )
		{
			return LoadEffect<Effect>( name );
		}

		/// <summary>
		/// loads an embedded Nez effect. These are any of the Effect subclasses in the Nez/Graphics/Effects folder.
		/// Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect duplication.
		/// </summary>
		/// <returns>The nez effect.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T LoadNezEffect<T>() where T : Effect, new()
		{
			var cacheKey = typeof( T ).Name + "-" + Utils.RandomString( 5 );
			var effect = new T();
			effect.Name = cacheKey;
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
		public T LoadEffect<T>( string name ) where T : Effect
		{
			// make sure the effect has the proper root directory
			if( !name.StartsWith( RootDirectory ) )
				name = RootDirectory + "/" + name;

			var bytes = EffectResource.GetFileResourceBytes( name );

			return LoadEffect<T>( name, bytes );
		}

		/// <summary>
		/// loads an ogl effect directly from its bytes and handles disposing of it when the ContentManager is disposed. Name should the the path
		/// relative to the Content folder or including the Content folder. Effects must have a constructor that accepts GraphicsDevice and
		/// byte[]. Note that this will return a unique instance if you attempt to load the same Effect twice to avoid Effect duplication.
		/// </summary>
		/// <returns>The effect.</returns>
		/// <param name="name">Name.</param>
		internal T LoadEffect<T>( string name, byte[] effectCode ) where T : Effect
		{
			var effect = Activator.CreateInstance( typeof( T ), Core.GraphicsDevice, effectCode ) as T;
			effect.Name = name + "-" + Utils.RandomString( 5 );
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
		public T LoadMonoGameEffect<T>() where T : Effect
		{
			var effect = Activator.CreateInstance( typeof( T ), Core.GraphicsDevice ) as T;
			effect.Name = typeof( T ).Name + "-" + Utils.RandomString( 5 );
			_loadedEffects[effect.Name] = effect;

			return effect;
		}

		/// <summary>
		/// loads an asset on a background thread with optional callback for when it is loaded. The callback will occur on the main thread.
		/// </summary>
		/// <param name="assetName">Asset name.</param>
		/// <param name="onLoaded">On loaded.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void LoadAsync<T>( string assetName, Action<T> onLoaded = null )
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
		public void LoadAsync<T>( string assetName, Action<object,T> onLoaded = null, object context = null )
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
		public void LoadAsync<T>( string[] assetNames, Action onLoaded = null )
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
		public void UnloadAsset<T>( string assetName ) where T : class, IDisposable
		{
			if( IsAssetLoaded( assetName ) )
			{
				try
				{
					FieldInfo fieldInfo = null;
					var fields = typeof( ContentManager ).GetRuntimeFields();
					foreach( var field in fields )
					{
						if( field.Name == "disposableAssets" )
						{
							fieldInfo = field;
							break;
						}
					}

					// first fetch the actual asset. we already know its loaded so we'll grab it directly
					#if FNA
					fieldInfo = ReflectionUtils.getFieldInfo( typeof( ContentManager ), "loadedAssets" );
					var LoadedAssets = fieldInfo.GetValue( this ) as Dictionary<string, object>;
					#endif

					var assetToRemove = LoadedAssets[assetName];

					var assets = fieldInfo.GetValue( this ) as List<IDisposable>;
					for( var i = 0; i < assets.Count; i++ )
					{
						// see if the asset is disposeable. If so, find and dispose of it.
						var typedAsset = assets[i] as T;
						if( typedAsset != null && typedAsset == assetToRemove )
						{
							typedAsset.Dispose();
							assets.RemoveAt( i );
							break;
						}
					}

					LoadedAssets.Remove( assetName );
				}
				catch( Exception e )
				{
					Debug.Error( "Could not unload asset {0}. {1}", assetName, e );
				}
			}
		}

		/// <summary>
		/// unloads an Effect that was loaded via loadEffect, loadNezEffect or loadMonoGameEffect
		/// </summary>
		/// <param name="effectName">Effect.name</param>
		public bool UnloadEffect( string effectName )
		{
			if( _loadedEffects.ContainsKey( effectName ) )
			{
				_loadedEffects[effectName].Dispose();
				_loadedEffects.Remove( effectName );
				return true;
			}
			return false;
		}

		/// <summary>
		/// unloads an Effect that was loaded via loadEffect, loadNezEffect or loadMonoGameEffect
		/// </summary>
		/// <param name="effectName">Effect.name</param>
		public bool UnloadEffect( Effect effect )
		{
			return UnloadEffect( effect.Name );
		}

		/// <summary>
		/// checks to see if an asset with assetName is loaded
		/// </summary>
		/// <returns><c>true</c> if this instance is asset loaded the specified assetName; otherwise, <c>false</c>.</returns>
		/// <param name="assetName">Asset name.</param>
		public bool IsAssetLoaded( string assetName )
		{
			#if FNA
			var fieldInfo = ReflectionUtils.getFieldInfo( typeof( ContentManager ), "loadedAssets" );
			var LoadedAssets = fieldInfo.GetValue( this ) as Dictionary<string, object>;
			#endif

			return LoadedAssets.ContainsKey( assetName );
		}

		/// <summary>
		/// provides a string suitable for logging with all the currently loaded assets and effects
		/// </summary>
		/// <returns>The loaded assets.</returns>
		internal string LogLoadedAssets()
		{
			#if FNA
			var fieldInfo = ReflectionUtils.getFieldInfo( typeof( ContentManager ), "loadedAssets" );
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
		/// reverse lookup. Gets the asset path given the asset. This is useful for making editor and non-runtime stuff.
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		public string GetPathForLoadedAsset( object asset )
		{
			#if FNA
			var fieldInfo = ReflectionUtils.getFieldInfo( typeof( ContentManager ), "loadedAssets" );
			var LoadedAssets = fieldInfo.GetValue( this ) as Dictionary<string, object>;
			#endif

			if( LoadedAssets.ContainsValue( asset ) )
			{
				foreach( var kv in LoadedAssets )
				{
					if( kv.Value == asset )
						return kv.Key;
				}
			}
			return null;
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


	/// <summary>
	/// the only difference between this class and NezContentManager is that this one can load embedded resources from the Nez.dll
	/// </summary>
	sealed class NezGlobalContentManager : NezContentManager
	{
		public NezGlobalContentManager( IServiceProvider serviceProvider, string rootDirectory ) : base( serviceProvider, rootDirectory )
		{}


		/// <summary>
		/// override that will load embedded resources if they have the "nez://" prefix
		/// </summary>
		/// <returns>The stream.</returns>
		/// <param name="assetName">Asset name.</param>
		protected override Stream OpenStream( string assetName )
		{
			if( assetName.StartsWith( "nez://" ) )
			{
				var assembly = ReflectionUtils.GetAssembly( this.GetType() );

				#if FNA
				// for FNA, we will just search for the file by name since the assembly name will not be known at runtime
				foreach( var item in assembly.GetManifestResourceNames() )
				{
					if( item.EndsWith( assetName.Substring( assetName.Length - 20 ) ) )
					{
						assetName = "nez://" + item;
						break;
					}
				}
				#endif
				return assembly.GetManifestResourceStream( assetName.Substring( 6 ) );
			}

			return base.OpenStream( assetName );
		}

	}

}

