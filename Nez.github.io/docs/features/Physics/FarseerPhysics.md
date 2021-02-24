---
id: FarseerPhysics
title: Farseer Physics/Collisions
---

For situations where you need a full-fledged physics simulation Nez provides the Farseer Physics Engine. Farseer physics is entirely optional and currently experimental/in development. The public API will change so beware. If you want to use Farseer you will need to add the Nez.FarseerPhysics project to your main solution and add a reference to it from your game project.



## Background and Goals
[Farseer Physics Engine](https://farseerphysics.codeplex.com/) is a C# port of the superb Box2D physics engine. The Farseer project was abandoned way back in 2013 but it still remains one of the most popular choices for MonoGame/FNA. Farseer covered almost all of the Box2D API and extended it providing a bunch of super useful tools, many not even physics specific (texture and polygon tools, for example).

Farseer (like most physics engines) operates in metric (kilo/meter/second) as opposed to pixel coordinates. This can make working directly with Farseer error prone. Every time you get or set data you have to convert to/from display/simulation units. The Nez high level API hides all of this and handles converting between display and simulation for you. When accessing the Farseer API directly it will not be converted for you. You can use the `FSConvert` static class to handle conversions via its `DisplayToSim` and `SimToDisplay` fields.

The Nez Farseer implementation has a few important goals that it aims to achieve:
- wrap the most commonly used Farseer features in easy to use Components. The Farseer API is low level. The high level Components will hide all of that complexity and make basic physics simple to use.
- allow full access to the Farseer API for advanced users. It should be noted that the [Farseer docs](https://farseerphysics.codeplex.com/documentation) are minimal and point users over to the [Box2D documentation](http://box2d.org/documentation/).
- extend Farseer and add some new functionality to it (Kinematic collision detection!). All Farseer code will remain in the `FarseerPhysics` namespace and all Nez additions will be in the `Nez.Farseer` namespace.
- provide most of the features Nez physics have. Nez physics lets you take full control over the movement/collision of a Collider. Physics engines like to control collision and response automatically. The `FarseerCollisions` class is already in place and lets you take control of collision and collision response just like with Nez Colliders. The `Mover` class and similar can all be done entirely with Farseer at this point.

The Farseer code is all in progress and the API is definitely not in its final state. Usually this would be kept out of the main repo until the API is solidified but since Farseer is entirely optional all in-progress changes will be public. If you are itching to get going with Farseer for now you are mostly on your own. The high level API (and all Nez-specific additions) are available in the Nez.FarseerPhysics/Nez folder.


## Using Farseer with Nez
First and foremost, you should always set your pixel-to-meter ratio before doing anything. By default, the value is set to 100. You can change this to whatever you want by calling `FSConvert.SetDisplayUnitToSimUnitRatio`. Behind the scenes, the high level API will be using this value to deal with converting to/from simulation units to pixels. If you choose to use the Farseer API directly be sure to remember to convert your units with the FSDebug `DisplayToSim` and `SimToDisplay` fields.

There are a couple options for using Farseer physics with Nez to provide some flexibility. Regardless of if you choose to use the Component-based high level API or use Farseer directly it is recommended to use the `FSWorld` `SceneComponent` to manage the Farseer `World` object. All of the the high level API will get the World object from the FSWorld SceneComponent. You can easily access it by just calling `Scene.GetOrCreateSceneComponent<FSWorld>()`. As the name implies, this will fetch the FSWorld SceneComponent or first create it then fetch it.

The `FSDebugView` Component can be added to your Scene to get a visual representation of the physics world. This is very handy for development and debugging of Farseer objects and it will work with the high or low level API.


## High Level API
The high level API wraps up the Farseer API in standard Nez Components. Farseer Components come in 3 different flavors explained below each of them with a fluent API for configuring the objects for easy method chaining and API exploration.

- **FSRigidBody**: wraps the Farseer Body. Handles keeping the Entity's Transform in sync with the Farseer Body. FSRigidBody's can be any of the three body types: Dynamic, Static or Kinematic (see Understanding Farseer Objects for details on each). An FSRigidBody is required for any of the other Farseer Components to be of any use.

- **FSCollisionShape**: wraps the Farseer Fixture and Shape objects. This is the physical shape of the collider. You can have 1 or more FSCollisionShapes on your Entity. Available FSCollisionShapes are circle, box, polygon, edge, ellipse and chain.

- **FSJoint**: wraps the Farseer Joint. Joints can be used to connect two FSRigidBodys in various different ways. Included joint types are: angle, distance, friction, gear, motor, mouse, prismatic, pulley, revolute (hinge), rope, weld and wheel.

Lets take a look at some basic examples of using the API.

Creates a Sprite and a dynamic Farseer rigid body with a circle collider
```csharp
// create an Entity and set the position and scale
CreateEntity( "circle-sprite" )
	.SetPosition( pos )
  	.SetScale( scale )

	// add an FSRigidBody and set the bodyType to dynamic
	.AddComponent<FSRigidBody>()
	.SetBodyType( BodyType.Dynamic )

	// add a circle shape for our collisions and set the radius to halve the texture width
	.AddComponent<FSCollisionCircle>()
	.SetRadius( texture.Width / 2 )

	// finally add a Sprite
	.AddComponent( new Sprite( texture ) );
```

Creates a static Farseer rigid body and an edge collider that goes from vert1 to vert2
```csharp
CreateEntity( "edge" )
	.SetPosition( pos )

	// add the FSRigidBody. By default it will be static
	.AddComponent<FSRigidBody>()

	// add our edge collision shape with two verts
	.AddComponent<FSCollisionEdge>()
  	.SetVertices( vert1, vert2 );
```

Creates a static Farseer rigid body with a chain collider. Chains are essentially a free form sequence of line segments that can be collided with from either side.
```csharp
// define the verts. The Vertices class has the sme API as List<Vector2>.
var verts = new Vertices();
verts.Add( new Vector2( 500, 10 ) );
verts.Add( new Vector2( 550, 50 ) );
verts.Add( new Vector2( 600, 70) );
verts.Add( new Vector2( 700, 20 ) );

createEntity( "chain" )
	// add our static FSRigidBody
	.AddComponent<FSRigidBody>()

	// add the chain shape and set the verts
	.AddComponent<FSCollisionChain>()
  	.SetVertices( verts );
```

This example shows how to use a joint to connect two FSRigidBodies. It is assumed that rigidBody1 and rigidBody2 exist and are FSRigidBodies.
```csharp
// add the weld joint to the first rigid body. Weld joints essentially glues two bodies together.
rigidBody1.AddComponent<FSWeldJoint>()
		 // configure the anchors for the two bodies. Anchors are relative to the position of each body.
	     .SetOtherBodyAnchor( new Vector2( 50, 0 ) )
	     .SetOwnerBodyAnchor( new Vector2( -50, 50 ) )

		 // configure the frequency and damping ratio
	     .SetFrequencyHz( 5 )
	     .SetDampingRatio( 0.1f )

		 // set the second FSRigidBody for the joint
	     .SetOtherBody( rigidBody2 );
```

Finally lets take a look at creating a slightly more complex collision shape.
```csharp
var vertList = new List<Vertices>();
// fill vertList with some polygon vertices

// create and configure our standard rigid body
var rb = CreateEntity( "compound-polygon" )
	.SetPosition( pos )
	.AddComponent<FSRigidBody>()
	.SetBodyType( BodyType.Dynamic );

// add an FSCollisionPolygon for each of the vert Lists
foreach( var verts in vertList )
{
	rb.AddComponent<FSCollisionPolygon>()
	  .SetVertices( verts );
}
```


## Low Level API
The low level API provides a small number of Components that handle syncing the Nez Transform with the Farseer Body. These can be used to get up and running quickly but they are more here to be used as a template so that you can create your own custom subclasses.

- **FSGenericBody**: manages the Farseer Body (and provides public access to it) and syncs it with the Nez Transform. That's about all you get with it. To make adding collision shapes and joints easier there are a pile of extension methods on the Body class. They all take care of converting from pixel space to simulation space as well.

- **FSRenderableBody**: manages the Farseer Body and syncs it with the Nez Transform. Where it differs from the `FSGenericBody` is that there are some subclasses that are instantly useable: `FSBoxBody`, `FSCircleBody`, `FSPolygonBody` and `FSCompoundPolygonBody`. These Components all take in a Texture2D/Sprite and create the Farseer Body, Fixture and Shape for you. They also handle rendering as well. `FSCompoundPolygonBody` will generate the collision shape based on the texture which can be really handy.


There are a few helpers available to make working with Farseer's API more convenient. The `Body` class has a bunch of extension methods for adding Fixtures and Joints that all let you use standard Nez pixel coordinates (as opposed to having to convert everything to Farseer simulation units). If you want to skip out completely on the `FSGenericBody` and `FSRenderableBody` classes and use Farseer directly you can use the `Nez.Farseer.BodyFactory` to create Farseer Bodies all in Nez pixel coordinates.




### Understanding Farseer Objects
Farseer consists of a few key objects that are paramount to understanding the API and being able to effectively use it.

- **World**: the world object is the manager of it all. It iterates all the objects in the world each frame steps through and makes sure everything is consistent and stable.

- **Body**: the body keeps track of world position. It is basically a point is space that is affected by forces such as impulses from collisions and gravity. Bodies come in 3 different flavors (Body.BodyTypes) that drastically effect how they work in the physics world.
  - *Dynamic*: objects which move around and are affected by forces and other dynamic, kinematic and static objects. Dynamic bodies are suitable for any object which needs to move and be affected by forces.
  - *Static*: objects which do not move and are not affected by forces. Dynamic bodies are affected by static bodies. Static bodies are perfect for ground, walls, and any object which does not need to move. Static bodies require less computing power.
  - *Kinematic*: objects that are somewhat in between static and dynamic bodies. Like static bodies, they do not react to forces, but like dynamic bodies, they do have the ability to move. Kinematic bodies are great for things where you want to be in full control of a body's motion. Nez extends Kinematic bodies with some added features that are not part of Farseer/Box2D. More on that later.

- **Shape**: a shape is what extends the point in space to a 2D shape. The centroid and stuff like area, inertia and mass is calculated for the shape.

- **Fixture**: a fixture attaches (fixes) the shape to the body so that the centroid of the shape becomes the body’s position. Whenever you move the body, you also move the shape. Once a collision occurs to the shape, the force is calculated and applied to the body.




