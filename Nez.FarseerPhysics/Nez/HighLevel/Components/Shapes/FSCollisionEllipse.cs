using FarseerPhysics;
using FarseerPhysics.Common;


namespace Nez.Farseer
{
	public class FSCollisionEllipse : FSCollisionPolygon
	{
		float _xRadius = 0.1f;
		float _yRadius = 0.1f;
		int _edgeCount = Settings.maxPolygonVertices;


		public FSCollisionEllipse()
		{}


		public FSCollisionEllipse( float xRadius, float yRadius )
		{
			_xRadius = xRadius;
			_yRadius = yRadius;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
		}


		#region Configuration

		public FSCollisionEllipse setRadiusX( float xRadius )
		{
			_xRadius = xRadius;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
			recreateFixture();
			return this;
		}


		public FSCollisionEllipse setRadiusY( float yRadius )
		{
			_yRadius = yRadius;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
			recreateFixture();
			return this;
		}


		public FSCollisionEllipse setEdgeCount( int edgeCount )
		{
			_edgeCount = edgeCount;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
			recreateFixture();
			return this;
		}

		#endregion

	}
}
