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
		{
			// we stick a 1x1 box in here as a placeholder until the next frame when the Collider is added to the Entity and can get more
			// accurate auto-sizing data
			shape = new Box( 1, 1 );
			_colliderRequiresAutoSizing = true;
		}


		/// <summary>
		/// creates a BoxCollider and uses the x/y components as the localOffset
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public BoxCollider( float x, float y, float width, float height )
		{
			_localOffset = new Vector2( x + width / 2, y + height / 2 );
			shape = new Box( width, height );
		}


		public BoxCollider( float width, float height ) : this( -width / 2, -height / 2, width, height )
		{}


		/// <summary>
		/// creates a BoxCollider and uses the x/y components of the Rect as the localOffset
		/// </summary>
		/// <param name="rect">Rect.</param>
		public BoxCollider( Rectangle rect ) : this( rect.X, rect.Y, rect.Width, rect.Height )
		{}


		#region Fluent setters

		/// <summary>
		/// sets the size of the BoxCollider
		/// </summary>
		/// <returns>The size.</returns>
		/// <param name="width">Width.</param>
		/// <param name="height">Height.</param>
		public BoxCollider setSize( float width, float height )
		{
			_colliderRequiresAutoSizing = false;
			var box = shape as Box;
			if( width != box.width || height != box.height )
			{
				// update the box, dirty our bounds and if we need to update our bounds in the Physics system
				box.updateBox( width, height );
				_isPositionDirty = true;
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}


		/// <summary>
		/// sets the width of the BoxCollider
		/// </summary>
		/// <returns>The width.</returns>
		/// <param name="width">Width.</param>
		public BoxCollider setWidth( float width )
		{
			_colliderRequiresAutoSizing = false;
			var box = shape as Box;
			if( width != box.width )
			{
				// update the box, dirty our bounds and if we need to update our bounds in the Physics system
				box.updateBox( width, box.height );
				_isPositionDirty = true;
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}


		/// <summary>
		/// sets the height of the BoxCollider
		/// </summary>
		/// <returns>The height.</returns>
		/// <param name="height">Height.</param>
		public BoxCollider setHeight( float height )
		{
			_colliderRequiresAutoSizing = false;
			var box = shape as Box;
			if( height != box.height )
			{
				// update the box, dirty our bounds and if we need to update our bounds in the Physics system
				box.updateBox( box.width, height );
				_isPositionDirty = true;
				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}

		#endregion


		public override void debugRender( Graphics graphics )
		{
			var poly = shape as Polygon;
			graphics.batcher.drawHollowRect( bounds, Debug.Colors.colliderBounds, Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPolygon( shape.position, poly.points, Debug.Colors.colliderEdge, true, Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPixel( entity.transform.position, Debug.Colors.colliderPosition, 4 * Debug.Size.lineSizeMultiplier );
			graphics.batcher.drawPixel( entity.transform.position + shape.center, Debug.Colors.colliderCenter, 2 * Debug.Size.lineSizeMultiplier );
		}


		public override string ToString()
		{
			return string.Format( "[BoxCollider: bounds: {0}", bounds );
		}

	}
}

