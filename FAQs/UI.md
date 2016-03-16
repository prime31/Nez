Nez UI
============
Nez UI is based on TableLayout ([click for documentation](https://github.com/EsotericSoftware/tablelayout/blob/master/README.md)) and the libGDX Scene2D UI system (docs are [here](https://github.com/libgdx/libgdx/wiki/Scene2d.ui)). You can find detailed docs on the libGDX table [here](https://github.com/libgdx/libgdx/wiki/Table). As of this writing the API is nearly identical. The main differences to be aware of when using the libGDX docs for reference are:

- the Actor class is Element in Nez
- the Widget and WidgetGroup classes dont exist in Nez. Similar functionality is found in the Element and Group classes but this is really only relevant if you are making your own custom controls.


Lets jump right in and see what its like to make a simple UI. We will make a UI with a ProgressBar, Slider (both horizontal) and Button centered in a vertical stack.

```csharp
// tables are very flexible and make good candidates to use at the root of your UI. They work much like HTML tables but with more flexibility.
var table = stage.addElement( new Table() );

// tell the table to fill all the available space. In this case that would be the entire screen.
table.setFillParent( true );

// add a ProgressBar
var bar = new ProgressBar( 0, 1, 0.1f, false, ProgressBarStyle.create( Color.Black, Color.White ) );
table.add( bar );

// this tells the table to move on to the next row
table.row();

// add a Slider
var slider = new Slider( 0, 1, 0.1f, false, SliderStyle.create( Color.DarkGray, Color.LightYellow ) );
table.add( slider );
table.row();

// if creating buttons with just colors (PrimitiveDrawables) it is important to explicitly set the minimum size since the colored textures created
// are only 1x1 pixels
var button = new Button( ButtonStyle.create( Color.Black, Color.DarkGray, Color.Green ) );
table.add( button ).setMinWidth( 100 ).setMinHeight( 30 );
```



Skins
==========
Nez UI supports a skin system similar to [libGDX skins](https://github.com/libgdx/libgdx/wiki/Skin). Skins are optional but highly recommended. They act as a container to hold all of your UI resources and offer a bunch of automatic conversions. You can create a skin programatically or via a JSON file that is run through the UI Skin Importer in the Pipeline tool. This gets the JSON parsed at build time so the data is ready to use at runtime. Below is example JSON with some comments added explaining the different elements.

```json
{
	// defines colors accessible via skin.getColor. These can also be referenced in actual style definitions below
	colors:
	{
		green: '#00ff00',
		orange: '#ff9900',
		blue: '#0000ff',
		black: [0, 0, 0, 255],
		gray: [50, 50, 50, 255],
		blue: [116, 139, 167, 255],
		dialogDim: [50, 50, 50, 50]
	},
	// array of any LibGdxAtlases. The path should be the same one you would use to load it via the content system.
	libGdxAtlases: [ 'bin/skin/uiskinatlas' ],

	// array of any TextureAtlases. The path should be the same one you would use to load it via the content system.
	textureAtlases: [ 'bin/skin/textureAtlas ],
	
	// the rest of the file is specific style types. The key (ButtonStyle here) is the exact class name from the UI element.
	ButtonStyle:
	{
		// "default" is the name of the style that is used at runtime to find it. Any font, color or IDrawable can be specified.
		// Nez UI will search any loaded atlases for the specified resource.
		default: { down: 'default-round-down', up: 'default-round' },
		toggle: { down: 'default-round-down', checkked: 'default-round-down', up: 'default-round' },
		// this ButtonStyle uses only references to colors. Nez UI will handle making appropriate resources at runtime for you.
		colored: { down: 'gray', up: 'black', over: 'blue' }
	},
	SplitPaneStyle:
	{
		'default-vertical': { handle: 'default-splitpane-vertical' },
		'default-horizontal': { handle: 'default-splitpane' }
	},
	WindowStyle:
	{
		// the titleFontColor directly references a color that we specified above in the colors section
		default: { titleFont: 'nez/NezDefaultBMFont', background: 'default-window', titleFontColor: 'white' },
		dialog: { titleFont: 'nez/NezDefaultBMFont', background: 'default-window', titleFontColor: 'white', stageBackground: 'dialogDim' }
	},
	ProgressBarStyle:
	{
		'default-horizontal': { background: 'default-slider', knob: 'default-slider-knob' },
		'default-vertical': { background: 'default-slider', knob: 'default-round-large' }
	},
	SliderStyle:
	{
		'default-horizontal': { background: 'default-slider', knob: 'default-slider-knob' },
		'default-vertical': { background: 'default-slider', knob: 'default-round-large' }
	},
	LabelStyle:
	{
		// fonts should be the same path you would use to load it via the content system
		default: { font: 'nez/NezDefaultBMFont', fontColor: 'white' }
	}
}
```

Now that we have the skin lets create a few elements with it.

```csharp
var skin = new Skin( "skins/uiskinconfig", Core.contentManager );

// notice that we can directly fetch the style for the button via the name we specified in the JSON
var button = new Button( skin.get<TextButtonStyle>( "default" ) );
	
var bar = new ProgressBar( 0, 1, 0.1f, vertical, skin.get<ProgressBarStyle>( "default-vertical" ) );

// this button uses the 'colored' style that we made using only colors. We have to remember to give it 
var button = new Button( skin.get<ButtonStyle>( "colored" ) );
table.add( button ).setMinWidth( 100 ).setMinHeight( 30 );
```


Programmatic Skin Creation
========
You do not have to use the JSON config file and pipeline importer to get the benefits of using a skin. Skins can also be created programmatically though it can be a bit tedious. Luckily, once you have code to make your skin using it is simple!


```csharp
// create the Skin
var skin = new Skin();

// add a texture atlas so we have some images to work with
skin.addSubtextures( Content.Load<LibGdxAtlas>( "skins/UIAtlas" ) );
	
// add a bunch of styles for our elements. Note that the getDrawable method is very flexible. The name passed to it can be any type of
// IDrawable or it can be a Subtexture, NinePatchSubtexture or Color. In the latter case Skin will create and manage the IDrawable
// for you automatically.
skin.add( "button", new ButtonStyle( skin.getDrawable( "default-round" ), skin.getDrawable( "default-round-down" ), null ) );

// add a toggle button. It needs a checked image (spelled incorrecly on purpose due to C# having 'checked' as a reserved word) to trigger
// this being a two state button.
skin.add( "toggle-button", new ButtonStyle( skin.getDrawable( "default-round-down" ), skin.getDrawable( "default-round-down" ), null )
{
	checkked = skin.getDrawable( "default-round" )
});

skin.add( "text-button", new TextButtonStyle {
	down = skin.getDrawable( "default-round-down" ),
	up = skin.getDrawable( "default-round" ),
	fontColor = Color.White
} );

skin.add( "progressbar-h", new ProgressBarStyle( skin.getDrawable( "default-slider" ), skin.getDrawable( "default-slider-knob" ) ) );

skin.add( "slider-h", new SliderStyle( skin.getDrawable( "default-slider" ), skin.getDrawable( "default-slider-knob" ) ) );

// a CheckBox differs from the toggle-button above in that it contains text next to the box
skin.add( "checkbox", new CheckBoxStyle( skin.getDrawable( "check-off" ), skin.getDrawable( "check-on" ), null, Color.White ) );

skin.add( "textfield", new TextFieldStyle( null, Color.White, skin.getDrawable( "cursor" ), skin.getDrawable( "selection" ), skin.getDrawable( "textfield" ) )
```
