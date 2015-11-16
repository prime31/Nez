using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Nez.Physics;


namespace Nez
{
	public class MultiCollider : Collider
	{
		public List<Collider> colliders = new List<Collider>();


		public override float width
		{
			get { return bounds.Width; }
			set { throw new NotImplementedException(); }
		}

		public override float height
		{
			get { return bounds.Height; }
			set { throw new NotImplementedException(); }
		}

		public override Rectangle bounds
		{
			get
			{
				var x = colliders[0].position.X;
				var y = colliders[0].position.Y;
				var right = colliders[0].position.X + colliders[0].width;
				var bottom = colliders[0].position.Y + colliders[0].height;

				for( var i = 1; i < colliders.Count; i++ )
				{
					if( colliders[i].position.X < x )
						x = colliders[i].position.X;
					if( colliders[i].position.Y < y )
						y = colliders[i].position.Y;
					if( colliders[i].position.X + colliders[i].width > right )
						right = colliders[i].position.X + colliders[i].width;
					if( colliders[i].position.Y + colliders[i].height > bottom )
						bottom = colliders[i].position.Y + colliders[i].height;
				}


				if( entity == null )
					return RectangleExtension.fromFloats( x, y, right - x, bottom - y );
				return RectangleExtension.fromFloats( entity.position.X + x, entity.position.Y + y, right - x, bottom - y );
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
			// we just set all of our colliders to have the proper entity. sub-colliders dont end up in the physics system
			foreach( var c in colliders )
				c.entity = entity;

			base.registerColliderWithPhysicsSystem();
		}


		public override void debugRender( Graphics graphics )
		{
			graphics.drawHollowRect( bounds, Color.LightGoldenrodYellow );
			foreach( var c in colliders )
				c.debugRender( graphics );
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
			entity.scene.physics.updateCollider( this, ref oldBounds );
		}


		public void removeCollider( Collider collider )
		{
			// store off our old bounds so we can update ourself in the physics system efficiently
			var oldBounds = bounds;

			colliders.Remove( collider );
			collider.entity = null;
			entity.scene.physics.updateCollider( this, ref oldBounds );
		}


		#region Collisions

		public override bool collidesWith( Vector2 from, Vector2 to )
		{
			foreach( var c in colliders )
				if( c.collidesWith( from, to ) )
					return true;

			return false;
		}


		public override bool collidesWith( BoxCollider boxCollider )
		{
			foreach( var c in colliders )
				if( c.collidesWith( boxCollider ) )
					return true;

			return false;
		}


		public override bool collidesWith( CircleCollider circle )
		{
			foreach( var c in colliders )
				if( c.collidesWith( circle ) )
					return true;

			return false;
		}


		public override bool collidesWith( MultiCollider list )
		{
			foreach( var c in colliders )
				if( c.collidesWith( list ) )
					return true;

			return false;
		}

		#endregion

	}
}

