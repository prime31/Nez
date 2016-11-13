using System.Collections.Generic;
using FarseerPhysics.Collision.Shapes;
using FarseerPhysics.Common;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Common.Decomposition;
using Microsoft.Xna.Framework;
using Nez.Textures;
using FarseerPhysics.Common.PolygonManipulation;


namespace Nez.Farseer
{
	/// <summary>
	/// creates a compound polygon based on an image
	/// </summary>
	public class FSCompoundPolygonBody : FSRenderableBody
	{
		protected List<Vertices> _verts = new List<Vertices>();


		public FSCompoundPolygonBody( World world, Subtexture subtexture, float density, Vector2 position = default( Vector2 ), BodyType bodyType = BodyType.Static )
			: base( world, subtexture, position, bodyType )
		{
			var data = new uint[subtexture.sourceRect.Width * subtexture.sourceRect.Height];
			subtexture.texture2D.GetData( 0, subtexture.sourceRect, data, 0, data.Length );

			var verts = PolygonTools.CreatePolygon( data, subtexture.sourceRect.Width );
			verts = SimplifyTools.DouglasPeuckerSimplify( verts, 2 );

			var decomposedVerts = Triangulate.ConvexPartition( verts, TriangulationAlgorithm.Bayazit );
			for( var i = 0; i < decomposedVerts.Count; i++ )
			{
				var polygon = decomposedVerts[i];
				polygon.Translate( -subtexture.center );
			}

			// add the fixtures
			var fixtures = Farseer.FixtureFactory.attachCompoundPolygon( decomposedVerts, density, body );

			// fetch all the Vertices and save a copy in case we need to scale them later
			foreach( var fixture in fixtures )
				_verts.Add( new Vertices( ( fixture.Shape as PolygonShape ).Vertices ) );
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
				for( var i = 0; i < body.FixtureList.Count; i++ )
				{
					var poly = body.FixtureList[i].Shape as PolygonShape;
					var verts = poly.Vertices;
					verts.Clear();
					verts.AddRange( _verts[i] );
					verts.Scale( transform.scale );
					poly.Vertices = verts;
				}
			}
		}

	}
}
