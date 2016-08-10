---
layout: docs
permalink: documentation/systems/rendering
---

Rendering
==========

First and foremost with regard to rendering is the type of filtering used for your textures. Nez has several subsystems (such as Renderers, Scenes and PostProcessors) that all need to know how you want your textures to look. Everything is configurable on a per object basis but you will want to set a default as well so you don't have to bother changing the SamplerState all over the place. `Core.defaultSamplerState` should be set before you create your first Scene. It defaults to `SamplerState.PointClamp` which is good for pixel art. If you are using high-def art then make sure you set it to SamplerState.LinearClamp so you don't get ugly results.

The Nez rendering setup was designed to be really easy to get up and running but at the same time flexible so that advanced users can do whatever they need to out of the box. The basic gist of how the rendering system works revolves around the `Renderer` class. You add one or more Renderers to your Scene (`addRenderer` and `removeRenderer` methods) and each of your Renderers will be called after all Entities/Components have had their update method called. All rendering is done into a RenderTexture which is then displayed (with optional post processing) after all Renders have finished rendering. Several default Renderers are provided to get you started and cover the most common setups. The included renderers are described below:

- **DefaultRenderer**: renders every RenderableComponent that is enabled in your scene
- **RenderLayerRenderer**: renders only the RenderableComponents in your Scene that are on the specified renderLayers
- **RenderLayerExcludeRenderer**: renders all the RenderableComponents in your Scene that are not on the specified renderLayers

You are free to subclass Renderer and render things in any way that you want. The Scene contains a renderableComponents field that contains all the RenderableComponents for easy access and filtering. The `RenderableComponentList` provides access by specific renderLayer as well. RenderableComponents are sorted by layerDepth in each of the renderLayer lists for fine-grained render order control. The Renderer class provides a solid, configurable base that lets you customize various attributes as well as render to a `RenderTexture` instead of directly to the framebuffer. If you do decide to render to a RenderTexture in most cases you will want to use a PostProcessor to draw it later. It should also be noted that RenderTextures on a Renderer are automatically resized for you when the screen size changes. You can change this behavior via the RenderTexture.resizeBehavior enum.

Sometimes you will want to do some rendering after all PostProcessors have run. For example, in most cases your UI will be rendered without any post process effects. To deal with cases like these a `Renderer` can set the `Renderer.wantsToRenderAfterPostProcessors` field. This must be set *before* calling `addRenderer` for it to take effect.



## Post Processors
Much like Renderers, you can add one or more PostProcessors to the Scene via the **addPostProcessor** and **removePostProcessor** methods. PostProcessors are called after all Renderers have been called. One common use case for a PostProcessor is to display a RenderTexture that a Renderer rendered into most often with some Effects applied. Applying effects to the fully rendered scene is also a very common use case. You can globally enable/disable PostProcessors via the **Scene.enablePostProcessing** bool. Additionally, each PostProcessor can be enabled/disable for fine-grained control.

A basic example of a PostProcessor is below. It takes a RenderTexture that a Renderer rendered into and composites that with the rest of the scene with an Effect.

```cs
public class SimplePostProcessor : PostProcessor
{
	RenderTexture _renderTexture;
	
	public SimplePostProcessor( RenderTexture renderTexture, Effect effect ) : base( 0 )
	{
		_renderTexture = renderTexture;
		this.effect = effect;
	}


	public override void process( RenderTarget2D source, RenderTarget2D destination )
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



## IFinalRenderDelegate
By default, Nez will take your fully rendered scene and render it to the screen. In some rare circumstances this may not be the way you want the final render to occur. By setting **Scene.finalRenderDelegate** you can take over that final render to the screen and do it however you want.