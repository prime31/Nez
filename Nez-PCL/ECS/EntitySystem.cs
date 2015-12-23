using System;
using System.Collections.Generic;


namespace Nez
{
	public class EntitySystem
	{
		protected Matcher _matcher;
		public Matcher matcher
		{
			get { return _matcher; }
		}

		protected List<Entity> _entities = new List<Entity>();

		protected Scene _scene;
		public Scene scene 
		{
			get { return _scene; }
			set 
			{
				_scene = value;
				_entities = new List<Entity>();
			}
		}

		public EntitySystem()
		{
			_matcher = Matcher.empty();
		}

		public EntitySystem(Matcher matcher) : this()
		{
			_matcher = matcher;
		}

		public virtual void onChange(Entity entity) 
		{
			bool contains = _entities.Contains( entity );
			bool interest = _matcher.isInterested( entity );

			if (interest && !contains)
			{
				Add(entity);
			}
			else if (!interest && contains)
			{
				Remove(entity);
			}
		}

		public virtual void Add(Entity entity) 
		{
			_entities.Add( entity );
			onAdded( entity );
		}

		public virtual void Remove(Entity entity) 
		{
			_entities.Remove( entity );
			onRemoved( entity );
		}

		public virtual void onAdded( Entity entity )
		{
		}

		public virtual void onRemoved( Entity entity )
		{
		}

		protected virtual void process(List<Entity> entities)
		{
		}

		protected virtual void begin()
		{
		}

		public void update()
		{
			begin();
			process(_entities);
			end();
		}

		protected virtual void end()
		{
		}
	}
}

