Dear ImGui Integration
==========

Dear ImGui is available via the Nez.ImGui project. The API is a wip and will be changing over time. As of now, the way it works is via a `GlobalManager`. You can toggle ImGui rendering via the `toggle-imgui` command in the debug console or by manually adding the manager:

```csharp
var imGuiManager = new ImGuiManager();
Core.RegisterGlobalManager( imGuiManager );

// toggle ImGui rendering on/off. It starts out enabled.
imGuiManager.SetEnabled( false );
```


Placeholder/wip content...

## Scene Graph Window

- PostProcessors
    - Add PostProcessor
- Entities
- double-click
- right-click


## Core Window

- FPS graph
- Core settings


## Entity Inspector Window

- Component inspectors
- right-click
- Add Component
- Renderables
    - Materials
        - Add Material
        - Add Effect
            - Effects


## Adding Data to the Component Inspector

You can also display custom data in the inspector for your `Component` by putting an `InspectorDelegateAttribute` on any parameterless method in your class. Whenever your `Component` is visible in the inspector the method will be called in the context of the inspector. Anything you draw will appear after the normal `Component` data.

```csharp
[InspectorDelegate]
public void testOtherMethod()
{
    ImGui.TextColored( new System.Numerics.Vector4( 0, 1, 0, 1 ), "Colored text..." );
    ImGui.Combo( "Combo Box", ref privateInt, "First\0Second\0Third\0No Way\0Fifth Option" );
}
```


## Useful Attributes

- `TooltipAttribute`: displays a tooltip with the text when the item is hovered in the inspector
- `RangeAttribute`: lets you specify a range and optionally choose between a slider or drag field for ints/floats
- `NotInspectableAttribute`: forces the inspector to not inspect the field.property
- `InspectableAttribute`: indicates a read-only/private field/property should be displayed. It will be grayed out and disabled.
- `CustomInspectorAttribute`: lets you specify a custom `AbstractTypeInspector` subclass that will be used to inspect the field/property



## Custom ImGui Windows

Once the ImGui manager is installed and enabled you can register and issue ImGui commands from any `Component` by just fetching the ImGuiManager and calling `registerDrawCommand`:

```csharp
public override void onAddedToEntity()
{
    Core.getGlobalManager<ImGuiManager>().registerDrawCommand( imGuiDraw );
}

void imGuiDraw()
{
    ImGui.Begin( "Your ImGui Window" );
    // your ImGui commands here
    ImGui.End();
}
```

You should deregister when your `Component` is no longer active by calling `unregisterDrawCommand`:

```csharp
public override void onRemovedFromEntity()
{
    Core.getGlobalManager<ImGuiManager>().unregisterDrawCommand( imGuiDraw );
}
```


## Advanced: Creating a Custom Inspector

If you have a type that you want to fully control the rendering of you can do that by decorating your class with the `CustomInspectorAttribute`. It requires a `Type` that is a subclass of `AbstractTypeInspector`.

Below, we will illustrate an example. The `WontShowInInspectorByDefault` class by default would not be rendered in the inspector. We specify a `CustomInspectorAttribute` with the Type subclass of `AbstractTypeInspector` that we want to be instantiated and control the inspector rendering. `AbstractTypeInspector` provides us with some useful protected members to get and set the value for the class (works for structs automatically as well). We null check the current value and provide a button to create a new instance. If an instance exists, we display some ImGui controls.

```csharp
[CustomInspector( typeof( WontShowInInspectorByDefaultInspector ) )]
public class WontShowInInspectorByDefault
{
	public string text = "the string";
	public float speed = 54;
	public int index = 10;
}

class WontShowInInspectorByDefaultInspector : AbstractTypeInspector
{
	public override void drawMutable()
	{
		var myObj = getValue<WontShowInInspectorByDefault>();
		if( myObj == null )
		{
			if( ImGui.Button( "Create Object" ) )
			{
				myObj = new WontShowInInspectorByDefault();
				setValue( myObj );
			}
		}
		else
		{
			ImGui.InputText( "text", ref myObj.text, 50 );
			ImGui.DragFloat( "speed", ref myObj.speed );
			ImGui.DragInt( "index", ref myObj.index );
		}
	}
}
```

