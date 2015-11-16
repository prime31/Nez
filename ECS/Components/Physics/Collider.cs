using System;
using Nez.Physics;
using Microsoft.Xna.Framework;


namespace Nez
{
	public abstract class Collider
	{
		public Entity entity;
		/// <summary>
		/// position is added to entity.position to get the final position for the collider
		/// </summary>
		public Vector2 position;
		public bool isTrigger;
		public int physicsLayer;
		public abstract float width { get; set; }
		public abstract float height { get; set; }

		public virtual Rectangle bounds
		{
			get
			{
				// TODO: cache this and block with a dirty flag so that we only update when necessary
				if( entity == null )
					return RectangleExtension.fromFloats( position.X, position.Y, width, height );
				return RectangleExtension.fromFloats( entity.position.X + position.X, entity.position.Y + position.Y, width, height );
			}
		}


		public Collider()
		{}


		public bool collidesWithAtPosition( Collider collider, Vector2 position )
		{
			var savedPosition = entity.position;
			entity.position = position;

			var result = collidesWith( collider );

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
			else
				throw new NotImplementedException( "Collisions against the collider type are not implemented!" );
		}


		public abstract bool collidesWith( Vector2 from, Vector2 to );
		public abstract bool collidesWith( BoxCollider boxCollider );
		public abstract bool collidesWith( CircleCollider circle );
		public abstract bool collidesWith( MultiCollider list );



		/// <summary>
		/// the parent Entity will call this at various times (when added to a scene, enabled, etc)
		/// </summary>
		public virtual void registerColliderWithPhysicsSystem()
		{
			entity.scene.physics.addCollider( this );
		}


		/// <summary>
		/// the parent Entity will call this at various times (when removed from a scene, disabled, etc)
		/// </summary>
		public virtual void unregisterColliderWithPhysicsSystem()
		{
			entity.scene.physics.removeCollider( this, true );
		}


		public virtual void debugRender( Graphics graphics )
		{
			graphics.drawHollowRect( bounds, Color.IndianRed );
		}

	}
}

