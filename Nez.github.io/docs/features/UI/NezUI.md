---
id: NezUI
title: Nez UI
---

Nez UI is based on TableLayout ([click for documentation](https://github.com/EsotericSoftware/tablelayout/blob/master/README.md)) and the libGDX Scene2D UI system (docs are [here](https://github.com/libgdx/libgdx/wiki/Scene2d.ui)). You can find detailed docs on the libGDX table [here](https://github.com/libgdx/libgdx/wiki/Table). As of this writing the API is nearly identical. The main differences to be aware of when using the libGDX docs for reference are:

- the Actor class is Element in Nez
- the Widget and WidgetGroup classes dont exist in Nez. Similar functionality is found in the Element and Group classes but this is really only relevant if you are making your own custom controls.


Lets jump right in and see what its like to make a UI. We will make a UI with a ProgressBar and Slider (both horizontal) and Button centered in a vertical stack. Note that the `Stage` referenced below is directly from the `UICanvas` component, which is a simple base component for managing a Stage.

```csharp
// tables are very flexible and make good candidates to use at the root of your UI. They work much like HTML tables but with more flexibility.
var table = Stage.AddElement( new Table() );

// tell the table to fill all the available space. In this case that would be the entire screen.
table.SetFillParent( true );

// add a ProgressBar
var bar = new ProgressBar( 0, 1, 0.1f, false, ProgressBarStyle.Create( Color.Black, Color.White ) );
table.Add( bar );

// this tells the table to move on to the next row
table.Row();

// add a Slider
var slider = new Slider( 0, 1, 0.1f, false, SliderStyle.Create( Color.DarkGray, Color.LightYellow ) );
table.Add( slider );
table.Row();

// if creating buttons with just colors (PrimitiveDrawables) it is important to explicitly set the minimum size since the colored textures created
// are only 1x1 pixels
var button = new Button( ButtonStyle.Create( Color.Black, Color.DarkGray, Color.Green ) );
table.Add( button ).SetMinWidth( 100 ).SetMinHeight( 30 );
```



## Skins
Nez UI supports a skin system similar to [libGDX skins](https://github.com/libgdx/libgdx/wiki/Skin). Skins are optional but highly recommended. They act as a container to hold all of your UI resources and offer a bunch of automatic conversions. Nez includes a simple, default skin (accessible via `Skin.CreateDefaultSkin`) that you can use to mock up UIs quickly. You can create a skin programatically or via a JSON file that is run through the UI Skin Importer in the Pipeline tool. This gets the JSON parsed at build time so the data is ready to use at runtime. Below is example JSON with some comments added explaining the different elements.

```javascript
{
	// defines colors accessible via skin.getColor. These can also be referenced in actual style definitions below
	Colors:
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
	LibGdxAtlases: [ 'bin/skin/uiskinatlas' ],

	// array of any TextureAtlases. The path should be the same one you would use to load it via the content system.
	TextureAtlases: [ 'bin/skin/textureAtlas' ],

	// the rest of the file is specific style types. The key (ButtonStyle here) is the exact class name from the UI element.
	ButtonStyle:
	{
		// "default" is the name of the style that is used at runtime to find it. Any font, color or IDrawable can be specified.
		// Nez UI will search any loaded atlases for the specified resource.
		default: { Down: 'default-round-down', Up: 'default-round' },
		toggle: { Down: 'default-round-down', Checked: 'default-round-down', Up: 'default-round' },
		// this ButtonStyle uses only references to colors. Nez UI will handle making appropriate resources at runtime for you.
		colored: { Down: 'gray', Up: 'black', Over: 'blue' }
	},
	SplitPaneStyle:
	{
		'default-vertical': { Handle: 'default-splitpane-vertical' },
		'default-horizontal': { Handle: 'default-splitpane' }
	},
	WindowStyle:
	{
		// the titleFontColor directly references a color that we specified above in the colors section
		default: { TitleFont: 'nez/NezDefaultBMFont', Background: 'default-window', TitleFontColor: 'white' },
		dialog: { TitleFont: 'nez/NezDefaultBMFont', Background: 'default-window', TitleFontColor: 'white', StageBackground: 'dialogDim' }
	},
	ProgressBarStyle:
	{
		'default-horizontal': { Background: 'default-slider', Knob: 'default-slider-knob' },
		'default-vertical': { Background: 'default-slider', Knob: 'default-round-large' }
	},
	SliderStyle:
	{
		'default-horizontal': { Background: 'default-slider', Knob: 'default-slider-knob' },
		'default-vertical': { Background: 'default-slider', Knob: 'default-round-large' }
	},
	LabelStyle:
	{
		// fonts should be the same path you would use to load it via the content system
		default: { Font: 'nez/NezDefaultBMFont', FontColor: 'white' },
		tooltip: { Font: 'nez/NezDefaultBMFont', FontColor: 'blue' },
	},
	TextTooltipStyle:
	{
		// note that LabelStyle referes the the 'tooltip' LabelStyle defined above
		default: { LabelStyle: 'tooltip', Background: 'gray' }
	}
}
```

Now that we have the skin lets create a few elements with it.

```csharp
var skin = new Skin( "skins/uiskinconfig", Core.Content );

// notice that we can directly fetch the style for the button via the name we specified in the JSON
var button = new Button( skin.Get<TextButtonStyle>( "default" ) );
// alternatively, we could create the button like this. Note that we are just giving it the skin so as long as there is
// a style named "default" that is what will be used.
var button = new Button( skin );

var bar = new ProgressBar( 0, 1, 0.1f, vertical, skin.Get<ProgressBarStyle>( "default-vertical" ) );

// this button uses the 'colored' style that we made using only colors. We have to remember to give it
// some girth since it isnt an image and has no height/width.
var button = new Button( skin.Get<ButtonStyle>( "colored" ) );
table.Add( button ).SetMinWidth( 100 ).SetMinHeight( 30 );
```


## Programmatic Skin Creation
You do not have to use the JSON config file and pipeline importer to get the benefits of using a skin. Skins can also be created programmatically though it can be a bit tedious. Luckily, once you have code to make your skin using it is simple!


```csharp
// create the Skin
var skin = new Skin();

// add a texture atlas so we have some images to work with
skin.AddSprites( Content.Load<LibGdxAtlas>( "skins/UIAtlas" ) );

// add a bunch of styles for our elements. Note that the getDrawable method is very flexible. The name passed to it can be any type of
// IDrawable or it can be a Sprite, NinePatchSprite or Color. In the latter case Skin will create and manage the IDrawable
// for you automatically.
skin.Add( "button", new ButtonStyle( skin.GetDrawable( "default-round" ), skin.GetDrawable( "default-round-down" ), null ) );

// add a toggle button. It needs a checked image to trigger this being a two state button.
skin.Add( "toggle-button", new ButtonStyle( skin.GetDrawable( "default-round-down" ), skin.GetDrawable( "default-round-down" ), null )
{
	Checked = skin.getDrawable( "default-round" )
});

skin.Add( "text-button", new TextButtonStyle {
	Down = skin.getDrawable( "default-round-down" ),
	Up = skin.getDrawable( "default-round" ),
	FontColor = Color.White
} );

skin.Add( "progressbar-h", new ProgressBarStyle( skin.GetDrawable( "default-slider" ), skin.GetDrawable( "default-slider-knob" ) ) );

skin.Add( "slider-h", new SliderStyle( skin.GetDrawable( "default-slider" ), skin.GetDrawable( "default-slider-knob" ) ) );

// a CheckBox differs from the toggle-button above in that it contains text next to the box
skin.Add( "checkbox", new CheckBoxStyle( skin.GetDrawable( "check-off" ), skin.GetDrawable( "check-on" ), null, Color.White ) );

skin.Add( "textfield", new TextFieldStyle( null, Color.White, skin.GetDrawable( "cursor" ), skin.GetDrawable( "selection" ), skin.GetDrawable( "textfield" ) )
```


## Gamepad Input
Nez UI supports gamepad input out of the box via the `IGamepadFocusable` interface. Buttons (and any subclasses such as TextButton, Checkbox, etc) and Sliders will work out of the box (note that Sliders require `ShouldUseExplicitFocusableControl` to be true). To enable gamepad input processing just set the first focusable element via the `stage.SetGamepadFocusElement` method. That will trigger the stage to use gamepad input. By default, the A button will be used for activating a UI Element. You can change this via the `stage.GamepadActionButton`. Also by default keyboard input (arrow keys and enter) will also work and is customizable via `KeyboardEmulatesGamepad` and `KeyboardActionKey`. If you have custom controls that would like to take part in gamepad input just implement the IGamepadFocusable interface on the element. If you are subclassing Button or Slider it is even easier: just override any of the 4 focus handlers: `OnFocused`, `OnUnfocused`, `OnActionButtonPressed` and `OnActionButtonReleased`.

If you want finer grained control over which Element gains focus when a particular direction is pressed on the gamepad you can manually set the `GamepadUp/Down/Left/RightElement` properties. Leaving any null will result in no focus change when that direction is pressed and the `OnUnhandledDirectionPressed` method will be called. Note that you must also set `IGamepadFocusable.ShouldUseExplicitFocusableControl` when setting these directly. Below is a simple example of setting up 2 buttons and a slider horizontally. The slider's value will be changed when up/down is pressed on the gamepad.


```csharp
// create buttons and a slider...

// be sure to enable explicit control on each Element!
leftButton.ShouldUseExplicitFocusableControl = true;
// when pressing right change control to the middleSlider
leftButton.GamepadRightElement = middleSlider;
// optional. This would make pressing left wrap around to the rightButton
leftButton.GamepadLeftElement = rightButton;

middleSlider.ShouldUseExplicitFocusableControl = true;
middleSlider.GamepadLeftElement = leftButton;
middleSlider.GamepadRightElement = rightButton;

rightButton.ShouldUseExplicitFocusableControl = true;
rightButton.GamepadLeftElement = middleSlider;
// optional. This would make pressing right wrap around to the leftButton
rightButton.GamepadRightElement = leftButton;

```
