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
		Dictionary<Type, Dictionary<string, object>> _resources = new Dictionary<Type, Dictionary<string, object>>();


		/// <summary>
		/// creates a default Skin that can be used for quick mockups. Includes button, textu button, checkbox, progress bar and slider styles.
		/// </summary>
		/// <returns>The default skin.</returns>
		public static Skin createDefaultSkin()
		{
			var skin = new Skin();

			// define our colors
			var buttonColor = new Color( 78, 91, 98 );
			var buttonOver = new Color( 168, 207, 115 );
			var buttonDown = new Color( 244, 23, 135 );
			var overFontColor = new Color( 85, 127, 27 );
			var downFontColor = new Color( 255, 255, 255 );
			var checkedOverFontColor = new Color( 247, 217, 222 );

			var checkboxOn = new Color( 168, 207, 115 );
			var checkboxOff = new Color( 63, 63, 63 );
			var checkboxOver = new Color( 130, 130, 130 );
			var checkboxOverFontColor = new Color( 220, 220, 220 );

			var barBg = new Color( 78, 91, 98 );
			var barKnob = new Color( 25, 144, 188 );
			var barKnobOver = new Color( 168, 207, 115 );
			var barKnobDown = new Color( 244, 23, 135 );

			var windowColor = new Color( 17, 17, 17 );

			var textFieldFontColor = new Color( 220, 220, 220 );
			var textFieldCursorColor = new Color( 83, 170, 116 );
			var textFieldSelectionColor = new Color( 180, 52, 166 );
			var textFieldBackgroundColor = new Color( 22, 22, 22 );

			var scrollPaneScrollBarColor = new Color( 44, 44, 44 );
			var scrollPaneKnobColor = new Color( 241, 156, 0 );

			var listBoxBackgroundColor = new Color( 20, 20, 20 );
			var listBoxSelectionColor = new Color( 241, 156, 0 );
			var listBoxHoverSelectionColor = new Color( 120, 78, 0 );

			var selectBoxBackgroundColor = new Color( 10, 10, 10 );

			// add all our styles
			var buttonStyle = new ButtonStyle
			{
				up = new PrimitiveDrawable( buttonColor, 10 ),
				over = new PrimitiveDrawable( buttonOver ),
				down = new PrimitiveDrawable( buttonDown )
			};
			skin.add( "default", buttonStyle );

			var textButtonStyle = new TextButtonStyle
			{
				up = new PrimitiveDrawable( buttonColor, 6, 2 ),
				over = new PrimitiveDrawable( buttonOver ),
				down = new PrimitiveDrawable( buttonDown ),
				overFontColor = overFontColor,
				downFontColor = downFontColor,
				pressedOffsetX = 1,
				pressedOffsetY = 1
			};
			skin.add( "default", textButtonStyle );

			var toggleButtonStyle = new TextButtonStyle
			{
				up = new PrimitiveDrawable( buttonColor, 10, 5 ),
				over = new PrimitiveDrawable( buttonOver ),
				down = new PrimitiveDrawable( buttonDown ),
				checkked = new PrimitiveDrawable( new Color( 255, 0, 0, 255 ) ),
				checkedOverFontColor = checkedOverFontColor,
				overFontColor = overFontColor,
				downFontColor = downFontColor,
				pressedOffsetX = 1,
				pressedOffsetY = 1
			};
			skin.add( "toggle", toggleButtonStyle );

			var checkboxStyle = new CheckBoxStyle
			{
				checkboxOn = new PrimitiveDrawable( 30, checkboxOn ),
				checkboxOff = new PrimitiveDrawable( 30, checkboxOff ),
				checkboxOver = new PrimitiveDrawable( 30, checkboxOver ),
				overFontColor = checkboxOverFontColor,
				downFontColor = downFontColor,
				pressedOffsetX = 1,
				pressedOffsetY = 1
			};
			skin.add( "default", checkboxStyle );

			var progressBarStyle = new ProgressBarStyle
			{
				background = new PrimitiveDrawable( 14, barBg ),
				knobBefore = new PrimitiveDrawable( 14, barKnobOver )
			};
			skin.add( "default", progressBarStyle );

			var sliderStyle = new SliderStyle
			{
				background = new PrimitiveDrawable( 6, barBg ),
				knob = new PrimitiveDrawable( 14, barKnob ),
				knobOver = new PrimitiveDrawable( 14, barKnobOver ),
				knobDown = new PrimitiveDrawable( 14, barKnobDown )
			};
			skin.add( "default", sliderStyle );

			var windowStyle = new WindowStyle
			{
				background = new PrimitiveDrawable( windowColor )
			};
			skin.add( "default", windowStyle );

			var textFieldStyle = TextFieldStyle.create( textFieldFontColor, textFieldCursorColor, textFieldSelectionColor, textFieldBackgroundColor );
			skin.add( "default", textFieldStyle );

			var labelStyle = new LabelStyle();
			skin.add( "default", labelStyle );

			var scrollPaneStyle = new ScrollPaneStyle
			{
				vScroll = new PrimitiveDrawable( 6, 0, scrollPaneScrollBarColor ),
				vScrollKnob = new PrimitiveDrawable( 6, 50, scrollPaneKnobColor ),
				hScroll = new PrimitiveDrawable( 0, 6, scrollPaneScrollBarColor ),
				hScrollKnob = new PrimitiveDrawable( 50, 6, scrollPaneKnobColor )
			};
			skin.add( "default", scrollPaneStyle );

			var listBoxStyle = new ListBoxStyle
			{
				fontColorHovered = new Color( 255, 255, 255 ),
				selection = new PrimitiveDrawable( listBoxSelectionColor, 5, 5 ),
				hoverSelection = new PrimitiveDrawable( listBoxHoverSelectionColor, 5, 5 ),
				background = new PrimitiveDrawable( listBoxBackgroundColor )
			};
			skin.add( "default", listBoxStyle );

			var selectBoxStyle = new SelectBoxStyle
			{
				listStyle = listBoxStyle,
				scrollStyle = scrollPaneStyle,
				background = new PrimitiveDrawable( selectBoxBackgroundColor, 4, 4 )
			};
			skin.add( "default", selectBoxStyle );

			var textTooltipStyle = new TextTooltipStyle
			{
				labelStyle = new LabelStyle( listBoxBackgroundColor ),
				background = new PrimitiveDrawable( checkboxOn, 4, 2 )
			};
			skin.add( "default", textTooltipStyle );

			return skin;
		}


		public Skin()
		{
		}


		/// <summary>
		/// creates a UISkin from a UISkinConfig
		/// </summary>
		/// <param name="configName">the path of the UISkinConfig xnb</param>
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

							// Get the method by simple name check since we know it's the only one
							var setStylesForStyleClassMethod = ReflectionUtils.getMethodInfo( this, "setStylesForStyleClass" );
							setStylesForStyleClassMethod = setStylesForStyleClassMethod.MakeGenericMethod( type );

							// Return not nec., but it shows that the style is being modified
							style = setStylesForStyleClassMethod.Invoke( this, new object[] { style, styleDict, contentManager, styleNames[j] } );

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
		/// Recursively finds and sets all styles for a specific style config class that are within 
		/// the dictionary passed in. This allows skins to contain nested, dynamic style declarations.
		///	For example, it allows a SelectBoxStyle to contain a listStyle that is declared inline 
		///	(and not a reference).
		/// </summary>
		/// <param name="styleClass">The style config class instance that needs to be "filled out"</param>
		/// <param name="styleDict">A dictionary that represents one style name within the style config class (i.e. 'default').</param>
		/// <param name="styleName">The style name that the dictionary represents (i.e. 'default').</param>
		/// <typeparam name="T">The style config class type (i.e. SelectBoxStyle)</typeparam>
		public T setStylesForStyleClass<T>( T styleClass, Dictionary<string, object> styleDict, NezContentManager contentManager, string styleName )
		{
			foreach( var styleConfig in styleDict )
			{
				var name = styleConfig.Key;
				var valueObject = styleConfig.Value;
				var identifier = valueObject.ToString();

				// if name has 'color' in it, we are looking for a color. we check color first because some styles have things like
				// fontColor so we'll check for font after color. We assume these are strings and do no error checking on 'identifier'
				if( name.ToLower().Contains( "color" ) )
				{
					ReflectionUtils.getFieldInfo( styleClass, name ).SetValue( styleClass, getColor( identifier ) );
				}
				else if( name.ToLower().Contains( "font" ) )
				{
					ReflectionUtils.getFieldInfo( styleClass, name ).SetValue( styleClass, contentManager.Load<BitmapFont>( identifier ) );
				}
				else if( name.ToLower().EndsWith( "style" ) )
				{
					var styleField = ReflectionUtils.getFieldInfo( styleClass, name );

					// Check to see if valueObject is a Dictionary object instead of a string. If so, it is an 'inline' style
					//	and needs to be recursively parsed like any other style. Otherwise, it is assumed to be a string and 
					//	represents an existing style that has been previously parsed.
					if( valueObject is Dictionary<string, object> )
					{
						// Since there is no existing field to reference, we create it and fill it out by hand
						var inlineStyle = Activator.CreateInstance( styleField.FieldType );

						// Recursively call this method with the new field type and dictionary
						var setStylesForStyleClassMethod = ReflectionUtils.getMethodInfo( this, "setStylesForStyleClass" );
						setStylesForStyleClassMethod = setStylesForStyleClassMethod.MakeGenericMethod( styleField.FieldType );
						inlineStyle = setStylesForStyleClassMethod.Invoke( this, new object[] { inlineStyle, valueObject as Dictionary<string, object>, contentManager, styleName } );
						styleField.SetValue( styleClass, inlineStyle );
					}
					else
					{
						// We have a style reference. First we need to find out what type of style name refers to from the field.
						// Then we need to fetch the "get" method and properly type it.
						var getStyleMethod = ReflectionUtils.getMethodInfo( this, "get", new Type[] { typeof( string ) } );
						getStyleMethod = getStyleMethod.MakeGenericMethod( styleField.FieldType );

						// now we look up the style and finally set it
						var theStyle = getStyleMethod.Invoke( this, new object[] { identifier } );
						styleField.SetValue( styleClass, theStyle );

						if( theStyle == null )
							Debug.error( "could not find a style reference named {0} when setting {1} on {2}", identifier, name, styleName );
					}
				}
				else
				{
					// we have an IDrawable. first we'll try to find a Subtexture and if we cant find one we will see if
					// identifier is a color
					var drawable = getDrawable( identifier );
					if( drawable != null )
						ReflectionUtils.getFieldInfo( styleClass, name ).SetValue( styleClass, drawable );
					else
						Debug.error( "could not find a drawable or color named {0} when setting {1} on {2}", identifier, name, styleName );
				}
			}

			return styleClass;
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
			Dictionary<string, object> typedResources;
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
			Dictionary<string, object> typedResources;
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
			Dictionary<string, object> typedResources;
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
			Dictionary<string, object> typedResources;
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

			Dictionary<string, object> typedResources;
			if( _resources.TryGetValue( typeof( T ), out typedResources ) )
				return (T)typedResources[typedResources.First().Key];

			return default( T );
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

			Dictionary<string, object> typedResources;
			if( !_resources.TryGetValue( typeof( T ), out typedResources ) )
				return default( T );

			if( !typedResources.ContainsKey( name ) )
				return default( T );

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
		/// Returns a tinted copy of a drawable found in the skin via getDrawable. Note that the new drawable is NOT
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

