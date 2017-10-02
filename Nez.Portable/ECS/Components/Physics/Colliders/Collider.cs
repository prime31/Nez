using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez
{
	public abstract class Collider : Component
	{
		/// <summary>
		/// the underlying Shape of the Collider
		/// </summary>
		public Shape shape;

		/// <summary>
		/// localOffset is added to entity.position to get the final position for the collider geometry. This allows you to add multiple
		/// Colliders to an Entity and position them separately and also lets you set the point of rotation/scale.
		/// </summary>
		public Vector2 localOffset
		{
			get { return _localOffset; }
			set { setLocalOffset( value ); }
		}

		/// <summary>
		/// represents the absolute position to this Collider. It is entity.transform.position + localPosition - origin.
		/// </summary>
		/// <value>The absolute position.</value>
		public Vector2 absolutePosition
		{
			get { return entity.transform.position + _localOffset; }
		}

		/// <summary>
		/// wraps Transform.rotation and returns 0 if this Collider does not rotate with the Entity else it returns Transform.rotation
		/// </summary>
		/// <value>The rotation.</value>
		public float rotation
		{
			get
			{
				if( shouldColliderScaleAndRotateWithTransform && entity != null )
					return entity.transform.rotation;
				return 0;
			}
		}

		/// <summary>
		/// if this collider is a trigger it will not cause collisions but it will still trigger events
		/// </summary>
		public bool isTrigger;

		/// <summary>
		/// physicsLayer can be used as a filter when dealing with collisions. The Flags class has methods to assist with bitmasks.
		/// </summary>
		public int physicsLayer = 1 << 0;

		/// <summary>
		/// layer mask of all the layers this Collider should collide with when Entity.move methods are used. defaults to all layers.
		/// </summary>
		public int collidesWithLayers = Physics.allLayers;

		/// <summary>
		/// if true, the Collider will scale and rotate following the Transform it is attached to
		/// </summary>
		public bool shouldColliderScaleAndRotateWithTransform = true;

		public virtual RectangleF bounds
		{
			get
			{
				if( _isPositionDirty || _isRotationDirty )
				{
					shape.recalculateBounds( this );
					_isPositionDirty = _isRotationDirty = false;
				}

				return shape.bounds;
			}
		}

		/// <summary>
		/// the bounds of this Collider when it was registered with the Physics system. Storing this allows us to always be able to
		/// safely remove the Collider from the Physics system even if it was moved before attempting to remove it.
		/// </summary>
		internal RectangleF registeredPhysicsBounds;
		protected bool _colliderRequiresAutoSizing;

		protected Vector2 _localOffset;
		internal float _localOffsetLength;

		/// <summary>
		/// flag to keep track of if our Entity was added to a Scene
		/// </summary>
		protected bool _isParentEntityAddedToScene;

		/// <summary>
		/// flag to keep track of if we registered ourself with the Physics system
		/// </summary>
		protected bool _isColliderRegistered;
		internal bool _isPositionDirty = true;
		internal bool _isRotationDirty = true;


		#region Fluent setters

		/// <summary>
		/// localOffset is added to entity.position to get the final position for the collider. This allows you to add multiple Colliders
		/// to an Entity and position them separately.
		/// </summary>
		/// <returns>The local offset.</returns>
		/// <param name="offset">Offset.</param>
		public Collider setLocalOffset( Vector2 offset )
		{
			if( _localOffset != offset )
			{
				unregisterColliderWithPhysicsSystem();
				_localOffset = offset;
				_localOffsetLength = _localOffset.Length();
				_isPositionDirty = true;
				registerColliderWithPhysicsSystem();
			}
			return this;
		}


		/// <summary>
		/// if set to true, the Collider will scale and rotate following the Transform it is attached to
		/// </summary>
		/// <returns>The should collider scale and rotate with transform.</returns>
		/// <param name="shouldColliderScaleAndRotateWithTransform">If set to <c>true</c> should collider scale and rotate with transform.</param>
		public Collider setShouldColliderScaleAndRotateWithTransform( bool shouldColliderScaleAndRotateWithTransform )
		{
			this.shouldColliderScaleAndRotateWithTransform = shouldColliderScaleAndRotateWithTransform;
			_isPositionDirty = _isRotationDirty = true;
			return this;
		}

		#endregion


		#region Component Lifecycle

		public override void onAddedToEntity()
		{
			if( _colliderRequiresAutoSizing )
			{
				// we only deal with boxes and circles here
				Assert.isTrue( this is BoxCollider || this is CircleCollider, "Only box and circle colliders can be created automatically" );

				var renderable = entity.getComponent<RenderableComponent>();
				Debug.warnIf( renderable == null, "Collider has no shape and no RenderableComponent. Can't figure out how to size it." );
				if( renderable != null )
				{
					var renderableBounds = renderable.bounds;

					// we need the size * inverse scale here because when we autosize the Collider it needs to be without a scaled Renderable
					var width = renderableBounds.width / entity.transform.scale.X;
					var height = renderableBounds.height / entity.transform.scale.Y;

					// circle colliders need special care with the origin
					if( this is CircleCollider )
					{
						var circleCollider = this as CircleCollider;
						circleCollider.radius = Math.Max( width, height ) * 0.5f;

						// fetch the Renderable's center, transfer it to local coordinates and use that as the localOffset of our collider
						localOffset = renderableBounds.center - entity.transform.position;
					}
					else
					{
						var boxCollider = this as BoxCollider;
						boxCollider.width = width;
						boxCollider.height = height;

						// fetch the Renderable's center, transfer it to local coordinates and use that as the localOffset of our collider
						localOffset = renderableBounds.center - entity.transform.position;
					}
				}
			}
			_isParentEntityAddedToScene = true;
			registerColliderWithPhysicsSystem();
		}


		public override void onRemovedFromEntity()
		{
			unregisterColliderWithPhysicsSystem();
			_isParentEntityAddedToScene = false;
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			// set the appropriate dirty flags
			switch( comp )
			{
				case Transform.Component.Position:
					_isPositionDirty = true;
					break;
				case Transform.Component.Scale:
					_isPositionDirty = true;
					break;
				case Transform.Component.Rotation:
					_isRotationDirty = true;
					break;
			}

			if( _isColliderRegistered )
				Physics.updateCollider( this );
		}


		public override void onEnabled()
		{
			registerColliderWithPhysicsSystem();
		}


		public override void onDisabled()
		{
			unregisterColliderWithPhysicsSystem();
		}

		#endregion


		/// <summary>
		/// the parent Entity will call this at various times (when added to a scene, enabled, etc)
		/// </summary>
		public virtual void registerColliderWithPhysicsSystem()
		{
			// entity could be null if properties such as origin are changed before we are added to an Entity
			if( _isParentEntityAddedToScene && !_isColliderRegistered )
			{
				Physics.addCollider( this );
				_isColliderRegistered = true;
			}
		}


		/// <summary>
		/// the parent Entity will call this at various times (when removed from a scene, disabled, etc)
		/// </summary>
		public virtual void unregisterColliderWithPhysicsSystem()
		{
			if( _isParentEntityAddedToScene && _isColliderRegistered )
				Physics.removeCollider( this );
			_isColliderRegistered = false;
		}


		#region collision checks

		/// <summary>
		/// checks to see if this shape overlaps any other Colliders in the Physics system
		/// </summary>
		/// <param name="collider">Collider.</param>
		public bool overlaps( Collider other )
		{
			return shape.overlaps( other.shape );
		}


		/// <summary>
		/// checks to see if this Collider collides with collider. If it does, true will be returned and result will be populated
		/// with collision data
		/// </summary>
		/// <returns><c>true</c>, if with was collidesed, <c>false</c> otherwise.</returns>
		/// <param name="collider">Collider.</param>
		/// <param name="result">Result.</param>
		public bool collidesWith( Collider collider, out CollisionResult result )
		{
			if( shape.collidesWithShape( collider.shape, out result ) )
			{
				result.collider = collider;
				return true;
			}
			return false;
		}


		/// <summary>
		/// checks to see if this Collider with motion applied (delta movement vector) collides with collider. If it does, true will be
		/// returned and result will be populated with collision data.
		/// </summary>
		/// <returns><c>true</c>, if with was collidesed, <c>false</c> otherwise.</returns>
		/// <param name="collider">Collider.</param>
		/// <param name="motion">Motion.</param>
		/// <param name="result">Result.</param>
		public bool collidesWith( Collider collider, Vector2 motion, out CollisionResult result )
		{
			// alter the shapes position so that it is in the place it would be after movement so we can check for overlaps
			var oldPosition = shape.position;
			shape.position += motion;

			var didCollide = shape.collidesWithShape( collider.shape, out result );
			if( didCollide )
				result.collider = collider;

			// return the shapes position to where it was before the check
			shape.position = oldPosition;

			return didCollide;
		}


		/// <summary>
		/// checks to see if this Collider collides with any other Colliders in the Scene. The first Collider it intersects will have its collision
		/// data returned in the CollisionResult.
		/// </summary>
		/// <returns><c>true</c>, if with was collidesed, <c>false</c> otherwise.</returns>
		/// <param name="result">Result.</param>
		public bool collidesWithAny( out CollisionResult result )
		{
			result = new CollisionResult();

			// fetch anything that we might collide with at our new position
			var neighbors = Physics.boxcastBroadphaseExcludingSelf( this, collidesWithLayers );

			foreach( var neighbor in neighbors )
			{
				// skip triggers
				if( neighbor.isTrigger )
					continue;

				if( collidesWith( neighbor, out result ) )
					return true;
			}

			return false;
		}


		/// <summary>
		/// checks to see if this Collider with motion applied (delta movement vector) collides with any collider. If it does, true will be
		/// returned and result will be populated with collision data. Motion will be set to the maximum distance the Collider can travel
		/// before colliding.
		/// </summary>
		/// <returns><c>true</c>, if with was collidesed, <c>false</c> otherwise.</returns>
		/// <param name="motion">Motion.</param>
		/// <param name="result">Result.</param>
		public bool collidesWithAny( ref Vector2 motion, out CollisionResult result )
		{
			result = new CollisionResult();

			// fetch anything that we might collide with at our new position
			var colliderBounds = bounds;
			colliderBounds.x += motion.X;
			colliderBounds.y += motion.Y;
			var neighbors = Physics.boxcastBroadphaseExcludingSelf( this, ref colliderBounds, collidesWithLayers );

			// alter the shapes position so that it is in the place it would be after movement so we can check for overlaps
			var oldPosition = shape.position;
			shape.position += motion;

			var didCollide = false;
			foreach( var neighbor in neighbors )
			{
				// skip triggers
				if( neighbor.isTrigger )
					continue;

				if( collidesWith( neighbor, out result ) )
				{
					// hit. back off our motion and our Shape.position
					motion -= result.minimumTranslationVector;
					shape.position -= result.minimumTranslationVector;
					didCollide = true;
				}
			}

			// return the shapes position to where it was before the check
			shape.position = oldPosition;

			return didCollide;
		}

		/// <summary>
		/// checks for collisions with other colliders. The collisionResults list will be populated with any collisions that occur.
		/// </summary>
		/// <param name="collisionResults">The list to populate with collision results.</param>
		public void getAllCollisions(List<CollisionResult> collisionResults)
		{
			// retrieve neighbors
			var neighbors = Physics.boxcastBroadphaseExcludingSelf( this, collidesWithLayers );

			foreach ( var neighbor in neighbors )
			{
				CollisionResult result;
				
				// skip triggers
				if( neighbor.isTrigger )
					continue;

				if ( collidesWith( neighbor, out result ) )
					collisionResults.Add(result);
			}
		}

		#endregion


		public override Component clone()
		{
			var collider = MemberwiseClone() as Collider;
			collider.entity = null;

			if( shape != null )
				collider.shape = shape.clone();

			return collider;
		}

	}
}

