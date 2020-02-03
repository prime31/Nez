using System.Collections.Generic;


namespace Nez
{
	public class EntityProcessorList
	{
		protected List<EntitySystem> _processors = new List<EntitySystem>();


		public void Add(EntitySystem processor)
		{
			_processors.Add(processor);
		}


		public void Remove(EntitySystem processor)
		{
			_processors.Remove(processor);
		}


		public virtual void OnComponentAdded(Entity entity)
		{
			NotifyEntityChanged(entity);
		}


		public virtual void OnComponentRemoved(Entity entity)
		{
			NotifyEntityChanged(entity);
		}


		public virtual void OnEntityAdded(Entity entity)
		{
			NotifyEntityChanged(entity);
		}


		public virtual void OnEntityRemoved(Entity entity)
		{
			RemoveFromProcessors(entity);
		}


		protected virtual void NotifyEntityChanged(Entity entity)
		{
			for (var i = 0; i < _processors.Count; i++)
				_processors[i].OnChange(entity);
		}


		protected virtual void RemoveFromProcessors(Entity entity)
		{
			for (var i = 0; i < _processors.Count; i++)
				_processors[i].Remove(entity);
		}


		public void Begin()
		{
		}


		public void Update()
		{
			for (var i = 0; i < _processors.Count; i++)
				_processors[i].Update();
		}


		public void LateUpdate()
		{
			for (var i = 0; i < _processors.Count; i++)
				_processors[i].LateUpdate();
		}


		public void End()
		{
		}


		public T GetProcessor<T>() where T : EntitySystem
		{
			for (var i = 0; i < _processors.Count; i++)
			{
				var processor = _processors[i];
				if (processor is T)
					return processor as T;
			}

			return null;
		}
	}
}