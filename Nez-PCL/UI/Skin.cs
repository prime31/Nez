using System;
using System.Collections.Generic;
using Nez.LibGdxAtlases;
using Nez.Textures;
using Microsoft.Xna.Framework;
using Nez.BitmapFonts;
using Nez.TextureAtlases;
using Nez.Systems;
using System.Linq;


namespace Nez.UI
{
	public class Skin
	{
		Dictionary<Type,Dictionary<string,object>> _resources = new Dictionary<Type,Dictionary<string,object>>();


		public Skin()
		{}


		/// <summary>
		/// creates a UISkin from a UISkinConfig
		/// </summary>
		/// <param name="config">Config.</param>
		/// <param name="contentManager">Content manager.</param>
		public Skin( string configName, NezContentManager contentManager )
		{
			var config = contentManager.Load<UISkinConfig>( configName );
			if( config.colors != null )
			{
				foreach( var entry in config.colors )
					add<Color>( entry.Key, config.colors[entry.Key] );
			}

			if( config.textureAtlases != null )
			{
				foreach( var atlas in config.textureAtlases )
					addSubtextures( contentManager.Load<TextureAtlas>( atlas ) );
			}

			if( config.libGdxAtlases != null )
				foreach( var atlas in config.libGdxAtlases )
					addSubtextures( contentManager.Load<LibGdxAtlas>( atlas ) );

			if( config.styles != null )
			{
				var styleClasses = config.styles.getStyleClasses();
				for( var i = 0; i < styleClasses.Count; i++ )
				{
					var styleType = styleClasses[i];
					try
					{
						var type = Type.GetType( "Nez.UI." + styleType, true );
						var styleNames = config.styles.getStyleNames( styleType );

						for( var j = 0; j < styleNames.Count; j++ )
						{
							var style = Activator.CreateInstance( type );
							var styleDict = config.styles.getStyleDict( styleType, styleNames[j] );

							foreach( var styleConfig in styleDict )
							{
								var name = styleConfig.Key;
								var identifier = styleConfig.Value;

								// if name has 'color' in it, we are looking for a color. we check color first because some styles have things like
								// fontColor so we'll check for font after color.
								if( name.ToLower().Contains( "color" ) )
								{
									type.GetField( name ).SetValue( style, getColor( identifier ) );
								}
								else if( name.ToLower().Contains( "font" ) )
								{
									type.GetField( name ).SetValue( style, contentManager.Load<BitmapFont>( identifier ) );
								}
								else
								{
									// we have an IDrawable. first we'll try to find a Subtexture and if we cant find one we will see if
									// identifier is a color
									var drawable = getDrawable( identifier );
									if( drawable != null )
										type.GetField( name ).SetValue( style, drawable );
									else
										Debug.error( "could not find a drawable or color named {0} when setting {1} on {2}", identifier, name, styleNames[i] );
								}
							}

							add( styleNames[j], style, type );
						}
					}
					catch( Exception e )
					{
						Debug.error( "Error creating style from UISkin: {0}", e );
					}
				}
			}
		}


		/// <summary>
		/// Adds all named subtextures from the atlas. If NinePatchSubtextures are found they will be explicitly added as such.
		/// </summary>
		/// <param name="atlas">Atlas.</param>
		public void addSubtextures( LibGdxAtlas atlas )
		{
			for( int i = 0, n = atlas.atlases.Count; i < n; i++ )
				addSubtextures( atlas.atlases[i] );
		}


		/// <summary>
		/// Adds all named subtextures from the atlas
		/// </summary>
		/// <param name="atlas">Atlas.</param>
		public void addSubtextures( TextureAtlas atlas )
		{
			for( int i = 0, n = atlas.subtextures.Length; i < n; i++ )
			{
				var subtexture = atlas.subtextures[i];
				if( subtexture is NinePatchSubtexture )
					add<NinePatchSubtexture>( atlas.regionNames[i], subtexture as NinePatchSubtexture );
				else
					add<Subtexture>( atlas.regionNames[i], subtexture );
			}
		}


		/// <summary>
		/// adds the typed resource to this skin
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="resource">Resource.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T add<T>( string name, T resource )
		{
			Dictionary<string,object> typedResources;
			if( !_resources.TryGetValue( typeof( T ), out typedResources ) )
			{
				typedResources = new Dictionary<string, object>();
				_resources.Add( typeof( T ), typedResources );
			}
			typedResources[name] = resource;
			return resource;
		}


		/// <summary>
		/// adds the typed resource to this skin
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="resource">Resource.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void add( string name, object resource, Type type )
		{
			Dictionary<string,object> typedResources;
			if( !_resources.TryGetValue( type, out typedResources ) )
			{
				typedResources = new Dictionary<string, object>();
				_resources.Add( type, typedResources );
			}
			typedResources[name] = resource;
		}


		/// <summary>
		/// removes the typed resource from this skin
		/// </summary>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void remove<T>( string name )
		{
			Dictionary<string,object> typedResources;
			if( _resources.TryGetValue( typeof( T ), out typedResources ) )
				typedResources.Remove( name );
		}


		/// <summary>
		/// checks to see if a typed resource exists with the given name
		/// </summary>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public bool has<T>( string name )
		{
			Dictionary<string,object> typedResources;
			if( _resources.TryGetValue( typeof( T ), out typedResources ) )
				return typedResources.ContainsKey( name );
			return false;
		}


		/// <summary>
		/// First checks for a resource named "default". If it cant find default it will return either the first resource of type T
		/// or default(T) if none are found.
		/// </summary>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T get<T>()
		{
			if( has<T>( "default" ) )
				return get<T>( "default" );

			Dictionary<string,object> typedResources;
			if( _resources.TryGetValue( typeof( T ), out typedResources ) )
				return (T)typedResources[typedResources.First().Key];

			return default(T);
		}


		/// <summary>
		/// Returns a named resource of the specified type or default(T) if it couldnt be found
		/// </summary>
		/// <param name="name">Name.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T get<T>( string name )
		{
			if( name == null )
				return get<T>();
			
			Dictionary<string,object> typedResources;
			if( !_resources.TryGetValue( typeof( T ), out typedResources ) )
				return default(T);

			if( !typedResources.ContainsKey( name ) )
				return default(T);
			
			return (T)typedResources[name];
		}


		public Color getColor( string name )
		{
			return get<Color>( name );
		}


		public BitmapFont getFont( string name )
		{
			return get<BitmapFont>( name );
		}


		public Subtexture getSubtexture( string name )
		{
			return get<Subtexture>( name );
		}


		public NinePatchSubtexture getNinePatchSubtexture( string name )
		{
			return get<NinePatchSubtexture>( name );
		}


		/// <summary>
		/// Returns a registered subtexture drawable. If no subtexture drawable is found but a Subtexture exists with the name, a
		/// subtexture drawable is created from the Subtexture and stored in the skin
		/// </summary>
		/// <returns>The subtexture drawable.</returns>
		/// <param name="name">Name.</param>
		public SubtextureDrawable getSubtextureDrawable( string name )
		{
			var subtextureDrawable = get<SubtextureDrawable>( name );
			if( subtextureDrawable != null )
				return subtextureDrawable;

			var subtexture = get<Subtexture>( name );
			if( subtexture != null )
			{
				subtextureDrawable = new SubtextureDrawable( subtexture );
				add<SubtextureDrawable>( name, subtextureDrawable );
			}

			return subtextureDrawable;
		}


		/// <summary>
		/// Returns a registered drawable. If no drawable is found but a Subtexture/NinePatchSubtexture exists with the name, then the
		/// appropriate drawable is created and stored in the skin. If name is a color a PrimitiveDrawable will be created and stored.
		/// </summary>
		/// <returns>The drawable.</returns>
		/// <param name="name">Name.</param>
		public IDrawable getDrawable( string name )
		{
			var drawable = get<IDrawable>( name );
			if( drawable != null )
				return drawable;

			// Check for explicit registration of ninepatch, subtexture or tiled drawable
			drawable = get<SubtextureDrawable>( name );
			if( drawable != null )
				return drawable;

			drawable = get<NinePatchDrawable>( name );
			if( drawable != null )
				return drawable;

			drawable = get<TiledDrawable>( name );
			if( drawable != null )
				return drawable;

			drawable = get<PrimitiveDrawable>( name );
			if( drawable != null )
				return drawable;

			// still nothing. check for a NinePatchSubtexture or a Subtexture and create a new drawable if we find one
			var ninePatchSubtexture = get<NinePatchSubtexture>( name );
			if( ninePatchSubtexture != null )
			{
				drawable = new NinePatchDrawable( ninePatchSubtexture );
				add<NinePatchDrawable>( name, drawable as NinePatchDrawable );
				return drawable;
			}

			var subtexture = get<Subtexture>( name );
			if( subtexture != null )
			{
				drawable = new SubtextureDrawable( subtexture );
				add<SubtextureDrawable>( name, drawable as SubtextureDrawable );
				return drawable;
			}

			// finally, we will check if name is a Color and create a PrimitiveDrawable if it is
			if( has<Color>( name ) )
			{
				var color = get<Color>( name );
				drawable = new PrimitiveDrawable( color );
				add<PrimitiveDrawable>( name, drawable as PrimitiveDrawable );
				return drawable;
			}

			return null;
		}


		/// <summary>
		/// Returns a registered tiled drawable. If no tiled drawable is found but a Subtexture exists with the name, a tiled drawable is
		/// created from the Subtexture and stored in the skin
		/// </summary>
		/// <returns>The tiled drawable.</returns>
		/// <param name="name">Name.</param>
		public TiledDrawable getTiledDrawable( string name )
		{
			var tiledDrawable = get<TiledDrawable>( name );
			if( tiledDrawable != null )
				return tiledDrawable;

			var subtexture = get<Subtexture>( name );
			if( subtexture != null )
			{
				tiledDrawable = new TiledDrawable( subtexture );
				add<TiledDrawable>( name, tiledDrawable );
			}

			return tiledDrawable;
		}


		/// <summary>
		/// Returns a registered ninepatch. If no ninepatch is found but a Subtexture exists with the name, a ninepatch is created from the
		/// Subtexture and stored in the skin.
		/// </summary>
		/// <returns>The nine patch.</returns>
		/// <param name="name">Name.</param>
		public NinePatchDrawable getNinePatchDrawable( string name )
		{
			var ninePatchDrawable = get<NinePatchDrawable>( name );
			if( ninePatchDrawable != null )
				return ninePatchDrawable;

			var ninePatchSubtexture = get<NinePatchSubtexture>( name );
			if( ninePatchSubtexture != null )
			{
				ninePatchDrawable = new NinePatchDrawable( ninePatchSubtexture );
				add<NinePatchDrawable>( name, ninePatchDrawable );
				return ninePatchDrawable;
			}

			var subtexture = get<NinePatchSubtexture>( name );
			if( subtexture != null )
			{
				ninePatchDrawable = new NinePatchDrawable( subtexture, 0, 0, 0, 0 );
				add<NinePatchDrawable>( name, ninePatchDrawable );
			}

			return ninePatchDrawable;
		}


		/// <summary>
		/// Returns a tinted copy of a drawable found in the skin via {@link #getDrawable(String)}. Note that the new drawable is NOT
		/// added to the skin! Tinting is only supported on SubtextureDrawables and NinePatchDrawables.
		/// </summary>
		/// <returns>The tinted drawable.</returns>
		/// <param name="name">Name.</param>
		/// <param name="tint">Tint.</param>
		public IDrawable newTintedDrawable( string name, Color tint )
		{
			var drawable = getDrawable( name );
			if( drawable is SubtextureDrawable )
				return ( drawable as SubtextureDrawable ).newTintedDrawable( tint );

			if( drawable is NinePatchDrawable )
				return ( drawable as NinePatchDrawable ).newTintedDrawable( tint );
			
			throw new Exception( "Unable to copy, unknown or unsupported drawable type: " + drawable );
		}

	}
}

