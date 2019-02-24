Dear ImGui Integration
==========
Dear ImGui is available via the Nez.ImGui project. The API is a wip and will be changing over time. As of now, the way it works is via a `GlobalManager`. You can toggle ImGui rendering via the `toggle-imgui` command in the debug console or by manually adding the manager:

```csharp
var imGuiManager = new ImGuiManager();
Core.registerGlobalManager( imGuiManager );

// toggle ImGui rendering on/off. It starts out enabled.
imGuiManager.setEnabled( false );
```

Once the ImGui manager is installed and enabled you can register and issue ImGui commands from any Component by just fetching the ImGuiManager and calling `registerDrawCommand`:

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

You should deregister when your Component is no longer active by calling `unregisterDrawCommand`:

```csharp
public override void onRemovedFromEntity()
{
    Core.getGlobalManager<ImGuiManager>().unregisterDrawCommand( imGuiDraw );
}
```


Some notes that need more details later...
- `CustomInspectorAttribute` example
- readonly props/fields can be made visible in the inspector by putting an InspectableAttribute on the prop/field.