using System;
using System.Collections.Generic;


namespace Nez
{
	public class EntityProcessorList
	{
		protected List<EntitySystem> _processors = new List<EntitySystem>();

		public EntityProcessorList()
		{
		}

		public void add(EntitySystem processor)
		{
			_processors.Add( processor );
		}

		public void remove(EntitySystem processor)
		{
			_processors.Remove( processor );
		}

		public virtual void onComponentAdded(Entity entity)
		{
			notifyEntityChanged( entity );
		}

		public virtual void onComponentRemoved(Entity entity)
		{
			notifyEntityChanged( entity );
		}

		public virtual void onEntityAdded(Entity entity)
		{
			notifyEntityChanged( entity );
		}

		public virtual void onEntityRemoved(Entity entity)
		{
			removeFromProcessors( entity );
		}

		protected virtual void notifyEntityChanged(Entity entity)
		{
			foreach( var processor in _processors )
			{
				processor.onChange( entity );
			}
		}

		protected virtual void removeFromProcessors(Entity entity)
		{
			foreach( var processor in _processors )
			{
				processor.Remove( entity );
			}
		}

		public void begin()
		{
		}

		public void update()
		{
			foreach( var processor in _processors )
			{
				processor.update();
			}
		}
	
		public void end()
		{
		}

		public T getProcessor<T>() where T : EntitySystem
		{
			for( var i = 0; i < _processors.Count; i++ )
			{
				var processor = _processors[i];
				if( processor is T )
					return processor as T;
			}

			return null;
		}

	}
}

