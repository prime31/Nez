Nez Physics/Collisions Nez的物理/碰撞
==========

这里有必要复述之前所说的内容: Nez 的物理*不是*一个真实的物理模拟. 它提供一个我管它叫*游戏*物理的东西. 你可以做一些比如 linecast 来检测碰撞, 覆盖检测, 碰撞检测, Sweep 测试以及更多其他的. 它不是一个完整的刚体物理模拟. *你*可以从头到尾完整控制你游戏的感觉. 如果你需要一个完整的物理模拟的话, 你需要参见可选的 [Farseer 物理实现](FarseerPhysics.md).

## 碰撞体: 物理系统的根基
在物理世界中没有碰撞体什么都不会发生. 碰撞体附着在 Entity 上, 并且有几个变种: BoxCollider, CircleCollider, 以及 PolygonCollider. 你可以用比如 `entity.AddComponent( new BoxCollider() )` 来添加一个碰撞体. 当你开启了调试绘制(DebugRender)后, 碰撞体将会以红色的线显示(调试绘制可通过设置`Core.DebugRenderEnabled`为`true`或者在控制台里输"debug-render"来开启). 碰撞体会在被加入实体后自动地加入SpatialHash, 这就要说到我们的下个话题了.

## SpatialHash: 一个你永远不会接触到但是很重要的东西

幕后的 SpatialHash 类全局地管理着你的游戏的碰撞体. **Physics** 静态类是一个公开的 SpatialHash 的包装类. SpatialHash 没有大小限制, 以便快速检测碰撞/linecast/覆盖. 比如, 如果你有一个在世界上到处移动的英雄, 但是你不必检测成百的所有的碰撞体, 你只需要向 SpatialHash 请求英雄附近的碰撞体即可. 这会很激进地降低碰撞检测的个数.

这里有一个可以对 SpatialHash 配置的东西, 它会显著地影响其的性能: 网格单元大小. SpatialHash 将空间分割成一个网格, 然后选择一个合适的网格大小来确保可能地碰撞查询到一个最小值. 默认这个网格的大小是 100px. 你可以*在创建场景之前*通过设置 `Physics.SpatialHashCellSize` 来更改它. 选择一个稍大于你的玩家,敌人平均尺寸的大小通常是最好的.

最后一个关于 SpatialHash 的事是: 它包含了一个可视化的调试器. 通过调起游戏内的控制台(按下波浪键)然后输入指令 **physics**, 然后 SpatialHash 网格和每个网格单元内的对象个数会被显示. 这在你在做设置 spatialHashCellSize 的决定时很有用.

## The Physics Clsss 物理类

**Physics** 类是你通往所有物理相关东西的大门. 这里有一些属性你可以设置, 比如上面提到的 spatialHashCellSize, raycastsHitTriggers 和 raycastStartInColliders. 见它们每个 Intellisense 文档上的解释. 其中一些常见有用的方法有:

- **Linecast**: 从头到尾投射一条线, 然后返回第一个满足 layerMask 的碰撞体
- **OverlapRectangle**: 检测任何位于矩形区域内的碰撞体
- **OverlapCircle**: 检测任何位于原型区域内的碰撞体
- **BoxcastBroadphase**: 返回所有拥有边界(bound)且与 collider.bounds 相交的碰撞体. 注意这是一个 Broad-Phase, 所以它只检测边界, 不会深入到碰撞体间的检测!

敏锐的读者可能会注意到上面所提到的 *layerMask*. layerMask 允许你决定碰撞体应该与什么碰撞体碰撞. 每一个碰撞体都有它的 `PhysicsLayer`, 所以每当你向物理系统发起查询时你可以选择只返回与 layerMask 相匹配的碰撞体. 所以物理方法都会接受一个 layerMask 参数, 默认是所有 layer. 明智地使用它筛选你的碰撞检测数, 并通过减少不必要的碰撞检测来提高性能.

## Putting the Physics System to Use 使用物理系统
Linecast 在像比如检查敌人视线碰撞, 检测实体的周围环境, 快速移动的子弹等等的时候极其有用. 这里是一些只是简单的打印日志的线投射(casting of line)的例子:

```cs
var hit = Physics.Linecast( start, end );
if( hit.Collider != null )
	Debug.Log( "ray hit {0}, entity: {1}", hit, hit.collider.entity );
```

Nez 有一些高级的碰撞/覆盖检测, 比如使用 Minkowski Sums, Separating Axis Theorem 和 good old trigonometry. 它们都在 Collider 的方法上被简单封装了. 现在让我们来看几个例子.

第一个例子, 也是最简单的处理碰撞的例子. `deltaMovement` 是一个移动实体的值, 通常为 `velocity * Time.DeltaTime`. `CollidesWithAny` 方法将会检查所有碰撞并设置 `deltaMovement` 以便解决任何碰撞.

```cs
// CollisionResult 将会包含一些真的很有用的信息比如说被击中的 Collider,
// 表面的法线, 以及 Minimum Translation Vector (MTV). 
// MTV 可以被直接用来移动碰撞中的实体至击中碰撞体的旁边.
CollisionResult collisionResult;

// 做些检查看看 entity.GetComponent<Collider> (实体的第一个碰撞体) 是否碰撞到了任何场景中其他的碰撞体
// 注意如果你有多个碰撞体的话你可以获取并遍历检测它们而不是只被检查了第一个
if( entity.GetComponent<Collider>().CollidesWithAny( ref deltaMovement, out collisionResult ) )
{
    // 打印 CollisionResult. 你可能想会加入一些粒子效果或者其他有关于你的游戏的东西.
	Debug.Log( "collision result: {0}", collisionResult );
}

// 移动实体到新的位置. deltaMovement 已经被设置以便我们解决碰撞
entity.Position += deltaMovement;
```

如果你需要在碰撞发生时做一小些更多的控制的话你也可以手动检查与其他碰撞体的碰撞. 下面这个代码块检查了一个特定碰撞体的碰撞. 注意此时 deltaMovement 没有被设置. 在解决碰撞时的 `MinimumTranslationVector` 也需要你来手动维护.

```cs
// 声明 CollisionResult
CollisionResult collisionResult;

// 检查是否 entity.GetComponent<Collider> 与 someOtherCollider 碰撞了
if( entity.GetComponent<Collider>().CollidesWith( someOtherCollider, deltaMovement, out collisionResult ) )
{
    // 直接移动实体到被撞击碰撞体相邻的位置, 然后打印 CollisionResult
	entity.Position += deltaMovement - collisionResult.MinimumTranslationVector;
	Debug.Log( "collision result: {0}", collisionResult );
}
```

我们可以更近一步地使用前面提到的 `Physics.BoxcastBroadphase` 方法, 或者更明确地说, 将我们自己排除在查询之外的版本. 那个方法将会为我们提供场景中我们附近的所有碰撞体, 然后我们可以使用这些碰撞体做我们实际的碰撞检测.

```cs
// 获取所有除了自己的我们可能会覆盖在当前位置的东西. 我们在这里不需要关心自己.
var neighborColliders = Physics.BoxcastBroadphaseExcludingSelf( entity.GetComponent<Collider>() );

// 遍历并进行每个碰撞体的覆盖检测
foreach( var collider in neighborColliders )
{
	if( entity.GetComponent<Collider>().Overlaps( collider ) )
		Debug.Log( "We are overlapping a Collider: {0}", collider );
}
```