using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class Entity : IComparable<Entity>
	{
		static uint _idGenerator;

		#region properties and fields

		/// <summary>
		/// the scene this entity belongs to
		/// </summary>
		public Scene Scene;

		/// <summary>
		/// entity name. useful for doing scene-wide searches for an entity
		/// </summary>
		public string Name;

		/// <summary>
		/// unique identifer for this Entity
		/// </summary>
		public readonly uint Id;

		/// <summary>
		/// encapsulates the Entity's position/rotation/scale and allows setting up a hieararchy
		/// </summary>
		public readonly Transform Transform;

		/// <summary>
		/// list of all the components currently attached to this entity
		/// </summary>
		public readonly ComponentList Components;

		/// <summary>
		/// use this however you want to. It can later be used to query the scene for all Entities with a specific tag
		/// </summary>
		public int Tag
		{
			get => _tag;
			set => SetTag(value);
		}

		/// <summary>
		/// specifies how often this entitys update method should be called. 1 means every frame, 2 is every other, etc
		/// </summary>
		public uint UpdateInterval = 1;

		/// <summary>
		/// enables/disables the Entity. When disabled colliders are removed from the Physics system and components methods will not be called
		/// </summary>
		public bool Enabled
		{
			get => _enabled;
			set => SetEnabled(value);
		}

		/// <summary>
		/// update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
		/// </summary>
		/// <value>The order.</value>
		public int UpdateOrder
		{
			get => _updateOrder;
			set => SetUpdateOrder(value);
		}

		/// <summary>
		/// if destroy was called, this will be true until the next time Entitys are processed
		/// </summary>
		public bool IsDestroyed => _isDestroyed;

		/// <summary>
		/// flag indicating if destroy was called on this Entity
		/// </summary>
		internal bool _isDestroyed;

		int _tag = 0;
		bool _enabled = true;
		internal int _updateOrder = 0;

		#endregion


		#region Transform passthroughs

		public Transform Parent
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.Parent;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetParent(value);
		}

		public int ChildCount
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.ChildCount;
		}

		public Vector2 Position
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.Position;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetPosition(value);
		}

		public Vector2 LocalPosition
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.LocalPosition;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetLocalPosition(value);
		}

		public float Rotation
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.Rotation;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetRotation(value);
		}

		public float RotationDegrees
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.RotationDegrees;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetRotationDegrees(value);
		}

		public float LocalRotation
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.LocalRotation;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetLocalRotation(value);
		}

		public float LocalRotationDegrees
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.LocalRotationDegrees;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetLocalRotationDegrees(value);
		}

		public Vector2 Scale
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.Scale;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetScale(value);
		}

		public Vector2 LocalScale
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.LocalScale;
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			set => Transform.SetLocalScale(value);
		}

		public Matrix2D WorldInverseTransform
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.WorldInverseTransform;
		}

		public Matrix2D LocalToWorldTransform
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.LocalToWorldTransform;
		}

		public Matrix2D WorldToLocalTransform
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get => Transform.WorldToLocalTransform;
		}

		#endregion


		public Entity(string name)
		{
			Components = new ComponentList(this);
			Transform = new Transform(this);
			Name = name;
			Id = _idGenerator++;
		}

		public Entity() : this(Utils.RandomString(8))
		{ }

		internal void OnTransformChanged(Transform.Component comp)
		{
			// notify our children of our changed position
			Components.OnEntityTransformChanged(comp);
		}


		#region Fluent setters

		/// <summary>
		/// sets the tag for the Entity
		/// </summary>
		/// <returns>The tag.</returns>
		/// <param name="tag">Tag.</param>
		public Entity SetTag(int tag)
		{
			if (_tag != tag)
			{
				// we only call through to the entityTagList if we already have a scene. if we dont have a scene yet we will be
				// added to the entityTagList when we do
				if (Scene != null)
					Scene.Entities.RemoveFromTagList(this);
				_tag = tag;
				if (Scene != null)
					Scene.Entities.AddToTagList(this);
			}

			return this;
		}

		/// <summary>
		/// sets the enabled state of the Entity. When disabled colliders are removed from the Physics system and components methods will not be called
		/// </summary>
		/// <returns>The enabled.</returns>
		/// <param name="isEnabled">If set to <c>true</c> is enabled.</param>
		public Entity SetEnabled(bool isEnabled)
		{
			if (_enabled != isEnabled)
			{
				_enabled = isEnabled;

				if (_enabled)
					Components.OnEntityEnabled();
				else
					Components.OnEntityDisabled();
			}

			return this;
		}

		/// <summary>
		/// sets the update order of this Entity. updateOrder is also used to sort tag lists on scene.entities
		/// </summary>
		/// <returns>The update order.</returns>
		/// <param name="updateOrder">Update order.</param>
		public Entity SetUpdateOrder(int updateOrder)
		{
			if (_updateOrder != updateOrder)
			{
				_updateOrder = updateOrder;
				if (Scene != null)
				{
					Scene.Entities.MarkEntityListUnsorted();
					Scene.Entities.MarkTagUnsorted(Tag);
				}
			}

			return this;
		}

		#endregion


		/// <summary>
		/// removes the Entity from the scene and destroys all children
		/// </summary>
		public void Destroy()
		{
			if (Scene == null)
				return;

			_isDestroyed = true;
			Scene.Entities.Remove(this);
			Transform.Parent = null;

			// destroy any children we have
			for (var i = Transform.ChildCount - 1; i >= 0; i--)
			{
				var child = Transform.GetChild(i);
				child.Entity.Destroy();
			}
		}

		/// <summary>
		/// detaches the Entity from the scene.
		/// the following lifecycle method will be called on the Entity: OnRemovedFromScene
		/// the following lifecycle method will be called on the Components: OnRemovedFromEntity
		/// </summary>
		public void DetachFromScene()
		{
			Scene.Entities.Remove(this);
			Components.DeregisterAllComponents();

			for (var i = 0; i < Transform.ChildCount; i++)
				Transform.GetChild(i).Entity.DetachFromScene();
		}

		/// <summary>
		/// attaches an Entity that was previously detached to a new scene
		/// </summary>
		/// <param name="newScene">New scene.</param>
		public void AttachToScene(Scene newScene)
		{
			Scene = newScene;
			newScene.Entities.Add(this);
			Components.RegisterAllComponents();

			for (var i = 0; i < Transform.ChildCount; i++)
				Transform.GetChild(i).Entity.AttachToScene(newScene);
		}

		/// <summary>
		/// creates a deep clone of this Entity. Subclasses can override this method to copy any custom fields. When overriding,
		/// the CopyFrom method should be called which will clone all Components, Colliders and Transform children for you. Note
		/// that the cloned Entity will not be added to any Scene! You must add them yourself!
		/// </summary>
		public virtual Entity Clone(Vector2 position = default(Vector2))
		{
			var entity = Activator.CreateInstance(GetType()) as Entity;
			entity.Name = Name + "(clone)";
			entity.CopyFrom(this);
			entity.Transform.Position = position;

			return entity;
		}

		/// <summary>
		/// copies the properties, components and colliders of Entity to this instance
		/// </summary>
		/// <param name="entity">Entity.</param>
		protected void CopyFrom(Entity entity)
		{
			// Entity fields
			Tag = entity.Tag;
			UpdateInterval = entity.UpdateInterval;
			UpdateOrder = entity.UpdateOrder;
			Enabled = entity.Enabled;

			Transform.Scale = entity.Transform.Scale;
			Transform.Rotation = entity.Transform.Rotation;

			// clone Components
			for (var i = 0; i < entity.Components.Count; i++)
				AddComponent(entity.Components[i].Clone());
			for (var i = 0; i < entity.Components._componentsToAdd.Count; i++)
				AddComponent(entity.Components._componentsToAdd[i].Clone());

			// clone any children of the Entity.transform
			for (var i = 0; i < entity.Transform.ChildCount; i++)
			{
				var child = entity.Transform.GetChild(i).Entity;

				var childClone = child.Clone();
				childClone.Transform.CopyFrom(child.Transform);
				childClone.Transform.Parent = Transform;
			}
		}


		#region Entity lifecycle methods

		/// <summary>
		/// Called when this entity is added to a scene after all pending entity changes are committed
		/// </summary>
		public virtual void OnAddedToScene()
		{ }

		/// <summary>
		/// Called when this entity is removed from a scene
		/// </summary>
		public virtual void OnRemovedFromScene()
		{
			// if we were destroyed, remove our components. If we were just detached we need to keep our components on the Entity.
			if (_isDestroyed)
				Components.RemoveAllComponents();
		}

		/// <summary>
		/// called each frame as long as the Entity is enabled
		/// </summary>
		public virtual void Update() => Components.Update();

		/// <summary>
		/// called if Core.debugRenderEnabled is true by the default renderers. Custom renderers can choose to call it or not.
		/// </summary>
		/// <param name="batcher">Batcher.</param>
		public virtual void DebugRender(Batcher batcher) => Components.DebugRender(batcher);

		#endregion


		#region Component Management

		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <param name="component">Component.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddComponent<T>(T component) where T : Component
		{
			component.Entity = this;
			Components.Add(component);
			component.Initialize();
			return component;
		}

		/// <summary>
		/// Adds a Component to the components list. Returns the Component.
		/// </summary>
		/// <returns>Scene.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T AddComponent<T>() where T : Component, new()
		{
			var component = new T();
			component.Entity = this;
			Components.Add(component);
			component.Initialize();
			return component;
		}

		/// <summary>
		/// Gets the first component of type T and returns it. If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetComponent<T>() where T : class => Components.GetComponent<T>(false);

		/// <summary>
		/// Tries to get the component of type T. If no components are found returns false.
		/// </summary>
		/// <returns>true if a component has been found.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public bool TryGetComponent<T>(out T component) where T : class
		{
			component = Components.GetComponent<T>(false);
			return component != null;
		}

		/// <summary>
		/// checks to see if the Entity has the component
		/// </summary>
		public bool HasComponent<T>() where T : class => Components.GetComponent<T>(false) != null;

		/// <summary>
		/// Gets the first Component of type T and returns it. If no Component is found the Component will be created.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetOrCreateComponent<T>() where T : Component, new()
		{
			var comp = Components.GetComponent<T>(true);
			if (comp == null)
				comp = AddComponent<T>();

			return comp;
		}

		/// <summary>
		/// Gets the first component of type T and returns it optionally skips checking un-initialized Components (Components who have not yet had their
		/// onAddedToEntity method called). If no components are found returns null.
		/// </summary>
		/// <returns>The component.</returns>
		/// <param name="onlyReturnInitializedComponents">If set to <c>true</c> only return initialized components.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public T GetComponent<T>(bool onlyReturnInitializedComponents) where T : class
		{
			return Components.GetComponent<T>(onlyReturnInitializedComponents);
		}

		/// <summary>
		/// Gets all the components of type T without a List allocation
		/// </summary>
		/// <param name="componentList">Component list.</param>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public void GetComponents<T>(List<T> componentList) where T : class => Components.GetComponents(componentList);

		/// <summary>
		/// Gets all the components of type T. The returned List can be put back in the pool via ListPool.free.
		/// </summary>
		/// <returns>The component.</returns>
		/// <typeparam name="T">The 1st type parameter.</typeparam>
		public List<T> GetComponents<T>() where T : class => Components.GetComponents<T>();

		/// <summary>
		/// removes the first Component of type T from the components list
		/// </summary>
		public bool RemoveComponent<T>() where T : Component
		{
			var comp = GetComponent<T>();
			if (comp != null)
			{
				RemoveComponent(comp);
				return true;
			}

			return false;
		}

		/// <summary>
		/// removes a Component from the components list
		/// </summary>
		/// <param name="component">The Component to remove</param>
		public void RemoveComponent(Component component) => Components.Remove(component);

		/// <summary>
		/// removes all Components from the Entity
		/// </summary>
		public void RemoveAllComponents()
		{
			for (var i = 0; i < Components.Count; i++)
				RemoveComponent(Components[i]);
		}

		#endregion


		public int CompareTo(Entity other)
		{
			var compare = _updateOrder.CompareTo(other._updateOrder);
			if (compare == 0)
				compare = Id.CompareTo(other.Id);
			return compare;
		}

		public override string ToString()
		{
			return string.Format("[Entity: name: {0}, tag: {1}, enabled: {2}, depth: {3}]", Name, Tag, Enabled, UpdateOrder);
		}
	}
}
