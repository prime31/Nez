using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez
{
	public class CircleCollider : Collider
	{
		public float radius
		{
			get { return ( (Circle)shape ).radius; }
			set { setRadius( value ); }
		}


		/// <summary>
		/// zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself when the
		/// entity is added to the scene.
		/// </summary>
		public CircleCollider()
		{ }


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

		public CircleCollider setRadius( float radius )
		{
			if( radius != ( (Circle)shape ).radius )
			{
				( (Circle)shape ).radius = radius;
				_isPositionDirty = true;

				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}
			return this;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			graphics.batcher.drawHollowRect( bounds, Color.White * 0.3f );
			graphics.batcher.drawCircle( shape.position, ( (Circle)shape ).radius, DefaultColors.colliderEdge );
			graphics.batcher.drawPixel( entity.transform.position, DefaultColors.colliderPosition, 4 );
			graphics.batcher.drawPixel( shape.position, DefaultColors.colliderCenter, 2 );
		}


		public override string ToString()
		{
			return string.Format( "[CircleCollider: bounds: {0}, radius: {1}", bounds, ( (Circle)shape ).radius );
		}

	}
}

