using System;
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


		public BoxCollider( float x, float y, float width, float height )
		{
			_localOffset = new Vector2( x, y );
			shape = new Box( width, height );
		}


		public BoxCollider( float width, float height ) : this( 0, 0, width, height )
		{}


		public BoxCollider( Rectangle rect ) : this( rect.X, rect.Y, rect.Width, rect.Height )
		{}


		#region Fluent setters

		public BoxCollider setWidth( float width )
		{
			if( width != ((Box)shape).width )
			{
				// store the old bounds so we can update ourself after modifying them
				var oldBounds = bounds;
				((Box)shape).width = width;
				_areBoundsDirty = true;

				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}


		public BoxCollider setHeight( float height )
		{
			if( height != ((Box)shape).height )
			{
				// store the old bounds so we can update ourself after modifying them
				var oldBounds = bounds;
				((Box)shape).height = height;
				_areBoundsDirty = true;

				if( entity != null && _isParentEntityAddedToScene )
					Physics.updateCollider( this );
			}

			return this;
		}

		#endregion


		public override string ToString()
		{
			return string.Format( "[BoxCollider: bounds: {0}", bounds );
		}

	}
}

