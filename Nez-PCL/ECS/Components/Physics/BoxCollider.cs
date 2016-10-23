using Microsoft.Xna.Framework;
using Nez.PhysicsShapes;


namespace Nez
{
	public class BoxCollider : Collider
	{
		public float width
		{
			get { return ((Box)shape).width; }
			set { setWidth( value ); }
		}

		public float height
		{
			get { return ((Box)shape).height; }
			set { setHeight( value ); }
		}


		/// <summary>
		/// zero param constructor requires that a RenderableComponent be on the entity so that the collider can size itself when the
		/// entity is added to the scene.
		/// </summary>
		public BoxCollider()
		{}


		/// <summary>
		/// creates a BoxCollider and uses the x/y components as the localOffset
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public BoxCollider( float x, float y, float width, float height )
		{
			_localOffset = new Vector2( x, y );
			shape = new Box( width, height );
		}


		public BoxCollider( float width, float height ) : this( 0, 0, width, height )
		{}


		/// <summary>
		/// creates a BoxCollider and uses the x/y components of the Rect as the localOffset
		/// </summary>
		/// <param name="rect">Rect.</param>
		public BoxCollider( Rectangle rect ) : this( rect.X, rect.Y, rect.Width, rect.Height )
		{}


		#region Fluent setters

		public BoxCollider setWidth( float width )
		{
			var box = shape as Box;
			if( width != box.width )
			{
				// update the box and if we need to update our bounds in the Physics system
				box.updateBox( width, box.height );
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}


		public BoxCollider setHeight( float height )
		{
			var box = shape as Box;
			if( height != box.height )
			{
				// update the box and if we need to update our bounds in the Physics system
				box.updateBox( box.width, height );
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			var poly = shape as Polygon;
			graphics.batcher.drawHollowRect( bounds, DefaultColors.colliderBounds );
			graphics.batcher.drawPolygon( shape.position, poly.points, DefaultColors.colliderEdge, true );
			graphics.batcher.drawPixel( entity.transform.position, DefaultColors.colliderPosition, 4 );
			graphics.batcher.drawPixel( entity.transform.position + shape.center, DefaultColors.colliderCenter, 2 );
		}


		public override string ToString()
		{
			return string.Format( "[BoxCollider: bounds: {0}", bounds );
		}

	}
}

