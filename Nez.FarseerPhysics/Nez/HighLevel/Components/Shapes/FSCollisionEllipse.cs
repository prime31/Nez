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


		public FSCollisionEllipse( float xRadius, float yRadius ) : this( xRadius, yRadius, Settings.maxPolygonVertices )
		{}


		public FSCollisionEllipse( float xRadius, float yRadius, int edgeCount )
		{
			Insist.isFalse( edgeCount > Settings.maxPolygonVertices, "edgeCount must be less than Settings.maxPolygonVertices" );

			_xRadius = xRadius;
			_yRadius = yRadius;
			_edgeCount = edgeCount;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
		}


		#region Configuration

		public FSCollisionEllipse setRadii( float xRadius, float yRadius )
		{
			_xRadius = xRadius;
			_yRadius = yRadius;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
			recreateFixture();
			return this;
		}


		public FSCollisionEllipse setEdgeCount( int edgeCount )
		{
			Insist.isFalse( edgeCount > Settings.maxPolygonVertices, "edgeCount must be less than Settings.maxPolygonVertices" );

			_edgeCount = edgeCount;
			_verts = PolygonTools.createEllipse( _xRadius * FSConvert.displayToSim, _yRadius * FSConvert.displayToSim, _edgeCount );
			recreateFixture();
			return this;
		}

		#endregion

	}
}
