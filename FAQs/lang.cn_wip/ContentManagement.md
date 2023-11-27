Content Management 资源管理
==========
Nez 自己内置了一套基于 MonoGame 资源管理系统的资源管理系统. 所有的资源管理都应该经过 Nez 的 `NezContentManager`, 该类继承于`ContentManager`. 调试控制台有一个叫 'assets' 的命令, 它会输出所有的场景资源以及全局资源, 这样你就能知道还有哪些资源还在内存中待着.

Nez 提供了全局和每个场景资源的容器. 你也可以在你需要管理短生命周期的资源的时候创建你自己的 `NezContentManager`. 你可以在任何时候使用 `UnloadAsset<T>` 方法释放资源. 注意 Effect 需要特殊的`UnloadEffect` 方法释放, 这是一个特例.


## Global Content 全局资源
`Core.Content` 中有个全局的 NezContentManager, 你可以用这个 manager 去加载生命周期为整个游戏的资源. 比如字体, 全局通用的动画, 全局通用的音效等.

## Scene Content 场景资源
每个场景都有它自己的 `NezContentManager` (Scene.Content), 用它加载场景特定的资源. 当前往新场景时, 旧场景的资源会被自动释放.

## Loading Effects 加载 Effects
相比于 MonoGame, Nez 有很多种它没有的方式去加载 Effects 以让加载和管理 Effects 更加容易简便. 尤其是当去解决 Effect 的一堆子类的时候(比如 AlphaTestEffect 和 BasicEffect). Nez 内置的Effects 加载起来都很方便. 可用的方法有:

- **LoadMonoGameEffect<T>**: 加载 MonoGame 内置的 Effect 比如 BasicEffect, AlphaTestEffect
- **LoadEffect/LoadEffect<T>**: 从文件直接加载一个 ogl/fxb Effect, 在资源管理器被释放时会自动处理 Effect 的释放
- **LoadEffect<T>( string name, byte[] effectCode )**: 从字节数组中加载一个 ogl/fxb Effect, 同样也会被自动释放
- **LoadNezEffect**: 加载一个 Nez 内置的 Effect. 它们继承于 `Effect`, 在 Nez 的 Graphics/Effects 目录下.


## Auto Generating Content Paths 自动生成 Content 路径
Nez 包含一个自动生成所有资源路径到一个 `Nez.Content` 的静态类的 T4 模板. 这允许你像下面一样写代码:

```csharp
// 在使用 ContentPathGenerator 之前你必须使用没有任何智能提示的字符串去指代你的资源
var tex = content.Load<Texture2D>( "Textures/Scene1/blueBird" );

// 在使用之后你就能在指代资源时使用智能提示了, 同时你不会错误地指代到一个不存在的资源
var tex = content.Load<Texture2D>( Nez.Content.Textures.Scene1.blueBird );
```

使用 ContentPathGenerator 最大的好处是你将不会错误地指示到一个不存在的资源, 你能在编译时就能检查你的资源路径是否正确. 下面是建立该工具的步骤:

- 复制 ContentPathGenerator.tt 到你的项目根目录(如果你要放在其他地方记得修改文件里的 `sourceFolder` 变量, 比如, 如果使用预编译的XNB文件在一个FNA项目里你需要设置 `sourceFolder = "Content/"`)
- 在文件属性面板里设置 "Custom Tool" 为 "TextTemplatingFileGenerator"
- 如果你使用 Visual Studio:   
   - 右键文件, 选择 "Tools" -> "Process T4 Template", 这样这个类就会被生成了
- 如果你使用 Visual Studio Code:
   - 安装 [mono dotnet-t4 tool](https://github.com/mono/t4).
      - 该工具能被全局安装: ```dotnet tool install -g dotnet-t4```
   - 用命令行执行 dotnet-t4 : ```t4 -o ContentPathGenerator.cs ContentPathGenerator.tt```
- 如果你使用 JetBrains Rider:
   - 右键文件, 选择 "Run Template"

## Async Loading 异步加载
`NezContentManager` 同样提供了一些异步加载资源的方法. 你能使用 `LoadAsync<T>` 方法加载单个或多个资源, 你需要传入一个回调方法这样你就能知道资源什么时候加载完成了.