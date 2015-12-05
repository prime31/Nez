using System;
using Microsoft.Xna.Framework;


namespace Nez
{
	public class BoxCollider : Collider
	{
		float _width;
		public override float width
		{
			get
			{
				return _width;
			}
			set
			{
				if( value != _width )
				{
					// store the old bounds so we can update ourself after modifying them
					var oldBounds = bounds;
					_width = value;
					if( entity != null && _isParentEntityAddedToScene )
						Physics.updateCollider( this, ref oldBounds );
					_areBoundsDirty = true;
				}
			}
		}

		float _height;
		public override float height
		{
			get
			{
				return _height;
			}
			set
			{
				if( value != _height )
				{
					// store the old bounds so we can update ourself after modifying them
					var oldBounds = bounds;
					_height = value;
					if( entity != null && _isParentEntityAddedToScene )
						Physics.updateCollider( this, ref oldBounds );
					_areBoundsDirty = true;
				}
			}
		}


		/// <summary>
		/// zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself when the
		/// entity is added to the scene.
		/// </summary>
		public BoxCollider()
		{}


		public BoxCollider( float x, float y, float width, float height )
		{
			_localPosition = new Vector2( x, y );
			_width = width;
			_height = height;
		}


		public BoxCollider( Rectangle rect ) : this( rect.X, rect.Y, rect.Width, rect.Height )
		{}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			var b = bounds;
			return Collisions.rectToLine( b.X, b.Y, b.Width, b.Height, from, to );
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			return bounds.Intersects( boxCollider.bounds );
		}


		public override bool collidesWith( CircleCollider circle )
		{
			var b = bounds;
			return Collisions.rectToCircle( b.X, b.Y, b.Width, b.Height, circle.bounds.getCenter(), circle._radius );
		}


		public override bool collidesWith( MultiCollider list )
		{
			return list.collidesWith( this );
		}


		public override bool collidesWith( PolygonCollider polygon )
		{
			throw new NotImplementedException();
		}

		#endregion


		public override string ToString()
		{
			return string.Format( "[BoxCollider: bounds: {0}", bounds );
		}

	}
}

