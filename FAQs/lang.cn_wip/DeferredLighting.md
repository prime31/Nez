Deferred Lighting 延迟光照
==========
(译注: 部分专业名词本人不是很了解)

延迟光照系统可以让你的场景拥有很真实的光线. 包括点光源, 平行光, 聚光灯和区域光源.

延迟光照是一个解决光照绘制的方法, 它会在所有物体渲染完毕时再渲染光源. 这种方式是很高效的. 它们的工作要点普遍是这样的: 首先, 我们把所有物体都渲染到一个漫反射渲染贴图, 同时渲染它们的法线贴图到一个法线贴图(通过多个渲染目标实现). 所有的灯光就这样被使用法线数据渲染到第三个贴图上. 最后, 这个贴图和漫反射贴图结合, 输出最后的结果. 上述的贴图你能够在运行时开关 `DeferredRenderer.EnableDebugBufferRender` 属性来查看.

## Material Setup 材质配置
这就是你做 2d 游戏的正常工作流程与延迟光照系统不一样的地方. 在大多数没有实时光照的游戏里, 你一点也不会需要沾 Material 系统的边. 延迟光照需要一些附加的信息(主要是法线贴图)去计算光照, 所以我们必须深入 Nez 的 Material 系统. 现在假设你知道如何去制作一些资源比如法线贴图, 这里我们不会去讨论如何制作这些东西.

Nez 的延迟光照系统采用了一个巧妙的叫自照明(self illumination)的技巧. 你的贴图的一部分不需要光就能可视. 它们总是被点亮的. 你可以通过调整你的法线贴图的 alpha 通道来决定贴图的哪一部分需要自光照. 0-无自照明 到 1-完全自照明. 不要忘记使用自照明时不要预乘 alpha 值!. Nez 需要完整的 alpha 通道来得到所有的自光照数据. 当使用自照明时你必须要让 Nez 知道, 通过设置 `DeferredSpriteMaterial.SetUseNormalAlphaChannelForSelfIllumination`. 一个额外的运行时控制自照明的方式是设置 `DeferredSpriteMaterial.SetSelfIlluminationPower`. 调整自照明强度能让你的场景增加一些很出色的氛围.

有时候你不想让你的物体有法线贴图(be normal mapped)或者你还没准备好贴图. 延迟光照系统也提供了这个功能, 它内置了一个在 Material 里能被配置的 "空法线贴图", 这让你能让一个物体只参与漫反射光照. 默认地, `DeferredLightingRenderer.material` 会是一个空的法线贴图. 每当渲染器绘制一个 RenderableComponent 时如果它有个空的 Material 那么它会使用渲染器自身的 Material. 意思就是说如果你向一个 RenderableComponent 扔了个空 Material 那么它只会被漫反射光照渲染.

下面是三个最常用的 Material 配置: 法线贴图光照, 法线贴图自照明, 和仅漫反射(normal mapped lit, normal mapped lit self illuminated and only diffuse (no normal map))

```cs
// 光照, 法线贴图 Material(lit, normal mapped Material). normalMapTexture 是你法线贴图 Texture2D 类型的引用
var standardMaterial = new DeferredSpriteMaterial( normalMapTexture );


// 漫反射光照 Material. 使用 NullNormalMapTexture.
var diffuseOnlylMaterial = new DeferredSpriteMaterial( deferredRenderer.NullNormalMapTexture );


// 带光照, 法线贴图和自照明的 Material
// (lit, normal mapped and self illuminated Material.)

// 首先我们需要用我们的法线贴图创建一个 Material. 注意你的法线贴图需要有 alpha 通道给自照明使用
// 需要把预乘 alpha 值关闭
var selfLitMaterial = new DeferredSpriteMaterial( selfLitNormalMapTexture );

// 我们可以通过 Material<T> 的 `TypedEffect` 来访问 Effect. 我们需要告诉 Effect 我们需要自照明并且可选的调整它的强度
selfLitMaterial.effect.SetUseNormalAlphaChannelForSelfIllumination( true )
	.SetSelfIlluminationPower( 0.5f );
```

## Scene Setup 场景配置
场景的配置就比较简单了. 你只需要扔一个 `DeferredLightingRenderer` 到你的场景就行了. 但是在它的构造器里你需要传递的值是很重要的! 你需要指定其应该使用哪个 RenderLayer 以及你的精灵(normal sprites)应该包含在哪个 RenderLayer 里.

```cs
// 定义你的 RenderLayers, 这样你会方便访问一些
const int LIGHT_LAYER = 1;
const int OBJECT_LAYER1 = 10;
const int OBJECT_LAYER2 = 20

// 加入 DeferredLightingRenderer 到你的场景里, 然后指定相应的 RenderLayer 和一堆你希望渲染到的 RenderLayers
var deferredRenderer = scene.AddRenderer( new DeferredLightingRenderer( 0, LIGHT_LAYER, OBJECT_LAYER1, OBJECT_LAYER2 ) );

// (可选的) 设置环境光照
deferredRenderer.SetAmbientColor( Color.Black );
```

## Entity Setup 实体配置
现在我们只需要确保在创建我们的 Renderables 之前使用正确的 RenderLayers (这很简单! 因为我们之前很聪明的把它们保存到了 `const int` 中) 和 Materials.

```cs
// 创建一个包含 sprite 的实体
var entity = Scene.CreateEntity( "sprite" );

// 加入一个 sprite, 这里还有个重要的部分: 确保设置 RenderLayer 和 Material.
entity.AddComponent( new Sprite( spriteTexture ) )
	.SetRenderLayer( OBJECT_LAYER1 )
	.SetMaterial( standardMaterial );


// 创建一个没有设置 Material 的 Entity. 它会使用默认仅漫反射的 DeferredLightingRenderer.Material
scene.CreateEntity( "diffuse-only" )
	.AddComponent( new Sprite( spriteTexture ) )
	.SetRenderLayer( OBJECT_LAYER1 );


// 创建一个包含我们点光源的 Entity
var lightEntity = scene.CreateEntity( "point-light" );

// 加入一个点光源组件然后确保 RenderLayer 是在光照层(LIGHT_LAYER)上!
lightEntity.AddComponent( new PointLight( Color.Yellow ) )
	.SetRenderLayer( LIGHT_LAYER );
```

