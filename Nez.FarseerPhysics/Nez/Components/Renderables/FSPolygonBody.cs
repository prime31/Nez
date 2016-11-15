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
		protected List<Vector2> _verts;


		public FSPolygonBody( Subtexture subtexture, List<Vector2> verts ) : base( subtexture )
		{			
			_verts = verts;
		}


		public override void initialize()
		{
			base.initialize();
			body.attachPolygon( new Vertices( _verts ), 1 );
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
				var poly = body.FixtureList[0].Shape as PolygonShape;
				var verts = poly.Vertices;
				verts.Clear();
				verts.AddRange( _verts );
				verts.Scale( transform.scale * FSConvert.displayToSim );
				poly.Vertices = verts;
			}
		}

	}
}
