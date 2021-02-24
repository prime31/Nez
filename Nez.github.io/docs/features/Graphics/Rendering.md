---
id: Rendering
title: Rendering
---

First and foremost with regard to rendering is the type of filtering used for your textures. Nez has several subsystems (such as Renderers, Scenes and PostProcessors) that all need to know how you want your textures to look. Everything is configurable on a per object basis but you will want to set a default as well so you don't have to bother changing the SamplerState all over the place. `Core.DefaultSamplerState` should be set before you create your first Scene. It defaults to `SamplerState.PointClamp` which is good for pixel art. If you are using high-def art then make sure you set it to SamplerState.LinearClamp so you don't get ugly results.

The Nez rendering setup was designed to be really easy to get up and running but at the same time flexible so that advanced users can do whatever they need to out of the box. The basic gist of how the rendering system works revolves around the `Renderer` class. You add one or more Renderers to your Scene (`AddRenderer` and `RemoveRenderer` methods) and each of your Renderers will be called after all Entities/Components have had their update method called. All rendering is done into a RenderTexture which is then displayed (with optional post processing) after all Renders have finished rendering. Several default Renderers are provided to get you started and cover the most common setups. The included renderers are described below:

- **DefaultRenderer**: renders every RenderableComponent that is enabled in your scene
- **RenderLayerRenderer**: renders only the RenderableComponents in your Scene that are on the specified renderLayers
- **RenderLayerExcludeRenderer**: renders all the RenderableComponents in your Scene that are not on the specified renderLayers

You are free to subclass Renderer and render things in any way that you want. The Scene contains a RenderableComponents field that contains all the RenderableComponents for easy access and filtering. The `RenderableComponentList` provides access by specific RenderLayer as well. By default, RenderableComponents are sorted RenderLayer and then by LayerDepth for fine-grained render order control. This is overrideable allowing you to sort however you want (see below). The Renderer class provides a solid, configurable base that lets you customize various attributes as well as render to a `RenderTexture` instead of directly to the backbuffer. If you do decide to render to a RenderTexture in most cases you will want to use a `PostProcessor` to draw it later. It should also be noted that RenderTextures on a Renderer are automatically resized for you when the screen size changes. You can change this behavior via the `RenderTexture.ResizeBehavior` enum.

Sometimes you will want to do some rendering after all PostProcessors have run. For example, in most cases your UI will be rendered without any post process effects. To deal with cases like these a `Renderer` can set the `Renderer.wantsToRenderAfterPostProcessors` field. This must be set *before* calling `AddRenderer` for it to take effect.


## Custom Sorting
You can set a custom sort for all the `IRenderables` in your Scene by setting the static field `RenderableComponentList.CompareUpdatableOrder`. The sort class must be a subclass of `IComparer<IRenderable>`. Each RenderLayer list of IRenderables will be sorted with the comparer. Note that you must set the sort dirty flag manually when appropriate based on your implementation. By default, lists are only dirtied when components are added. You can set the dirty flag for any RenderLayer via `Scene.RenderableComponents.SetRenderLayerNeedsComponentSort`. For example, if you are doing an isometric game and sorting based on the y-value of the Entity's Transform you would need to call `Scene.RenderableComponents.SetRenderLayerNeedsComponentSort` whenever the Entity moves on the y-axis.


## Post Processors
Much like Renderers, you can add one or more PostProcessors to the Scene via the **AddPostProcessor** and **RemovePostProcessor** methods. PostProcessors are called after all Renderers have been called. One common use case for a PostProcessor is to display a RenderTexture that a Renderer rendered into most often with some Effects applied. Applying effects to the fully rendered scene is also a very common use case. You can globally enable/disable PostProcessors via the **Scene.EnablePostProcessing** bool. Additionally, each PostProcessor can be enabled/disable for fine-grained control.

A basic example of a PostProcessor is below. It takes a RenderTexture that a Renderer rendered into and composites that with the rest of the scene with an Effect.

```csharp
public class SimplePostProcessor : PostProcessor
{
	RenderTexture _renderTexture;
	
	public SimplePostProcessor( RenderTexture renderTexture, Effect effect ) : base( 0 )
	{
		_renderTexture = renderTexture;
		this.effect = effect;
	}


	public override void Process( RenderTarget2D source, RenderTarget2D destination )
	{
			Core.graphicsDevice.SetRenderTarget( destination );

			Graphics.instance.spriteBatch.Begin( effect: effect );
			// render source contains all of the Scene that was not rendered into _renderTexture
			Graphics.instance.spriteBatch.Draw( source, Vector2.Zero, Color.White );
			
			// now we render the contents of our _renderTexture on top of it
			Graphics.instance.spriteBatch.Draw( _renderTexture, Vector2.Zero );
			Graphics.instance.spriteBatch.End();
	}
}
```


## Mixing 2D and 3D Content
Nez is technically a 2D framework. That being said, there may be times when you want to stick some 3D content into your 2D game. Nez attempts to make it very simple to add 3D content. The Nez Camera class is a 5D system (2D + 3D). What it is doing under the hood is really quite simple: it provides a `ViewMatrix3D` and `ProjectionMatrix3D` that you can use to render 3D content. The matrices returned will end up with a frustum that at a z-position of 0 exactly matches the 2D orthographic matrices. The grand result of all this is that you can position your 3D objects x and y values and it will match exactly your 2D content.

There are a few 3D specific fields that you can tweak on the Camera as well (all 3D fields/properties have a suffix of **3D** for clarity): `PositionZ3D`, `NearClipPlane3D`, `FarClipPlane3D`. When dealing with 3D content it is important to note that your distances and scales will be very large. The reason for this is because the 2D orthographic view is huge compared to a standard 3D camera. You may need to play with your scales depending on which modeling software you use.

Nez provides some built in Components to make it as simple as possible to get some 3D content on screen. The `Cube3D`, `Sphere3D` and `Torus3D` classes can be added to an Entity straight away. These classes procedurally create their meshes. The `Model3D` class can be used when you want to display a 3D model that was processed by the MonoGame content builder. It will display a standard `Model`.

All of the *3D classes subclass `Renderable3D`. `Renderable3D` is a thin wrapper that provides Vector3s for position, rotation and scale. These wrap the 2D values from the standard `Transform` class where possible so that you can use `Transform` parenting with 2D and 3D objects seamlessly.


## IFinalRenderDelegate
By default, Nez will take your fully rendered scene and render it to the screen. In some rare circumstances this may not be the way you want the final render to occur. By setting **Scene.FinalRenderDelegate** you can take over that final render to the screen and do it however you want. For an example of using the *IFinalRenderDelegate* see the `PixelMosaicRenderDelegate` class.