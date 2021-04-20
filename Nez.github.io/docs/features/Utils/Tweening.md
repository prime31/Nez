---
id: Tweening
title: Tweening
---

The word Inbetweening, or tweening for short, stands for the process of animating an by updating values for position, size, color, opacity, etc. inbetween frames

In games, tweens are used to smoothly transition Vectors, floats and Colors etc.

## Usage
:::info
The sample code can be found in the Tweening scene in [Nez-Samples](https://github.com/prime31/Nez-Samples)
:::

### Basics

We start by including the Tweening namespace.

```cs
using Nez.Tweens;
```

Tweens can be easily created via extension methods in `TweenExt` and `ObjectExt`.

Whenever a tween is created it will then be updated every frame by the `TweenManager`(A component that is registered by default in `Nez.Core`)

The following types can be tweened
- int
- float
- Color
- Vector2
- Vector3

For this example, I am using a simple square Entity.

```cs
Entity tweenTargetEntity = CreateEntity("tweenTarget");
tweenTargetEntity.Position = new Vector2(100, 100);
tweenTargetEntity.AddComponent(
    new PrototypeSpriteRenderer(10,10) {Color = Color.Yellow
    });
```

I then create a Tween that smoothly transforms the position of this entity over a timespan of 5 seconds, using the `TweenPositionTo` extension method.
```cs
tweenTargetEntity
    .TweenPositionTo(new Vector2(250, 250), 5f)
    .Start(); 
```

### Easing types

You may have noticed that the tweeing transition is not linear.
That's because we didn't specify the easing type.
If we don't specify the easing type (The way the value transitions), the `DefaultEaseType` from the `TweenManager` will be used. This is set to `EaseType.QuartIn` by default.

We can change the default easing type as follows.
```cs
TweenManager.DefaultEaseType = EaseType.Linear;
```

We can also specify the easing type using the `ITween<T>.SetEaseType` Extension method

```cs
tweenTargetEntity
    .TweenPositionTo(new Vector2(250, 250), 5f)
    .SetEaseType(EaseType.Linear)
    .Start(); 
```

Here you can see the effect of each `EaseType`

![Tweening](Images/tweening.gif)

### Repeating loops

You can also create repeating tweens by using the `SetLoops` extension method like this.
```cs
SetLoops(LoopType.RestartFromBeginning, -1)
```

I set the loop count to `-1`. This makes the loop repeat infinitely.

### Callback

With the `SetCompletionHandler` extension method, you can register a callback that will be called when the tween finishes.
```cs
tweenTargetEntity
    .TweenPositionTo(new Vector2(250, 100), 1f).SetEaseType(EaseType.Linear)
    .SetCompletionHandler((x) =>
    {
        System.Console.WriteLine("Tween completed");
    })
    .Start();
```

:::tip 
For performance optimizations, you can also use the `SetContext` extension method. to avoid [closure allocations](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/local-functions#heap-allocations) or to retrieve tweens with a specific context in the `TweenManager`. 

```cs
tweenTargetEntity
    .TweenPositionTo(new Vector2(250, 100), 1f).SetEaseType(EaseType.Linear)
    .SetContext(tweenTargetEntity)
    .SetCompletionHandler((x) => { System.Console.WriteLine(x.Context); })
    .Start();
```
:::



### Relative target

With the `SetIsRelative` extension method you can set the `taget` relative to the `from`

:::note
Note if you use `TweenPositionTo(new Vector2(250, 100), 1f)` the `from` is `Vector2.Zero`.
You can set it with the `SetFrom` extension method
:::

### Pausing and Resuming
You can pause and Resume Tweens with the `Resume` and `Pause` extension methods

### Delay and Duration
You can set the delay and duration of a tween using the `SetDelay` and `SetDuration` extension methods.