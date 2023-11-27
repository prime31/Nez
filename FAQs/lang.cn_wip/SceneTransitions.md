Scene Transitions 场景过渡
==========
每个游戏都需要或多或少的过渡特效, 这样它们看起来就会更精美一点. 在 Nez 中这很简单! 过渡可以很简单地在多场景或单场景实现. Nez 内置了一些过渡效果并且也允许你很简单的制作自定义的过渡效果.

## Using Included Transitions 使用内置的过渡效果
所有的内置过渡效果都能在单场景内和多场景间使用. 它们其中的一些已经配置好了过渡的具体表现. 以下是一些例子:

```cs
// 加载一个新的场景, 使用 WindTransition 过渡效果. 主要你需要提供一个提供目标场景的 `Func<Scene>` 委托.
Core.StartSceneTransition( new WindTransition( () => new YourNextScene() ) );


// 使用 SquaresTransition 进行单场景内过渡
var transition = new SquaresTransition();

// 对于单场景内过渡我们可能更想知道现在屏幕变成什么样子了, 所以我们可以:
transition.OnScreenObscured = MyOnScreenObscuredMethod;
Core.StartSceneTransition( transition );


void MyOnScreenObscuredMethod()
{
	// 移动摄像机到新位置
	// 重置Entities
}
```


## Custom Transitions 自定义过渡
过渡的厉害之处在于能够创建我们自己符合游戏风格的类型. 你可以使用内置的过渡为基础. 这篇指南将会讲述这些细节, 包括怎么做和为什么.

过渡通常有两种方式: 单部分和双部分. 单部分的过渡会渲染逐渐模糊上一个场景, 然后加载新场景, 然后从旧的渲染到带有效果的新场景渲染(比如, 旧场景滑出屏幕). 一个双部分的过渡首先会展示一个过渡特效, 然后加载新场景, 过渡展示新场景(比如: 慢慢变黑, 加载新场景, 然后渐变回到新场景). 这两种方式的处理方法都是很相似的

- 继承 `SceneTransition`
- 在构造器里加载 `Effect` 如果你需要的话
- `Render` 每帧都会被调用所以你能控制渲染结果. 在双部分过渡时有个flag叫 `_isNewSceneLoaded`, 你可以用这个知道你现在在哪个部分.
- 重写 `OnBeginTransition` 方法, 这个方法组成了所有的过渡代码. 它是被作为协程调用的, 所以你可以在控制流里进行yield.
	- (可选的)对于双部分过渡, 你可能会先把第一个部分过渡完, 比如渐变为黑色.
	- yield调用加载下一个场景: `yield return Core.StartCoroutine( LoadNextScene() )`. 注意你甚至需要在场景内过渡(intra-Scene transitions)里干这事. Nez会准确的设置 `_isNewSceneLoaded` 这个flag即使是在场景内过渡里, 这是为了让同样的例子中双部分过渡生效.
	- 开始你的过渡 (比如, 上个场景画面渐渐消失)
	- 调用 `TransitionComplete`, 以结束过渡及清理 RenderTarget2D.
	- 释放任何用到的 Effects/贴图, 或者你也可以重写 `TransitionComplete` 然后在里面做清理工作. 但是注意记得调用下基类的方法!

`SceneTransition` 类有个轻量很好用的方法在你在使用带 Effect 的过渡时. `TickEffectProgressProperty` 能让你 yield 调用它在一个协程里, 它会帮你设置你的 Effect 的 `_progress` 属性(0-1或者1-0). 单双部分过渡都可用. 关于这个的例子你可以去看看内置的过渡 shaders.

让我们来看看一个使用 Effect 的过渡是什么样子的. 注释详细的告诉你都发生了些什么.

```cs
public class SuperTransition : SceneTransition
{
	/// <summary>
	/// 过渡时间长度
	/// </summary>
	public float Duration = 1f;

	/// <summary>
	/// 对动画使用的缓动类型
	/// </summary>
	public EaseType EaseType = EaseType.QuartOut;

	Effect _effect;
	Rectangle _destinationRect;


	/// <summary>
	/// 构造器. 注意场景内过渡时 sceneLoadAction 可null. 并不会影响什么
	/// </summary>
	public SuperTransition( Func<Scene> sceneLoadAction ) : base( sceneLoadAction, true )
	{
		// 储存场景的画面(store off the bounds of the scene render), 这样我们能在渲染方法里展示它
		_destinationRect = previousSceneRender.Bounds;

		// 加载 Effect
		_effect = Core.content.loadEffect( "TransitionEffect.mgfxo" );
	}


	public override IEnumerator OnBeginTransition()
	{
		// 加载新场景. 如果 sceneLoadAction 是空的的话我们把 _isNewSceneLoaded 设置为true就行了.
		yield return Core.StartCoroutine( LoadNextScene() );

		// 使用我们自己的 Effect 去过渡. 这个调用会让 _progress EffectParameter 从0到1使用我们自己的缓动类型去额外控制
		yield return Core.StartCoroutine( TickEffectProgressProperty( _effect, duration, easeType ) );

		// 调用这个方法让 SceneTransition 知道我们完事了, 然后它会做清理工作并且停止渲染.
		TransitionComplete();

		// 因为完事了, 所以记得清理 Effect
		Core.Content.UnloadEffect( _effect );
	}


	public override void Render( Batcher batcher )
	{
		// 在这里, 我们仅仅是直接用我们的 Effect 渲染到缓冲区里. 我们就渲染 SceneTransition 给我们的上一个场景的画面
		// 它就包含了上个场景的最后一个画面
		Core.graphicsDevice.SetRenderTarget( null );
		batcher.Begin( BlendState.NonPremultiplied, Core.DefaultSamplerState, DepthStencilState.None, null, _effect );
		batcher.Draw( previousSceneRender, _destinationRect, Color.White );
		bbatcher.End();
	}
}
```
