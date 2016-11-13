Farseer Physics/Collisions
==========
For situations where you need a full-fledged physics simulation Nez provides the Farseer Physics Engine. Farseer physics is entirely optional and currently experimental/in development. The public API will change so beware. If you want to use Farseer you will need to add the Nez.FarseerPhysics project to your main solution and add a reference to it from your game project.



## Background and Goals
[Farseer Physics Engine](https://farseerphysics.codeplex.com/) is a C# port of the superb Box2D physics engine. The Farseer project was abandoned way back in 2013 but it still remains one of the most popular choices for MonoGame/FNA. Farseer covered almost all of the Box2D API and extended it providing a bunch of super useful tools, many not even physics specific (texture and polygon tools, for example).

Farseer (like most physics engines) operates in metric (kilo/meter/second) as opposed to pixel coordinates. This makes working directly with Farseer error prone. Every time you get or set data you have to convert to/from display/simulation units. The Nez high level API will hide all of this and handle converting between display and simulation for you. When accessing the Farseer API directly it will not be converted for you.

The Nez Farseer implementation has a few important goals that it aims to achieve:
- wrap the most commonly used Farseer features in easy to use Components. The Farseer API is low level. The high level Components will hide all of that complexity and make basic physics simple to use.
- allow full access to the Farseer API for advanced users. It should be noted that the [Farseer docs](https://farseerphysics.codeplex.com/documentation) are minimal and point users over to the [Box2D documentation](http://box2d.org/documentation/).
- extend Farseer and add some new functionality to it
- provide most of the features Nez physics have. Nez physics lets you take full control over the movement/collision of a Collider. Physics engines like to control collision and response automatically. The `FarseerCollisions` class is already in place and lets you take control of collision and collision response just like with Nez Colliders. The `Mover` class and similar can all be done entirely with Farseer at this point.

The Farseer code is all in progress and the API is definitely not in its final state. Usually this would be kept out of the main repo until the API is solidified but since Farseer is entirely optional all in-progress changes will be public. If you are itching to get going with Farseer for now you are mostly on your own. The high level API is available in the Nez.FarseerPhysics/Nez folder.



## More coming soon after solidifying the API


