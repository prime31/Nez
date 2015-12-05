using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	// TODO: should colliders have a scale property as well or is it enough to set width/height?
	public abstract class Collider
	{
		public Entity entity;

		/// <summary>
		/// position is added to entity.position to get the final position for the collider
		/// </summary>
		protected Vector2 _localPosition;
		public Vector2 localPosition
		{
			get { return _localPosition; }
			set
			{
				if( _localPosition != value )
				{
					unregisterColliderWithPhysicsSystem();
					_localPosition = value;
					_areBoundsDirty = true;
					registerColliderWithPhysicsSystem();
				}
			}
		}

		protected Vector2 _origin;
		public Vector2 origin
		{
			get { return _origin; }
			set
			{
				if( _origin != value )
				{
					unregisterColliderWithPhysicsSystem();
					_origin = value;
					_areBoundsDirty = true;
					registerColliderWithPhysicsSystem();
				}
			}
		}

		/// <summary>
		/// helper property for setting the origin in normalized fashion (0-1 for x and y)
		/// </summary>
		/// <value>The origin normalized.</value>
		public Vector2 originNormalized
		{
			get { return new Vector2( _origin.X / width, _origin.Y / height ); }
			set { origin = new Vector2( value.X * width, value.Y * height ); }
		}

		/// <summary>
		/// if this collider is a trigger it will not cause collisions but it will still trigger events
		/// </summary>
		public bool isTrigger;

		/// <summary>
		/// physicsLayer can be used as a filter when dealing with collisions. The Flags class has methods to assist with bitmasks.
		/// </summary>
		public int physicsLayer = 1 << 0;
		public abstract float width { get; set; }
		public abstract float height { get; set; }

		protected Rectangle _bounds;
		public virtual Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = RectangleExtension.fromFloats( entity.position.X + _localPosition.X - _origin.X, entity.position.Y + _localPosition.Y - _origin.Y, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}

		protected bool _isParentEntityAddedToScene;
		protected bool _areBoundsDirty = true;


		public Collider()
		{}


		public bool collidesWithAtPosition( Collider collider, Vector2 position )
		{
			// store off the position so we can restore it after the check. we ignore position changes on the entity while we do this so that
			var savedPosition = entity.position;
			entity.position = position;

			var result = collidesWith( collider );

			// restore position
			entity.position = savedPosition;

			return result;
		}


		public bool collidesWith( Collider collider )
		{
			if( collider is BoxCollider )
				return collidesWith( collider as BoxCollider );
			else if( collider is CircleCollider )
				return collidesWith( collider as CircleCollider );
			else if( collider is MultiCollider )
				return collidesWith( collider as MultiCollider );
			else if( collider is PolygonCollider )
				return collidesWith( collider as PolygonCollider );
			else
				throw new NotImplementedException( "Collisions against the collider type are not implemented!" );
		}


		public abstract bool collidesWith( Vector2 from, Vector2 to );
		public abstract bool collidesWith( BoxCollider boxCollider );
		public abstract bool collidesWith( CircleCollider circle );
		public abstract bool collidesWith( MultiCollider list );
		public abstract bool collidesWith( PolygonCollider polygon );



		/// <summary>
		/// Called when the parent entity is added to a scene
		/// </summary>
		public virtual void onEntityAddedToScene()
		{
			if( width == 0 || height == 0 )
			{
				var renderable = entity.getComponent<RenderableComponent>();
				Debug.warnIf( renderable == null, "Collider has no width/height and no RenderableComponent. Can't figure out how to size it." );
				if( renderable != null )
				{
					var renderableBounds = renderable.bounds;

					width = renderableBounds.Width;
					height = renderableBounds.Height;

					// circle colliders need special care with the origin
					if( this is CircleCollider )
					{
						// fetch the Renderable's center, transfer it to local coordinates and use that as the origin of our collider
						var center = renderableBounds.Center;
						var localCenter = center.ToVector2() - entity.position;
						origin = localCenter;
					}
					else
					{
						originNormalized = renderable.originNormalized;
					}
				}
			}
			_isParentEntityAddedToScene = true;
			registerColliderWithPhysicsSystem();
		}


		/// <summary>
		/// Called when the parent entity is removed from a scene
		/// </summary>
		public virtual void onEntityRemovedFromScene()
		{
			unregisterColliderWithPhysicsSystem();
			_isParentEntityAddedToScene = false;
		}


		public virtual void onEntityPositionChanged()
		{
			_areBoundsDirty = true;
		}


		/// <summary>
		/// the parent Entity will call this at various times (when added to a scene, enabled, etc)
		/// </summary>
		public virtual void registerColliderWithPhysicsSystem()
		{
			// entity could be null if proper such as origin are changed before we are added to an Entity
			if( _isParentEntityAddedToScene )
				Physics.addCollider( this );
		}


		/// <summary>
		/// the parent Entity will call this at various times (when removed from a scene, disabled, etc)
		/// </summary>
		public virtual void unregisterColliderWithPhysicsSystem()
		{
			if( _isParentEntityAddedToScene )
				Physics.removeCollider( this, true );
		}


		public virtual void debugRender( Graphics graphics )
		{
			graphics.drawHollowRect( bounds, Color.IndianRed );
		}

	}
}

