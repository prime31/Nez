---
layout: docs
permalink: /documentation/tutorials/custom-components
---

Creating Custom Components
==========

A large portion of the code you write for your game will be in Component subclasses. In this tutorial, we will create a simple Component that moves an Entity around responding to input.

We are introducing a few new systems in the process. Input (mouse/keyboard/gamepad/touchscreen) is all handled through the `Input` class. It contains a series of static methods that you can use to fetch the current state of affairs of all the input devices. We will also be using the `Time` class. It provides access to a key bit of information to assist in making framerate independent movement: the delta frame time. You can access it via `Time.deltaTime`.

Below is the code for the new Component. The comments explain what is happening in detail.

```cs
// first off, we subclass Component. We want to get update calls every frame so we add the IUpdatable interface
// (which just consists of the update method).
public class SimpleMover : Component, IUpdatable
{
	public float speed = 100f;
	
	public void update()
	{
		var moveDir = Vector2.Zero;

		// Input provides access to the keyboard here. We check for the left/right/up/down arrow keys and set the movement direction accordingly.
		if( Input.isKeyDown( Keys.Left ) )
			moveDir.X = -1f;
		else if( Input.isKeyDown( Keys.Right ) )
			moveDir.X = 1f;

		if( Input.isKeyDown( Keys.Up ) )
			moveDir.Y = -1f;
		else if( Input.isKeyDown( Keys.Down ) )
			moveDir.Y = 1f;

		// every Entity has a transform property. The transform defines the Entity's physical representation in space (position/rotation/scale).
		// here we are just modifying the position to move the Entity around. We multiply the movement by Time.deltaTime to keep things
		// framerate independent.
		entity.transform.position += moveDir * speed * Time.deltaTime;
	}
}
```

Using our new Component is trivial now. Expanding on the code from the Getting Started tutorial we will make entityOne moveable by adding our SimpleMover Component to it.

```cs
    var entityOne = myScene.createEntity( "entity-one" );
    entityOne.addComponent( new Sprite( texture ) );
    entityOne.addComponent( new SimpleMover() );
```

Components don't live in a bubble and it's pretty common for one Component to rely on one or more other Components. Let's take a look at how we can access other Components from our Component. Let's make a DamageComponent that reduces the SimpleMover's speed for 2 seconds. We'll do this two different ways to introduce two new Nez features: coroutines and the TimerManager. Commented code is below.

```cs
public class DamageComponent : Component
{
	// we'll store a reference to our SimpleMover for easy access
	SimpleMover _mover;
	
	// this method is called when a Component is added to an Entity. It is called after all the Components are added in a frame so it is
	// safe to access the other Components from here.
	public virtual void onAddedToEntity()
	{
		// we can access any other Components on the Entity via the getComponent method. Just pass in the Type of the Component and it
		// will return the Component or null if no Component of that Type is on the Entity.
		_mover = entity.getComponent<SimpleMover>();
	}
	
	
	// this method is called elsewhere (perhaps when a bullet hits the Entity). It reduces the SimpleMovers speed for 2 seconds
	public void takeDamage()
	{
		// reduce the SimpleMovers speed
		_mover.speed = 70f;
		
		// use the TimerManager to schedule a callback after 2 seconds have elapsed
		Core.schedule( 2f, t => _mover.speed = 100f );
	}
	
	
	public void takeDamageTwo()
	{
		// reduce the SimpleMovers speed
		_mover.speed = 70f;
		
		// use a coroutine to reset the SimpleMovers speed after 2 seconds have elapsed
		Core.startCoroutine( resetSpeedAfterDelay() );
	}
	
	
	IEnumerator resetSpeedAfterDelay()
	{
		// let the CoroutineManager know we want to wait for 2 seconds
		yield return Coroutine.waitForSeconds( 2f );
		
		// reset the speed
		_mover.speed = 100f;
	}
}
```

We didn't delve too deeply into coroutines here but they are a powerful feature for breaking up long running tasks. I highly recommend doing a quick Google search if you are not familiar with them.
