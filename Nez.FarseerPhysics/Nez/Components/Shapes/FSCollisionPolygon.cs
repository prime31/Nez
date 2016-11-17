using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSCollisionPolygon : FSCollisionShape
	{
		/// <summary>
		/// verts are stored in display units. We convert to sim units if the Transform.scale changes.
		/// </summary>
		protected Vertices _verts;
		Vector2 _center;


		public FSCollisionPolygon()
		{
			_fixtureDef.shape = new PolygonShape();
		}


		public FSCollisionPolygon( List<Vector2> vertices ) : this()
		{
			_verts = new Vertices( vertices );
		}


		#region Configuration

		public FSCollisionPolygon setVertices( Vertices vertices )
		{
			_verts = new Vertices( vertices );
			recreateFixture();
			return this;
		}


		public FSCollisionPolygon setVertices( List<Vector2> vertices )
		{
			_verts = new Vertices( vertices );
			recreateFixture();
			return this;
		}


		public FSCollisionPolygon setCenter( Vector2 center )
		{
			_center = center;
			recreateFixture();
			return this;
		}

		#endregion


		public override void onAddedToEntity()
		{
			updateVerts();
			createFixture();
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			if( comp == Transform.Component.Scale )
				recreateFixture();
		}


		internal override void createFixture()
		{
			updateVerts();
			base.createFixture();
		}


		protected void recreateFixture()
		{
			destroyFixture();
			updateVerts();
			createFixture();
		}


		protected void updateVerts()
		{
			var defVerts = ( _fixtureDef.shape as PolygonShape ).vertices;
			defVerts.attachedToBody = false;

			defVerts.Clear();
			defVerts.AddRange( _verts );
			defVerts.scale( transform.scale * FSConvert.displayToSim );
			defVerts.translate( ref _center );

			( _fixtureDef.shape as PolygonShape ).setVerticesNoCopy( defVerts );
		}

	}
}
