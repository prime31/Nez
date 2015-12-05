using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class CircleCollider : Collider
	{
		public float _radius;
		public float radius
		{
			get { return _radius; }
			set
			{
				// store the old bounds so we can update ourself after modifying them
				var oldBounds = bounds;
				_radius = value;
				_areBoundsDirty = true;

				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this, ref oldBounds );
			}
		}

		public override float width
		{
			get { return _radius * 2f; }
			set { radius = value * 0.5f; }
		}

		/// <summary>
		/// for a circle, height is radius * 2
		/// </summary>
		/// <value>The height.</value>
		public override float height
		{
			get { return _radius * 2f; }
			set { radius = value * 0.5f; }
		}

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = RectangleExtension.fromFloats( entity.position.X + _localPosition.X + origin.X - _radius, entity.position.Y + _localPosition.Y + origin.Y - _radius, width, height );
					_areBoundsDirty = false;
				}

				return _bounds;
			}
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
			_radius = radius;
		}


		/// <summary>
		/// creates a <see cref="Nez.CircleCollider"/> with radius. Note that when specifying a radius if using a RenderableComponent on the Entity as well you
		/// will need to set the appropriate origin to align the <see cref="Nez.CircleCollider"/>
		/// </summary>
		/// <param name="radius">Radius.</param>
		/// <param name="origin">Origin.</param>
		public CircleCollider( float radius, Vector2 origin )
		{
			_radius = radius;
			_origin = origin;
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.drawCircle( bounds.getCenter(), _radius, Color.IndianRed );
			graphics.drawPixel( bounds.getCenter(), Color.IndianRed, 4 );
		}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			return Collisions.circleToLine( bounds.getPosition(), _radius, from, to );
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			return Collisions.boundsToCircle( boxCollider.bounds, bounds.getCenter(), _radius );
		}


		public override bool collidesWith( CircleCollider circle )
		{
			return Collisions.circleToCircle( bounds.getCenter(), _radius, circle.bounds.getCenter(), circle._radius );
		}


		public override bool collidesWith( MultiCollider list )
		{
			return list.collidesWith( this );
		}


		public override bool collidesWith( PolygonCollider polygon )
		{
			return Collisions.polygonToCircle( polygon, bounds.getCenter(), _radius );
		}

		#endregion


		public override string ToString()
		{
			return string.Format( "[CircleCollider: bounds: {0}, radius: {1}", bounds, _radius );
		}

	}
}

