using System;
using System.Collections.Generic;
using System.Reflection;


namespace Nez
{
	/// <summary>
	/// indicates that an Entity should be pooled. Note that it also means the Entity must have a paramaterless constructor
	/// </summary>
	public class PooledEntityAttribute : Attribute
	{}


	/// <summary>
	/// Entity subclasses that have the PooledEntityAttribute will be automatically pooled when removed from a scene
	/// </summary>
	public class EntityCache
	{
		internal Dictionary<Type, Queue<Entity>> _cache { get; private set; }


		public EntityCache()
		{
			_cache = new Dictionary<Type,Queue<Entity>>();

			// HACK: make sure this works with PCL change below
			//foreach( var type in Assembly.GetEntryAssembly().GetTypes() )
			foreach( var type in Assembly.GetExecutingAssembly().GetTypes() )
			{
				if( type.GetCustomAttributes( typeof( PooledEntityAttribute ), false ).Length > 0 )
				{
					if( !typeof( Entity ).IsAssignableFrom( type ) )
						throw new Exception( "Type '" + type.Name + "' cannot be Pooled because it doesn't derive from Entity" );
					else if( type.GetConstructor( null ) == null ) // HACK: not sure how to do this the PCL way
						throw new Exception( "Type '" + type.Name + "' cannot be Pooled because it doesn't have a parameterless constructor" );
					else
						_cache.Add( type, new Queue<Entity>() );
				}
			}
		}


		public T create<T>() where T : Entity, new()
		{
			if( !_cache.ContainsKey( typeof( T ) ) )
				return new T();

			var queue = _cache[typeof( T )];
			if( queue.Count == 0 )
				return new T();
			else
				return queue.Dequeue() as T;
		}


		internal void push( Entity entity )
		{
			var type = entity.GetType();
			Queue<Entity> queue;
			if( _cache.TryGetValue( type, out queue ) )
				queue.Enqueue( entity );
		}

	}
}

