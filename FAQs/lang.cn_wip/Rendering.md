Rendering 绘制
==========

首先最需要关心的是你的贴图所使用的过滤类型. Nez 有几个子系统(比如Renderer, Scene 和 PostProcessors)都需要知道你对你的贴图的期望的外观. 任何东西都是可以被逐对象设置的, 但是同时你可能会想设置一个默认的外观让你不用每个地方都设置. `Core.DefaultSamplerState` 就是你在创建你第一个场景时所需要的设置的. 它默认是 `SamplerState.PointClamp` (对像素游戏友好). 如果你用的是高分辨率图的话记得设置成 `SamplerState.LinearClamp` 不然会得到很丑很模糊的结果.

Nez 的渲染准备工作被设置的十分简单同时在运行时的时候它也表现的很有弹性, 同时比较深入的用户能很轻易自定义他们所需要的. 渲染系统的要点主要围绕着 `Renderer` 这个东西. 你可以加入一个或者多个 Renderer (方法`AddRenderer` 和 `RemoveRenderer`) 然后每个 Renderer 就会在你所有 Entity 和 Component Update 后做它们的渲染工作. 所有渲染工作都会最终在 RenderTexture 上完成(这个东西之后会显示在屏幕上, 同时也有自定义的 PostProcess). Nez 提供了几个默认的 Render 以便让你很容易的开始, 这些Render包含:

- **DefaultRenderer**: 渲染你场景中所有的 `RenderableComponent`
- **RenderLayerRenderer**: 只渲染指定图层的 `RenderableComponent`
- **RenderLayerExcludeRenderer**: 渲染除指定图层外的 `RenderableComponent`
  

你可以很自由地继承 Renderer 并且以任何你喜欢的方式绘制东西. 场景包含一个 RenderableComponents 字段包含所有你场景中的 RenderableComponent 让你很容易的控制和过滤这些渲染部件. `RenderableComponentList` 也同时提供了对应到渲染层的控制. 默认的, RenderableComponent 会先以 RenderLayer 排序然后以 LayerDepth 精密的排序. 这个排序同时也是可重写的. Renderer 提供了一个很稳定可控的基类让你能够自定义很多不同的特性包括渲染到 `RenderTexture` 而不是直接地到显示缓冲区内. 如果你决定去渲染一个 `RenderTexture`, 大部分情况下你会想用 `PostProcessor` 让它等会再绘制. 同样需要注意的是, 在 Renderer 上的 `RenderTexture` 会自动重设大小当屏幕大小改变时. 你可以用 `RenderTexture.ResizeBehavior` 枚举来改变这个默认的行为.


有些时候你可能会想在所有的 PostProcessor 完成后再绘制些东西. 比如, 在大部分例子中, UI 总是会被以没有任何后处理效果(post processor)而绘制. 为了解决这些问题你可以设置 `Renderer` 的 `Renderer.wantsToRenderAfterPostProcessors` 字段. 这个字段必须在调用 `AddRenderer` **之前**设置才会起效.

## Custom Sorting 自定义排序

你可以设置所有 `IRenderables` 的自定义渲染排序通过设置你场景的 `RenderableComponentList.CompareUpdatableOrder` 字段. 这个排序类必须实现 `IComparer<IRenderable>` 接口. 每个渲染层的 IRenderables 们都会通过这个排序器进行排序. 注意你必须在必要时手动设置排序的脏标志(dirty flag). 默认的, 列表只会在 component 被加入时才会产生脏标志. 你可以通过 `Scene.RenderableComponents.SetRenderLayerNeedsComponentSort` 给任何一个 RenderLayer 设置这个脏标志. 比如, 你可能正在制作一个等距(isometric)游戏然后你需要把场景中的物体以它的y坐标做排序. 你这时候可能就会想在物体y坐标改变时重新排序渲染顺序通过调用 `Scene.RenderableComponents.SetRenderLayerNeedsComponentSort`.


## Post Processors 后处理

像 Renderer 一样, 你可以在场景中通过 **AddPostProcessor** 和 **RemovePostProcessor** 方法添加 PostProcessor. PostProcessor 会在所有渲染工作做完后调用. 一个通常的使用例子是, 带一些 Effect 地绘制一个被其他 Renderer 绘制完后的 RenderTexture. 给整个场景都使用一个 Effect 也是个很常用的例子. 你可以通过改变 **Scene.EnablePostProcessing** 这个值来全局启用/禁用 PostProcessor. 额外地, 每个 PostProcessor 都能精细的控制它们是否启用.

一个基本的使用例子如下. 它用了一个其他 Renderer 渲染完后的 RenderTexture, 给它上个 Effect, 然后和场景中其他东西放在一起.

```cs
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
			// source包含场景上所有没渲染到_renderTexture里的东西
			Graphics.instance.spriteBatch.Draw( source, Vector2.Zero, Color.White );
			
			// 现在我们可以渲染我们的_renderTexture在它上面了
			Graphics.instance.spriteBatch.Draw( _renderTexture, Vector2.Zero );
			Graphics.instance.spriteBatch.End();
	}
}
```


## Mixing 2D and 3D Content 混合 2d 3d 内容

Nez 是一个专注2d的框架. 这可能令人失望, 但是有些时候你可以贴一些 3d 内容到你的 2d 游戏里. Nez 尽量让添加 3d 内容变简单一些. Nez 的 Camera 类是一个5D系统(2D + 3D). 在这种情况下它也很简单: Nez 提供了一个 `ViewMatrix3D` 和一个 `ProjectionMatrix3D` 让你能够渲染 3d 物体. 这些矩阵会以一个 z 为 0 的平截头体结束以符合 2D 的正交矩阵. 这最大的好处是你设置的 3D 物体的 xy 值与你的 2D 内容相符合.

`Camera` 上也有一小些 3D 特定的字段你可以调整(所有 3D 相关的字段/属性都以**3D**前缀标明清楚): `PositionZ3D`, `NearClipPlane3D`, `FarClipPlane3D`. 当解决 3D 内容时注意它们的距离和缩放比例可能会非常大. 原因是因为 2D 的正交视野相比于标准的 3D 摄像机来说大了很多. 你可能需要根据你的模型软件来调弄你的缩放比例.

Nez 提供了一些内建的组件让 3D 内容呈现在屏幕上尽可能的容易. 比如 `Cube3D`, `Sphere3D`, `Torus3D` 类就可以被直接加入到 Entity 上. 这些类都会自己创建它们对应的 Mesh. `Model3D` 类也可以用来呈现由 MonoGame content builder 制作的 3D 模型. 它会呈现一个标准的 `Model`.

所有的 3D 类都会继承 `Renderable3D`. `Renderable3D` 是一个小的 wrapper, 它提供了一些 `Vector3` 类型的位置, 旋转, 位移属性. 它们封装了来自标准的 `Transform` 类的 2D 值, 所以你可以毫无阻碍地在 2D 和 3D 对象之间继承 `Transform`.

## IFinalRenderDelegate
默认 Nez 会将你渲染完全后的场景渲染到屏幕上, 在一些少见的情况下可能这不是你想要的结果. 通过设置 **Scene.FinalRenderDelegate**, 你可以接管最后渲染到屏幕上的操作然后做任何你想要做的事. 比如说一个使用*IFinalRenderDelegate*的例子, 见 `PixelMosaicRenderDelegate` 类.