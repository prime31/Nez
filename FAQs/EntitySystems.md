Entity Systems
============
Nez supports entity systems much like you have probably already seen in other entity-component systems. Entity Systems are an easy way to encapsulate game logic that spans across different entities and components. Nez systems are heavily inspired by [Artemis-odb](https://github.com/junkdog/artemis-odb)

Systems are executed in the order you add them to the scene. You might want to add your physics system after your input logic. You might want to do all bullet collision checks after the physics system has calculated the new positions. Adding the systems in the proper order allows you to do this.


## Basic systems
Nez provides a set of basic systems that you can directly use or that you can extend to fit your needs.


## EntitySystem
The base of all systems. It provides an unsorted list of entities matching the components required.

Here's an example of sorting the entities before consuming them for whatever use you might need.

```cs
protected override void Process( List<Entity> entities )
{
	entities.Sort( cooldownSort );
	foreach( var entity in entities )
	{
		DoSomethingWithTheEntity( entity );
	}
}
```


## EntityProcessingSystem
A basic entity processing system. Use this as the base for processing many entities with specific components. All you need to do is override `Process( Entity entity )`. Here's an example of a bullet collision system using EntityProcessingSystem.

```cs
public override void Process( Entity entity )
{
	var damage = entity.GetComponent<DamageComponent>();
	var colliders = Physics.BoxcastBroadphase( entity.GetComponent<Collider>.bounds, damage.LayerMask );

	foreach( var coll in colliders )
	{
		if( entity.GetComponent<Collider>.CollidesWith( coll, out collResult ) )
		{
			TriggerDamage( coll.entity, entity );
			entity.Enabled = false;
			entity.Scene.RemoveEntity( entity );
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
	public float Cooldown = -1;
	public float MinInterval = 2;
	public float MaxInterval = 60;
	public int MinCount = 1;
	public int MaxCount = 1;
	public EnemyType EnemyType = EnemyType.Worm;
	public int NumSpawned = 0;
	public int NumAlive = 0;

	public SpawnerComponent( EnemyType enemyType )
	{
		this.EnemyType = enemyType;
	}
}
```

And this is the system that does the actual logic of spawning new enemies based on some simple rules.

```cs
public class SpawnerSystem : EntityProcessingSystem
{
	public SpawnerSystem( Matcher matcher ) : base( matcher )
	{}


	public override void Process( Entity entity )
	{
		var spawner = entity.GetComponent<SpawnerComponent>();
		if( spawner.NumAlive <= 0 )
			spawner.Enabled = true;

		if( !spawner.Enabled )
			return;

		if( spawner.Cooldown == -1 )
		{
			ScheduleSpawn( spawner );
			spawner.Cooldown /= 4;
		}

		spawner.Cooldown -= Time.DeltaTime;
		if( spawner.Cooldown <= 0 )
		{
			ScheduleSpawn( spawner );

			for( var i = 0; i < Nez.Random.Range( spawner.MinCount, spawner.MaxCount ); i++ )
			{
				EntityFactory.CreateEnemy( entity.Position.X, entity.Position.Y, spawner.EnemyType, entity );
				spawner.NumSpawned++;
				spawner.NumAlive++;
			}

			if( spawner.NumAlive > 0 )
				spawner.Enabled = false;
		}
	}


	private void ScheduleSpawn( SpawnerComponent spawner )
	{
		spawner.Cooldown = Nez.Random.Range( spawner.MinInterval, spawner.MaxInterval );
	}
}
```


## Matchers
Matchers are the equivalent of Artemis-odb's Aspects. They match entities based on a pattern of components. Matchers are used to define what components a system is interested in.

**Note: Matchers do not match Component types inherited from the requested type.**

## Using matchers
Matchers are passed during creation of an EntitySystem and define what components the system is interested in.

```cs
myScene.AddEntityProcessor( new PlayerControlSystem( new Matcher().All( typeof( PlayerControlComponent ) ) ) );
```

You can pass an arbitrary number of matchers to the constructor of EntitySystem.

```cs
myScene.AddEntityProcessor( new BulletCollisionSystem( new Matcher().All( typeof( DamageComponent ), typeof( BulletComponent ) ) ) );
```

A matcher can either match all entities that have a list of components, or it can match against entities that have at least one of a list of components, or it can match against entites that do not have a certain component.


## Match all
Match all the entities that have both the BuffComponent AND the DamageComponent.

```cs
new Matcher().All( typeof( BuffComponent ), typeof( DamageComponent ) );
```


## Match one
Match all the entities that have at least a BuffComponent or a DamageComponent.

```cs
new Matcher().One( typeof( BuffComponent ), typeof( DamageComponent ) );
```


## Match exclude
Match all the entities that do not have the BuffComponent.

```cs
new Matcher().Exclude( typeof( BuffComponent ) );
```
