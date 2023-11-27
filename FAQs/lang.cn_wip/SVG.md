Nez SVG Support Nez 的矢量图 svg 支持
==========
Nez 包含一些导入矢量图的基本支持. Nez 没有完全支持矢量图的计划. 现在支持矢量图的目的是得到一些矩形, 贝塞尔曲线这些图形. 因为这些能够被用于关卡布局, AI寻路系统或者一些其他的目的.

svg 解析器支持 组, 路径, 矩形, 线, 圆, 椭圆, 多边形, 折线和图片. 图片可以被内嵌, 或者是一个网址, 或者在你的 Content 目录里也行. 同样一个 debug renderer 组件被包含方便你测试矢量图. 你可以直接像这样将矢量图加入到场景中的实体:

```csharp
var svgEntity = CreateEntity( "svg" );
svgEntity.AddComponent( new SvgDebugComponent( "mySvgFile.svg" ) );
```

`SvgDebugComponent` 是形状如何被访问的最佳例子. 在本页还未很详细说明时这个类就是个完整的文档. 在类里每个被支持的图形都有其对应的渲染方法. 所以你能很容易知道从 svg 文件到图形是怎么被渲染处理的.


## Making Paths Faster 提高Path的性能
如果你希望首先使用 Path, 那最好不要使用默认的 `ISvgPathBuilder`. Nez 对它开箱即用的实现很烂(SvgDebugComponent也是). 是因为 PCLs 不能直接访问 System.Drawing, 所以 ISvgPathBuilder 是使用反射实现的.

为了让所有事情正常运行, 你应该做下面这些事:
- 向你的主项目加入 Graphics/SVG/Shapes/Paths/SvgPathBuilder.cs 这个文件
- 当你创建你自己的 SvgDebugComponent 时记得传递 `SvgPathBuilder` 作为第二个参数
- 享受创建贝塞尔曲线和其他 Path 的 80 倍速度提升


## Parsing Paths Directly 直接解析路径
你可以只读取矢量图里的 path 而不用去解析整个文件. 你只需要拿到矢量图中 'path' 元素的 'd' 属性的值. 这会让你很快速很简单的得到贝塞尔曲线和其他类型的 path 数据. 以下是一些实例代码:

```csharp
var svgPath = new SvgPath();
svgPath.d = "-矢量图中'd'属性的值-";
var points = svgPath.GetTransformedDrawingPoints( new SvgPathBuilder() );
```