Nez Physics/Collisions
==========

It serves to reiterate what has been stated before: Nez physics is *not* a realistic physics simulation. It provides what I call *game* physics. You can do things like linecasts to detect colliders, overlap checks, collision checks, sweep tests and more. What you don't get is a full rigid body simulation. *You* get to control your game's feel from top to bottom. If you are looking for a full physics simulation see the optional [Farseer Physics implementation](FarseerPhysics.md).



## Colliders: The Root of the Physics System
Nothing happens in the physics system without Colliders. Colliders live on the Entity class and come in several varieties: BoxCollider, CircleCollider and PolygonCollider. You can add a Collider like so: `entity.AddComponent( new BoxCollider() )`. When you have debugRender enabled Colliders will be displayed with red lines (to enable debugRender either set `Core.DebugRenderEnabled = true` or open the console and type "debug-render"). Colliders are automatically added to the SpatialHash when you add them to an Entity, which brings us to our next topic.



## The SpatialHash: You'll never touch it but it's still important
Under the covers lies the SpatialHash class which manages Colliders globally for your game. The static **Physics** class is the public wrapper for the SpatialHash. The SpatialHash has no set size limits and is used to make collision/linecast/overlap checks really fast. As an example, if you have a hero moving around the world instead of having to check every Collider (which could be hundreds) for a collision you can just ask the SpatialHash for all the Colliders near your hero. That narrows down your collision checks drastically.

There is one configurable aspect to the SpatialHash that can greatly affect how performant it is: the cell size. The SpatialHash splits up space into a grid and choosing a proper grid size can keep your possible collision queries to a minimum. By default the grid size is 100 pixels. You can change this by setting `Physics.SpatialHashCellSize` *before* creating a Scene. Choosing a size that is slightly larger than your average player/enemy size usually works best.

One last thing about the SpatialHash: it includes a visual debugger. By pulling up the in-game console (press the tilde key) and running the command **physics** the SpatialHash grid and number of objects in each cell will be displayed. This is handy for helping to decide what your spatialHashCellSize should be.



## The Physics Class
The **Physics** class is your gateway to all things Physics. There are some properties you can set such as the aforementioned spatialHashCellSize, raycastsHitTriggers and raycastsStartInColliders. See the intellisense docs for an explanation of each. Some of the more useful and commonly used methods are:

- **Linecast**: casts a line from start to end and returns the first hit of a collider that matches layerMask
- **OverlapRectangle**: check if any collider falls within a rectangular area
- **OverlapCircle**: check if any collider falls within a circular area
- **BoxcastBroadphase**: returns all colliders with bounds that are intersected by collider.bounds. Note that this is a broadphase check so it only checks bounds and does not do individual Collider-to-Collider checks!

Astute readers will have noticed the *layerMask* mentioned above. The layerMask lets you decide which colliders are collided with. Each Collider can have its `PhysicsLayer` set so that when you query the Physics system you can choose to get back only Colliders that match the passed in layerMask. All Physics methods accept a layerMask parameter that defaults to all layers. Use this wisely to filter your collision checks and keep things as performant as possible by not doing unnecessary collision checks.



## Putting the Physics System to Use
Linecasts are extremely useful for various things like checking line-of-sight for enemies, detecting the spatial surroundings of an Entity, fast-moving bullets, etc. Here is an example of casting a line from start to end that just logs the data if it hits something:

```cs
var hit = Physics.Linecast( start, end );
if( hit.Collider != null )
	Debug.Log( "ray hit {0}, entity: {1}", hit, hit.collider.entity );
```

Nez has some more advanced collision/overlap checks using methods such as Minkowski Sums, Separating Axis Theorem and good old trigonometry. These are all wrapped up in simple to use methods on the Collider class for you. Lets take a look at some examples.

This first example is the easiest way to deal with collisions. `deltaMovement` is the amount that you would like to move the Entity, typically `velocity * Time.DeltaTime`. The `CollidesWithAny` method will check all collisons and adjust deltaMovement to resolve any collisions.

```cs
// CollisionResult will contain some really useful information such as the Collider that was hit,
// the normal of the surface hit and the minimum translation vector (MTV). The MTV can be used to
// move the colliding Entity directly adjacent to the hit Collider.
CollisionResult collisionResult;

// do a check to see if entity.getComponent<Collider> (the first Collider on the Entity) collides with any other Colliders in the Scene
// Note that if you have multiple Colliders you could fetch and loop through them instead of only checking the first one.
if( entity.GetComponent<Collider>().CollidesWithAny( ref deltaMovement, out collisionResult ) )
{
	// log the CollisionResult. You may want to use it to add some particle effects or anything else relevant to your game.
	Debug.Log( "collision result: {0}", collisionResult );
}

// move the Entity to the new position. deltaMovement is already adjusted to resolve collisions for us.
entity.Position += deltaMovement;
```


If you need a bit more control over what happens when a collision occurs you can manually check for collisions with other Colliders as well. This next snippet checks for a collision with a specific Collider. Note that when doing this deltaMovement is not adjust for you. It is up to you to take into account the `MinimumTranslationVector` when resolving the collision.

```cs
// declare the CollisionResult
CollisionResult collisionResult;

// do a check to see if entity.getComponent<Collider> collides with someOtherCollider
if( entity.GetComponent<Collider>().CollidesWith( someOtherCollider, deltaMovement, out collisionResult ) )
{
	// move entity to the position directly adjacent to the hit Collider then log the CollisionResult
	entity.Position += deltaMovement - collisionResult.MinimumTranslationVector;
	Debug.Log( "collision result: {0}", collisionResult );
}
```

We can take the above example a step further using the previously mentioned `Physics.BoxcastBroadphase` method, or more specifically a version of it that excludes ourself from the query. That method will give us all the colliders in the Scene that are in our vicinity which we can then use to do our actual collision checks on.

```cs
// fetch anything that we might overlap with at our position excluding ourself. We don't care about ourself here.
var neighborColliders = Physics.BoxcastBroadphaseExcludingSelf( entity.GetComponent<Collider>() );

// loop through and check each Collider for an overlap
foreach( var collider in neighborColliders )
{
	if( entity.GetComponent<Collider>().Overlaps( collider ) )
		Debug.Log( "We are overlapping a Collider: {0}", collider );
}
```

