using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class CircleCollider : Collider
	{
		public float radius;

		public override float width
		{
			get { return radius * 2f; }
			set
			{
				// store the old bounds so we can update ourself after modifying them
				var oldBounds = bounds;
				radius = value * 0.5f;
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this, ref oldBounds );
				_areBoundsDirty = true;
			}
		}

		/// <summary>
		/// for a circle, height is radius * 2
		/// </summary>
		/// <value>The height.</value>
		public override float height
		{
			get { return radius * 2f; }
			set
			{
				// store the old bounds so we can update ourself after modifying them
				var oldBounds = bounds;
				radius = value * 0.5f;
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this, ref oldBounds );
				_areBoundsDirty = true;
			}
		}

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = RectangleExtension.fromFloats( entity.position.X + _position.X - radius, entity.position.Y + _position.Y - radius, width, height );
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
			this.radius = radius;
		}


		public CircleCollider( float radius, Vector2 origin )
		{
			this.radius = radius;
			_origin = origin;
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.drawCircle( bounds.getCenter(), radius, Color.IndianRed );
		}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			return Collisions.circleToLine( bounds.getPosition(), radius, from, to );
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			return Collisions.boundsToCircle( boxCollider.bounds, bounds.getCenter(), radius );
		}


		public override bool collidesWith( CircleCollider circle )
		{
			return Vector2.DistanceSquared( bounds.getCenter(), circle.bounds.getCenter() ) < ( radius + circle.radius ) * ( radius + circle.radius );
		}


		public override bool collidesWith( MultiCollider list )
		{
			return list.collidesWith( this );
		}

		#endregion


		public override string ToString()
		{
			return string.Format( "[CircleCollider: bounds: {0}, radius: {1}", bounds, radius );
		}

	}
}

