Entity Systems
============
Nez supports entity systems much like you have probably already seen in other entity-component systems. Entity Systems are an easy way to encapsulate game logic that spans across different entities and components. Nez systems are heavily inspired by [Artemis-odb](https://github.com/junkdog/artemis-odb)

Systems are executed in the order you add them to the scene. You might want to add your physics system after your input logic. You might want to do all bullet collision checks after the physics system has calculated the new positions. Adding the systems in the proper order allows you to do this.


## Basic systems
Nez provides a set of basic systems that you can directly use or that you can extend to fit your needs.


## EntitySystem
The base of all systems. It provied an unsorted list of entities matching the components required.

Here's an example of sorting the entities before consuming them for whatever use you might need.

```cs
protected override void process( List<Entity> entities )
{
	entities.Sort( cooldownSort );
	foreach( var entity in entities )
	{
		doSomethingWithTheEntity( entity );
	}
}
```


## EntityProcessingSystem
A basic entity processing system. Use this as the base for processing many entities with specific components. All you need to do is override `process( Entity entity )`. Here's an example of a bullet collision system using EntityProcessingSystem.

```cs
public override void process( Entity entity )
{
	var damage = entity.getComponent<DamageComponent>();
	var colliders = Physics.boxcastBroadphase( entity.getComponent<Collider>.bounds, damage.layerMask );

	foreach( var coll in colliders )
	{
		if( entity.getComponent<Collider>.collidesWith( coll, out collResult ) )
		{
			triggerDamage( coll.entity, entity );
			entity.enabled = false;
			entity.scene.removeEntity( entity );
		}
	}
}
```


## ProcessingSystem
A basic processing system that doesn't rely on entities. It's got no entities associated but it's still being called each frame. Use this as a base class for generic systems that need to coordinate other systems


## PassiveSystem
A basic container that doesn't rely on entities and that doesn't get called each frame. Handy for storing passive actions that might be called by other systems.


## Example system
Here's an example of a system in charge of spawning new enemies. That's the component that holds information about each spawner.

```cs
public class SpawnerComponent : Component
{
	public float cooldown = -1;
	public float minInterval = 2;
	public float maxInterval = 60;
	public int minCount = 1;
	public int maxCount = 1;
	public EnemyType enemyType = EnemyType.Worm;
	public int numSpawned = 0;
	public int numAlive = 0;

	public SpawnerComponent( EnemyType enemyType )
	{
		this.enemyType = enemyType;
	}
}
```

And this is the system that does the actual logic of spawning new enemies based on some simple rules.

```cs
public class SpawnerSystem : EntityProcessingSystem
{
	public SpawnerSystem( Matcher matcher ) : base( matcher )
	{}


	public override void process( Entity entity )
	{
		var spawner = entity.getComponent<SpawnerComponent>();
		if( spawner.numAlive <= 0 )
			spawner.enabled = true;

		if( !spawner.enabled )
			return;

		if( spawner.cooldown == -1 )
		{
			scheduleSpawn( spawner );
			spawner.cooldown /= 4;
		}

		spawner.cooldown -= Time.deltaTime;
		if( spawner.cooldown <= 0 )
		{
			scheduleSpawn( spawner );

			for( var i = 0; i < Nez.Random.range( spawner.minCount, spawner.maxCount ); i++ )
			{
				EntityFactory.createEnemy( entity.position.X, entity.position.Y, spawner.enemyType, entity );
				spawner.numSpawned++;
				spawner.numAlive++;
			}

			if( spawner.numAlive > 0 )
				spawner.enabled = false;
		}
	}


	private void scheduleSpawn( SpawnerComponent spawner )
	{
		spawner.cooldown = Nez.Random.range( spawner.minInterval, spawner.maxInterval );
	}
}
```


## Matchers
Matchers are the equivalent of Artemis-odb's Aspects. They match entities based on a pattern of components. Matchers are used to define what components a system is interested in.

**Note: Matchers do not match Component types inherited from the requested type.**

## Using matchers
Matchers are passed during creation of an EntitySystem and define what components the system is interested in.

```cs
myScene.addEntityProcessor( new PlayerControlSystem( new Matcher().all( typeof( PlayerControlComponent ) ) ) );
```

You can pass an arbitrary number of matchers to the constructor of EntitySystem.

```cs
myScene.addEntityProcessor( new BulletCollisionSystem( new Matcher().all( typeof( DamageComponent ), typeof( BulletComponent ) ) ) );
```

A matcher can either match all entities that have a list of components, or it can match against entities that have at least one of a list of components, or it can match against entites that do not have a certain component.


## Match all
Match all the entities that have both the BuffComponent AND the DamageComponent.

```cs
new Matcher().all( typeof( BuffComponent ), typeof( DamageComponent ) );
```


## Match one
Match all the entities that have at least a BuffComponent or a DamageComponent.

```cs
new Matcher().one( typeof( BuffComponent ), typeof( DamageComponent ) );
```


## Match exclude
Match all the entities that do not have the BuffComponent.

```cs
new Matcher().exclude( typeof( BuffComponent ) );
```
