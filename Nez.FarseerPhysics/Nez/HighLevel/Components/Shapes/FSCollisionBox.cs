using FarseerPhysics.Common;


namespace Nez.Farseer
{
	public class FSCollisionBox : FSCollisionPolygon
	{
		float _width = 0.1f;
		float _height = 0.1f;


		public FSCollisionBox()
		{}


		public FSCollisionBox( float width, float height )
		{
			_width = width;
			_height = height;
			_verts = PolygonTools.createRectangle( FSConvert.displayToSim * _width / 2, FSConvert.displayToSim * _height / 2 );
		}


		#region Configuration

		public FSCollisionBox setSize( float width, float height )
		{
			_width = width;
			_height = height;
			_verts = PolygonTools.createRectangle( FSConvert.displayToSim * _width / 2, FSConvert.displayToSim * _height / 2 );
			_areVertsDirty = true;
			recreateFixture();
			return this;
		}

		#endregion

	}
}
