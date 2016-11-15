Farseer Physics/Collisions
==========
For situations where you need a full-fledged physics simulation Nez provides the Farseer Physics Engine. Farseer physics is entirely optional and currently experimental/in development. The public API will change so beware. If you want to use Farseer you will need to add the Nez.FarseerPhysics project to your main solution and add a reference to it from your game project.



## Background and Goals
[Farseer Physics Engine](https://farseerphysics.codeplex.com/) is a C# port of the superb Box2D physics engine. The Farseer project was abandoned way back in 2013 but it still remains one of the most popular choices for MonoGame/FNA. Farseer covered almost all of the Box2D API and extended it providing a bunch of super useful tools, many not even physics specific (texture and polygon tools, for example).

Farseer (like most physics engines) operates in metric (kilo/meter/second) as opposed to pixel coordinates. This can make working directly with Farseer error prone. Every time you get or set data you have to convert to/from display/simulation units. The Nez high level API hides all of this and handles converting between display and simulation for you. When accessing the Farseer API directly it will not be converted for you. You can use the `FSConvert` static class to handle conversions via its `displayToSim` and `simToDisplay` fields.

The Nez Farseer implementation has a few important goals that it aims to achieve:
- wrap the most commonly used Farseer features in easy to use Components. The Farseer API is low level. The high level Components will hide all of that complexity and make basic physics simple to use.
- allow full access to the Farseer API for advanced users. It should be noted that the [Farseer docs](https://farseerphysics.codeplex.com/documentation) are minimal and point users over to the [Box2D documentation](http://box2d.org/documentation/).
- extend Farseer and add some new functionality to it (Kinematic collision detection!). All Farseer code will remain in the `FarseerPhysics` namespace and all Nez additions will be in the `Nez.Farseer` namespace.
- provide most of the features Nez physics have. Nez physics lets you take full control over the movement/collision of a Collider. Physics engines like to control collision and response automatically. The `FarseerCollisions` class is already in place and lets you take control of collision and collision response just like with Nez Colliders. The `Mover` class and similar can all be done entirely with Farseer at this point.

The Farseer code is all in progress and the API is definitely not in its final state. Usually this would be kept out of the main repo until the API is solidified but since Farseer is entirely optional all in-progress changes will be public. If you are itching to get going with Farseer for now you are mostly on your own. The high level API (and all Nez-specific additions) are available in the Nez.FarseerPhysics/Nez folder.



## Understanding Farseer Objects
Farseer consists of a few key objects that are paramount to understanding the API and being able to effectively use it.

- **World**: the world object is the manager of it all. It iterates all the objects in the world each frame steps through and makes sure everything is consistent and stable.

- **Body**: the body keeps track of world position. It is basically a point is space that is affected by forces such as impulses from collisions and gravity. Bodies come in 3 different flavors (Body.BodyTypes) that drastically effect how they work in the physics world.
  - *Dynamic*: objects which move around and are affected by forces and other dynamic, kinematic and static objects. Dynamic bodies are suitable for any object which needs to move and be affected by forces.
  - *Static*: objects which do not move and are not affected by forces. Dynamic bodies are affected by static bodies. Static bodies are perfect for ground, walls, and any object which does not need to move. Static bodies require less computing power.
  - *Kinematic*: objects that are somewhat in between static and dynamic bodies. Like static bodies, they do not react to forces, but like dynamic bodies, they do have the ability to move. Kinematic bodies are great for things where you want to be in full control of a body's motion. Nez extends Kinematic bodies with some added features that are not part of Farseer/Box2D. More on that later.

- **Shape**: a shape is what extends the point in space to a 2D shape. The centroid and stuff like area, inertia and mass is calculated for the shape.

- **Fixture**: a fixture attaches (fixes) the shape to the body so that the centroid of the shape becomes the body’s position. Whenever you move the body, you also move the shape. Once a collision occurs to the shape, the force is calculated and applied to the body.



## Using Farseer with Nez
There are a couple options for using Farseer physics with Nez to provide some flexibility. The recommended approach is to use or subclass `FarseerScene`. FarseerScene has a `world` field that houses the Farseer World object and takes care of the physics step each frame.

Alternatively, you can add an `FSWorld` Component to your Scene to get the same effect. Note that you must add your FSWorld Component before attempting to use any of the built in Components since they rely on the World existing.

The `FSDebugView` Component can be added to your Scene to get a visual representation of the physics world. This is very handy for development and debugging of Farseer objects.



## More coming soon after solidifying the API


