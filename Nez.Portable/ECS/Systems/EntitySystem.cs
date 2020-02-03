using System.Collections.Generic;


namespace Nez
{
	public class EntitySystem
	{
		public Matcher Matcher => _matcher;

		public Scene Scene
		{
			get => _scene;
			set
			{
				_scene = value;
				_entities = new List<Entity>();
			}
		}

		protected Matcher _matcher;
		protected List<Entity> _entities = new List<Entity>();
		protected Scene _scene;


		public EntitySystem()
		{
			_matcher = Matcher.Empty();
		}


		public EntitySystem(Matcher matcher)
		{
			_matcher = matcher;
		}


		public virtual void OnChange(Entity entity)
		{
			var contains = _entities.Contains(entity);
			var interest = _matcher.IsInterested(entity);

			if (interest && !contains)
				Add(entity);
			else if (!interest && contains)
				Remove(entity);
		}


		public virtual void Add(Entity entity)
		{
			_entities.Add(entity);
			OnAdded(entity);
		}


		public virtual void Remove(Entity entity)
		{
			_entities.Remove(entity);
			OnRemoved(entity);
		}


		public virtual void OnAdded(Entity entity)
		{
		}


		public virtual void OnRemoved(Entity entity)
		{
		}


		protected virtual void Process(List<Entity> entities)
		{
		}


		protected virtual void LateProcess(List<Entity> entities)
		{
		}


		protected virtual void Begin()
		{
		}


		public void Update()
		{
			Begin();
			Process(_entities);
		}


		public void LateUpdate()
		{
			LateProcess(_entities);
			End();
		}


		protected virtual void End()
		{
		}
	}
}