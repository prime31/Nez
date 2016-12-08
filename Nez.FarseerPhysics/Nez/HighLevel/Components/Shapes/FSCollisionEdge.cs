using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSCollisionEdge : FSCollisionShape
	{
		Vector2 _vertex1 = new Vector2( -0.01f, 0 );
		Vector2 _vertex2 = new Vector2( 0.01f, 0 );

		
		public FSCollisionEdge()
		{
			_fixtureDef.shape = new EdgeShape();
		}


		#region Configuration

		public FSCollisionEdge setVertices( Vector2 vertex1, Vector2 vertex2 )
		{
			_vertex1 = vertex1;
			_vertex2 = vertex2;
			recreateFixture();
			return this;
		}

		#endregion


		void recreateFixture()
		{
			destroyFixture();

			var edgeShape = _fixtureDef.shape as EdgeShape;
			edgeShape.vertex1 = _vertex1 * transform.scale * FSConvert.displayToSim;
			edgeShape.vertex2 = _vertex2 * transform.scale * FSConvert.displayToSim;

			createFixture();
		}

	}
}
