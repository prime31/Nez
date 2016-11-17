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
			_verts = PolygonTools.createRectangle( _width, _height );
		}


		#region Configuration

		public FSCollisionBox setWidth( float width )
		{
			_width = width;
			_verts = PolygonTools.createRectangle( _width, _height );
			recreateFixture();
			return this;
		}


		public FSCollisionBox setHeight( float height )
		{
			_height = height;
			_verts = PolygonTools.createRectangle( _width, _height );
			recreateFixture();
			return this;
		}

		#endregion

	}
}
