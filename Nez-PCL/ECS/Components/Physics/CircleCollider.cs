using System;
using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez
{
	public class CircleCollider : Collider
	{
		public float radius
		{
			get { return ((Circle)shape).radius; }
			set { setRadius( value ); }
		}


		/// <summary>
		/// zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself when the
		/// entity is added to the scene.
		/// </summary>
		public CircleCollider()
		{}


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


		/// <summary>
		/// creates a <see cref="Nez.CircleCollider"/> with radius. Note that when specifying a radius if using a RenderableComponent on the Entity as well you
		/// will need to set the appropriate origin to align the <see cref="Nez.CircleCollider"/>
		/// </summary>
		/// <param name="radius">Radius.</param>
		/// <param name="origin">Origin.</param>
		public CircleCollider( float radius, Vector2 origin )
		{
			shape = new Circle( radius );
			_origin = origin;
		}


		#region Fluent setters

		public CircleCollider setRadius( float radius )
		{
			if( radius != ( (Circle)shape ).radius )
			{
				// store the old bounds so we can update ourself after modifying them
				var oldBounds = bounds;
				( (Circle)shape ).radius = radius;
				_areBoundsDirty = true;

				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}
			return this;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			graphics.batcher.drawCircle( bounds.center, ((Circle)shape).radius, Color.IndianRed );
			graphics.batcher.drawPixel( bounds.center, Color.IndianRed, 4 );
		}


		public override string ToString()
		{
			return string.Format( "[CircleCollider: bounds: {0}, radius: {1}", bounds, ((Circle)shape).radius );
		}

	}
}

