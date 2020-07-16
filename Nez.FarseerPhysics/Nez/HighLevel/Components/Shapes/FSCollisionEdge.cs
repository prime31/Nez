using FarseerPhysics.Collision.Shapes;
using Microsoft.Xna.Framework;


namespace Nez.Farseer
{
	public class FSCollisionEdge : FSCollisionShape
	{
		Vector2 _vertex1 = new Vector2(-0.01f, 0);
		Vector2 _vertex2 = new Vector2(0.01f, 0);


		public FSCollisionEdge()
		{
			_fixtureDef.Shape = new EdgeShape();
		}


		#region Configuration

		public FSCollisionEdge SetVertices(Vector2 vertex1, Vector2 vertex2)
		{
			_vertex1 = vertex1;
			_vertex2 = vertex2;
			RecreateFixture();
			return this;
		}

		#endregion


		void RecreateFixture()
		{
			DestroyFixture();

			var edgeShape = _fixtureDef.Shape as EdgeShape;
			edgeShape.Vertex1 = _vertex1 * Transform.Scale * FSConvert.DisplayToSim;
			edgeShape.Vertex2 = _vertex2 * Transform.Scale * FSConvert.DisplayToSim;

			CreateFixture();
		}
	}
}