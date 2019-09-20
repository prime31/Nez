Verlet Physics
============
Nez includes a flexible verlet physics system. It comes with example objects such as lines, tires, balls, cloth and more. Verlet physics differ greatly from the normal rigid body physics most folks are accustomed to (Box2D, Chipmunk, PhysX, Bullet, etc). The goal of Nez's verlet physics simulation is not to be a full physics simulation. It is here to provide some neat effects and interactions to jazz up your games.

A few things should be noted up front: the verlet physics system is not a renderer. It handles the physics simulation. It is up to you to render your meshes and textures on top of the simulation data. Verlet objects can interact with standard Nez Colliders but in the name of efficiency they will not interact with each other.


## Basic Example
Getting started using the verlet physics is pretty simple. All you have to do is create a World object and call it's `Update` method. An example Component that does this is below. The verlet system has a built-in debug visual system. It is being used here so that you have something to visualize.

```cs
public class VerletDemo : RenderableComponent, IUpdatable
{
	public override float Width { get { return 800; } }
	public override float Height { get { return 600; } }

	World _world;

	public override void OnAddedToEntity()
	{
		// create the verlet world which handles simulation
		_world = new World( new Rectangle( 0, 0, 800, 600 ) );

		// add a couple built-in Composite objects
		_world.AddComposite( new Tire( new Vector2( 100, 100 ), 50, 20 ) );
		_world.AddComposite( new Cloth( new Vector2( 10, 10 ), 200, 100 ) );
	}

	public void Update()
	{
		_world.Update();
	}

	public override void Render( Batcher batcher, Camera camera )
	{
		_world.DebugRender( batcher );
	}
}
```


## Verlet Objects: Particles, Constraints and Composites
The verlet simulation consists of just two objects: `Particles` and `Constraints`. Particles by default have a 0 radius but they can be made as large as you would like. Particles also have mass which is used when integrating movement. You can optionally pin a Particle which will make it fixed in space and you can optionally have Particles collide with normal Nez Colliders.

Constraints are just mathematic rules that are solved and adjust the Particle positions. At the very simplest we have a `DistanceConstraint`. As it's name implies it will maintain a certain distance between any two Particles. On it's own that isn't very interesting but where it gets fun is that Constraints have a `stiffness` property. A low stiffness means that the Constraint will squash and stretch whereas a high stiffness will act more rigid.

Also included is `AngleConstraint` which will keep a specific angle between any 3 points. Note that you are not limited to just these Constraints. The verlet system is built to allow you to implement your own unique Constraints. All you have to do is subclass Constraint and implement the abstract methods. You can make constraints do whatever you want.

In the example above, we used two of the built-in `Composites`, Tire and Cloth (both classes are simple subclasses of Composite). Composites are just a container for Particles and Constraints so that managing them is easier. Whenever you have an object that you will be creating multiple times you can just subclass Composite to encapsulate it. You can also create Composites on the fly. Lets take a look at how to do that:

```cs
// create the Composite object
var composite = new Composite();

// add some particles. In this case we have three particles in a 'v' shape
composite.AddParticle( new Particle( new Vector2( 50, 50 ) ) );
composite.AddParticle( new Particle( new Vector2( 150, 50 ) ) );
composite.AddParticle( new Particle( new Vector2( 150, 150 ) ) );

// add a squishy AngleConstraint to keep the 'v' angle in place
composite.AddConstraint( new AngleConstraint( composite.Particles[0], composite.Particles[1], composite.Particles[2], 0.1f ) );

// add two DistanceConstraints so the two ends of the 'v' keep their length fairly constant (0.8 stiffness so there is a little give)
composite.AddConstraint( new DistanceConstraint( composite.Particles[0], composite.Particles[1], 0.8f ) );
composite.AddConstraint( new DistanceConstraint( composite.Particles[1], composite.Particles[2], 0.8f ) );

// add the Composite to the World simulation
_world.AddComposite( composite );
```

