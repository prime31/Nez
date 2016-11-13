using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using Microsoft.Xna.Framework;
using Nez.Textures;


namespace Nez.Farseer
{
	public class FSPolygonBody : FSRenderableBody
	{
		protected List<Vector2> _verts;


		public FSPolygonBody( World world, Subtexture subtexture, List<Vector2> verts, float density, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
			: base( world, subtexture, position, bodyType )
		{			
			_verts = verts;
			Farseer.FixtureFactory.attachPolygon( new Vertices( verts ), density, body );
		}


		public FSPolygonBody( World world, Subtexture subtexture, Vertices verts, float density, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
			: base( world, subtexture, position, bodyType )
		{
			_verts = verts;
			Farseer.FixtureFactory.attachPolygon( verts, density, body );
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
				verts.Scale( transform.scale );
				poly.Vertices = verts;
			}
		}

	}
}
