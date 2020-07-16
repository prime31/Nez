using FarseerPhysics;
using FarseerPhysics.Common;


namespace Nez.Farseer
{
	public class FSCollisionEllipse : FSCollisionPolygon
	{
		float _xRadius = 0.1f;
		float _yRadius = 0.1f;
		int _edgeCount = Settings.MaxPolygonVertices;


		public FSCollisionEllipse()
		{
		}


		public FSCollisionEllipse(float xRadius, float yRadius) : this(xRadius, yRadius, Settings.MaxPolygonVertices)
		{
		}


		public FSCollisionEllipse(float xRadius, float yRadius, int edgeCount)
		{
			Insist.IsFalse(edgeCount > Settings.MaxPolygonVertices,
				"edgeCount must be less than Settings.maxPolygonVertices");

			_xRadius = xRadius;
			_yRadius = yRadius;
			_edgeCount = edgeCount;
			_verts = PolygonTools.CreateEllipse(_xRadius * FSConvert.DisplayToSim, _yRadius * FSConvert.DisplayToSim,
				_edgeCount);
		}


		#region Configuration

		public FSCollisionEllipse SetRadii(float xRadius, float yRadius)
		{
			_xRadius = xRadius;
			_yRadius = yRadius;
			_verts = PolygonTools.CreateEllipse(_xRadius * FSConvert.DisplayToSim, _yRadius * FSConvert.DisplayToSim,
				_edgeCount);
			RecreateFixture();
			return this;
		}


		public FSCollisionEllipse SetEdgeCount(int edgeCount)
		{
			Insist.IsFalse(edgeCount > Settings.MaxPolygonVertices,
				"edgeCount must be less than Settings.maxPolygonVertices");

			_edgeCount = edgeCount;
			_verts = PolygonTools.CreateEllipse(_xRadius * FSConvert.DisplayToSim, _yRadius * FSConvert.DisplayToSim,
				_edgeCount);
			RecreateFixture();
			return this;
		}

		#endregion
	}
}