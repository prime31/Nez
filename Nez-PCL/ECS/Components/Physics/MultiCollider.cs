using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;


namespace Nez
{
	/// <summary>
	/// Collider that encompases one or more sub-colliders. Note that if you change the position/size of a sub-collider you have to make sure
	/// the bounds are dirtied on this collider for the update to take place in the Physics system.
	/// </summary>
	public class MultiCollider : Collider
	{
		public List<Collider> colliders = new List<Collider>();


		public override float width
		{
			get { return bounds.Width; }
			set { throw new Exception( "width/height are not settable on MultiColliders" ); }
		}

		public override float height
		{
			get { return bounds.Height; }
			set { throw new Exception( "width/height are not settable on MultiColliders" ); }
		}

		public override Rectangle bounds
		{
			get
			{
				if( _areBoundsDirty )
				{
					_bounds = new Rectangle();
					for( var i = 0; i < colliders.Count; i++ )
					{
						// we temporarily set the entity here so that we can calculate the bounds then unset it
						colliders[i].entity = entity;
						colliders[i]._areBoundsDirty = true;
						var b = colliders[i].bounds;
						colliders[i].entity = null;
						RectangleExt.union( ref _bounds, ref b, out _bounds );
					}

					_areBoundsDirty = false;
				}

				return _bounds;
			}
		}


		public MultiCollider( Collider firstCollider, params Collider[] colliderArray )
		{
			colliders.Add( firstCollider );

			foreach( var c in colliderArray )
				colliders.Add( c );
		}


		public override void registerColliderWithPhysicsSystem()
		{
			// we just set all of our colliders to have the proper entity. sub-colliders dont end up in the physics system. only
			// ourself with the full bounds does.
			for( var i = 0; i < colliders.Count; i++ )
				colliders[i].entity = entity;

			base.registerColliderWithPhysicsSystem();
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.spriteBatch.drawHollowRect( bounds, Color.LightGoldenrodYellow );
			for( var i = 0; i < colliders.Count; i++ )
				colliders[i].debugRender( graphics );
		}


		public void addCollider( Collider collider )
		{
			Debug.assertIsNotNull( entity, "You cant add colliders until you add the MultiCollider to an Entity!" );

			// store off our old bounds so we can update ourself in the physics system efficiently
			var oldBounds = bounds;

			// we do not add the extra colliders to the SpatialHash since our bounds handles encompassing all of them
			// that means we dont call through on onEnabled/disabled and the other Collider methods
			colliders.Add( collider );
			collider.entity = entity;

			if( entity != null && _isParentEntityAddedToScene )
				Physics.updateCollider( this, ref oldBounds );
		}


		public void removeCollider( Collider collider )
		{
			// store off our old bounds so we can update ourself in the physics system efficiently
			var oldBounds = bounds;

			colliders.Remove( collider );
			collider.entity = null;

			if( entity != null && _isParentEntityAddedToScene )
				Physics.updateCollider( this, ref oldBounds );
		}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			for( var i = 0; i < colliders.Count; i++ )
				if( colliders[i].collidesWith( from, to ) )
					return true;

			return false;
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			for( var i = 0; i < colliders.Count; i++ )
				if( colliders[i].collidesWith( boxCollider ) )
					return true;

			return false;
		}


		public override bool collidesWith( CircleCollider circle )
		{
			for( var i = 0; i < colliders.Count; i++ )
				if( colliders[i].collidesWith( circle ) )
					return true;

			return false;
		}


		public override bool collidesWith( MultiCollider list )
		{
			for( var i = 0; i < colliders.Count; i++ )
				if( colliders[i].collidesWith( list ) )
					return true;

			return false;
		}


		public override bool collidesWith( PolygonCollider polygon )
		{
			for( var i = 0; i < colliders.Count; i++ )
				if( colliders[i].collidesWith( polygon ) )
					return true;

			return false;
		}

		#endregion

	}
}

