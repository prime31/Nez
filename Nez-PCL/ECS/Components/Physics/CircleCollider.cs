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


		public CircleCollider( float radius )
		{
			_radius = radius;
		}


		public CircleCollider( float radius, Vector2 origin )
		{
			_radius = radius;
			_origin = origin;
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.drawCircle( bounds.getCenter(), _radius, Color.IndianRed );
			graphics.drawPixel( entity.position + localPosition, Color.Blue, 4 );
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
			return Vector2.DistanceSquared( bounds.getCenter(), circle.bounds.getCenter() ) < ( _radius + circle._radius ) * ( _radius + circle._radius );
		}


		public override bool collidesWith( MultiCollider list )
		{
			return list.collidesWith( this );
		}

		#endregion


		public override string ToString()
		{
			return string.Format( "[CircleCollider: bounds: {0}, radius: {1}", bounds, _radius );
		}

	}
}

