using Nez.PhysicsShapes;


namespace Nez
{
	public class CircleCollider : Collider
	{
		[Inspectable]
		public float Radius
		{
			get => ((Circle) Shape).Radius;
			set => SetRadius(value);
		}


		/// <summary>
		/// zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself when the
		/// entity is added to the scene.
		/// </summary>
		public CircleCollider()
		{
			// we stick a 1px circle in here as a placeholder until the next frame when the Collider is added to the Entity and can get more
			// accurate auto-sizing data
			Shape = new Circle(1);
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
		public CircleCollider(float radius)
		{
			Shape = new Circle(radius);
		}


		#region Fluent setters

		/// <summary>
		/// sets the radius for the CircleCollider
		/// </summary>
		/// <returns>The radius.</returns>
		/// <param name="radius">Radius.</param>
		public CircleCollider SetRadius(float radius)
		{
			_colliderRequiresAutoSizing = false;
			var circle = Shape as Circle;
			if (radius != circle.Radius)
			{
				circle.Radius = radius;
				circle.OriginalRadius = radius;
				_isPositionDirty = true;

				if (Entity != null && _isParentEntityAddedToScene && Enabled)
					Physics.UpdateCollider(this);
			}

			return this;
		}

		#endregion


		public override void DebugRender(Batcher batcher)
		{
			batcher.DrawHollowRect(Bounds, Debug.Colors.ColliderBounds, Debug.Size.LineSizeMultiplier);
			batcher.DrawCircle(Shape.Position, ((Circle) Shape).Radius, Debug.Colors.ColliderEdge,
				Debug.Size.LineSizeMultiplier);
			batcher.DrawPixel(Entity.Transform.Position, Debug.Colors.ColliderPosition,
				4 * Debug.Size.LineSizeMultiplier);
			batcher.DrawPixel(Shape.Position, Debug.Colors.ColliderCenter, 2 * Debug.Size.LineSizeMultiplier);
		}

		public override string ToString()
		{
			return string.Format("[CircleCollider: bounds: {0}, radius: {1}", Bounds, ((Circle) Shape).Radius);
		}
	}
}