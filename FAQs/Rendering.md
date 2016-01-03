Rendering
==========

The Nez rendering setup was designed to be really easy to get up and running but at the same time flexible so that advanced users can do whatever they need to out of the box. The basic gist of how the rendering system works revolves around the **Renderer** class. You add one or more Renderers to your Scene (**addRenderer** and **removeRenderer** methods) and each of your Renderers will be called after all Entities/Components have had their update method called. All rendering is done into a RenderTexture which is then displayed (with optional post processing) after all Renders have finished rendering. Several default Renderers are provided to get you started and cover the most common setups. If you create your scene with the **Scene.createWithDefaultRenderer** method as the name suggests it will create a DefaultRenderer for you. The included renderers are described below:

- **DefaultRenderer**: renders every RenderableComponent that is enabled in your scene
- **RenderLayerRenderer**: renders only the RenderableComponents in your Scene that are on renderLayer
- **RenderLayerExcludeRenderer**: renders all the RenderableComponents in your Scene that are not on renderLayer

You are free to subclass Renderer and render things in any way that you want. The Scene contains a renderableComponents field that contains all the RenderableComponents for easy access and filtering. The RenderableComponentList provides access to all the RenderableComponents in the scene or just those on a specific renderLayer. RenderableComponents are sorted by layerDepth in each of the renderLayer lists for fine-grained render order control. The Renderer class provides a solid, configurable base that lets you customize various attributes as well as render to a RenderTexture instead of directly to the framebuffer. If you do decide to render to a RenderTexture in most cases you will want to use a PostProcessor to draw it later.


Post Processing
==========

Much like Renderers, you can add one or more PostProcessors to the Scene via the **addPostProcessor** and **removePostProcessor** methods. PostProcessors are called after all Renderers have been called. The most common use case for a PostProcessor is to display a RenderTexture that a Renderer rendered into most often with some Effects applied. You can globally enable/disable PostProcessors via the **Scene.enablePostProcessing** bool. Additionally, each PostProcessor can be enabled/disable for fine-grained control.

The most basic example of a PostProcessor is below. It takes a RenderTexture that a Renderer rendered into and displays it onscreen.

```
public class SimplePostProcessor : PostProcessor
{
	public SimplePostProcessor( RenderTexture renderTexture, Effect effect ) : base( 0 )
	{
		_renderTexture = renderTexture;
		this.effect = effect;
	}


	public override void process( RenderTexture source, RenderTexture destination )
	{
	    drawFullScreenQuad( source, destination, effect );
	}
}
```