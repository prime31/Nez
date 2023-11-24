Nez Core
==========
在 Nez 的世界里最根基的类是 Core, Core 继承于 XNA 框架中的 Game 类, 你的游戏的类也应该继承于 Core. Core 提供了一系列通过静态字段和方法操作所有重要的子系统的方式

## Graphics 图形
Nez 会在初始化时帮你实例化一个 Graphics 对象 (通过`Graphics.Instance`获取). 该对象包含一个默认的 BitmapFont 字体, 这样你可以很快的用这个不错的字体来渲染文字, 通常这已经满足了大部分你的渲染需求 (MonoGames 的 SpriteFont 通常被压缩的很糟糕). Graphics 对象提供了直接操作 SpriteBatch 对象的能力. 另外你还可以通过 Nez 实现的 SpriteBatch 的拓展函数绘制矩形, 圆, 线等图形.

## Scene 场景
当你将 `Core.Scene` 设置为一个新场景时, Nez 将会渲染完当前场景, 触发 `CoreEvents.SceneChanged` 事件, 然后开始渲染新的场景. 更多关于Scenes的信息见 FAQ: [Scene-Entity-Component](./Scene-Entity-Component.md)


## Sprites
2D 游戏中 sprites 是很重要的东西, Nez 提供了非常多的渲染 sprites 的方式, 从最基础的单贴图渲染, 到精灵图集的支持, 到 9Patch sprites 的支持. 一些常见的 sprite 组件有 `SpriteRenderer`, `SpriteAnimator`, `SpriteTrail`, `TiledSprite`, `ScrollingSprite` and `PrototypeSprite`. 在2D游戏中最重要也最常见的是静态 sprites 和 sprites 动画, 例子如下:

```csharp
// 从单张图片加载一个贴图到一个静态的 SpriteRenderer
var texture = Content.Load<Texture2D>("SomeTex");

var entity = CreateEntity("SpriteExample");
entity.AddComponent(new SpriteRenderer(texture));
```

```csharp
// 从 16*16 的动画图集加载一个贴图
var texture = Content.Load<Texture2D>("SomeCharacterTex");
var sprites = Sprite.SpritesFromAtlas(texture, 16, 16);
			
var entity = CreateEntity("SpriteExample");

// 加入一个 SpriteAnimator 组件, 它的作用是渲染当前动画的当前帧对应的贴图
var animator = entity.AddComponent<SpriteAnimator>();

// 加入一些动画
animator.AddAnimation("Run", sprites[0], sprites[1], sprites[2]);
animator.AddAnimation("Idle", sprites[3], sprites[4]);

// 等一会后播放这个动画
animator.Play("Run");
```


## Sprite Atlases 精灵图集
大多数时候, 使用精灵图集来做一个 2d 游戏是很明智的. Nez 有一个精灵图集的打包工具和一个运行时的精灵图集加载器. 详见[这个README](../Nez.SpriteAtlasPacker/README.md). 这里是一个简单的使用例子. 我们将会使用如下的目录结构做例子, 贴图可以在任何一个目录里, 其中 `door-dir` 目录的贴图将不会是动画的一部分. 子目录里的贴图才是动画的一部分, 并且子目录的名字也是动画的名字.

- root-dir
	- player
	- enemy1
	- enemy2

为了生成一个精灵图集和数据文件, Nez 需要使用如下命令加载该图集.

`mono SpriteAtlasPacker.exe -image:roots.png -map:roots.atlas path/to/root-dir`

复制 `roots.png` 和 `roots.atlas` 到你的项目的 Content 目录. 注意这两个文件名字必须相同. 现在我们可以用如下代码加载和使用图集了:

```csharp
var atlas = Content.LoadSpriteAtlas("Content/roots.atlas");

// 从图集获取精灵
var sprite = atlas.GetSprite("sprite-name.png");

// 获取一个动画
var animation = atlas.GetAnimation("enemy1");

// SpriteAnimator 提供了一个更简单的方法直接使用精灵图集
// 这里的 animator 已经被其他地方加载了
animator.AddAnimationsFromAtlas(atlas);
animator.Play("enemy2");
```


## Physics 物理系统
注意不要混淆 Nez 的物理系统和现实物理引擎 (比如Box2D, Farseer, Chipmunk)! 这不是 Nez 的目的. 物理系统在这里的目的是提供空间和碰撞信息的, 并不是去提供一个完整、真实的物理模拟. 物理系统的核心是一个 SpatialHash, 它会在你 增加/移除/移动 碰撞体(Colliders) 自动更新. 你可以高性能地通过 **Physics** 类中的很多物理相关的方法去处理 BroadPhase 碰撞检测, 比如 boxcast 和 raycast 等方式. Nez 的物理系统内部用很多不同的形状比如矩形, 圆形, 多边形去检测碰撞. Entity 类提供了用于移动的函数来处理这一大堆复杂的东西, 你要做的仅仅是去查询物理系统得到的结果或者是自己去处理 narrow phase 碰撞检测.


## TimerManager 计时器管理器
TimerManager 是一个简单的帮助类, 你可以传入一个 Action 来让它帮你执行一次或者带或不带延迟的重复执行很多次. **Core.Schedule** 方法提供了更简单的方式去操作 TimerManager. 调用 **Schedule** 方法会返回一个ITimer对象, 该对象有一个 **Stop** 方法让你能在再次开始前停止这个计时器.


## CoroutineManager 协程管理器
CoroutineManager 允许你传入一个 IEnumerator, 之后每一帧都会触发一次. 这允许你将一个长时间运行的任务分割成很多部分. 使用 **Core.StartCoroutine** 方法开启一个协程, 该方法返回一个 ICoroutine 对象, 该对象只有一个方法: **Stop**. 协程的运行可以随时使用 C# 中的 yield 语句暂停. 你可以 yield 返回一个 `Coroutine.WaitForSeconds`, 它的作用是暂停这个函数 N 秒. 在协程函数里面你也可以 yield 返回另一个迭代器, 只有内部的迭代器完成后才会再继续执行外层的迭代器.


## Emitter\<CoreEvents\>
Core 提供了一个 Emitter 让你可以在一些关键时候触发一些事件. 具体是通过 **Core.Emitter.AddObserver** 和 **Core.Emitter.RemoveObserver**. **CoreEvents** 枚举定义了一系列可用的事件.

**Emitter\<T\>** 类同样对你自己的类有效. 你可以使用int, 枚举, 或任何结构体键入(原文动词:key)一个事件. 它真的只被设计为只有 int 和枚举可用, 但是并没有一个合适的泛型约束去约束成这两个类型. 注意如果你使用枚举类型作为事件类型的话我们推荐传入一个 IEqualityComparer\<T\> 到 Emitter 的构造器以避免装箱, 这能有效提升性能. 你可以复制一个简单的模板在 **CoreEventsComparer** 去制作自己的IEqualityComparer\<T\>.

## Debug Console 调试控制台
如果你使用 DEBUG 编译符号编译你的程序的话, Nez 会包含一个简单的控制台以提供一些有用的信息. 按下你键盘上的 ~ 键去打开/关闭控制台. 在打开的时候你可以输入 'help' 去查看有什么可以用的指令. 比如输出所有加载的资源、总实体数目、被 SpatialHash 管理的碰撞体之类的. 同时你可以输入 'help COMMAND' 以得到有关 COMMAND 的更详细的信息

![in-game debug console](../images/console.png)

你可以同样很简单地加入你自己的命令到调试控制台里. 只需要加入一个 **CommandAttribute** 特性到任何一个静态方法并且指定命令昵称和帮助说明. 同时命令允许带一个参数. 这是其中一个简单的内建指令的例子:

```cs
[Command( "assets", "Logs all loaded assets. Pass 's' for scene assets or 'g' for global assets" )]
static void LogLoadedAssets( string whichAssets = "s" )
```

## Global Managers 全局管理器
Nez 让你能够添加一个全局管理器对象, 它会有一个每帧在 Scene.update 方法之前被调用的 update 方法. 任何在场景变更时需要持久化的系统都可以放在这里. Nez 有一些自己的系统放置在全局管理器里, 比如 scheduler, 协程管理器, 和缓动管理器. 你可以通过 `Core.RegisterGlobalManager` 和 `Core.UnregisterGlobalManager` 来 注册/注销 你自己的全局管理器.


一些其他重要的静态类
==

## Time
Time 类提供了一个简单, 静态的方式去访问 deltaTime, unscaledDeltaTime, timeScale 和一些其他有用的属性. 为了更好的使用它也提供了一个 altDeltaTime/altTimeScale 让你很轻松的建立不同的时间轴去运行, 而不是让你自己管理.


## Input
你会很容易猜到, Input 类允许你访问所有的输入(鼠标, 键盘, 手柄). 所有常见的按钮术语的定义如下:

- **Down**: 当键被按下时一直触发
- **Pressed**: 仅当按下的第一帧时触发
- **Released**: 仅当按下后松开的第一帧触发

Nez 也提供了几个虚拟输入的类, 这允许你合并不同的按键到一个类中方便你查询. 比如, 你可以设立一个 VirtualButton 映射很多不同的实体输入方式. 比如让物体向右移动, 你可以创建一个虚拟按键用D键, 右箭头键, Dpad-right 键和手柄的旋轴. 仅仅通过查询虚拟按键的状态就能知道它们其中一个按键是否按下. 类似的其他常见的场景也一样. 虚拟输入允许你自己模拟一个输入, 比如模拟按键输入(VirtualButton), 模拟手柄旋轴输入(`VirtualJoystick`)和手柄的数字输入(原文 digital(on/off) joystick emulation)(`VirtualIntegerAxis`). 下面是一个映射不同实体按键到一个虚拟按键的示例:

```csharp
    void SetupVirtualInput()
    {
        // 设置一个发射火球的虚拟按键, 我们将会加入 z 键或者手柄上的 a 键
        _fireInput = new VirtualButton();
        _fireInput.AddKeyboardKey( Keys.X )
                    .AddGamePadButton( 0, Buttons.A );

        // 来自 dpad 的水平输入, 手柄旋轴或者键盘的左右键
        _xAxisInput = new VirtualIntegerAxis();
        _xAxisInput.AddGamePadDPadLeftRight()
                    .AddGamePadLeftStickX()
                    .AddKeyboardKeys( VirtualInput.OverlapBehavior.TakeNewer, Keys.Left, Keys.Right );

        // 来自 dpad 的纵向输入, 手柄旋轴或者键盘的上下键
        _yAxisInput = new VirtualIntegerAxis();
        _yAxisInput.AddGamePadDpadUpDown()
                    .AddGamePadLeftStickY()
                    .AddKeyboardKeys( VirtualInput.OverlapBehavior.TakeNewer, Keys.Up, Keys.Down );
    }
        
            
    void IUpdatable.Update()
    {
        // 获取上面我们设置的虚拟按键的状态
        var moveDir = new Vector2( _xAxisInput.Value, _yAxisInput.Value );
        var isShooting = _fireInput.IsPressed;
    }
```


## Debug
Debug类提供了一些输出和少量绘制函数. `Insist` 类提供了一类断言条件. 这些类仅仅在 DEBUG 符号定义时你才能自由使用它们. 在你使用非 DEBUG 符号编译时它们不会被编译进你的游戏.


## Flags
你喜欢把一大堆单独的数据打包进一个单独的 int 但是处理起来超级麻烦吗? Flags 类可以帮助你! 它包含很多处理 int 的逐 bit 的方法. 这在你处理比如 Collider.physicsLayer 的时候很有用!
