using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez
{
	public class CircleCollider : Collider
	{
		[Inspectable]
		public float radius
		{
			get => ( (Circle)shape ).radius;
			set => setRadius( value );
		}


		/// <summary>
		/// zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself when the
		/// entity is added to the scene.
		/// </summary>
		public CircleCollider()
		{
			// we stick a 1px circle in here as a placeholder until the next frame when the Collider is added to the Entity and can get more
			// accurate auto-sizing data
			shape = new Circle( 1 );
			_colliderRequiresAutoSizing = true;
		}


		/// <summary>
		/// creates a CircleCollider with radius. Note that when specifying a radius if using a RenderableComponent on the Entity as well you
		/// will need to set the origin to align the CircleCollider. For example, if the RenderableComponent has a 0,0 origin and a CircleCollider
		/// with a radius of 1.5f * renderable.width is created you can offset the origin by just setting the originNormalied to the center
		/// divided by the scaled size:
		/// 
		/// 	entity.collider = new CircleCollider( moonTexture.Width * 1.5f );
		///     entity.collider.originNormalized = Vector2Extension.halfVector() / 1.5f;
		/// </summary>
		/// <param name="radius">Radius.</param>
		public CircleCollider( float radius )
		{
			shape = new Circle( radius );
		}


		#region Fluent setters

		/// <summary>
		/// sets the radius for the CircleCollider
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="radius">Radius.</param>
		public CircleCollider setRadius( float radius )
		{
			_colliderRequiresAutoSizing = false;
			var circle = shape as Circle;
			if( radius != circle.radius )
			{
				circle.radius = radius;
				circle._originalRadius = radius;
				_isPositionDirty = true;

				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}
			return this;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			graphics.batcher.drawHollowRect( bounds, Debug.Colors.colliderBounds, Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawCircle( shape.position, ( (Circle)shape ).radius, Debug.Colors.colliderEdge, Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPixel( entity.transform.position, Debug.Colors.colliderPosition, 4 * Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPixel( shape.position, Debug.Colors.colliderCenter, 2 * Debug.Size.lineSizeMultiplier );
		}

		public override string ToString()
		{
			return string.Format( "[CircleCollider: bounds: {0}, radius: {1}", bounds, ( (Circle)shape ).radius );
		}

	}
}

