using System;
using System.Collections.Generic;


namespace Nez
{
	/// <summary>
	/// Basic entity processing system. Use this as the base for processing many entities with specific components
	/// </summary>
	public abstract class EntityProcessingSystem : EntitySystem
	{
		public EntityProcessingSystem( Matcher matcher ) : base( matcher )
		{}


		/// <summary>
		/// Processes a specific entity. It's called for all the entities in the list.
		/// </summary>
		/// <param name="entity">Entity.</param>
		public abstract void process( Entity entity );


		/// <summary>
		/// Goes through all the entities of this system and processes them one by one
		/// </summary>
		/// <param name="entities">Entities.</param>
		protected override void process( List<Entity> entities )
		{
			for( var i = 0; i < entities.Count; i++ )
				process( entities[i] );
		}
	}
}

