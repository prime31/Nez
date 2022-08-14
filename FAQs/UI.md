Nez UI
============
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
Nez UI supports a skin system similar to [libGDX skins](https://github.com/libgdx/libgdx/wiki/Skin). Skins are optional but recommended. They act as a container to hold all of your UI resources and offer a bunch of automatic conversions. Nez includes a simple, default skin (accessible via `Skin.CreateDefaultSkin`) that you can use to mock up UIs quickly. You can create a skin programatically as well. See the `Skin.CreateDefaultSkin` for an example.

```csharp
// create the Skin
var skin = new Skin();

// add a texture atlas so we have some images to work with
skin.AddSprites( Content.Load<SpriteAtlas>( "skins/UIAtlas" ) );

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
