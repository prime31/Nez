Scene/Entity/Component 场景/实体/组件
==========

Nez 的核心系统是一个 EC 架构(Entity-Component). Nez 的 EC 架构和其他你用过的 EC 架构很相似, 所以你应该很快就会习惯的.

## Scene 场景
EC 架构的根是场景. 场景可以是任何你游戏内的部分, 比如什么菜单啊, 关卡啊, 制作人名单啊之类的. 场景管理了一系列的实体(Entities), 渲染器(Renderers)和后处理效果(PostProcessors, 通过加入或删除方法), 并在适合的时间里调用它们的方法. 你可以使用 **FindEntity** 和 **FindEntitiesByTag** 方法来查找实体. 你同样也能够创建一个带摄像机的场景.

场景提供了一个叫 NezContentManager (Scene.contentManager)的东西, 用这个东西你能够加载场景所需的资源. 当场景结束时这些资源会被自动释放. 如果你需要加载全局的资源的话(跨场景), 你可以用 Core.contentManager, 它仅可以以显式的方式释放资源.

一个场景可以直接包含一系列的叫 `SceneComponent` 的组件. 场景组件是被 add/get/getOrCreate/removeSceneComponent 这些方法管理的. 一个场景组件可以被理解为简化的组件. 它只包含相对于组件来说的一小部分生命周期方法(onEnabled/onDisabled/update/onRemovedFromScene). 在你需要添加一个不依赖于实体的组件时场景组件是个不错的选择. Nez 使用场景组件的一个例子就是 Farseer, 它管理了整个场景的物理模拟.

Nez 提供了一系列高效可拓展的决定场景最终渲染效果的方法. 它使用了一个叫 `SceneResolutionPolicy` 的概念去管理物体如何渲染. `SceneResolutionPolicy` 和你期望的宽高决定了 RenderTarget2D 的大小和窗口的大小. 场景分辨率策略(SceneResolutionPolicy)也包含了用于像素游戏的很多像素完美的变种. 这些变种可以会导致 letter boxing 或 pillar boxing. 但你能通过 **Scene.LetterboxColor** 来控制. 同样你能设置场景的默认策略, 使用 **Scene.SetDefaultDesignResolution** 方法就行了. 以下是一些自带的策略:

- **None**: 默认的, RenderTarget2D 的大小与屏幕一致
- **ExactFit**: 应用画面完全填充入窗口中, 会尝试拉伸以适合屏幕
- **NoBorder**: 填充指定区域, 无失真, 会保持横纵比的情况下裁剪超出范围
- **NoBorderPixelPerfect**: 像素完美版本的 NoBorder, 缩放比会限制为整数.
- **ShowAll**: 应用程序无扭曲无裁剪保持横纵比的置于窗口中, 窗口多余部分会出现黑边
- **ShowAllPixelPerfect**: 像素完美版本的 ShowAll, 缩放比会限制为整数.
- **FixedHeight**: 保持期望高, 改变宽度以适应画布变化. 无失真. 但是你得保证你的程序在不同的比例都正常.
- **FixedHeightPixelPerfect**: 固定高度的像素完美版本, 缩放比会限制为整数.
- **FixedWidth**: 同FixedHeight, 但是保持期望宽, 改变高以适应.
- **FixedWidthPixelPerfect**: 固定宽度的像素完美版本.
- **BestFit**: 应用采用最适合的方式适应期望的宽高. 可在溢出区域(bleed area)内可选的裁剪, 并可能使用 letter/pillar boxing. 类似于 ShowAll, 但是会有水平或竖直方向的溢出(padding). 给予你一个像以前的 TitleSafeArea 的区域. 比如: 如果期望宽高是 1348\*900, 溢出区域为148\*140, 那么安全区域会是1200\*760 (期望宽高减去溢出区域)


## Entity 实体
实体可以被场景管理并且可以被场景加入或移除. 你既可以直接创建 Entity 类的实例并且自行加入组件(通过**AddComponent**加入,**GetComponent**查找), 你也可以继承它然后再实例化. 在基层你可以认为实体就是组件的容器. 实体有一系列方法可以被场景调用在整个生命周期内.

实体生命周期方法:

- **OnAddedToScene**: 被加入场景时调用(注: 在调用场景的加入方法后实体会被加入场景的待加入列表, 在帧结束时会一次性全部加入)
- **OnRemovedFromScene**: 实体被移出场景时调用
- **Update**: 在实体 enabled 属性为真时每帧调用
- **DebugRender**: 如果 Core.debugRenderEnabled 为真, 默认的渲染器(renderer)会调用. 自定义渲染器可以选择调用与否. 默认渲染器实现会调用所有组件的 DebugRender 方法以及附加的 Collider.

一些实体上关键的属性:

- **UpdateOrder**: 控制实体的更新顺序
- **Tag**: 随意使用这个属性吧. 之后你能在场景里用任意一个 tag 查询实体, 具体方法是 **Scene.FindEntitiesByTag**
- **Colliders**: 由该实体管理的碰撞体, 加入任何碰撞体的同时也会在物理系统中注册这个碰撞体.
- **UpdateInterval**: 控制改实体的更新频率, 1 表示 1帧/次, 2 表示 2帧/次, 其他同理.


## Component 组件
组件被实体加入和管理. 它们组成了整个游戏, 它们通常是一些可复用的代码, 用来决定你的实体该表现为什么样子. Nez 内置了几个继承于组件的常用组件, 包括用于文字显示的, 图片显示的, 显示动图的, tile map 显示等.

组件的生命周期方法:

- **Initialize**: 在组件创建和指定 Entity 字段之后, onAddedToEntity 之前调用.
- **OnAddedToEntity**: 在组件被加入到一个实体时.(注: 同实体,在一帧结束后再一次性调用)
- **OnRemovedFromEntity**: 组件被移除时, 建议在这里干有关释放资源的事.
- **OnEntityPositionChanged**: 当实体位置变更时调用.
- **Update**: 当组件实现的 IUpdatable 接口, 且实体和组件 enabled 属性为真时每帧调用
- **DebugRender**: 同实体, `debugRenderEnabled` 为真时调用
- **OnEnabled**: 父实体或本体被 enable 时
- **OnDisabled**: 父实体或本体被 disable 时

在这里说一个比较重要的组件的抽象子类 `RenderableComponent`. 它是一个比较特殊的组件, 它有个 `Render` 方法能被重写, 被 Renderer 用. 该组件会自动帮干很多棘手的活(比如管理超出边界时的剔除), 并且包含很多与显示渲染有关的好用的方法和属性. 看看例子中继承于 `RenderableComponent` 的类你能了解更多关于它的知识.


## Materials 材质

每个 `RenderableComponent` 都有一个可选的 `Material` 属性. 材质(Material)允许你设置任何一个渲染的细节, 比如混色方案, 采样方案, 和 Effect(默认为空).
