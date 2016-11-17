using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSPolygonBody : FSRenderableBody
	{
		/// <summary>
		/// verts are stored in display units. We convert to sim units if the Transform.scale changes.
		/// </summary>
		protected Vertices _verts;


		public FSPolygonBody( Subtexture subtexture, List<Vector2> verts ) : base( subtexture )
		{			
			_verts = new Vertices( verts );
		}


		public override void initialize()
		{
			base.initialize();
			body.attachPolygon( _verts, 1 );
		}


		public override void onEntityTransformChanged( Transform.Component comp )
		{
			base.onEntityTransformChanged( comp );
			if( _ignoreTransformChanges )
				return;

			// we only care about scale. base handles pos/rot
			if( comp == Transform.Component.Scale )
			{
				// fetch the Vertices, clear them, add our originals and scale them
				var poly = body.fixtureList[0].shape as PolygonShape;
				var verts = poly.vertices;
				verts.Clear();
				verts.AddRange( _verts );
				verts.scale( transform.scale );
				poly.setVerticesNoCopy( verts );

				// wake the body if it is asleep to update collisions
				if( !body.isAwake )
					body.isAwake = true;
			}
		}

	}
}
